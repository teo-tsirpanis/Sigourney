using System;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Serilog;
using Serilog.Sinks.MSBuild;
using ILogger = Serilog.ILogger;
using Logger = Serilog.Core.Logger;

namespace Sigourney
{
    /// <summary>
    /// An MSBuild task that executes a <see cref="Weaver"/>.
    /// </summary>
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public abstract class MSBuildWeaver : Task
    {
        [Required] public bool SignAssembly { get; set; }

        public string? IntermediateDirectory { get; set; }

        [Required] public string? KeyOriginatorFile { get; set; }

        [Required] public string? AssemblyOriginatorKeyFile { get; set; }

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
        /// the less flexible MSBuild's <see cref="Log"/> property.</remarks>
        protected ILogger Log2 = Logger.None;

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
            Log2 = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.MSBuild(this)
                .CreateLogger()
                .ForContext(MSBuildProperties.File, AssemblyPath);
            try
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
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }
    }
}
