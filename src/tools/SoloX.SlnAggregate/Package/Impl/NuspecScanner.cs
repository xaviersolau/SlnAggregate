// ----------------------------------------------------------------------
// <copyright file="NuspecScanner.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Package.Impl
{
    /// <summary>
    /// Nuspec file scanner.
    /// </summary>
    public class NuspecScanner : IPackageScanner
    {
        /// <inheritdoc/>
        public void Scan(IAggregator aggregator, Dictionary<string, PackageDeclaration> output)
        {
            if (aggregator == null || aggregator.RootPath == null)
            {
                throw new ArgumentNullException($"{nameof(aggregator)} or one of its property must not be null.");
            }

            if (output == null)
            {
                throw new ArgumentNullException($"{nameof(output)} must not be null.");
            }

            var nugetFiles = Directory.EnumerateFiles(
                aggregator.RootPath,
                "*.nuspec",
                SearchOption.AllDirectories).ToArray();

            var xmlNsResolver = new NsResolver(
                new Dictionary<string, string>()
                {
                    { "n", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd" },
                });

            foreach (var nugetFile in nugetFiles)
            {
                var nuspecFileName = Path.GetFileNameWithoutExtension(nugetFile);
                var nugetName = nuspecFileName;

                using var nugetFileStream = File.OpenRead(nugetFile);
                using var xmlReader = XmlReader.Create(nugetFileStream);
                var xmlProj = XDocument.Load(xmlReader);

                var packageId = xmlProj.XPathSelectElements(
                    "/n:package/n:metadata/n:id",
                    xmlNsResolver)
                    .SingleOrDefault();

                if (packageId != null)
                {
                    nugetName = packageId.Value;
                }

                // match with the project name
                var prj = aggregator.AllProjects.Where(p => p.Name == nugetName || p.Name == nuspecFileName).FirstOrDefault();
                if (prj != null && !output.ContainsKey(nugetName))
                {
                    var version = xmlProj.XPathSelectElements(
                        "/n:package/n:metadata/n:version",
                        xmlNsResolver)
                        .SingleOrDefault();

                    output.Add(nugetName, new PackageDeclaration(nugetFile, nugetName, version?.Value, new[] { prj }));
                }
            }
        }

        private class NsResolver : IXmlNamespaceResolver
        {
            private IDictionary<string, string> nsMap;

            public NsResolver(IDictionary<string, string> nsMap)
            {
                this.nsMap = nsMap;
            }

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
            {
                return this.nsMap;
            }

            public string LookupNamespace(string prefix)
            {
                return this.nsMap[prefix];
            }

            public string LookupPrefix(string namespaceName)
            {
                return this.nsMap
                    .Where(kv => kv.Value == namespaceName)
                    .Select(kv => kv.Key)
                    .FirstOrDefault();
            }
        }
    }
}
