using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace MVVMCrossPlugin_VSAddIn
{
    public class ProjectWizard : IWizard
    {
        private DTE _dte;
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var pluginName = "MyPlugin";
            var safeProjectName = replacementsDictionary["$safeprojectname$"];
            if (safeProjectName != null)
            {
                var lastDot = safeProjectName.LastIndexOf(".", StringComparison.Ordinal);
                pluginName = safeProjectName.Substring(lastDot + 1);
            }
            //_dte = (DTE) automationObject;

            replacementsDictionary.Add("$pluginname$", pluginName);
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }
    }
}
