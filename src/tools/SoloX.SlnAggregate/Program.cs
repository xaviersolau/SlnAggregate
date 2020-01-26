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
        private const string CliArgPath = "path";
        private const string CliArgPush = "push";
        private const string CliArgList = "folders";

        private static readonly IReadOnlyDictionary<string, (string key, string value)> ArgAliases
            = new Dictionary<string, (string, string)>()
            {
                [$"-{CliArgPush}"] = ($"--{CliArgPush}", "true"),
            };

        private readonly ILogger<Program> logger;
        private readonly IConfiguration configuration;
        private readonly IAggregator aggregator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="configuration">The configuration that contains all arguments.</param>
        /// <param name="aggregator">Aggregator to use to build aggregated solution.</param>
        /// <param name="logger">Logger to use for logging.</param>
        public Program(IConfiguration configuration, IAggregator aggregator, ILogger<Program> logger)
        {
            this.configuration = configuration;
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
            builder.AddCommandLine(ConvertAliases(args ?? Array.Empty<string>()).ToArray());
            var configuration = builder.Build();

            using var service = Program.CreateServiceProvider(configuration);

            return service.GetService<Program>().Run();
        }

        /// <summary>
        /// Run the tools command.
        /// </summary>
        /// <returns>Error code.</returns>
        public int Run()
        {
            this.logger.LogInformation($"Processing Sln aggregate.");
            var path = this.configuration.GetValue<string>(CliArgPath);

            var push = this.configuration.GetValue<bool>(CliArgPush, false);

            var folders = this.configuration
                .GetValue(CliArgList, string.Empty)
                .Split(',', ';');

            if (string.IsNullOrEmpty(path))
            {
                this.logger.LogError($"Missing path parameter.");
                return -1;
            }

            this.aggregator.Setup(path, folders.Any() ? folders : null);

            if (push)
            {
                this.aggregator.PushShadowProjects();
            }
            else
            {
                this.aggregator.GenerateSolution();
            }

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

        private static IEnumerable<string> ConvertAliases(string[] args)
        {
            foreach (var arg in args)
            {
                if (ArgAliases.TryGetValue(arg, out var alias))
                {
                    yield return alias.key;
                    yield return alias.value;
                }
                else
                {
                    yield return arg;
                }
            }
        }
    }
}
