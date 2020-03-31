// Copyright (c) 2020 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;

namespace Sigourney
{
    /// <summary>
    /// A class containing configuration data for a <see cref="Weaver"/>.
    /// </summary>
    /// <remarks>The necessary ones are typically provided by MSBuild,
    /// but custom applications can give their own.</remarks>
    [PublicAPI]
    public class WeaverConfig
    {
        private string? _intermediateDirectory;

        private bool _signAssembly;

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
        public bool SignAssembly
        {
            get => _signAssembly;
            set => _signAssembly = value;
        }

        /// <summary>
        /// The "obj/" directory used in the build.
        /// </summary>
        /// <remarks>It is derieved from the MSBuild
        /// "IntermediateDirectory" property.</remarks>
        public string? IntermediateDirectory
        {
            get => _intermediateDirectory;
            set => _intermediateDirectory = value;
        }

        /// <summary>
        /// All configuration items passed from MSBuild to Sigourney.
        /// </summary>
        /// <remarks>To add more, write <code>&lt;SigourneyConfig Include="" Key="Value" /&gt;
        /// </code></remarks>
        public readonly IReadOnlyDictionary<string, string> AllConfiguration;

        private void Init()
        {
            AllConfiguration.TryGetValue(nameof(IntermediateDirectory), out _intermediateDirectory);
            AllConfiguration.TryGetValue("KeyOriginatorFile", out var kof);
            AllConfiguration.TryGetValue("AssemblyOriginatorKeyFile", out var aokf);
            KeyFilePath = kof ?? aokf;
            if (AllConfiguration.TryGetValue(nameof(SignAssembly), out var sa))
                bool.TryParse(sa, out _signAssembly);
        }

        /// <summary>
        /// Creates a <see cref="WeaverConfig"/> with an
        /// empty <see cref="AllConfiguration"/> dictionary.
        /// </summary>
        public WeaverConfig() : this(new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Creates a <see cref="WeaverConfig"/> from the
        /// given <see cref="IReadOnlyDictionary{String,String}"/>.
        /// </summary>
        /// <param name="config">The given dictionary.</param>
        public WeaverConfig(IReadOnlyDictionary<string, string> config)
        {
            AllConfiguration = config;
            Init();
        }

        /// <summary>
        /// Creates a <see cref="WeaverConfig"/> from the
        /// given sequence of MSBuild <see cref="ITaskItem"/>s.
        /// </summary>
        /// <param name="items">The given sequence of items.</param>
        public WeaverConfig(IEnumerable<ITaskItem> items)
        {
            AllConfiguration =
                items
                    .SelectMany(x => x.CloneCustomMetadata().Cast<DictionaryEntry>())
                    .ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
        }
    }
}
