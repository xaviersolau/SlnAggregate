// ----------------------------------------------------------------------
// <copyright file="IPackageScanner.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Package
{
    /// <summary>
    /// Package scanner interface.
    /// </summary>
    public interface IPackageScanner
    {
        /// <summary>
        /// Scan from aggregator input to fill-in the package declaration list.
        /// </summary>
        /// <param name="aggregator">Aggregator used as input.</param>
        /// <param name="output">the package declaration list to fill-in.</param>
        public void Scan(IAggregator aggregator, Dictionary<string, PackageDeclaration> output);
    }
}
