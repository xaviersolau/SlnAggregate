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
    public class NuspecPackageScannerTest : APackageScannerTest
    {
        [Fact]
        public void It_should_find_the_package_id_from_the_nuspec_file()
        {
            var scanner = new NuspecScanner();

            RunAndAssertScannerTest(scanner, "PackageLib1", "1.2.3", "./Resources", "./Lib1/Lib1.csproj");
        }
    }
}
