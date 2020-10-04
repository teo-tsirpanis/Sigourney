using System.IO;
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
        /// It has to be set if multiple weavers exist in the same assembly,
        /// or the sentinel API is used. In the latter case, its value must
        /// be the same with the one passed in <see cref="GetNextSentinel.ProductName"/>.
        /// </remarks>
        public string? ProductName { get; set; }

        /// <summary>
        /// The path to save the weaved assembly.
        /// </summary>
        /// <remarks>Defaults to <see cref="AssemblyPath"/>
        /// if not specified.</remarks>
        public string? OutputPath { get; set; }

        /// <summary>
        /// The path of the output sentinel file that corresponds to this task.
        /// </summary>
        /// <remarks>
        /// After weaving the assembly, Sigourney will create a file
        /// in this path, containing <see cref="ProductName"/>.
        /// If this parameter is set, <see cref="ProductName"/> must
        /// be set as well or the build will fail.
        /// </remarks>
        /// <seealso cref="GetNextSentinel"/>
        public string? OutputSentinel { get; set; }

        /// <summary>
        /// Additional configuration that Sigourney's internal implementation needs.
        /// </summary>
        /// <remarks>
        /// In MSBuild, this task parameter should be passed as <c>@(SigourneyConfiguration)</c>.
        /// </remarks>
        public ITaskItem[]? Configuration { get; set; }

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

        /// <inheritdoc/>
        public override bool Execute()
        {
            if (!string.IsNullOrEmpty(OutputSentinel) && string.IsNullOrEmpty(ProductName))
            {
                Log.LogError("The ProductName property must be set if the OutputSentinel property is too.");
                return false;
            }

            var config = WeaverConfig.TryCreate(Configuration);
            if (config == null)
                Log.LogMessage("Something went wrong with the Configuration task parameter. Sigourney's functionality might be limited.");

            Weaver.Weave(AssemblyPath, OutputPath, DoWeave, Log2, config, ProductName);

            if (!string.IsNullOrEmpty(OutputSentinel) && !Log.HasLoggedErrors)
                File.WriteAllText(OutputSentinel, ProductName!);
            return !Log.HasLoggedErrors;
        }
    }
}
