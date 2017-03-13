using EnvDTE;
using EnvDTE80;
using System;

namespace QueryFirst
{
    class VSOutputWindow
    {
        private OutputWindowPane _outPane;
        public VSOutputWindow(DTE2 env)
        {
            OutputWindow outputWindow = env.ToolWindows.OutputWindow;

            for (uint i = 1; i <= outputWindow.OutputWindowPanes.Count; i++)
            {
                if (outputWindow.OutputWindowPanes.Item(i).Name.Equals("QueryFirst", StringComparison.CurrentCultureIgnoreCase))
                {
                    _outPane = outputWindow.OutputWindowPanes.Item(i);
                    break;
                }
            }

            if (_outPane == null)
                _outPane = outputWindow.OutputWindowPanes.Add("QueryFirst");

        }
        public void Write(string message)
        {
            _outPane.OutputString(message);
        }
    }
}
