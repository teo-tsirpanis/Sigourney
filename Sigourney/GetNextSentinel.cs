using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sigourney
{
    /// <summary>
    /// Gets the names of Sigourney's sentinel files for the next weaver.
    /// </summary>
    /// <remarks>
    /// <para>To support incremental builds across projects that use multiple weavers,
    /// Sigourney uses two properties named <c>SigourneyInputSentinel</c> and
    /// <c>SigourneyOutputSentinel</c>. Each target that calls an <see cref="MSBuildWeaver"/>
    /// descendant has to set the former as its input and the latter as its output.
    /// Before such target runs, another target has to run that runs this task that
    /// updates these two properties.</para>
    /// <para>Now, what's a sentinel. Sentinels are little temporary files that
    /// correspond to each weaver that gets executed in a project build. How they
    /// exactly work is an implementation detail but with their help MSBuild can
    /// entirely skip weaving targets when they are not needed to run.</para>
    /// <para>Even without the sentinels, Sigourney itself will not weave an assembly
    /// that has already been weaved, thanks to the <c>ProcessedBy</c> class it always adds.</para>
    /// </remarks>
    public class GetNextSentinel : Task
    {

        private static readonly Regex _sentinelFileRegex =
            new Regex(@"ProcessedBySigourney\.(\d+)$", RegexOptions.Compiled);

        private int GetSentinelIndex(string sentinelFile)
        {
            var match = _sentinelFileRegex.Match(sentinelFile);
            if (match.Success)
            {
                var number = match.Groups[1].Value;
                Log.LogMessage("Sentinel index recognized as {0}.", number);
                return int.Parse(number);
            }
            Log.LogError("Sentinel file {0} is not recognized.", sentinelFile);
            return -1;
        }

        private string GetSentinelFileName(int idx) =>
            Path.Combine(IntermediateDirectory, $"ProcessedBySigourney.{idx}");

        private void VerifySentinelFile(int idx)
        {
            try
            {
                var sentinelContent = File.ReadAllText(OutputSentinel);
                if (sentinelContent.Equals(WeaverName, StringComparison.Ordinal))
                    return;
                Log.LogMessage("Sentinel file #{0} was found but it was for the weaver named {1} instead.", idx, WeaverName);
                File.Delete(OutputSentinel);
            }
            catch (FileNotFoundException)
            {
                Log.LogMessage("Sentinel file #{0} was not found.", idx);
            }
        }

        /// <summary>
        /// The project's intermediate directory.
        /// </summary>
        [Required]
        public string IntermediateDirectory { get; set; } = null!;

        /// <summary>
        /// A unique name that identifies the weaver.
        /// </summary>
        [Required]
        public string WeaverName { get; set; } = null!;

        /// <summary>
        /// The assembly's path.
        /// </summary>
        [Required]
        public string AssemblyPath { get; set; } = null!;

        /// <summary>
        /// Sigourney's current input sentinel file path.
        /// </summary>
        [Output] // The Required attribute does not only mean that the
        // parameter must be assigned, but also that it must not be empty.
        public string InputSentinel { get; set; } = "";

        /// <summary>
        /// Sigourney's current output sentinel file path.
        /// </summary>
        [Output]
        public string OutputSentinel { get; set; } = "";

        /// <inheritDoc/>
        public override bool Execute()
        {
            if (string.IsNullOrEmpty(InputSentinel))
            {
                InputSentinel = AssemblyPath;
                OutputSentinel = GetSentinelFileName(0);
                VerifySentinelFile(0);
            }
            else
            {
                var sentinelIdx = GetSentinelIndex(OutputSentinel);
                if (sentinelIdx < 0) return false;
                sentinelIdx++;
                InputSentinel = OutputSentinel;
                OutputSentinel = GetSentinelFileName(sentinelIdx);
                VerifySentinelFile(sentinelIdx);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
