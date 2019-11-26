// ----------------------------------------------------------------------
// <copyright file="SolutionRepository.cs" company="SoloX Software">
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
    /// Solution repository where to find the projects to aggregate.
    /// </summary>
    public class SolutionRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionRepository"/> class.
        /// </summary>
        /// <param name="path">Solution repository path.</param>
        /// <param name="projects">Solutions projects.</param>
        public SolutionRepository(string path, IEnumerable<Project> projects)
        {
            this.RelativePath = path;
            this.Id = Guid.NewGuid();
            this.Name = Path.GetFileNameWithoutExtension(this.RelativePath);
            this.Projects = projects;
        }

        /// <summary>
        /// Gets the repository path.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the solution repository ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the solution repository name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the solution repository projects.
        /// </summary>
        public IEnumerable<Project> Projects { get; }
    }
}
