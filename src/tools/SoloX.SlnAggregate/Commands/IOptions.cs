// ----------------------------------------------------------------------
// <copyright file="IOptions.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace SoloX.SlnAggregate.Commands
{
    /// <summary>
    /// Base interface describing the tool options.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// Gets or sets the path where to find the projects to aggregate.
        /// </summary>
        [Value(
            0,
            MetaName = "root path",
            HelpText = "Root path where to find projects.",
            Required = true)]
        string Path { get; set; }

        /// <summary>
        /// Gets or sets filters.
        /// </summary>
        [Option("filters", Separator = ';', HelpText = "Filters to match the projects to aggregate.")]
        IEnumerable<string> Filters { get; set; }
    }
}
