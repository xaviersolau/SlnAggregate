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
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Work\TermDeposits\";

            var aggregator = new Aggregator();

            aggregator.Setup(path);

            aggregator.GenerateSolution();
        }
    }
}
