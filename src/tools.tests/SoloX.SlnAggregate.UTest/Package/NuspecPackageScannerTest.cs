// ----------------------------------------------------------------------
// <copyright file="NuspecPackageScannerTest.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Package.Impl;
using Xunit;

namespace SoloX.SlnAggregate.UTest.Package
{
    public class NuspecPackageScannerTest
    {
        [Fact]
        public void It_should_find_the_package_id_from_the_nuspec_file()
        {
            var scanner = new NuspecScanner();
            var scanResult = new Dictionary<string, PackageDeclaration>();

            var project = new Project("./Resources/Lib1.nuspec");

            var aggregatorMock = new Mock<IAggregator>();
            aggregatorMock.SetupGet(a => a.AllProjects).Returns(new[] { project });
            aggregatorMock.SetupGet(a => a.RootPath).Returns(".");

            scanner.Scan(aggregatorMock.Object, scanResult);

            Assert.NotEmpty(scanResult);

            Assert.True(scanResult.ContainsKey("PackageLib1"));
        }
    }
}
