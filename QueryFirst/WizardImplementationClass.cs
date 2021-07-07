using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;
using EnvDTE;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace QueryFirst
{
    public class WizardImplementation : IWizard
    {
        //private UserInputForm inputForm;
        //private string customMessage;

        // This method is called before opening any item that 
        // has the OpenInEditor attribute.
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        // This method is only called for item templates,
        // not for project templates.
        public void ProjectItemFinishedGenerating(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string path = item.FileNames[0];
            string parentPath = null;

            if (path.EndsWith(".gen.cs"))
                parentPath = path.Replace(".gen.cs", ".sql");
            else if (path.EndsWith("Results.cs"))
                parentPath = path.Replace("Results.cs", ".sql");

            if (!string.IsNullOrEmpty(parentPath))
            {
                ProjectItem parent = item.DTE.Solution.FindProjectItem(parentPath);
                if (parent == null)
                    return;
                item.Remove();
                var newItem = parent.ProjectItems.AddFromFile(path);

                if (path.EndsWith("Results.cs"))
                {
                    // correct class names
                    var _userPartialClass = File.ReadAllText(path);
                    _userPartialClass = _userPartialClass.Replace("Results¤", "");

                    WriteAndFormat(newItem, _userPartialClass);
                }
            }
        }

        // This method is called after the project is created.
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            //try
            //{
            //    // Display a form to the user. The form collects 
            //    // input for the custom message.
            //    inputForm = new UserInputForm();
            //    inputForm.ShowDialog();

            //    customMessage = UserInputForm.CustomMessage;

            //    // Add custom parameters.
            //    replacementsDictionary.Add("$custommessage$",
            //        customMessage);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        // This method is only called for item templates,
        // not for project templates.
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        private void WriteAndFormat(ProjectItem genFile, string code)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool rememberToClose = false;
            if (!genFile.IsOpen)
            {
                genFile.Open();
                rememberToClose = true;
            }
            var textDoc = ((TextDocument)genFile.Document.Object());
            var ep = textDoc.CreateEditPoint();
            ep.ReplaceText(textDoc.EndPoint, code, 0);
            ep.SmartFormat(textDoc.EndPoint);
            genFile.Save();
            if (rememberToClose)
            {
                genFile.Document.Close();
            }
        }
    }
}