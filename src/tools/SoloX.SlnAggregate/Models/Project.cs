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
        public Project(string path)
        {
            RelativePath = path;
            Guid = Guid.NewGuid();
            Name = Path.GetFileNameWithoutExtension(RelativePath);
        }

        public string RelativePath { get; }
        public Guid Guid { get; }
        public string Name { get; }
    }
}
