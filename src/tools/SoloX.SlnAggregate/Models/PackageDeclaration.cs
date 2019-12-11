// ----------------------------------------------------------------------
// <copyright file="PackageDeclaration.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace SoloX.SlnAggregate.Models
{
    /// <summary>
    /// Package declaration containing the list of projects that are packaged in.
    /// </summary>
    public class PackageDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDeclaration"/> class.
        /// </summary>
        /// <param name="path">Package declaration file path.</param>
        /// <param name="id">Package Id.</param>
        /// <param name="version">Package version.</param>
        /// <param name="projects">Projects included in the package.</param>
        public PackageDeclaration(string path, string id, string version, IEnumerable<Project> projects)
        {
            this.RelativePath = path;
            this.Id = id;
            this.Version = version;
            this.Projects = projects;
        }

        /// <summary>
        /// Gets package Id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets package version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets package declaration file path.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets project included in the package.
        /// </summary>
        public IEnumerable<Project> Projects { get; }
    }
}
