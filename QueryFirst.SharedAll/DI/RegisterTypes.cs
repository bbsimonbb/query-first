using System;
using System.Collections.Generic;
using System.Reflection;

namespace QueryFirst
{
    public sealed class RegisterTypes
    {
        public static void Register(List<string> helperAssemblies, bool force = false)
        {
            try
            {
                var ctr = TinyIoCContainer.Current;


                //kludge
                if (force == true)
                {
                    ctr.Dispose();
                }
                List<Assembly> assemblies = new List<Assembly>();
                if (helperAssemblies != null)
                {
                    foreach (var ass in helperAssemblies)
                    {
                        try
                        {
                            assemblies.Add(Assembly.LoadFrom(ass));
                        }
                        catch (Exception ex)
                        {
                            QfConsole.WriteLine($@"Error loading assembly {ass}\n{ex}");
                        }
                    }
                }
                assemblies.Add(Assembly.GetExecutingAssembly());
                // QueryFirst.CoreLib, with shared code, corelib is no longer an assembly
                //assemblies.Add(Assembly.GetAssembly(typeof(AdoSchemaFetcher)));
                var then = new DateTime();
                // First in wins
                TinyIoCContainer.Current.AutoRegister(assemblies, DuplicateImplementationActions.RegisterSingle);
                var canResolve = TinyIoCContainer.Current.CanResolve<IProvider>( "MyGroovyProvider");
                var now = new DateTime();
                QfConsole.WriteLine($@"{assemblies.Count} assemblies registered in {(now - then).TotalMilliseconds}ms");
                // IProvider, for instance, has multiple implementations. To resolve we use the provider name on the connection string, 
                // which must correspond to the fully qualified name of the implementation. ie. QueryFirst.Providers.SqlClient for SqlServer

            }
            catch (Exception ex)
            {
                QfConsole.WriteLine(ex.ToString());
            }
        }
    }
}
