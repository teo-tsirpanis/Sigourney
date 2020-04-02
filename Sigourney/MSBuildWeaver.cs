using System;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Serilog;

namespace Sigourney
{
    /// <summary>
    /// An MSBuild task that executes a <see cref="Weaver"/>.
    /// </summary>
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public abstract class MSBuildWeaver: Task
    {
        /// <summary>
        /// Configuration needed by Sigourney.
        /// </summary>
        /// <remarks>In MSBuild, pass the item list
        /// <c>@(SigourneyConfig)</c> to this property.</remarks>
        [Required]
        public ITaskItem[] SigourneyConfig { get; set; } = new ITaskItem[0];

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
        /// The instance of the weaver to be used.
        /// </summary>
        protected abstract Weaver CreateWeaver();

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.MSBuild(this)
                .CreateLogger();
            try
            {
                CreateWeaver().Weave(AssemblyPath, OutputPath, log, new WeaverConfig(SigourneyConfig));
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
