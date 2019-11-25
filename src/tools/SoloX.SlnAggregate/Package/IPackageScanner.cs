using SoloX.SlnAggregate.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoloX.SlnAggregate.Package
{
    public interface IPackageScanner
    {
        /// <summary>
        /// Scan from aggregator input to fill-in the package declaration list.
        /// </summary>
        /// <param name="aggregator">Aggregator used as input.</param>
        /// <param name="output">the package declaration list to fill-in.</param>
        public void Scan(Aggregator aggregator, Dictionary<string, PackageDeclaration> output);
    }
}
