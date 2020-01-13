// ----------------------------------------------------------------------
// <copyright file="ShadowProjectPushService.cs" company="SoloX Software">
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
using SoloX.SlnAggregate.Models;

namespace SoloX.SlnAggregate.Services.Impl
{
    /// <summary>
    /// RevertShadowProjectService class to revert a shadow project file back to the original project.
    /// </summary>
    public class ShadowProjectPushService : AShadowProjectService, IShadowProjectPushService
    {
        /// <inheritdoc/>
        public string PushShadow(IAggregator aggregator, Project csProject)
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

            var shadowPath = ConvertToShadowProjectFilePath(csProject.RelativePath);

            using var shadowStream = File.OpenRead(Path.Combine(path, shadowPath));
            var xmlProj = XDocument.Load(shadowStream);

            // Revert Assembly name.
            RevertAssemblyName(csProject, xmlProj);

            // Revert Root name-space.
            RevertRootNamespace(csProject, xmlProj);

            // Revert Project References to package.
            RevertPackageReferences(csProject, path, aggregator.PackageDeclarations, xmlProj);

            // Revert Shadow Project References.
            RevertShadowProjectReferences(xmlProj);

            using var projectStream = File.Create(Path.Combine(path, csProject.RelativePath));
            using var xmlWriter = XmlWriter.Create(projectStream, new XmlWriterSettings() { Indent = true, });

            xmlProj.WriteTo(xmlWriter);

            xmlWriter.Flush();

            return csProject.RelativePath;
        }

        private static void RevertAssemblyName(Project csProject, XDocument xmlProj)
        {
            var assemblyName = xmlProj.XPathSelectElements("/Project/PropertyGroup/AssemblyName").Single();
            if (assemblyName.Value == csProject.Name)
            {
                assemblyName.Remove();
            }
        }

        private static void RevertRootNamespace(Project csProject, XDocument xmlProj)
        {
            var rootNamespace = xmlProj.XPathSelectElements("/Project/PropertyGroup/RootNamespace").Single();
            if (rootNamespace.Value == csProject.Name)
            {
                rootNamespace.Remove();
            }
        }

        private static void RevertShadowProjectReferences(XDocument xmlProj)
        {
            var projectReferences = xmlProj.XPathSelectElements("/Project/ItemGroup/ProjectReference");
            foreach (var projectReference in projectReferences)
            {
                var includeAttr = projectReference.Attribute(XName.Get("Include"));
                var includeValue = includeAttr.Value;
                includeAttr.Value = ConvertFromShadowToProjectFilePath(includeValue);
            }
        }

        private static void RevertPackageReferences(
            Project csProject,
            string path,
            IReadOnlyDictionary<string, PackageDeclaration> nugets,
            XDocument xmlProj)
        {
            var projectToNugetMap = GetProjectToNugetMap(path, csProject, nugets);

            var packagesToAdd = new HashSet<PackageDeclaration>();

            var projectReferences = xmlProj.XPathSelectElements("/Project/ItemGroup/ProjectReference");
            foreach (var projectReference in projectReferences.ToArray())
            {
                var includeAttr = projectReference.Attribute(XName.Get("Include"));
                var includeValue = includeAttr.Value;

                if (projectToNugetMap.TryGetValue(includeValue, out var nugetSpec))
                {
                    // Remove the project ref with a package ref
                    var parent = projectReference.Parent;
                    projectReference.Remove();
                    if (!parent.HasElements)
                    {
                        parent.Remove();
                    }

                    packagesToAdd.Add(nugetSpec);
                }
            }

            foreach (var package in packagesToAdd)
            {
                AddPackageReference(
                    xmlProj,
                    XDocument.Parse($"<PackageReference Include=\"{package.Id}\" Version=\"{package.Version}\" />").Root);
            }
        }

        private static void AddPackageReference(XDocument xmlProj, XElement packageRefNode)
        {
            var projectReferenceNode = xmlProj.XPathSelectElements("/Project/ItemGroup/PackageReference").FirstOrDefault();
            if (projectReferenceNode == null)
            {
                var projectReferenceItemGroupNode = XDocument.Parse($"<ItemGroup></ItemGroup>").Root;
                projectReferenceItemGroupNode.Add(packageRefNode);

                var lastProjectRefGroup = xmlProj.XPathSelectElements("/Project/ItemGroup/ProjectReference").LastOrDefault();
                if (lastProjectRefGroup != null)
                {
                    lastProjectRefGroup.Parent.AddAfterSelf(projectReferenceItemGroupNode);
                }
                else
                {
                    var lastPropertyGroup = xmlProj.XPathSelectElements("/Project/PropertyGroup").Last();
                    lastPropertyGroup.AddAfterSelf(projectReferenceItemGroupNode);
                }
            }
            else
            {
                var projectReferenceItemGroupNode = projectReferenceNode.Parent;
                projectReferenceItemGroupNode.Add(packageRefNode);
            }
        }

        private static IReadOnlyDictionary<string, PackageDeclaration> GetProjectToNugetMap(
            string path, Project csProject, IReadOnlyDictionary<string, PackageDeclaration> nugets)
        {
            var map = new Dictionary<string, PackageDeclaration>();

            foreach (var nuget in nugets)
            {
                foreach (var nugetSpecProject in nuget.Value.Projects)
                {
                    var prjPath = ConvertToShadowProjectFilePath(nugetSpecProject.RelativePath);

                    prjPath = Path.GetRelativePath(
                        Path.GetDirectoryName(Path.Combine(path, csProject.RelativePath)),
                        Path.Combine(path, prjPath));

                    prjPath = prjPath.Replace("\\", "/", StringComparison.InvariantCulture);

                    map.Add(prjPath, nuget.Value);
                }
            }

            return map;
        }
    }
}
