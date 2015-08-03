using CyclopsScaffold.UI;
using EnvDTE;
using Microsoft.AspNet.Scaffolding;
using Microsoft.AspNet.Scaffolding.Core.Metadata;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CyclopsScaffold
{
    public class CustomCodeGenerator : CodeGenerator
    {
        CustomViewModel _viewModel;

        /// <summary>
        /// Constructor for the custom code generator
        /// </summary>
        /// <param name="context">Context of the current code generation operation based on how scaffolder was invoked(such as selected project/folder) </param>
        /// <param name="information">Code generation information that is defined in the factory class.</param>
        public CustomCodeGenerator(
            CodeGenerationContext context,
            CodeGeneratorInformation information)
            : base(context, information)
        {
            _viewModel = new CustomViewModel(Context);
        }


        /// <summary>
        /// Any UI to be displayed after the scaffolder has been selected from the Add Scaffold dialog.
        /// Any validation on the input for values in the UI should be completed before returning from this method.
        /// </summary>
        /// <returns></returns>
        public override bool ShowUIAndValidate()
        {
            // Bring up the selection dialog and allow user to select a model type
            SelectModelWindow window = new SelectModelWindow(_viewModel);
            bool? showDialog = window.ShowDialog();
            return showDialog ?? false;
        }

        /// <summary>
        /// This method is executed after the ShowUIAndValidate method, and this is where the actual code generation should occur.
        /// In this example, we are generating a new file from t4 template based on the ModelType selected in our UI.
        /// </summary>
        public override void GenerateCode()
        {
            Project projectActive = Context.ActiveProject;
            Solution solution = projectActive.DTE.Solution; 

            List<Project> list = new List<Project>();
            foreach (Project projectA in solution.Projects)
            {
                if (projectA.Kind == Constants.vsProjectKindSolutionItems)
                    list.AddRange(GetSolutionFolderProjects(projectA));
                else
                    list.Add(projectA);
            }

            var dbContextClass = _viewModel.SelectedContextType;

            var modelTypes = _viewModel.SelectedModelType;

            //find context projects
            Project projectContext = null;
            foreach(Project projectA in list){
                if (dbContextClass.CodeType.Namespace.FullName.StartsWith(projectA.Name + "."))
                    projectContext = projectA;
            }

            string defaultNamespace = null;
            string AreaPath = string.Empty;

            //return;

            if (_viewModel.SelectedController != "MVC")
                AddWebApiConfig(projectActive, dbContextClass, modelTypes.Select(p => p.CodeType).ToList(), projectContext.Name + ".Models");

            #region Area Init
            if (_viewModel.IsArea)
            {
                string AreaName = _viewModel.SelectedArea;
                ProjectItem AreaItem = AddArea(projectActive, AreaName);
                AreaPath = string.Format("Areas\\{0}\\", AreaName);
                defaultNamespace = AreaItem.GetDefaultNamespace();
            }
            #endregion Area Init

            var selectionRelativePath = AreaPath;
            AddMvcModels(projectActive, selectionRelativePath + "Models\\", defaultNamespace);

            foreach (var codeType in modelTypes.Select(p => p.CodeType))
            {
                // Get the Entity Framework Meta Data
                IEntityFrameworkService efService = (IEntityFrameworkService)Context.ServiceProvider.GetService(typeof(IEntityFrameworkService));
                ModelMetadata efMetadata = efService.AddRequiredEntity(Context, dbContextClass.TypeName, codeType.FullName);

                if (efMetadata.PrimaryKeys.Count() > 1)
                    continue;

                if (_viewModel.SelectedController != "MVC")
                    AddViewsControllers(projectActive, selectionRelativePath + "Controllers\\", codeType, defaultNamespace);

                AddBLL(projectContext, "BLL\\", codeType, dbContextClass, efMetadata);

                if (_viewModel.SelectedController == "WEBAPI")
                    AddMvcControllers(projectActive, selectionRelativePath + "Controllers\\API\\", codeType, efMetadata, defaultNamespace);
                else if (_viewModel.SelectedController == "ODataV3" || _viewModel.SelectedController == "ODataV4")
                    AddMvcControllers(projectActive, selectionRelativePath + "Controllers\\OData\\", codeType, efMetadata, defaultNamespace);
                else
                    AddMvcControllers(projectActive, selectionRelativePath + "Controllers\\", codeType, efMetadata, defaultNamespace);

                AddMvcViews(projectActive, selectionRelativePath + "Views\\", codeType, dbContextClass, efMetadata, defaultNamespace);
            }
        }

        private void AddBLL(Project project, string selectionRelativePath, CodeType codeType, ModelType dbContextClass, ModelMetadata efMetadata)
        {
            // Get the selected code type
            var defaultNamespace = (project.Name + ".BLL");

            string modelTypeVariable = GetTypeVariable(codeType.Name);

            string BLLName = codeType.Name + "Service";
            string outputFolderPath = Path.Combine(selectionRelativePath, BLLName);

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"ModelType", codeType},
                {"Namespace", defaultNamespace},
                {"dbContext", dbContextClass.ShortTypeName},
                {"MetadataModel", efMetadata},
                {"EntitySetVariable", modelTypeVariable},
                {"RequiredNamespaces", new HashSet<string>(){codeType.Namespace.FullName, (project.Name + ".Models")}}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "BLL",
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private void AddMvcControllers(Project project, string selectionRelativePath, CodeType codeType, ModelMetadata efMetadata, string defaultNamespace = null)
        {
            //get model namespace
            string modelNamespace = string.Empty;
            if (string.IsNullOrEmpty(defaultNamespace))
                modelNamespace = (project.Name + ".Models");
            else
                modelNamespace = defaultNamespace + ".Models";

            // Get the selected code type
            if (string.IsNullOrEmpty(defaultNamespace))
                defaultNamespace = (project.Name + ".Controllers");
            else
                defaultNamespace += ".Controllers";

            if (_viewModel.SelectedController == "WEBAPI")
                defaultNamespace += ".API";
            else if (_viewModel.SelectedController == "ODataV3" || _viewModel.SelectedController == "ODataV4")
                defaultNamespace += ".OData";

            string modelTypeVariable = GetTypeVariable(codeType.Name);

            string controllerName = codeType.Name + "Controller";
            string outputFolderPath = Path.Combine(selectionRelativePath, controllerName);

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"ModelType", codeType},
                {"Namespace", defaultNamespace},
                {"MetadataModel", efMetadata},
                {"RequiredNamespaces", new HashSet<string>(){codeType.Namespace.FullName, (GetParentNameSpace(codeType.Namespace.FullName) + ".BLL"), modelNamespace}}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "Controllers\\" + _viewModel.SelectedController,
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private void AddViewsControllers(Project project, string selectionRelativePath, CodeType codeType, string defaultNamespace = null)
        {
            // Get the selected code type
            if (string.IsNullOrEmpty(defaultNamespace))
                defaultNamespace = (project.Name + ".Controllers");
            else
                defaultNamespace += ".Controllers";

            string controllerName = codeType.Name + "Controller";
            string outputFolderPath = Path.Combine(selectionRelativePath, controllerName);

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"ModelType", codeType},
                {"Namespace", defaultNamespace}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "Controllers\\Views",
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private void AddMvcModels(Project project, string selectionRelativePath, string defaultNamespace = null)
        {
            // Get the selected code type
            if (string.IsNullOrEmpty(defaultNamespace))
                defaultNamespace = (project.Name + ".Models");
            else
                defaultNamespace += ".Models";

            string outputFolderPath = Path.Combine(selectionRelativePath, "JsonUIModels");

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"Namespace", defaultNamespace}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "Models",
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private void AddWebApiConfig(Project project, ModelType dbContextClass, List<CodeType> ListModelType, string ModelNameSpace)
        {
            string outputFolderPath = "App_Start\\WebApiConfig";

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"Namespace", project.Name},
                {"dbContext", dbContextClass.ShortTypeName},
                {"ListModelType", ListModelType},
                {"ModelNameSpace", ModelNameSpace}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "WebApiConfig\\" + _viewModel.SelectedController,
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private void AddMvcViews(Project project, string selectionRelativePath, CodeType codeType, ModelType dbContextClass, ModelMetadata efMetadata, string defaultNamespace = null)
        {
            // Get the selected code type
            if (string.IsNullOrEmpty(defaultNamespace))
                defaultNamespace = (project.Name + ".Views");
            else
                defaultNamespace += ".Views";

            string outputFolderPath = Path.Combine(selectionRelativePath, codeType.Name + "\\Index");

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"ModelType", codeType},
                {"Namespace", defaultNamespace},
                {"dbContext", dbContextClass.ShortTypeName},
                {"MetadataModel", efMetadata}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "Views\\Views" + _viewModel.SelectedView + _viewModel.SelectedController,
                parameters,
                skipIfExists: _viewModel.SkipIfExists);
        }

        private ProjectItem AddArea(Project pj, string AreaName)
        {
            ProjectItem AreaItem = null;
            foreach (ProjectItem pi in pj.ProjectItems)
            {
                if (pi.Name == "Areas")
                    AreaItem = pi;
            }

            if (AreaItem == null)
                AreaItem = pj.ProjectItems.AddFolder("Areas");

            ProjectItem AreaNameItem = null;
            foreach (ProjectItem pi in AreaItem.ProjectItems)
            {
                if (pi.Name == AreaName)
                    AreaNameItem = pi;
            }

            if (AreaNameItem == null)
            {
                AreaNameItem = AreaItem.ProjectItems.AddFolder(AreaName);
                AreaNameItem.ProjectItems.AddFromTemplate("Templates\\MvcAreaItemTemplate\\MvcAreaItemTemplate.cs.vstemplate", AreaName);

                // Setup the scaffolding item creation parameters to be passed into the T4 template.
                var parameters = new Dictionary<string, object>()
                {
                    {"AreaName", AreaName},
                    {"Namespace", AreaNameItem.GetDefaultNamespace()}
                };

                string areaName = AreaName + "AreaRegistration";
                var outputFolderPath = Path.Combine(string.Format("Areas\\{0}", AreaName), areaName);

                this.AddFileFromTemplate(pj,
                    outputFolderPath,
                    "AreaRegistration",
                    parameters,
                    skipIfExists: _viewModel.SkipIfExists);
            }

            return AreaNameItem;
        }

        #region function library
        private string GetParentNameSpace(string strNamespace)
        {
            string[] names = strNamespace.Split('.');

            if (names != null || names.Count() > 0)
                return names[0];
            else
                return strNamespace;
        }
        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            //for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            foreach (ProjectItem item in solutionFolder.ProjectItems)
            {
                //var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                var subProject = item.SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == Constants.vsProjectKindSolutionItems)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }

        private string GetTypeVariable(string typeName)
        {
            return typeName.Substring(0, 1).ToLower() + typeName.Substring(1, typeName.Length - 1);
        }
        #endregion function library

        #region unused
        //private void AddDTOModels(Project project, string selectionRelativePath, CodeType codeType, ModelMetadata efMetadata)
        //{
        //    var defaultNamespace = (project.Name + ".Models");

        //    string outputFolderPath = Path.Combine(selectionRelativePath, codeType.Name + "DTO");

        //    // Setup the scaffolding item creation parameters to be passed into the T4 template.
        //    var parameters = new Dictionary<string, object>()
        //    {
        //        {"ModelType", codeType},
        //        {"Namespace", defaultNamespace},
        //        {"MetadataModel", efMetadata}
        //    };

        //    foreach (var pop in efMetadata.Properties)
        //    {
        //        //pop.TypeName;
        //    }

        //    // Add the custom scaffolding item from T4 template.
        //    this.AddFileFromTemplate(project,
        //        outputFolderPath,
        //        "DTO",
        //        parameters,
        //        skipIfExists: _viewModel.SkipIfExists);
        //}
        #endregion unused
    }
}
