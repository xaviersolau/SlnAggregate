using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Package
{
    public class CsprojScanner : IPackageScanner
    {
        public void Scan(Aggregator aggregator, Dictionary<string, PackageDeclaration> output)
        {
            foreach (var project in aggregator.AllProjects)
            {
                using var projectFileStream = File.OpenRead(Path.Combine(aggregator.RootPath, project.RelativePath));
                var xmlProj = XDocument.Load(projectFileStream);

                var prjName = project.Name;

                var packageId = xmlProj.XPathSelectElements("/Project/PropertyGroup/PackageId").SingleOrDefault();

                if (packageId != null)
                {
                    prjName = packageId.Value;
                }

                if (!output.ContainsKey(prjName))
                {
                    output.Add(prjName, new PackageDeclaration(project.RelativePath, prjName, new[] { project }));
                }
            }
        }
    }
}
