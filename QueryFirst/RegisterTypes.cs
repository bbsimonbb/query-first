using System;
using System.Collections.Generic;
using System.Reflection;
using TinyIoC;

namespace QueryFirst
{
    public sealed class RegisterTypes
    {
        // https://csharpindepth.com/articles/singleton
        private static RegisterTypes instance = null;
        private static readonly object padlock = new object();

        RegisterTypes()
        {
        }

        public static RegisterTypes Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RegisterTypes();
                    }
                    return instance;
                }
            }
        }

        public void Register( VSOutputWindow outputWindow, bool force = false)
        {
            try
            {
                var ctr = TinyIoCContainer.Current;


                //kludge
                if (force == true)
                {
                    ctr.Dispose();
                }
                else if (TinyIoCContainer.Current.CanResolve<IWrapperClassMaker>())
                {
                    //_VSOutputWindow.Write("Already registered\n");
                    return;
                }
                System.Configuration.KeyValueConfigurationElement helperAssembly = null;

                List<Assembly> assemblies = new List<Assembly>();
                if (helperAssembly != null && !string.IsNullOrEmpty(helperAssembly.Value))
                {
                    assemblies.Add(Assembly.LoadFrom(helperAssembly.Value));
                }
                assemblies.Add(Assembly.GetExecutingAssembly());
                TinyIoCContainer.Current.AutoRegister(assemblies, DuplicateImplementationActions.RegisterSingle);
                // IProvider, for instance, has multiple implementations. To resolve we use the provider name on the connection string, 
                // which must correspond to the fully qualified name of the implementation. ie. QueryFirst.Providers.SqlClient for SqlServer

                //_VSOutputWindow.Write("Registered types...\n");
            }
            catch (Exception ex)
            {
                //outputWindow.Write(ex.Message + '\n' + ex.StackTrace);
            }
        }
    }
}
