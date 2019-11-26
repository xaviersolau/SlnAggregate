// ----------------------------------------------------------------------
// <copyright file="Project.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoloX.SlnAggregate.Models
{
    /// <summary>
    /// Project information.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="path">The project path.</param>
        public Project(string path)
            : this(path, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="path">The project path.</param>
        /// <param name="id">The project ID.</param>
        public Project(string path, Guid id)
        {
            this.RelativePath = path;
            this.Id = id;
            this.Name = Path.GetFileNameWithoutExtension(this.RelativePath);
        }

        /// <summary>
        /// Gets the project path.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the project ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string Name { get; }
    }
}
