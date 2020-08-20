using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NugetTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var curDir = System.IO.Directory.GetCurrentDirectory();
                ISettings settings = Settings.LoadSpecificSettings(curDir, Settings.DefaultSettingsFileName);

                List<Lazy<INuGetResourceProvider>> resourceProviders = new List<Lazy<INuGetResourceProvider>>();
                resourceProviders.AddRange(NuGet.Protocol.Core.Types.Repository.Provider.GetCoreV3());  // Add v3 API support

                IPackageSourceProvider packageSourceProvider = new PackageSourceProvider(settings);

                SourceRepositoryProvider _sourceRepositories = new SourceRepositoryProvider(packageSourceProvider, resourceProviders);

                string projectRoot = System.IO.Path.GetFullPath("ProjectRoot");
                if (!System.IO.Directory.Exists(projectRoot)) System.IO.Directory.CreateDirectory(projectRoot);

                FolderNuGetProject nuGetProject = new FolderNuGetProject(projectRoot);

                string packagesFolderPath = System.IO.Path.GetFullPath("Packages");
                if (!System.IO.Directory.Exists(packagesFolderPath)) System.IO.Directory.CreateDirectory(packagesFolderPath);

                NuGetPackageManager packageManager = new NuGetPackageManager(_sourceRepositories, settings, packagesFolderPath)
                {
                    PackagesFolderNuGetProject = nuGetProject
                };

                bool allowPrereleaseVersions = true;
                bool allowUnlisted = true;
                DependencyBehavior dependencyBehavior = DependencyBehavior.Lowest;

                ResolutionContext resolutionContext = new ResolutionContext(
                         dependencyBehavior, allowPrereleaseVersions, allowUnlisted, VersionConstraints.None);

                INuGetProjectContext projectContext = new ConsoleProjectContext(NullLogger.Instance);

                IEnumerable<SourceRepository> primarySourceRepositories = _sourceRepositories.GetRepositories();

                //packageIdentity
                string packageId = "Newtonsoft.Json";
                string packageVersion = "12.0.3";
                PackageIdentity packageIdentity = new PackageIdentity(packageId, new NuGet.Versioning.NuGetVersion(packageVersion));

                var installPackageAsyncTask = packageManager.InstallPackageAsync(packageManager.PackagesFolderNuGetProject,
                    packageIdentity, resolutionContext, projectContext,
                    primarySourceRepositories, Array.Empty<SourceRepository>(), CancellationToken.None);

                installPackageAsyncTask.GetAwaiter().GetResult();
            }
            catch (Exception exp)
            {
                using (var sw = new System.IO.StreamWriter("error.txt"))
                {
                    sw.Write(exp.ToString());
                }

                Console.WriteLine(exp.ToString());
                throw;
            }
        }
    }
}