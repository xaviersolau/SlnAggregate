// ----------------------------------------------------------------------
// <copyright file="IAggregator.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate
{
    /// <summary>
    /// Aggregator interface to generate a unique solution file containing all projects
    /// from all sub-repositories.
    /// </summary>
    public interface IAggregator
    {
        /// <summary>
        /// Gets root path where to generate the aggregated solution and where to find the repositories.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// Gets solution repositories loaded from the root folder.
        /// </summary>
        IReadOnlyList<SolutionRepository> SolutionRepositories { get; }

        /// <summary>
        /// Gets all sub-projects.
        /// </summary>
        IReadOnlyList<Project> AllProjects { get; }

        /// <summary>
        /// Gets the sub-package declarations.
        /// </summary>
        IReadOnlyDictionary<string, PackageDeclaration> PackageDeclarations { get; }

        /// <summary>
        /// Setup the Aggregator with the given root folder.
        /// </summary>
        /// <param name="rootPath">The root folder where to find solution assets.</param>
        /// <param name="folders">optional list of sub folders to aggregate, by default all folders will be aggregated.</param>
        void Setup(string rootPath, string[] folders = null);

        /// <summary>
        /// Generate the aggregated solution.
        /// </summary>
        void GenerateSolution();

        /// <summary>
        /// Push the shadow projects.
        /// </summary>
        void PushShadowProjects();
    }
}
