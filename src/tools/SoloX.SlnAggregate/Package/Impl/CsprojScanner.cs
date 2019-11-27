// ----------------------------------------------------------------------
// <copyright file="CsprojScanner.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Package.Impl
{
    /// <summary>
    /// Csproj file package scanner.
    /// </summary>
    public class CsprojScanner : IPackageScanner
    {
        /// <inheritdoc/>
        public void Scan(IAggregator aggregator, Dictionary<string, PackageDeclaration> output)
        {
            if (aggregator == null || aggregator.AllProjects == null)
            {
                throw new ArgumentNullException($"{nameof(aggregator)} or one of its property must not be null.");
            }

            if (output == null)
            {
                throw new ArgumentNullException($"{nameof(output)} must not be null.");
            }

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
