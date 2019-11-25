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
        public SolutionRepository(string path, IEnumerable<Project> projects)
        {
            RelativePath = path;
            Guid = Guid.NewGuid();
            Name = Path.GetFileNameWithoutExtension(RelativePath);
            Projects = projects;
        }

        public string RelativePath { get; }
        public Guid Guid { get; }
        public string Name { get; }
        public IEnumerable<Project> Projects { get; }
    }
}
