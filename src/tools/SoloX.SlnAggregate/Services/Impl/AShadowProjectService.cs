// ----------------------------------------------------------------------
// <copyright file="AShadowProjectService.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace SoloX.SlnAggregate.Services.Impl
{
    /// <summary>
    /// AShadowProjectService implementation base.
    /// </summary>
    public class AShadowProjectService
    {
        private const string CsprojExt = ".csproj";
        private const string ShadowCsprojExt = ".Shadow.csproj";

        /// <summary>
        /// Tells if the given path match a shadow project file.
        /// </summary>
        /// <param name="path">The path to match.</param>
        /// <returns>True if the path match a shadow file name.</returns>
        protected static bool IsShadowProjectFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException($"The argument {nameof(path)} must not be null.");
            }

            return path.EndsWith(ShadowCsprojExt, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Convert the given project file to shadow project file.
        /// </summary>
        /// <param name="projectPath">The project file to convert.</param>
        /// <returns>The shadow project file.</returns>
        protected static string ConvertToShadowProjectFilePath(string projectPath)
        {
            if (projectPath == null)
            {
                throw new ArgumentNullException($"The argument {nameof(projectPath)} must not be null.");
            }

            return projectPath.Replace(CsprojExt, ShadowCsprojExt, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Convert the given shadow project file to the original project file.
        /// </summary>
        /// <param name="projectPath">The shadow project file to convert.</param>
        /// <returns>The original project file.</returns>
        protected static string ConvertFromShadowToProjectFilePath(string projectPath)
        {
            if (projectPath == null)
            {
                throw new ArgumentNullException($"The argument {nameof(projectPath)} must not be null.");
            }

            return projectPath.Replace(ShadowCsprojExt, CsprojExt, StringComparison.InvariantCulture);
        }
    }
}
