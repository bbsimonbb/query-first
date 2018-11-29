//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;

//namespace QueryFirst
//{
//    public class DesignTimeConnectionString
//    {
//        ICodeGenerationContext _ctx;
//        public DesignTimeConnectionString(ICodeGenerationContext ctx)
//        {
//            _ctx = ctx;
//        }
//        protected ConnectionStringSettings _v;

//        /// <summary>
//        /// For fetching the query schema at design time.
//        /// </summary>
//        public virtual ConnectionStringSettings v
//        {
//            get
//            {
//                return new ConnectionStringSettings("QfDefaultConnection", _ctx.Config.DefaultConnection, _ctx.Config.Provider);
//            }
//        }
//        public bool IsPresent
//        {
//            get { return !string.IsNullOrEmpty(v.ConnectionString); }
//        }
//        public bool IsProviderValid
//        {
//            get { return TinyIoC.TinyIoCContainer.Current.CanResolve<IProvider>(v.ProviderName); }
//        }
//    }
//}
