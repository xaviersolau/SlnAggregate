// ----------------------------------------------------------------------
// <copyright file="AggregatorTest.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SoloX.SlnAggregate.Impl;
using SoloX.SlnAggregate.Package;
using SoloX.SlnAggregate.Package.Impl;
using Xunit;

namespace SoloX.SlnAggregate.ITest
{
    public class AggregatorTest
    {
        [Fact]
        public void It_should_generate_the_aggregated_sln_file_and_the_shadow_projects()
        {
            TestWithAggregator(aggregator =>
            {
                aggregator.Setup(@"Resources/RootSln1");

                aggregator.GenerateSolution();
            });

            Assert.True(File.Exists(@"Resources/RootSln1/RootSln1.sln"));
            Assert.True(File.Exists(@"Resources/RootSln1/RootSln1.guid.cache"));
            Assert.True(File.Exists(@"Resources/RootSln1/SlnLib1/Lib1/Lib1.Shadow.csproj"));
            Assert.True(File.Exists(@"Resources/RootSln1/SlnLib2/Lib2/Lib2.Shadow.csproj"));
        }

        [Fact]
        public void It_should_detect_the_package_declarations()
        {
            TestWithAggregator(aggregator =>
            {
                aggregator.Setup(@"Resources/RootSln1");

                Assert.Equal(2, aggregator.PackageDeclarations.Count);

                Assert.Single(aggregator.PackageDeclarations.Values.Where(pkg => pkg.Id == "PackageLib1"));
                Assert.Single(aggregator.PackageDeclarations.Values.Where(pkg => pkg.Id == "Lib2"));
            });
        }

        [Fact]
        public void It_should_setup_the_shadow_projects_with_root_namespace_and_assembly_name()
        {
            TestWithAggregator(aggregator =>
            {
                aggregator.Setup(@"Resources/RootSln1");

                aggregator.GenerateSolution();
            });

            Assert.True(File.Exists(@"Resources/RootSln1/SlnLib1/Lib1/Lib1.Shadow.csproj"));

            var shadowText = File.ReadAllText(@"Resources/RootSln1/SlnLib1/Lib1/Lib1.Shadow.csproj");
            Assert.Contains(
                "<RootNamespace>Lib1</RootNamespace>",
                shadowText,
                StringComparison.InvariantCulture);
            Assert.Contains(
                "<AssemblyName>Lib1</AssemblyName>",
                shadowText,
                StringComparison.InvariantCulture);
        }

        [Fact]
        public void It_should_replace_the_package_ref_by_project_ref_when_posible()
        {
            TestWithAggregator(aggregator =>
            {
                aggregator.Setup(@"Resources/RootSln1");

                aggregator.GenerateSolution();
            });

            Assert.True(File.Exists(@"Resources/RootSln1/SlnLib2/Lib2/Lib2.Shadow.csproj"));

            var shadowText = File.ReadAllText(@"Resources/RootSln1/SlnLib2/Lib2/Lib2.Shadow.csproj");
            Assert.Contains(
                @$"<ProjectReference Include=""../../SlnLib1/Lib1/Lib1.Shadow.csproj"" />",
                shadowText,
                StringComparison.InvariantCulture);
            Assert.Contains(
                @"<PackageReference Include=""Another.Package"" Version=""1.2.3"" />",
                shadowText,
                StringComparison.InvariantCulture);
        }

        private static void TestWithAggregator(Action<IAggregator> test)
        {
            IServiceCollection sc = new ServiceCollection();
            sc.AddSlnAggregate();
            using (var provider = sc.BuildServiceProvider())
            {
                var aggregator = provider.GetService<IAggregator>();

                test(aggregator);
            }
        }
    }
}
