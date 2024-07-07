using Microsoft.Management.Deployment;
using System.Security.Principal;
using Windows.ApplicationModel;


// Include WinGet Namespace
using WindowsPackageManager.Interop;

namespace WingetTest
{
    internal class Program
    {
        static public void Main(string[] args)
        {
            // var WinGetFactory = new WindowsPackageManagerElevatedFactory();
            var WinGetFactory = new WindowsPackageManagerStandardFactory();
            var WinGetManager = WinGetFactory.CreatePackageManager();

            // CHANGE THIS INDEX
            int selectedIndex = 0;
            
            PackageCatalogReference installedSearchCatalogRef;
            if (selectedIndex < 0)
            {
                installedSearchCatalogRef = WinGetManager.GetLocalPackageCatalog(LocalPackageCatalog.InstalledPackages);
            }
            else
            {
                PackageCatalogReference selectedRemoteCatalogRef = WinGetManager.GetPackageCatalogs().ToArray().ElementAt(selectedIndex);
                
                Console.WriteLine($"Searching on package catalog {selectedRemoteCatalogRef.Info.Name} ");
                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetFactory.CreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.Add(selectedRemoteCatalogRef);
                createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;
                installedSearchCatalogRef = WinGetManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
            }
            
            var ConnectResult = installedSearchCatalogRef.Connect();
            if (ConnectResult.Status != ConnectResultStatus.Ok)
            {
                throw new Exception("WinGet: Failed to connect to local catalog.");
            }

            FindPackagesOptions findPackagesOptions = WinGetFactory.CreateFindPackagesOptions();
            PackageMatchFilter filter = WinGetFactory.CreatePackageMatchFilter();
            filter.Field = PackageMatchField.Id;
            filter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
            filter.Value = "";
            findPackagesOptions.Filters.Add(filter);

            var TaskResult = ConnectResult.PackageCatalog.FindPackages(findPackagesOptions);

            Console.WriteLine("Begin enumeration");
            foreach (var match in TaskResult.Matches.ToArray())
            {
                if (match.CatalogPackage.DefaultInstallVersion != null)
                    Console.WriteLine($"Package {match.CatalogPackage.Name} is available Online: " + match.CatalogPackage.DefaultInstallVersion.PackageCatalog.Info.Name);
                //else
                    //Console.WriteLine("Package is local only: " + match.CatalogPackage.Id);
            }
            Console.WriteLine("End enumeration");
        }
    }
}
