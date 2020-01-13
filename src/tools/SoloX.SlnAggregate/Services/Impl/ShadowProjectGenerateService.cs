// ----------------------------------------------------------------------
// <copyright file="ShadowProjectGenerateService.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SoloX.SlnAggregate.Impl;
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Services.Impl
{
    /// <summary>
    /// ShadowProjectService implementation that is responsible to generate a shadow project file.
    /// </summary>
    public class ShadowProjectGenerateService : AShadowProjectService, IShadowProjectGenerateService
    {
        /// <inheritdoc/>
        public string GenerateShadow(IAggregator aggregator, Project csProject)
        {
            if (csProject == null)
            {
                throw new ArgumentNullException($"The argument {nameof(csProject)} must not be null.");
            }

            if (aggregator == null)
            {
                throw new ArgumentNullException($"The argument {nameof(aggregator)} must not be null.");
            }

            var path = aggregator.RootPath;

            using var projectStream = File.OpenRead(Path.Combine(path, csProject.RelativePath));
            var xmlProj = XDocument.Load(projectStream);

            // Update project references
            UpdateProjectReferencesWithShadows(xmlProj);

            // Convert package references
            ConvertPackageReferences(csProject, path, aggregator.PackageDeclarations, xmlProj);

            // Setup root name-space
            SetupRootNamespace(csProject, xmlProj);

            // Setup assembly name
            SetupAssemblyName(csProject, xmlProj);

            var shadowPath = ConvertToShadowProjectFilePath(csProject.RelativePath);
            using var shadowStream = File.Create(Path.Combine(path, shadowPath));

            using var xmlWriter = XmlWriter.Create(shadowStream, new XmlWriterSettings() { Indent = true, });

            xmlProj.WriteTo(xmlWriter);

            xmlWriter.Flush();

            return shadowPath;
        }

        /// <inheritdoc/>
        bool IShadowProjectGenerateService.IsShadowProjectFilePath(string path)
        {
            return IsShadowProjectFilePath(path);
        }

        private static void SetupAssemblyName(Project csProject, XDocument xmlProj)
        {
            var assemblyName = xmlProj.XPathSelectElements("/Project/PropertyGroup/AssemblyName").SingleOrDefault();
            if (assemblyName == null)
            {
                var pGrp = xmlProj.XPathSelectElements("/Project/PropertyGroup").First();
                pGrp.Add(XDocument.Parse($"<AssemblyName>{csProject.Name}</AssemblyName>").Root);
            }
        }

        private static void SetupRootNamespace(Project csProject, XDocument xmlProj)
        {
            var rootNamespace = xmlProj.XPathSelectElements("/Project/PropertyGroup/RootNamespace").SingleOrDefault();

            if (rootNamespace == null)
            {
                var pGrp = xmlProj.XPathSelectElements("/Project/PropertyGroup").First();
                pGrp.Add(XDocument.Parse($"<RootNamespace>{csProject.Name}</RootNamespace>").Root);
            }
        }

        private static void ConvertPackageReferences(
            Project csProject,
            string path,
            IReadOnlyDictionary<string, PackageDeclaration> nugets,
            XDocument xmlProj)
        {
            var packageReferences = xmlProj.XPathSelectElements("/Project/ItemGroup/PackageReference");
            foreach (var packageReference in packageReferences.ToArray())
            {
                var includeAttr = packageReference.Attribute(XName.Get("Include"));
                var includeValue = includeAttr.Value;

                if (nugets.TryGetValue(includeValue, out var nugetSpec))
                {
                    // Replace the package ref with a project ref
                    var parent = packageReference.Parent;
                    packageReference.Remove();
                    if (!parent.HasElements)
                    {
                        parent.Remove();
                    }

                    foreach (var nugetSpecProject in nugetSpec.Projects)
                    {
                        var prjPath = ConvertToShadowProjectFilePath(nugetSpecProject.RelativePath);

                        prjPath = Path.GetRelativePath(
                            Path.GetDirectoryName(Path.Combine(path, csProject.RelativePath)),
                            Path.Combine(path, prjPath));

                        prjPath = prjPath.Replace("\\", "/", StringComparison.InvariantCulture);

                        AddProjectReference(xmlProj, XDocument.Parse($"<ProjectReference Include=\"{prjPath}\" />").Root);
                    }
                }
            }
        }

        private static void AddProjectReference(XDocument xmlProj, XElement projectRefNode)
        {
            var projectReferenceNode = xmlProj.XPathSelectElements("/Project/ItemGroup/ProjectReference").FirstOrDefault();
            if (projectReferenceNode == null)
            {
                var projectReferenceItemGroupNode = XDocument.Parse($"<ItemGroup></ItemGroup>").Root;
                projectReferenceItemGroupNode.Add(projectRefNode);
                var lastPropertyGroup = xmlProj.XPathSelectElements("/Project/PropertyGroup").Last();
                lastPropertyGroup.AddAfterSelf(projectReferenceItemGroupNode);
            }
            else
            {
                var projectReferenceItemGroupNode = projectReferenceNode.Parent;
                projectReferenceItemGroupNode.Add(projectRefNode);
            }
        }

        private static void UpdateProjectReferencesWithShadows(XDocument xmlProj)
        {
            var projectReferences = xmlProj.XPathSelectElements("/Project/ItemGroup/ProjectReference");
            foreach (var projectReference in projectReferences)
            {
                var includeAttr = projectReference.Attribute(XName.Get("Include"));
                var includeValue = includeAttr.Value;
                includeAttr.Value = ConvertToShadowProjectFilePath(includeValue);
            }
        }
    }
}
