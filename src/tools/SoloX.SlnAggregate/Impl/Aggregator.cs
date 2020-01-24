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
using Newtonsoft.Json;
using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Package;
using SoloX.SlnAggregate.Services;

namespace SoloX.SlnAggregate.Impl
{
    /// <summary>
    /// Aggregator implementation that is generating a unique solution file containing all projects
    /// from all sub-repositories.
    /// </summary>
    public class Aggregator : IAggregator
    {
        private const string CsprojFilePattern = "*.csproj";

        private readonly IEnumerable<IPackageScanner> packageScanners;
        private readonly IShadowProjectGenerateService shadowProjectGenerateService;
        private readonly IShadowProjectPushService shadowProjectPushService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregator"/> class.
        /// </summary>
        /// <param name="packageScanners">The package scanner to use to detect packages.</param>
        /// <param name="shadowProjectGenerateService">The shadow project generate service to handle the project
        /// file generation.</param>
        /// <param name="shadowProjectPushService">The shadow project push service to handle the project changes.</param>
        public Aggregator(
            IEnumerable<IPackageScanner> packageScanners,
            IShadowProjectGenerateService shadowProjectGenerateService,
            IShadowProjectPushService shadowProjectPushService)
        {
            this.packageScanners = packageScanners;
            this.shadowProjectGenerateService = shadowProjectGenerateService;
            this.shadowProjectPushService = shadowProjectPushService;
        }

        /// <inheritdoc/>
        public string RootPath { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<SolutionRepository> SolutionRepositories { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<Project> AllProjects { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, PackageDeclaration> PackageDeclarations { get; private set; }

        /// <inheritdoc/>
        public void Setup(string rootPath, string[] folders = null)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException($"{nameof(rootPath)} must not be null.");
            }

            this.RootPath = rootPath;

            this.SolutionRepositories = this.LoadSolutionRepositories(folders);

            this.AllProjects = this.SolutionRepositories.SelectMany(sr => sr.Projects).ToArray();

            this.PackageDeclarations = this.ScanPackageDeclarations();
        }

        /// <inheritdoc/>
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
                    var shadow = this.GenerateShadow(csProject, guidProjectCache);

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

        /// <inheritdoc/>
        public void PushShadowProjects()
        {
            foreach (var csFolder in this.SolutionRepositories)
            {
                foreach (var csProject in csFolder.Projects)
                {
                    this.shadowProjectPushService.PushShadow(this, csProject);
                }
            }
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

        private Project GenerateShadow(
            Project csProject,
            IDictionary<string, Guid> guidProjectCache)
        {
            var shadowPath = this.shadowProjectGenerateService.GenerateShadow(this, csProject);

            if (!guidProjectCache.TryGetValue(shadowPath, out var guid))
            {
                guid = Guid.NewGuid();

                guidProjectCache.Add(shadowPath, guid);
            }

            return new Project(shadowPath, guid);
        }

        private List<SolutionRepository> LoadSolutionRepositories(string[] folders)
        {
            var slnRepositoryFolders = Directory.EnumerateDirectories(
                this.RootPath,
                "*",
                SearchOption.TopDirectoryOnly).ToArray();

            var resolvedFilteredFolders = folders?
                .Select(f => Path.Combine(this.RootPath, f))
                .ToArray();

            var slnRepositories = new List<SolutionRepository>();

            foreach (var slnRepositoryFolder in slnRepositoryFolders)
            {
                if (resolvedFilteredFolders != null && !resolvedFilteredFolders.Contains(slnRepositoryFolder))
                {
                    continue;
                }

                var projects = new List<Project>();

                var prjFiles = Directory.EnumerateFiles(
                    slnRepositoryFolder,
                    CsprojFilePattern,
                    SearchOption.AllDirectories)
                    .Where(p => !this.shadowProjectGenerateService.IsShadowProjectFilePath(p))
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

            foreach (var scanner in this.packageScanners)
            {
                scanner.Scan(this, nugets);
            }

            return nugets;
        }
    }
}
