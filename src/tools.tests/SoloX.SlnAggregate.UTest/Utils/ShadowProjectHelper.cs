// ----------------------------------------------------------------------
// <copyright file="ShadowProjectHelper.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Services.Impl;
using Xunit;

namespace SoloX.SlnAggregate.UTest.Utils
{
    public static class ShadowProjectHelper
    {
        public static IAggregator CreateAggregator(
            string rootPath,
            Project project,
            Dictionary<string, PackageDeclaration> packages)
        {
            var projects = new Project[] { project };
            var aggregatorMock = new Mock<IAggregator>();
            aggregatorMock.SetupGet(a => a.AllProjects).Returns(projects);
            aggregatorMock.SetupGet(a => a.RootPath).Returns(rootPath);
            aggregatorMock.SetupGet(a => a.PackageDeclarations).Returns(packages);

            return aggregatorMock.Object;
        }

        public static string GenerateShadow(string rootPath, string projectPath, Dictionary<string, PackageDeclaration> packages)
        {
            var project = new Project(projectPath);
            var aggregator = CreateAggregator(rootPath, project, packages);

            return GenerateShadow(aggregator, project);
        }

        public static string GenerateShadow(IAggregator aggregator, Project project)
        {
            var rootPath = aggregator.RootPath;
            var projectPath = project.RelativePath;
            var expectedShadowPath = projectPath.Replace(".csproj", ".Shadow.csproj", StringComparison.InvariantCultureIgnoreCase);
            var expectedShadowFullPath = rootPath + expectedShadowPath.Replace("./", "/", StringComparison.InvariantCulture);

            var shadowProjectService = new ShadowProjectService();
            var shadowPath = shadowProjectService.GenerateShadow(aggregator, project);

            Assert.NotNull(shadowPath);
            Assert.EndsWith(".Shadow.csproj", shadowPath, StringComparison.InvariantCultureIgnoreCase);
            Assert.Equal(expectedShadowPath, shadowPath);

            var shadowFullPath = Path.Combine(rootPath, shadowPath);
            Assert.Equal(Path.GetFullPath(expectedShadowFullPath), Path.GetFullPath(shadowFullPath));
            Assert.True(File.Exists(shadowFullPath));

            return shadowFullPath;
        }
    }
}
