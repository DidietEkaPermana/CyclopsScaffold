using EnvDTE;
using Microsoft.AspNet.Scaffolding;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CyclopsScaffold.UI
{
    /// <summary>
    /// View model for code types so that it can be displayed on the UI.
    /// </summary>
    public class CustomViewModel
    {
        List<ModelType> _SelectedModelType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The code generation context</param>
        public CustomViewModel(CodeGenerationContext context)
        {
            Context = context;
            _SelectedModelType = new List<ModelType>();
        }

        /// <summary>
        /// This gets all the Model types from the active project.
        /// </summary>
        public IEnumerable<ModelType> ContextTypes
        {
            get
            {
                ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));

                return codeTypeService
                    .GetAllCodeTypes(Context.ActiveProject)
                    .Where(p => p.IsValidDbContextType())
                    .Select(codeType => new ModelType(codeType));
            }
        }

        public IEnumerable<ModelType> ModelTypes
        {
            get
            {
                ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));

                return codeTypeService
                    .GetAllCodeTypes(Context.ActiveProject)
                    .Where(codeType => codeType.IsValidWebProjectEntityType() && codeType.Namespace.FullName == SelectedContextType.CodeType.Namespace.FullName)
                    .Select(codeType => new ModelType(codeType));
            }
        }

        public IEnumerable<string> Areas
        {
            get
            {
                List<string> areas = new List<string>();

                ProjectItem AreaItem = null;
                foreach (ProjectItem pi in Context.ActiveProject.ProjectItems)
                {
                    if (pi.Name == "Areas")
                        AreaItem = pi;
                }

                if (AreaItem != null)
                {
                    foreach (ProjectItem pi in AreaItem.ProjectItems)
                    {
                        areas.Add(pi.Name);
                    }
                }

                return areas;
            }
        }

        public ModelType SelectedContextType { get; set; }

        public List<ModelType> SelectedModelType { get{return _SelectedModelType;}}

        public CodeGenerationContext Context { get; private set; }

        public bool IsArea { get; set; }

        public string SelectedArea { get; set; }

        public bool SkipIfExists { get; set; }
    }
}
