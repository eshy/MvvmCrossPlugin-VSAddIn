using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace MVVMCrossPlugin_VSAddIn
{
    public class ProjectWizard : IWizard
    {
        private DTE _dte;
        private string _destinationDirectory;
        private string _safeProjectName;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                _dte = automationObject as DTE;
                _destinationDirectory = replacementsDictionary["$destinationdirectory$"];
                _safeProjectName = replacementsDictionary["$safeprojectname$"];

                var pluginName = "MyPlugin";
                var safeProjectName = replacementsDictionary["$safeprojectname$"];
                if (safeProjectName != null)
                {
                    var lastDot = safeProjectName.LastIndexOf(".", StringComparison.Ordinal);
                    pluginName = safeProjectName.Substring(lastDot + 1);
                }
                replacementsDictionary.Add("$pluginname$", pluginName);
            }
            catch (WizardCancelledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new WizardCancelledException("Wizard Exception", ex);
            }
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
            if (!_destinationDirectory.EndsWith(_safeProjectName + Path.DirectorySeparatorChar + _safeProjectName))
                return;

            //The projects were created under a seperate folder -- lets fix it
            var projectsObjects =
            (from Project childProject in _dte.Solution.Projects
                select new Tuple<Project, Project>(null, childProject)).ToList();

            var projectPaths = (from p in projectsObjects select p.Item2.FileName).ToList();

            for (var i = projectsObjects.Count-1; i>=0;i--)
            {
                _dte.Solution.Remove(projectsObjects[i].Item2);
            }
            foreach (var projectBadPath in projectPaths)
            {
                var projectGoodPath = projectBadPath.Replace(
                    _safeProjectName + Path.DirectorySeparatorChar + _safeProjectName + Path.DirectorySeparatorChar,
                    _safeProjectName + Path.DirectorySeparatorChar);

                //If it's the core project, we'll move it to .Core for now
                if (Path.GetDirectoryName(projectGoodPath) == _destinationDirectory)
                {
                    projectGoodPath = projectGoodPath.Replace(_destinationDirectory,
                        string.Concat(_destinationDirectory, ".Core"));
                } else if (projectGoodPath.Contains(".Nuget"))
                {
                    projectGoodPath = projectGoodPath.Replace(_safeProjectName + ".Nuget", "nuspec");
                }

                Directory.Move(Path.GetDirectoryName(projectBadPath), Path.GetDirectoryName(projectGoodPath));

            }

            //Move Core project back to correct folder
            MoveDirectory(_destinationDirectory + ".Core", _destinationDirectory);

            foreach (var projectBadPath in projectPaths)
            {
                if (projectBadPath.Contains(".Nuget"))
                {
                    continue;
                }
                var projectGoodPath = projectBadPath.Replace(
                    _safeProjectName + Path.DirectorySeparatorChar + _safeProjectName + Path.DirectorySeparatorChar,
                    _safeProjectName + Path.DirectorySeparatorChar);
                _dte.Solution.AddFromFile(projectGoodPath);
            }


            ThreadPool.QueueUserWorkItem(dir =>
            {
                System.Threading.Thread.Sleep(2000);
                if (Directory.Exists(_destinationDirectory + ".Core"))
                {
                    Directory.Delete(_destinationDirectory + ".Core", true);
                }
            }, _destinationDirectory);
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(Path.GetDirectoryName);
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }
    }
}
