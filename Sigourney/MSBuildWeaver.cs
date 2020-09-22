using System;
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
    /// An MSBuild task that executes a <see cref="Weaver"/>.
    /// </summary>
    [PublicAPI]
    public abstract class MSBuildWeaver : Task
    {
        private ILogger? _log2;
        public bool SignAssembly { get; set; }

        public string? IntermediateDirectory { get; set; }

        public string? KeyOriginatorFile { get; set; }

        public string? AssemblyOriginatorKeyFile { get; set; }

        /// <summary>
        /// The path of the assembly to weave.
        /// </summary>
        /// <remarks>Required.</remarks>
        [Required]
        public string AssemblyPath { get; set; } = "";

        /// <summary>
        /// The path to save the weaved assembly.
        /// </summary>
        /// <remarks>Defaults to <see cref="AssemblyPath"/>
        /// if not specified.</remarks>
        public string? OutputPath { get; set; }

        /// <summary>
        /// A Serilog <see cref="ILogger"/> that redirects events to MSBuild.
        /// </summary>
        /// <remarks>The type was named <c>Log2</c> to distinguish itself from
        /// the less flexible <see cref="Log"/> property.</remarks>
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

        /// <inheritdoc/>
        public override bool Execute()
        {
            if (string.IsNullOrEmpty(IntermediateDirectory))
            {
                Log.LogError("The MSBuild task parameter 'IntermediateDirectory' is not specified." +
                             " Please set it to '$(ProjectDir)$(IntermediateOutputPath)'.");
                return false;
            }

            var config = new WeaverConfig
            {
                KeyFilePath = KeyOriginatorFile ?? AssemblyOriginatorKeyFile,
                SignAssembly = SignAssembly,
                IntermediateDirectory = IntermediateDirectory
            };
            Weaver.Weave(AssemblyPath, OutputPath, DoWeave, Log2, config);
            return !Log.HasLoggedErrors;
        }
    }
}
