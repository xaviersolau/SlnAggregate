// ----------------------------------------------------------------------
// <copyright file="Program.cs" company="SoloX Software">
// Copyright (c) SoloX Software. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoloX.SlnAggregate.Commands;

namespace SoloX.SlnAggregate
{
    /// <summary>
    /// SlnAggregate tool main entry point class.
    /// </summary>
    public class Program
    {
        private readonly ILogger<Program> logger;
        private readonly IAggregator aggregator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="aggregator">Aggregator to use to build aggregated solution.</param>
        /// <param name="logger">Logger to use for logging.</param>
        public Program(IAggregator aggregator, ILogger<Program> logger)
        {
            this.aggregator = aggregator;
            this.logger = logger;
        }

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args">Tools arguments.</param>
        /// <returns>Error code.</returns>
        public static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection();
            var configuration = builder.Build();

            using var service = Program.CreateServiceProvider(configuration);

            return Parser.Default
                .ParseArguments<AggregateOptions, PushOptions>(args)
                .MapResult(
                    (AggregateOptions opt) => service.GetService<Program>().Run(opt),
                    (PushOptions opt) => service.GetService<Program>().Run(opt),
                    (err) => -1);
        }

        /// <summary>
        /// Run the push command.
        /// </summary>
        /// <param name="opt">Push options.</param>
        /// <returns>Error code.</returns>
        public int Run(PushOptions opt)
        {
            this.logger.LogInformation($"Processing push command.");

            if (opt == null)
            {
                throw new ArgumentNullException(nameof(opt));
            }

            this.Setup(opt);

            this.aggregator.PushShadowProjects();

            return 0;
        }

        /// <summary>
        /// Run the aggregate command.
        /// </summary>
        /// <param name="opt">Aggregate options.</param>
        /// <returns>Error code.</returns>
        public int Run(AggregateOptions opt)
        {
            this.logger.LogInformation($"Processing aggregate command.");

            if (opt == null)
            {
                throw new ArgumentNullException(nameof(opt));
            }

            this.Setup(opt);

            this.aggregator.GenerateSolution();

            return 0;
        }

        private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
        {
            IServiceCollection sc = new ServiceCollection();

            sc.AddLogging(b => b.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Warning));
            sc.AddSingleton(configuration);
            sc.AddSlnAggregate();
            sc.AddTransient<Program>();

            return sc.BuildServiceProvider();
        }

        private void Setup(IOptions opt)
        {
            this.aggregator.Setup(opt.Path, opt.Filters.Any() ? opt.Filters : null);
        }
    }
}
