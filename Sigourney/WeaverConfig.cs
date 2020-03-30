// Copyright (c) 2020 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace Sigourney
{
    public class WeaverConfig
    {
        public string? IntermediateDirectory { get; set; }
        public string? KeyOriginatorFile { get; set; }

        public string? AssemblyOriginatorKeyFile { get; set; }

        public string? KeyFilePath => KeyOriginatorFile ?? AssemblyOriginatorKeyFile;

        public bool SignAssembly { get; set; } = false;
    }
}
