// ----------------------------------------------------------------------
// <copyright file="ShadowProjectPushServiceTest.cs" company="SoloX Software">
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
    public class ShadowProjectPushServiceTest
    {
        [Fact]
        public void It_should_revert_the_shadow_to_the_original_project_with_package_ref()
        {
            var projectPath = "./Shadow/Lib2.csproj";
            var rootPath = "./Resources";

            var packages = new Dictionary<string, PackageDeclaration>();

            var packageProjectPath = "./Lib1/Lib1.csproj";
            var packageId = "PackageLib1";
            var packageProject = new Project(packageProjectPath);

            packages.Add(packageId, new PackageDeclaration(packageProjectPath, packageId, "3.2.1", new Project[] { packageProject }));

            var project = new Project(projectPath);
            var aggregator = ShadowProjectHelper.CreateAggregator(rootPath, project, packages);

            var revertShadowService = new ShadowProjectPushService();

            var revertedPath = revertShadowService.PushShadow(aggregator, project);

            var fullPath = Path.Combine(rootPath, revertedPath);

            var snapshotLocation = SnapshotHelper.GetLocationFromCallingCodeProjectRoot("Services");

            SnapshotHelper.AssertSnapshot(
                File.ReadAllText(fullPath),
                nameof(this.It_should_revert_the_shadow_to_the_original_project_with_package_ref),
                snapshotLocation);
        }

        [Fact]
        public void It_should_revert_the_shadow_to_the_original_project_with_project_ref()
        {
            var projectPath = "./Shadow/Lib3.csproj";
            var rootPath = "./Resources";

            var packages = new Dictionary<string, PackageDeclaration>();

            var project = new Project(projectPath);
            var aggregator = ShadowProjectHelper.CreateAggregator(rootPath, project, packages);

            var revertShadowService = new ShadowProjectPushService();

            var revertedPath = revertShadowService.PushShadow(aggregator, project);

            var fullPath = Path.Combine(rootPath, revertedPath);

            var snapshotLocation = SnapshotHelper.GetLocationFromCallingCodeProjectRoot("Services");

            SnapshotHelper.AssertSnapshot(
                File.ReadAllText(fullPath),
                nameof(this.It_should_revert_the_shadow_to_the_original_project_with_project_ref),
                snapshotLocation);
        }
    }
}
