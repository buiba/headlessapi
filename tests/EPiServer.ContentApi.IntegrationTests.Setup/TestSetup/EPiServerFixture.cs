using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using EPiServer.Framework.Initialization;
using EPiServer.Web.Hosting;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class EPiServerFixture : IDisposable
    {
        public EPiServerFixture()
        {
            try
            {
                LoadAllAssembliesInFolder(Environment.CurrentDirectory);
                SetupHostingEnvironment(Environment.CurrentDirectory);
                InitializationModule.FrameworkInitialization(HostType.TestFramework);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                InitializationModule.FrameworkUninitialize();
            }
            catch { }
        }

        /// <summary>
        /// Loads all assemblies in specified folder.
        /// </summary>
        /// <param name="path">The folder path.</param>
        private static void LoadAllAssembliesInFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }

            var loadedAssemblies = new HashSet<string>(new AssemblyList().AllowedAssemblies.Select(a => a.FullName), StringComparer.OrdinalIgnoreCase);
            Parallel.ForEach(Directory.GetFileSystemEntries(path, "*.dll"), (file) =>
            {
                var assemblyName = AssemblyName.GetAssemblyName(file);
                // Skip assembly that is already loaded to current application domain
                if (loadedAssemblies.Contains(assemblyName.FullName))
                {
                    return;
                }

                try
                {
                    Assembly.Load(assemblyName);
                }
                catch (FileLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
            });
        }

        private static void SetupHostingEnvironment(string applicationPath)
        {
            var hostingEnvironment = new TestHostingEnvironment
            {
                ApplicationVirtualPath = "/",
                ApplicationPhysicalPath = applicationPath
            };
            GenericHostingEnvironment.Instance = hostingEnvironment;
            var backupVpp = new TestVPP();

            hostingEnvironment.RegisterVirtualPathProvider(backupVpp);
        }

        private class TestVPP : VirtualPathProvider { }
    }
}
