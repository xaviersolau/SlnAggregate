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
            : this(path, Guid.NewGuid())
        {
        }

        public Project(string path, Guid guid)
        {
            RelativePath = path;
            Guid = guid;
            Name = Path.GetFileNameWithoutExtension(RelativePath);
        }

        public string RelativePath { get; }
        public Guid Guid { get; }
        public string Name { get; }
    }
}
