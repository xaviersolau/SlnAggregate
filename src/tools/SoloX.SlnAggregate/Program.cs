// ----------------------------------------------------------------------
// <copyright file="Program.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoloX.SlnAggregate
{
    /// <summary>
    /// SlnAggregate tool main entry point class.
    /// </summary>
    public class Program
    {
        private readonly ILogger<Program> logger;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="configuration">The configuration that contains all arguments.</param>
        public Program(IConfiguration configuration)
        {
            this.configuration = configuration;

            IServiceCollection sc = new ServiceCollection();

            sc.AddLogging(b => b.AddConsole());
            sc.AddSingleton(configuration);
            sc.AddSlnAggregate();

            this.Service = sc.BuildServiceProvider();

            this.logger = this.Service.GetService<ILogger<Program>>();
        }

        private ServiceProvider Service { get; }

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args">Tools arguments.</param>
        /// <returns>Error code.</returns>
        public static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            return new Program(config).Run();
        }

        /// <summary>
        /// Run the tools command.
        /// </summary>
        /// <returns>Error code.</returns>
        public int Run()
        {
            var path = this.configuration.GetValue<string>("path");

            if (string.IsNullOrEmpty(path))
            {
                this.logger.LogError($"Missing path parameter.");
                return -1;
            }

            var aggregator = this.Service.GetService<IAggregator>();

            aggregator.Setup(path);

            aggregator.GenerateSolution();

            return 0;
        }
    }
}
