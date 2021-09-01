using System;
using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#pragma warning disable CS1591

namespace Sigourney
{
    /// <summary>
    /// This task is deprecated; Sigourney does not
    /// need it anymore to support incremental builds.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GetNextSentinel : Task
    {
        [Required]
        public string IntermediateDirectory { get; set; } = null!;

        [Required]
        public string WeaverName { get; set; } = null!;

        [Required]
        public string AssemblyPath { get; set; } = null!;

        [Output]
        public string InputSentinel { get; set; } = "";

        [Output]
        public string OutputSentinel { get; set; } = "";

        /// <inheritDoc/>
        public override bool Execute()
        {
            Log.LogWarning("As of Sigourney 0.3.0, the sentinel API is deprecated and not required for supporting incremental builds.");
            // Returning an empty string would cause the targets not to execute.
            // We deliberately return a dummy file to maintain compatibility.
            // It won't be created; the sentinels were already stated to be blackboxes.
            if (string.IsNullOrEmpty(InputSentinel) || string.IsNullOrEmpty(OutputSentinel))
                InputSentinel = OutputSentinel = $"{Guid.NewGuid()}.dummy";
            return true;
        }
    }
}
