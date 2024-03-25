# Using WinGet APIs from C# .NET
Minimal example on how to use WinGet COM APIs from C#.NET. 

The code on the project named `WindowsPackageManager Interop` was extracted and modified from [Dev Home](https://github.com/microsoft/devhome/).

Usage instructions:
1. Add the `WindowsPackageManager Interop.csproj` to your solution
2. Import the `WindowsPackageManager.Interop` Namespace.
3. Create a WinGet Package Manager Factory using the `WindowsPackageManagerStandardFactory` class.
4. Operate WinGet as stated on the official documentation: https://github.com/microsoft/winget-cli/blob/master/doc/specs/%23888%20-%20Com%20Api.md
> [!WARNING]  
> The `CreateAppInstaller()` WinRT function does not exist. `CreatePackageManager()` must be used instead.

> [!CAUTION]  
> If an elevated process initializes WinGet with the `WindowsPackageManagerStandardFactory()` class, the method `CreatePackageManager()` will crash. The class `WindowsPackageManagerElevatedFactory()` must be used instead


# Usage example
The following example can be found on the `Demo Console App` project on this same repository.
The code prompts the user for a query and prints the found WinGet packages, filtering them by name.
```cs
using System.Security.Principal;

// Include WinGet Namespace
using WindowsPackageManager.Interop;

namespace WingetTest
{
    internal class Program
    {
        // Read a query and call the FindPackagesForQuery() method
        static public void Main(string[] args)
        {
            while(true)
            {
                Console.Write("Enter search query: ");
                string? Query = Console.ReadLine();
                if(Query == null || Query == "")
                    break;

                FindPackagesForQuery(Query).Wait();
            }
        }

        private static async Task FindPackagesForQuery(string Query)
        {   
            WindowsPackageManagerFactory WinGetFactory;
            bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

            // If the user is an administrator, use the elevated factory. Otherwhise COM will crash
            if(IsAdministrator)
                WinGetFactory = new WindowsPackageManagerElevatedFactory();
            else
                WinGetFactory = new WindowsPackageManagerStandardFactory();

            // Create Package Manager and get available catalogs
            var Manager = WinGetFactory.CreatePackageManager();
            var AvailableCatalogs = Manager.GetPackageCatalogs();
                        
            foreach (var Catalog in AvailableCatalogs.ToArray())
            {
                // Create a filter to search for packages
                var FilterList = WinGetFactory.CreateFindPackagesOptions();

                // Add the query to the filter
                var NameFilter = WinGetFactory.CreatePackageMatchFilter();
                NameFilter.Field = Microsoft.Management.Deployment.PackageMatchField.Name;
                NameFilter.Value = Query;
                FilterList.Filters.Add(NameFilter);

                // Find the packages with the filters
                var SearchResults = await Catalog.Connect().PackageCatalog.FindPackagesAsync(FilterList);
                foreach (var Match in SearchResults.Matches.ToArray())
                {
                    // Print the packages
                    var Package = Match.CatalogPackage;
                    Console.WriteLine(Package.Name);
                }
            }
        }
    }
}

```
