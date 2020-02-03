// ----------------------------------------------------------------------
// <copyright file="AggregateOptions.cs" company="SoloX Software">
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
    /// Options for the Aggregate command.
    /// </summary>
    [Verb("aggregate", HelpText = "Aggregate the projects into one shadow solution.")]
    public class AggregateOptions : IOptions
    {
        /// <inheritdoc/>
        public string Path { get; set; }

        /// <inheritdoc/>
        public IEnumerable<string> Filters { get; set; }
    }
}
