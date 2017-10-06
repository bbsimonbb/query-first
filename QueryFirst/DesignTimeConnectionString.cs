using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class DesignTimeConnectionString
    {
        CodeGenerationContext _ctx;
        public DesignTimeConnectionString(CodeGenerationContext ctx)
        {
            _ctx = ctx;
        }
        protected ConnectionStringSettings _v;
		protected AppSettingsSection _d;

        /// <summary>
        /// For recuperating the query schema at design time.
        /// </summary>
        public virtual ConnectionStringSettings v
        {
            get
            {
                if (_v == null)
                {
                    // if the query defines a QfDefaultConnection, use it.
                    var match = Regex.Match(_ctx.Query.Text, "--QfDefaultConnection(=|:)(?<cstr>.*)$",RegexOptions.Multiline);
                    //var match = Regex.Match(Query.Text, "--QfDefaultConnectionString ?(=|:)? ?\"?(?<cstr>[^ \"]*)\" ? $");
                    if (match.Success)
                    {
                        string providerName;
                        var matchProviderName = Regex.Match(_ctx.Query.Text, "--QfDefaultConnectionProviderName(=|:)(?<pn>.*)$");
                        if (matchProviderName.Success)
                        {
                            providerName = matchProviderName.Groups["pn"].Value;
                            _v = new ConnectionStringSettings("QfDefaultConnection", match.Groups["cstr"].Value, matchProviderName.Groups["pn"].Value);
                        }
                        else
                        {
                            _v = new ConnectionStringSettings("QfDefaultConnection", match.Groups["cstr"].Value, "System.Data.SqlClient");
                        }

                    }
                    else if (_ctx.ProjectConfig != null)
                    {
                        _v = _ctx.ProjectConfig.ConnectionStrings["QfDefaultConnection"];
                    }
                }
                return _v;
            }
        }
		/// <summary>
		/// For determining pre-build parameter parsing (requires extension)
		/// </summary>
		public bool AlwaysParse
		{
			get
			{
				try
				{
					if (_ctx.ProjectConfig.AppSettings == null)
						return false;
					if (_ctx.ProjectConfig.AppSettings["QfCommentDesignTimeInRelease"] == null)
						return false;

					return Convert.ToBoolean(_ctx.ProjectConfig.AppSettings["QfCommentDesignTimeInRelease"].Value);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
		public bool IsPresent
        {
            get { return !string.IsNullOrEmpty(v.ConnectionString); }
        }
        public bool IsProviderValid
        {
            get { return TinyIoC.TinyIoCContainer.Current.CanResolve<IProvider>(v.ProviderName); }
        }
    }
}
