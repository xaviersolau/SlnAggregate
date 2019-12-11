// ----------------------------------------------------------------------
// <copyright file="APackageScannerTest.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Package;
using Xunit;

namespace SoloX.SlnAggregate.UTest.Package
{
    public abstract class APackageScannerTest
    {
        protected static void RunAndAssertScannerTest(
            IPackageScanner scanner,
            string expectedPackageName,
            string expectedPackageVersion,
            string rootPath,
            params string[] projectPaths)
        {
            var scanResult = new Dictionary<string, PackageDeclaration>();

            var projects = projectPaths.Select(p => new Project(p)).ToArray();

            var aggregatorMock = new Mock<IAggregator>();
            aggregatorMock.SetupGet(a => a.AllProjects).Returns(projects);
            aggregatorMock.SetupGet(a => a.RootPath).Returns(rootPath);

            scanner.Scan(aggregatorMock.Object, scanResult);

            Assert.NotEmpty(scanResult);

            Assert.True(scanResult.ContainsKey(expectedPackageName));

            var packageEntry = scanResult[expectedPackageName];

            Assert.Equal(expectedPackageVersion, packageEntry.Version);

            Assert.Equal(projects.Length, packageEntry.Projects.Count());

            foreach (var expectedProject in projects)
            {
                Assert.Contains(expectedProject, packageEntry.Projects);
            }
        }
    }
}
