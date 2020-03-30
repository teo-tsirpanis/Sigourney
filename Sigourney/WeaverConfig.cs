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

        public string? KeyFilePath { get; set; }

        public bool SignAssembly
        {
            get => _signAssembly;
            set => _signAssembly = value;
        }

        public string? IntermediateDirectory
        {
            get => _intermediateDirectory;
            set => _intermediateDirectory = value;
        }

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

        public WeaverConfig() : this(new Dictionary<string, string>())
        {
        }

        public WeaverConfig(IReadOnlyDictionary<string, string> config)
        {
            AllConfiguration = config;
            Init();
        }

        public WeaverConfig(IEnumerable<ITaskItem> items)
        {
            AllConfiguration = items.SelectMany(x => x.CloneCustomMetadata().Cast<DictionaryEntry>())
                .ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
        }
    }
}
