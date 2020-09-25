// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using Microsoft.Build.Framework;

namespace Sigourney
{
    /// <summary>
    /// A class containing configuration data for a <see cref="Weaver"/>.
    /// </summary>
    /// <remarks>The necessary ones are typically provided by MSBuild,
    /// but custom applications can give their own.</remarks>
    public class WeaverConfig
    {
        /// <summary>
        /// The file path to the strong-name key (.snk) of the assembly.
        /// </summary>
        /// <remarks>It is derived from the MSBuild "KeyOriginatorFile"
        /// and "AssemblyOriginatorKeyFile" properties (the first, and
        /// if it does not exist, the second one).</remarks>
        public string? KeyFilePath { get; set; }

        /// <summary>
        /// Whether the assembly is strong-named.
        /// </summary>
        /// <remarks>It is derived from the MSBuild
        /// "SignAssembly" property.</remarks>
        public bool SignAssembly { get; set; }

        /// <summary>
        /// The "obj/" directory used in the build.
        /// </summary>
        /// <remarks>It is derieved from the MSBuild
        /// "IntermediateDirectory" property.</remarks>
        public string? IntermediateDirectory { get; set; }

        internal static WeaverConfig? TryCreate(ITaskItem[]? items)
        {
            if (items == null || items.Length != 1) return null;

            var item = items[0];
            var signAssembly = bool.TryParse(item.GetMetadata(nameof(SignAssembly)), out var result) && result;

            static string? GetMetadata(ITaskItem item, string key)
            {
                var value = item.GetMetadata(key);
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var keyOriginatorFile = GetMetadata(item, "KeyOriginatorFile");
            var assemblyOriginatorKeyFile = GetMetadata(item, "AssemblyOriginatorKeyFile");
            var keyFilePath = keyOriginatorFile ?? assemblyOriginatorKeyFile;

            var intermediateDirectory = GetMetadata(item, nameof(IntermediateDirectory));

            return new WeaverConfig() {
                KeyFilePath = keyFilePath,
                SignAssembly = signAssembly,
                IntermediateDirectory = intermediateDirectory
            };
        }
    }
}
