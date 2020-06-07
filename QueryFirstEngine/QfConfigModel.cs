using System;
using System.Collections.Generic;
using System.Text;

namespace QueryFirst
{
    public class QFConfigModel
    {
        public string defaultConnection { get; set; }
        public string provider { get; set; }
        public string helperAssembly { get; set; }
        public bool makeSelfTest { get; set; }
        public bool connectEditor2DB { get; set; }
    }
}
