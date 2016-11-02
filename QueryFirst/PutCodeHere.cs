using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace QueryFirst
{
    public class PutCodeHere
    {
        private string _fullPath;
        private ProjectItem _item;
        public PutCodeHere(ProjectItem item)
        {
            _item = item;
            _fullPath = item.FileNames[0];
        }
        public void WriteAndFormat(string code)
        {
            bool rememberToClose = false;
            if (!_item.IsOpen)
            {
                _item.Open();
                rememberToClose = true;
            }
            var textDoc = ((TextDocument)_item.Document.Object());
            var ep = textDoc.CreateEditPoint();
            ep.ReplaceText(textDoc.EndPoint, code, 0);
            ep.SmartFormat(textDoc.EndPoint);
            _item.Save();
            if (rememberToClose)
            {
                _item.Document.Close();
            }
        }
    }
}
