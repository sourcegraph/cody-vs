using Microsoft.VisualStudio.Setup.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    internal static class AssemblyLoader
    {
        private static string[] folders = new string[]
        {
            @"Common7\IDE\WebViewHost",
            @"Common7\ServiceHub\SharedAssemblies",
            @"Common7\IDE\CommonExtensions\Microsoft\VBCSharp\LanguageServices\Core"
        };

        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public static IEnumerable<(string, string)> GetVisualStudioInstallPaths()
        {
            const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);

            var result = new List<(string, string)>();

            try
            {
                var query = new SetupConfiguration() as ISetupConfiguration2;
                var e = query.EnumAllInstances();

                int fetched;
                var instances = new ISetupInstance[1];

                do
                {
                    e.Next(1, instances, out fetched);

                    if (fetched > 0)
                    {
                        var instance2 = (ISetupInstance2)instances[0];
                        result.Add((instance2.GetInstallationVersion(), instance2.GetInstallationPath()));
                    }
                }
                while (fetched > 0);
            }
            catch (COMException ex) when (ex.HResult == REGDB_E_CLASSNOTREG)
            {
            }
            catch (Exception)
            {
            }

            return result;
        }

        private static string SelectInstallPath()
        {
            return GetVisualStudioInstallPaths()
                .First(x => x.Item1.StartsWith("17"))
                .Item2;
        }

        private static string finalPath = null;
        private static string SelectStreamJsonRpcPath()
        {
            if(!string.IsNullOrEmpty(finalPath)) return finalPath;

            var vsPath = SelectInstallPath();
            foreach(var folder in folders)
            {
                var path = Path.Combine(vsPath, folder);
                var libPath = Path.Combine(path, "StreamJsonRpc.dll");
                if (File.Exists(libPath))
                {
                    finalPath = path;
                    return path;
                }
            }

            return null;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var folderPath = SelectStreamJsonRpcPath();
            var assemblyFile = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");

            if (!File.Exists(assemblyFile)) return null;
            return Assembly.LoadFrom(assemblyFile);
        }
    }
}
