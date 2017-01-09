/*
Author: Arno0x0x, Twitter: @Arno0x0x

-------------------- x64 platform ----------------
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:parseAssembly.exe parseAssembly.cs

Or, with debug information:
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /define:DEBUG /out:dbc2_agent_debug.exe *.cs

https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp

*/

namespace ParseAssembly
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Net;

    class ParseAssembly
    {
        static void Main(string[] args)
        {
            string assemblyPath = String.Empty;
            bool base64Encoded = false;
            byte[] assemblyBytes;

            // Validating args
            if (args.Length == 0)
            {
                Console.WriteLine("[ERROR], missing argument. Please specify an assembly to load, either as a local file or a URL to download from.");
                Console.WriteLine("[USAGE]: parseAssembly.exe <assembly path or URL> [base64]");
                return;
            }

            // Retrieve the assembly path
            assemblyPath = args[0];
            Console.WriteLine("Loading assembly: {0}",assemblyPath);

            // Check if the assembly if base64 encoded
            if (args.Length == 2)
            {
                if(args[1] == "base64") base64Encoded = true;
            }

            // Check if the assembly is to be loaded from a URL
            if (assemblyPath.StartsWith("http")) 
            {
                WebClient webclient = new WebClient();
                IWebProxy defaultProxy = WebRequest.DefaultWebProxy;
                if (defaultProxy != null)
                {
                    defaultProxy.Credentials = CredentialCache.DefaultCredentials;
                    webclient.Proxy = defaultProxy;
                }

                if (base64Encoded) assemblyBytes = Convert.FromBase64String(webclient.DownloadString(assemblyPath));
                else assemblyBytes = webclient.DownloadData(assemblyPath);
            }
            // Else it's a local file
            else
            {
                if (base64Encoded) assemblyBytes = Convert.FromBase64String(File.ReadAllText(assemblyPath));
                else assemblyBytes = File.ReadAllBytes(assemblyPath);
            }

            try
            {
                Assembly a = Assembly.Load(assemblyBytes);

                // Find the assembly entry point if any
                MethodInfo entryPoint = a.EntryPoint;
                if (entryPoint != null)
                {
                    Console.WriteLine("----- Assembly EntryPoint -----");
                    foreach ( ParameterInfo pi in entryPoint.GetParameters() )
                    {
                        Console.WriteLine("\tMethod: {0}, Parameter: Type={1}, Name={2}", entryPoint.Name, pi.ParameterType, pi.Name);
                    }
                }
                

                foreach(Type type in a.GetExportedTypes())
                {                  
                    Console.WriteLine(type.FullName);

                    foreach(MemberInfo mi in type.GetMembers())
                    {
                        // Member is a method
                        if (mi.MemberType == MemberTypes.Method)
                        {
                            foreach ( ParameterInfo pi in ((MethodInfo) mi).GetParameters() )
                            {
                                Console.WriteLine("\tMethod: {0}, Parameter: Type={1}, Name={2}", mi.Name, pi.ParameterType, pi.Name);
                            }
                        }

                        // If the member is a property, display information about the property's accessor methods.
                        else if (mi.MemberType==MemberTypes.Property)
                        {
                            foreach (MethodInfo am in ((PropertyInfo) mi).GetAccessors() )
                            {
                                Console.WriteLine("\tProperty: {0}, Accessor method: {1}", mi.Name, am);
                            }
                        }


                    }
                }

                /* CODE FOR ACTUALLY INVOKE THE ASSEMBLY ENTRY POINT
                MethodInfo method = a.EntryPoint;
                object o = a.CreateInstance(method.Name);
                method.Invoke(o, (new object[] {new string[]{}}));

                - OR -
                dynamic c = Activator.CreateInstance(Type.GetType("AssemblyNameSpace.Class"););
                c.ClassMethod(@"Hello");
                */
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                    ex = ex.InnerException;
                }
            }
        }
    }
}