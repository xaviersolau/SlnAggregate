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
        public static string GenerateShadow(string rootPath, string projectPath, Dictionary<string, PackageDeclaration> packages)
        {
            var expectedShadowPath = projectPath.Replace(".csproj", ".Shadow.csproj", StringComparison.InvariantCultureIgnoreCase);
            var expectedShadowFullPath = rootPath + expectedShadowPath.Replace("./", "/", StringComparison.InvariantCulture);

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

            return shadowFullPath;
        }
    }
}
