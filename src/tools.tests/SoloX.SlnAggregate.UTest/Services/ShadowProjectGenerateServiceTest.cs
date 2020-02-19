// ----------------------------------------------------------------------
// <copyright file="ShadowProjectGenerateServiceTest.cs" company="SoloX Software">
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
using SoloX.SlnAggregate.UTest.Utils;
using Xunit;

namespace SoloX.SlnAggregate.UTest.Services
{
    public class ShadowProjectGenerateServiceTest
    {
        [Fact]
        public void It_should_generate_a_shadow_project_file()
        {
            var projectPath = "./Lib2/Lib2.csproj";
            var rootPath = "./Resources/Project1";

            var snapshotName = nameof(this.It_should_generate_a_shadow_project_file);
            var packages = new Dictionary<string, PackageDeclaration>();

            this.GenerateShadowAndAssertSnapshot(rootPath, projectPath, packages, snapshotName);
        }

        [Fact]
        public void It_should_generate_a_shadow_project_file_with_package_replacement()
        {
            var projectPath = "./Lib2/Lib2.csproj";
            var rootPath = "./Resources/Project1";

            var snapshotName = nameof(this.It_should_generate_a_shadow_project_file_with_package_replacement);
            var packages = new Dictionary<string, PackageDeclaration>();

            var packageProjectPath = "./Lib1/Lib1.csproj";
            var packageId = "PackageLib1";
            var packageProject = new Project(packageProjectPath);

            packages.Add(packageId, new PackageDeclaration(packageProjectPath, packageId, "1.0.0", new Project[] { packageProject }));

            this.GenerateShadowAndAssertSnapshot(rootPath, projectPath, packages, snapshotName);
        }

        [Fact]
        public void It_should_generate_a_shadow_project_file_with_multi_projects_package_replacement()
        {
            var projectPath = "./Lib3/Lib3.csproj";
            var rootPath = "./Resources/Project3";

            var snapshotName = nameof(this.It_should_generate_a_shadow_project_file_with_multi_projects_package_replacement);
            var packages = new Dictionary<string, PackageDeclaration>();

            var packageId = "PackageLibs";
            var packageProjectPath1 = "./Libs/Lib1/Lib1.csproj";
            var packageProjectPath2 = "./Libs/Lib2/Lib2.csproj";
            var packageProject1 = new Project(packageProjectPath1);
            var packageProject2 = new Project(packageProjectPath2);

            packages.Add(
                packageId,
                new PackageDeclaration(
                    "./Libs/Libs.nuspec",
                    packageId,
                    "1.0.0",
                    new Project[] { packageProject1, packageProject2 }));

            this.GenerateShadowAndAssertSnapshot(rootPath, projectPath, packages, snapshotName);
        }

        private void GenerateShadowAndAssertSnapshot(string rootPath, string projectPath, Dictionary<string, PackageDeclaration> packages, string snapshotName)
        {
            var shadowFullPath = ShadowProjectHelper.GenerateShadow(rootPath, projectPath, packages);

            var snapshotLocation = SnapshotHelper.GetLocationFromCallingCodeProjectRoot("Services");

            SnapshotHelper.AssertSnapshot(
                File.ReadAllText(shadowFullPath),
                snapshotName,
                snapshotLocation);
        }
    }
}
