// ----------------------------------------------------------------------
// <copyright file="ShadowProjectServiceTest.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SoloX.CodeQuality.Test.Helpers;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Services.Impl;
using Xunit;

namespace SoloX.SlnAggregate.UTest.Services
{
    public class ShadowProjectServiceTest
    {
        [Fact]
        public void It_should_generate_a_shadow_project_file()
        {
            var snapshotName = nameof(this.It_should_generate_a_shadow_project_file);
            var packages = new Dictionary<string, PackageDeclaration>();

            this.GenerateShadowAndAssertSnapshot(packages, snapshotName);
        }

        [Fact]
        public void It_should_generate_a_shadow_project_file_with_package_replacement()
        {
            var snapshotName = nameof(this.It_should_generate_a_shadow_project_file_with_package_replacement);
            var packages = new Dictionary<string, PackageDeclaration>();

            var packageProjectPath = "./Lib1/Lib1.csproj";
            var packageId = "PackageLib1";
            var packageProject = new Project(packageProjectPath);

            packages.Add(packageId, new PackageDeclaration(packageProjectPath, packageId, new Project[] { packageProject }));

            this.GenerateShadowAndAssertSnapshot(packages, snapshotName);
        }

        private void GenerateShadowAndAssertSnapshot(Dictionary<string, PackageDeclaration> packages, string snapshotName)
        {
            var projectPath = "./Lib2/Lib2.csproj";
            var rootPath = "./Resources";

            var expectedShadowPath = "./Lib2/Lib2.Shadow.csproj";
            var expectedShadowFullPath = "./Resources/Lib2/Lib2.Shadow.csproj";

            var project = new Project(projectPath);
            var projects = new Project[] { project };
            var aggregatorMock = new Mock<IAggregator>();
            aggregatorMock.SetupGet(a => a.AllProjects).Returns(projects);
            aggregatorMock.SetupGet(a => a.RootPath).Returns(rootPath);
            aggregatorMock.SetupGet(a => a.PackageDeclarations).Returns(packages);

            var shadowProjectService = new ShadowProjectService();
            var shadowPath = shadowProjectService.GenerateShadow(aggregatorMock.Object, project);

            Assert.NotNull(shadowPath);
            Assert.EndsWith(".Shadow.csproj", shadowPath, StringComparison.InvariantCultureIgnoreCase);
            Assert.Equal(expectedShadowPath, shadowPath);

            var shadowFullPath = Path.Combine(rootPath, shadowPath);
            Assert.Equal(Path.GetFullPath(expectedShadowFullPath), Path.GetFullPath(shadowFullPath));
            Assert.True(File.Exists(shadowFullPath));

            var snapshotLocation = SnapshotHelper.GetLocationFromCallingCodeProjectRoot("Services");

            SnapshotHelper.AssertSnapshot(
                File.ReadAllText(shadowFullPath),
                snapshotName,
                snapshotLocation);
        }
    }
}
