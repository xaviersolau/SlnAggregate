// ----------------------------------------------------------------------
// <copyright file="SlnAggregateExtensions.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SoloX.SlnAggregate.Impl;
using SoloX.SlnAggregate.Package;
using SoloX.SlnAggregate.Package.Impl;
using SoloX.SlnAggregate.Services;
using SoloX.SlnAggregate.Services.Impl;

namespace SoloX.SlnAggregate
{
    /// <summary>
    /// Sln extensions to setup the solution aggregate tool.
    /// </summary>
    public static class SlnAggregateExtensions
    {
        /// <summary>
        /// Setup the Service collection in order to inject the Solution aggregate tool implementation.
        /// </summary>
        /// <param name="services">The services collection instance to setup.</param>
        /// <returns>The service collection given as input.</returns>
        public static IServiceCollection AddSlnAggregate(this IServiceCollection services)
        {
            return services
                .AddTransient<IAggregator, Aggregator>()
                .AddTransient<IShadowProjectService, ShadowProjectService>()
                .AddTransient<IPackageScanner, CsprojScanner>()
                .AddTransient<IPackageScanner, NuspecScanner>();
        }
    }
}
