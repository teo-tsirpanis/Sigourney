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
    /// To support incremental builds, Sigourney uses little files called sentinels. Sentinels are represented by
    /// two MSBuild properties called <c>SigourneyInputSentinel</c> and <c>SigourneyOutputSentinel</c>.
    /// Each weaver a project uses corresponds to one sentinel file. The sentinel for the first weaver
    /// is called <c>ProcessedBySigourney.0</c> and so on. When the weaver finishes its job, it creates
    /// a sentinel with the corresponding index in the <c>obj/</c> directory. The weaver's MSBuild target's
    /// input is set to the input setinel and the output, to the output sentinel, establishing a temporal chain of
    /// dependencies between the weavers, convinving MSBuild that a weaver does not need to run again.
    /// Before a weaving target is executed, an auxilary target calls this task and updates the paths of these sentinel
    /// files. When the first weaver starts, both properties are empty. In this case, this task will set the input
    /// sentinel to the compiled assembly, and the output sentinel to the first one. This is how this chain is formed.
    /// </remarks>
    public class GetNextSentinel : Task
    {

        private static readonly Regex _sentinelFileRegex =
            new Regex(@"ProcessedBySigourney\.(\d)+$", RegexOptions.Compiled);

        private int GetSentinelIndex(string sentinelFile)
        {
            var match = _sentinelFileRegex.Match(sentinelFile);
            if (match.Success)
                return int.Parse(match.Groups[0].Value);
            Log.LogError("Sentinel file {0} is not recognized.", sentinelFile);
            return -1;
        }

        private string GetSentinelFileName(int idx) =>
            Path.Combine(IntermediateDirectory, $"ProcessedBySigourney.{idx}");

        private void VerifySentinelFile(int idx)
        {
            try
            {
                var sentinelProduct = File.ReadAllText(OutputSentinel);
                if (sentinelProduct.Equals(ProductName, StringComparison.Ordinal))
                    return;
                Log.LogMessage("Sentinel file #{0} was found but it was for {1} instead.", idx, ProductName);
            }
            catch (FileNotFoundException)
            {
                Log.LogMessage("Sentinel file #{0} was not found.", idx);
            }

            File.WriteAllText(OutputSentinel, ProductName);
        }

        /// <summary>
        /// The project's intermediate directory.
        /// </summary>
        [Required]
        public string IntermediateDirectory { get; set; } = null!;

        /// <summary>
        /// A unique name that identifies the weaver.
        /// </summary>
        /// <remarks>It's unrelated to the product name that is passed
        /// as a parameter in <see cref="Weaver.Weave"/>.</remarks>
        [Required]
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// The assembly's path.
        /// </summary>
        [Required]
        public string AssemblyPath { get; set; } = null!;

        /// <summary>
        /// Sigourney's current input sentinel file path.
        /// </summary>
        [Required, Output]
        public string InputSentinel { get; set; } = null!;

        /// <summary>
        /// Sigourney's current output sentinel file path.
        /// </summary>
        [Required, Output]
        public string OutputSentinel { get; set; } = null!;

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
