// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;

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

        internal string GetIntermediateDirectory()
        {
            if (IntermediateDirectory == null)
                throw new ArgumentNullException(nameof(IntermediateDirectory),
                    "The intermediate directory of a WeaverConfig object must not be null.");
            return IntermediateDirectory;
        }
    }
}
