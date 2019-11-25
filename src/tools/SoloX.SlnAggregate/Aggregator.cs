using SoloX.SlnAggregate.Models;
using SoloX.SlnAggregate.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SoloX.SlnAggregate
{
    public class Aggregator
    {
        private const string CsprojFilePattern = "*.csproj";
        private const string CsprojExt = ".csproj";
        private const string ShadowCsprojExt = ".Shadow.csproj";

        public string RootPath { get; private set; }

        public IReadOnlyList<SolutionRepository> SolutionRepositories { get; private set; }

        public IReadOnlyList<Project> AllProjects { get; private set; }

        public IReadOnlyDictionary<string, PackageDeclaration> PackageDeclarations { get; private set; }

        public void Setup(string rootPath)
        {
            this.RootPath = rootPath;

            this.SolutionRepositories = this.LoadSolutionRepositories();

            this.AllProjects = this.SolutionRepositories.SelectMany(sr => sr.Projects).ToArray();

            this.PackageDeclarations = this.ScanPackageDeclarations();
        }

        private List<SolutionRepository> LoadSolutionRepositories()
        {
            var slnRepositoryFolders = Directory.EnumerateDirectories(
                RootPath,
                "*",
                SearchOption.TopDirectoryOnly
            ).ToArray();

            var slnRepositories = new List<SolutionRepository>();

            foreach (var slnRepositoryFolder in slnRepositoryFolders)
            {
                var projects = new List<Project>();

                var prjFiles = Directory.EnumerateFiles(
                    slnRepositoryFolder,
                    CsprojFilePattern,
                    SearchOption.AllDirectories
                )
                .Where(p => !p.EndsWith(ShadowCsprojExt, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();


                foreach (var prjFile in prjFiles)
                {
                    var relativePath = Path.GetRelativePath(RootPath, prjFile);
                    projects.Add(new Project(relativePath));
                }

                if (projects.Any())
                {
                    slnRepositories.Add(new SolutionRepository(slnRepositoryFolder.Replace(RootPath, string.Empty), projects));
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

        public void GenerateSolution()
        {
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
                    .Replace("%%FOLDER%%", csFolder.Name)
                    .Replace("%%FOLDERGUID%%", csFolder.Guid.ToString())
                );

                foreach (var csProject in csFolder.Projects)
                {
                    var shadow = GenerateShadow(csProject, RootPath, this.PackageDeclarations);

                    projects.Append(projectTmpl
                        .Replace("%%PROJECT%%", shadow.Name)
                        .Replace("%%PROJECTPATH%%", shadow.RelativePath)
                        .Replace("%%PROJECTGUID%%", shadow.Guid.ToString())
                    );
                    configs.Append(configTmpl
                        .Replace("%%PROJECTGUID%%", shadow.Guid.ToString())
                    );
                    nestedList.Append(nestedTmpl
                        .Replace("%%PROJECTGUID%%", shadow.Guid.ToString())
                        .Replace("%%FOLDERGUID%%", csFolder.Guid.ToString())
                    );
                }
            }

            slnTmpl = slnTmpl
                .Replace("##PROJECTS##", projects.ToString())
                .Replace("##FOLDERS##", folders.ToString())
                .Replace("##CONFIGS##", configs.ToString())
                .Replace("##NESTED##", nestedList.ToString());

            slnTmpl = slnTmpl
                .Replace("%%SLNGUID%%", Guid.NewGuid().ToString());

            var solutionFileName = Path.GetFileName(Path.TrimEndingDirectorySeparator(RootPath));
            File.WriteAllText(Path.Combine(RootPath, $"{solutionFileName}.sln"), slnTmpl);
        }

        private static Project GenerateShadow(
            Project csProject,
            string path,
            IReadOnlyDictionary<string, PackageDeclaration> nugets)
        {
            var shadowPath = csProject.RelativePath.Replace(CsprojExt, ShadowCsprojExt);

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
            return new Project(shadowPath);
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
                        var prjPath = nugetSpecProject.RelativePath.Replace(CsprojExt, ShadowCsprojExt);

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
                includeAttr.Value = includeValue.Replace(CsprojExt, ShadowCsprojExt);
            }
        }
    }
}
