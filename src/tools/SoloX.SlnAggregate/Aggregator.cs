// ----------------------------------------------------------------------
// <copyright file="Aggregator.cs" company="SoloX Software">
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
using Newtonsoft.Json;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Package;

namespace SoloX.SlnAggregate
{
    /// <summary>
    /// Aggregator implementation that is generating a unique solution file containing all projects
    /// from all sub-repositories.
    /// </summary>
    public class Aggregator
    {
        private const string CsprojFilePattern = "*.csproj";
        private const string CsprojExt = ".csproj";
        private const string ShadowCsprojExt = ".Shadow.csproj";

        /// <summary>
        /// Gets root path where to generate the aggregated solution and where to find the repositories.
        /// </summary>
        public string RootPath { get; private set; }

        /// <summary>
        /// Gets solution repositories loaded from the root folder.
        /// </summary>
        public IReadOnlyList<SolutionRepository> SolutionRepositories { get; private set; }

        /// <summary>
        /// Gets all sub-projects.
        /// </summary>
        public IReadOnlyList<Project> AllProjects { get; private set; }

        /// <summary>
        /// Gets the sub-package declarations.
        /// </summary>
        public IReadOnlyDictionary<string, PackageDeclaration> PackageDeclarations { get; private set; }

        /// <summary>
        /// Setup the current Aggregator instance with the given root folder.
        /// </summary>
        /// <param name="rootPath">The root folder where to find solution assets.</param>
        public void Setup(string rootPath)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException($"{nameof(rootPath)} must not be null.");
            }

            this.RootPath = rootPath;

            this.SolutionRepositories = this.LoadSolutionRepositories();

            this.AllProjects = this.SolutionRepositories.SelectMany(sr => sr.Projects).ToArray();

