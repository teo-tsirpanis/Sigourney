// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;

namespace Sigourney
{
    /// <summary>
    /// A class containing optional configuration that is passed to <see cref="Weaver.Weave"/>.
    /// </summary>
    /// <remarks>It is automaticallly created by MSBuild but standalone
    /// applications using Sigourney can use their own.</remarks>
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
        /// Whether the assembly is strong-named with delay-signing.
        /// </summary>
        /// <remarks>It is derived from the MSBuild <c>DelaySign</c> property.</remarks>
        public bool DelaySign { get; set; }

        /// <summary>
        /// The "obj/" directory used in the build.
        /// </summary>
        /// <remarks>It is derieved from the MSBuild "IntermediateOutputPath" property.</remarks>
        // TODO: Deprecate in the next minor release and replace with IntermediateOutputPath.
        public string? IntermediateDirectory { get; set; }

        /// <summary>
        /// Additional assemblies that Mono.Cecil will take into consideration.
        /// </summary>
        /// <remarks>It is derived from the MSBuild "ReferencePath" item</remarks>
        public List<AssemblyReference> References { get; } = new List<AssemblyReference>();

        private static char[] _pathSeparator = new char[] { ';' };

        /// <summary>
        /// Creates a <see cref="WeaverConfig"/> from the MSBuild
        /// items taken from <c>@(SigourneyConfiguration)</c>.
        /// </summary>
        /// <param name="items">The array of items to process.
        /// They have to be supplied by an MSBuild task parameter
        /// with a value of <c>@(SigourneyConfiguration)</c>.</param>
        /// <returns>A <see cref="WeaverConfig"/> or <see langword="null"/>
        /// if <paramref name="items"/> is invalid.</returns>
        public static WeaverConfig? TryCreateFromSigourneyConfiguration(ITaskItem[]? items)
        {
            if (items == null || items.Length != 1) return null;

            var item = items[0];
            var config = new WeaverConfig();

            string? GetMetadata(string key)
            {
                var value = item.GetMetadata(key);
                return string.IsNullOrEmpty(value) ? null : value;
            }

            bool GetMetadataBool(string key) =>
                bool.TryParse(item.GetMetadata(key), out bool result) && result;

            config.DelaySign = GetMetadataBool(nameof(DelaySign));
            config.SignAssembly = GetMetadataBool(nameof(SignAssembly));

            var keyOriginatorFile = GetMetadata("KeyOriginatorFile");
            var assemblyOriginatorKeyFile = GetMetadata("AssemblyOriginatorKeyFile");
            config.KeyFilePath = keyOriginatorFile ?? assemblyOriginatorKeyFile;

            config.IntermediateDirectory = GetMetadata(nameof(IntermediateDirectory));

            var referencesMetadata = GetMetadata(nameof(References));
            if (referencesMetadata != null)
            {
                var references =
                    from x in referencesMetadata.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries)
                    where !string.IsNullOrWhiteSpace(x)
                    select new AssemblyReference(x);
                config.References.AddRange(references);
            }

            return config;
        }
    }
}
