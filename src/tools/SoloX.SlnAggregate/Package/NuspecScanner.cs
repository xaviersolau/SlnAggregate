using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Package
{
    /// <summary>
    /// Nuspec scanner.
    /// </summary>
    public class NuspecScanner : IPackageScanner
    {
        public void Scan(Aggregator aggregator, Dictionary<string, PackageDeclaration> output)
        {
            var nugetFiles = Directory.EnumerateFiles(
                aggregator.RootPath,
                "*.nuspec",
                SearchOption.AllDirectories
            ).ToArray();

            foreach (var nugetFile in nugetFiles)
            {
                var nugetName = Path.GetFileNameWithoutExtension(nugetFile);

                // match with the project name
                var prj = aggregator.AllProjects.Where(p => p.Name == nugetName).FirstOrDefault();
                if (prj != null && !output.ContainsKey(nugetName))
                {
                    output.Add(nugetName, new PackageDeclaration(nugetFile, nugetName, new[] { prj }));
                }
            }
        }
    }
}