            this.PackageDeclarations = this.ScanPackageDeclarations();
        }

        /// <summary>
        /// Generate the aggregated solution.
        /// </summary>
        public void GenerateSolution()
        {
            var solutionFileName = Path.GetFileName(Path.TrimEndingDirectorySeparator(this.RootPath));

            var slnFile = Path.Combine(this.RootPath, $"{solutionFileName}.sln");
            var cacheFile = Path.Combine(this.RootPath, $"{solutionFileName}.guid.cache");

            var guidProjectCache = LoadGuidProjectCache(cacheFile);

            var resPath = Path.GetDirectoryName(typeof(Aggregator).Assembly.Location);

            var projectTmpl = File.ReadAllText(Path.Combine(resPath, "Resources", "PROJECT.tmpl"));
            var folderTmpl = File.ReadAllText(Path.Combine(resPath, "Resources", "FOLDER.tmpl"));
            var configTmpl = File.ReadAllText(Path.Combine(resPath, "Resources", "CONFIG.tmpl"));
            var nestedTmpl = File.ReadAllText(Path.Combine(resPath, "Resources", "NESTED.tmpl"));

            var slnTmpl = File.ReadAllText(Path.Combine(resPath, "Resources", "SLN.tmpl"));

            var projects = new StringBuilder();
            var folders = new StringBuilder();
            var configs = new StringBuilder();
            var nestedList = new StringBuilder();

            foreach (var csFolder in this.SolutionRepositories)
            {
                folders.Append(folderTmpl
                    .Replace("%%FOLDER%%", csFolder.Name, StringComparison.InvariantCulture)
                    .Replace("%%FOLDERGUID%%", csFolder.Id.ToString(), StringComparison.InvariantCulture));

                foreach (var csProject in csFolder.Projects)
                {
                    var shadow = GenerateShadow(csProject, this.RootPath, this.PackageDeclarations, guidProjectCache);

                    projects.Append(projectTmpl
                        .Replace("%%PROJECT%%", shadow.Name, StringComparison.InvariantCulture)
                        .Replace("%%PROJECTPATH%%", shadow.RelativePath, StringComparison.InvariantCulture)
                        .Replace("%%PROJECTGUID%%", shadow.Id.ToString(), StringComparison.InvariantCulture));

                    configs.Append(configTmpl
                        .Replace("%%PROJECTGUID%%", shadow.Id.ToString(), StringComparison.InvariantCulture));

                    nestedList.Append(nestedTmpl
                        .Replace("%%PROJECTGUID%%", shadow.Id.ToString(), StringComparison.InvariantCulture)
                        .Replace("%%FOLDERGUID%%", csFolder.Id.ToString(), StringComparison.InvariantCulture));
                }
            }

            slnTmpl = slnTmpl
                .Replace("##PROJECTS##", projects.ToString(), StringComparison.InvariantCulture)
                .Replace("##FOLDERS##", folders.ToString(), StringComparison.InvariantCulture)
                .Replace("##CONFIGS##", configs.ToString(), StringComparison.InvariantCulture)
                .Replace("##NESTED##", nestedList.ToString(), StringComparison.InvariantCulture);

            slnTmpl = slnTmpl
                .Replace("%%SLNGUID%%", Guid.NewGuid().ToString(), StringComparison.InvariantCulture);

            File.WriteAllText(slnFile, slnTmpl);

            SaveGuidProjectCache(cacheFile, guidProjectCache);
        }

        private static void SaveGuidProjectCache(string cacheFile, IDictionary<string, Guid> guidProjectCache)
        {
            using var fileWriter = File.CreateText(cacheFile);
            var serializer = new JsonSerializer();
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.Serialize(fileWriter, guidProjectCache);
        }

        private static IDictionary<string, Guid> LoadGuidProjectCache(string cacheFile)
        {
            if (File.Exists(cacheFile))
            {
                using var fileReader = File.OpenText(cacheFile);
                var serializer = new JsonSerializer();
                return (IDictionary<string, Guid>)serializer.Deserialize(fileReader, typeof(Dictionary<string, Guid>));
            }

            return new Dictionary<string, Guid>();
        }

        private static Project GenerateShadow(
            Project csProject,
            string path,
            IReadOnlyDictionary<string, PackageDeclaration> nugets,
            IDictionary<string, Guid> guidProjectCache)
        {
            var shadowPath = csProject.RelativePath.Replace(CsprojExt, ShadowCsprojExt, StringComparison.InvariantCulture);

            using var projectFileStream = File.OpenRead(Path.Combine(path, csProject.RelativePath));
            var xmlProj = XDocument.Load(projectFileStream);

            // Update project references
            UpdateProjectReferencesWithShadows(xmlProj);

            // Convert package references
            ConvertPackageReferences(csProject, path, nugets, xmlProj);

            // Setup root name-space
            SetupRootNamespace(csProject, xmlProj);

            // Setup assembly name
            SetupAssemblyName(csProject, xmlProj);

            using var output = File.Create(Path.Combine(path, shadowPath));
            using var xmlWriter = XmlWriter.Create(output, new XmlWriterSettings() { Indent = true, });

            Console.Out.WriteLine($"Writing {shadowPath}");

            xmlProj.WriteTo(xmlWriter);

            xmlWriter.Flush();

            if (!guidProjectCache.TryGetValue(shadowPath, out var guid))
            {
                guid = Guid.NewGuid();

                guidProjectCache.Add(shadowPath, guid);
            }

            return new Project(shadowPath, guid);
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
                    packageReference.Remove();

                    foreach (var nugetSpecProject in nugetSpec.Projects)
                    {
                        var prjPath = nugetSpecProject.RelativePath.Replace(CsprojExt, ShadowCsprojExt, StringComparison.InvariantCulture);

                        prjPath = Path.GetRelativePath(
                            Path.GetDirectoryName(Path.Combine(path, csProject.RelativePath)),
                            Path.Combine(path, prjPath));

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
                includeAttr.Value = includeValue.Replace(CsprojExt, ShadowCsprojExt, StringComparison.InvariantCulture);
            }
        }

        private List<SolutionRepository> LoadSolutionRepositories()
        {
            var slnRepositoryFolders = Directory.EnumerateDirectories(
                this.RootPath,
                "*",
                SearchOption.TopDirectoryOnly).ToArray();

            var slnRepositories = new List<SolutionRepository>();

            foreach (var slnRepositoryFolder in slnRepositoryFolders)
            {
                var projects = new List<Project>();

                var prjFiles = Directory.EnumerateFiles(
                    slnRepositoryFolder,
                    CsprojFilePattern,
                    SearchOption.AllDirectories)
                    .Where(p => !p.EndsWith(ShadowCsprojExt, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                foreach (var prjFile in prjFiles)
                {
                    var relativePath = Path.GetRelativePath(this.RootPath, prjFile);
                    projects.Add(new Project(relativePath));
                }

                if (projects.Any())
                {
                    slnRepositories.Add(
                        new SolutionRepository(
                            slnRepositoryFolder.Replace(
                                this.RootPath,
                                string.Empty,
                                StringComparison.InvariantCulture),
                            projects));
                }
            }

            return slnRepositories;
        }

        private Dictionary<string, PackageDeclaration> ScanPackageDeclarations()
        {
            var nugets = new Dictionary<string, PackageDeclaration>();

            new NuspecScanner().Scan(this, nugets);
            new CsprojScanner().Scan(this, nugets);

            return nugets;
        }
    }
}
