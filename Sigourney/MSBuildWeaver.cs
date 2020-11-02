using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Serilog;
using Serilog.Sinks.MSBuild;
using ILogger = Serilog.ILogger;

namespace Sigourney
{
    /// <summary>
    /// An abstract MSBuild task that weaves assemblies with Sigourney.
    /// </summary>
    [PublicAPI]
    public abstract class MSBuildWeaver : Task
    {
        private static readonly List<AssemblyReference> _emptyReferenceList = new List<AssemblyReference>();

        private ILogger? _log2;

        private readonly Lazy<WeaverConfig?> _configThunk;

        /// <summary>
        /// Creates an <see cref="MSBuildWeaver"/>.
        /// </summary>
        public MSBuildWeaver()
        {
            // We assume that the Configuration items are
            // not modified by user code; they are a black box.
            _configThunk = new Lazy<WeaverConfig?>(() => WeaverConfig.TryCreate(Configuration));
        }

        /// <summary>
        /// The path of the assembly to weave.
        /// </summary>
        /// <remarks>Required.</remarks>
        [Required]
        public string AssemblyPath { get; set; } = "";

        /// <summary>
        /// A unique name for your weaver.
        /// </summary>
        /// <remarks>
        /// If many weavers use the same weaver name, only one of them will
        /// be executed. Defaults to the name of the assembly containing this task.
        /// </remarks>
        public string? WeaverName { get; set; }

        /// <summary>
        /// The path to save the weaved assembly.
        /// </summary>
        /// <remarks>Defaults to <see cref="AssemblyPath"/>.</remarks>
        public string? OutputPath { get; set; }

        /// <summary>
        /// Unused and ignored as of Sigourney 0.3.0.
        /// Preserved for backwards compatibility.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string? OutputSentinel { get; set; }

        /// <summary>
        /// Additional configuration that enhances Sigourney's functionality.
        /// </summary>
        /// <remarks>
        /// The content of this task parameter is a blackbox and
        /// should be passed as <c>@(SigourneyConfiguration)</c>.
        /// </remarks>
        public ITaskItem[]? Configuration { get; set; }

        /// <summary>
        /// The assembly's references.
        /// </summary>
        public IReadOnlyList<AssemblyReference> AssemblyReferences =>
            _configThunk.Value?.References ?? _emptyReferenceList;

        /// <summary>
        /// A Serilog <see cref="ILogger"/> that redirects events to MSBuild.
        /// </summary>
        /// <remarks>The type was named <c>Log2</c> to distinguish itself from
        /// the less flexible <see cref="Task.Log"/> property.</remarks>
        public ILogger Log2
        {
            get
            {
                if (_log2 != null) return _log2;
                var log = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.MSBuild(this)
                    .CreateLogger()
                    .ForContext(MSBuildProperties.File, AssemblyPath);
                Interlocked.Exchange(ref _log2, log);

                return _log2!;
            }
        }

        /// <summary>
        /// Performs the actual weaving of the assembly.
        /// </summary>
        /// <param name="asm">A Mono.Cecil <see cref="AssemblyDefinition"/>
        /// representation of the assembly to weave.</param>
        /// <returns>Whether the assembly was actually weaved.
        /// If it returns <see langword="false"/> and the output
        /// assembly path is not specified, the input assembly
        /// file will not be overwritten. If the output path is
        /// specified, the input assembly will be copied there as-is.</returns>
        protected abstract bool DoWeave(AssemblyDefinition asm);

        /// <summary>
        /// Checks the task parameters for any errors and weaves the assembly at
        /// <see cref="AssemblyPath"/>. The overriden <see cref="DoWeave"/> might
        /// be called if needed.
        /// </summary>
        /// <returns>
        /// Whether any error has been logged by either
        /// <see cref="Task.Log"/> or <see cref="Log2"/>.
        /// </returns>
        public override bool Execute()
        {
            // .NET Framework-based MSBuild uses the Sigourney.dll at the Sigourney package
            // directory, while dotnet msbuild uses the Sigourney.dll next to the weaver.
            Log.LogMessage(MessageImportance.Low, "Using Sigourney's assembly at {0}",
                typeof(MSBuildWeaver).Assembly.Location);

            var config = _configThunk.Value;
            if (config == null)
                Log.LogWarning("Something went wrong with the weaver's Configuration task parameter. Please set it to @(SigourneyConfiguration). Sigourney's functionality might be limited.");

            Weaver.Weave(AssemblyPath, OutputPath, DoWeave, Log2, config, WeaverName);

            // Log2 internally uses Log to send the logging events.
            // Any error Log2 might send counts towards Log.HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}
