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
