// ----------------------------------------------------------------------
// <copyright file="IShadowProjectService.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using SoloX.SlnAggregate.Impl;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Services
{
    /// <summary>
    /// IShadowProjectService interface to generate a shadow project file.
    /// </summary>
    public interface IShadowProjectService
    {
        /// <summary>
        /// Generate the shadow project file form a given project.
        /// </summary>
        /// <param name="aggregator">The aggregator containing the project.</param>
        /// <param name="csProject">The project used as input to generate the shadow project file.</param>
        /// <returns>The shadow project file path relative to the aggregator root path.</returns>
        string GenerateShadow(IAggregator aggregator, Project csProject);

        /// <summary>
        /// Tells if the given path match a shadow project file pattern.
        /// </summary>
        /// <param name="path">The path to match.</param>
        /// <returns>True if the path match a shadow project file pattern.</returns>
        bool IsShadowProjectFilePath(string path);
    }
}
