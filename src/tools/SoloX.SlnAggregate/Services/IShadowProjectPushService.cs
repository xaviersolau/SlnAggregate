// ----------------------------------------------------------------------
// <copyright file="IShadowProjectPushService.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Services
{
    /// <summary>
    /// IShadowProjectPushService interface to push a shadow project file back to the original project.
    /// </summary>
    public interface IShadowProjectPushService
    {
        /// <summary>
        /// Push the shadow project file changes to the original projects.
        /// </summary>
        /// <param name="aggregator">The aggregator containing the project.</param>
        /// <param name="csProject">The shadow project used as input to revert to the original project file.</param>
        /// <returns>The reverted project file path relative to the aggregator root path.</returns>
        string PushShadow(IAggregator aggregator, Project csProject);
    }
}
