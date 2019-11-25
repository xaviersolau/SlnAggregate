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
        public PackageDeclaration(string path, string id, IEnumerable<Project> projects)
        {
            RelativePath = path;
            Id = id;
            Projects = projects;
        }

        public string Id { get; }

        public string RelativePath { get; }

        public IEnumerable<Project> Projects { get; }
    }
}
