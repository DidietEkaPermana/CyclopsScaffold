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
            var project = Context.ActiveProject;

            //var codeType = _viewModel.SelectedModelType.CodeType;

            //ICodeTypeService codeTypeService = (ICodeTypeService)Context
            //        .ServiceProvider.GetService(typeof(ICodeTypeService));


            //var modelTypes = codeTypeService
            //                            .GetAllCodeTypes(project)
            //                            .Where(p => p.IsValidDbContextType())
            //                            .Select(p => new ModelType(p));

            //var dbContextClass = modelTypes.Where(p => p.CodeType.Namespace.FullName == codeType.Namespace.FullName).FirstOrDefault();

            var dbContextClass = _viewModel.SelectedModelType;

            ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));


            var modelTypes = codeTypeService
                                        .GetAllCodeTypes(project)
                                        .Where(p => p.IsValidWebProjectEntityType() && p.Namespace.FullName == dbContextClass.CodeType.Namespace.FullName)
                                        .Select(p => new ModelType(p));

            var selectionRelativePath = "Models\\";
            AddMvcModels(project, selectionRelativePath);

            foreach (var codeType in modelTypes.Select(p => p.CodeType))
            {
                // Get the Entity Framework Meta Data
                IEntityFrameworkService efService = (IEntityFrameworkService)Context.ServiceProvider.GetService(typeof(IEntityFrameworkService));
                ModelMetadata efMetadata = efService.AddRequiredEntity(Context, dbContextClass.TypeName, codeType.FullName);

                if (efMetadata.PrimaryKeys.Count() > 1)
                    continue;

                selectionRelativePath = "Controllers\\";
                AddMvcControllers(project, selectionRelativePath, codeType, dbContextClass, efMetadata);

                selectionRelativePath = "Views\\";
                AddMvcViews(project, selectionRelativePath, codeType, efMetadata);
            }
        }

        private void AddMvcControllers(Project project, string selectionRelativePath, CodeType codeType, ModelType dbContextClass, ModelMetadata efMetadata)
        {
            // Get the selected code type
            var defaultNamespace = (project.Name + ".Controllers");

            //get context
            ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));


            string modelTypeVariable = GetTypeVariable(codeType.Name);

            string controllerName = codeType.Name + "Controller";
            string outputFolderPath = Path.Combine(selectionRelativePath, controllerName);

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
                "Controller",
                parameters,
                skipIfExists: false);
        }

        private void AddMvcModels(Project project, string selectionRelativePath)
        {
            // Get the selected code type
            var defaultNamespace = (project.Name + ".Models");

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
                skipIfExists: false);
        }

        private void AddMvcViews(Project project, string selectionRelativePath, CodeType codeType, ModelMetadata efMetadata)
        {
            // Get the selected code type
            var defaultNamespace = (project.Name + ".Views");

            string outputFolderPath = Path.Combine(selectionRelativePath, codeType.Name + "\\Index");

            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            var parameters = new Dictionary<string, object>()
            {
                {"ModelType", codeType},
                {"Namespace", defaultNamespace},
                {"MetadataModel", efMetadata}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFileFromTemplate(project,
                outputFolderPath,
                "Views",
                parameters,
                skipIfExists: false);
        }

        #region function library
        private string GetTypeVariable(string typeName)
        {
            return typeName.Substring(0, 1).ToLower() + typeName.Substring(1, typeName.Length - 1);
        }
        #endregion function library
    }
}
