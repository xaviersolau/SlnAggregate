// ----------------------------------------------------------------------
// <copyright file="PushOptions.cs" company="SoloX Software">
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
    /// Options for the Push command.
    /// </summary>
    [Verb("push", HelpText = "Push the modification done in the shadow projects to the original projects.")]
    public class PushOptions : IOptions
    {
        /// <inheritdoc/>
        public string Path { get; set; }

        /// <inheritdoc/>
        public IEnumerable<string> Filters { get; set; }
    }
}
