// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

// Source code based on Fody.
// https://github.com/Fody/Fody/blob/6.1.0/FodyIsolated/StrongNameKeyFinder.cs

using System.IO;
using System.Linq;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    internal static class StrongNameKeyFinder
    {
        internal static byte[]? FindStrongNameKey(WeaverConfig? config, AssemblyDefinition asm, ILogger log)
        {
            if (config == null || !config.SignAssembly) return null;

            var keyFilePath = GetKeyFilePath(config, asm, log);
            if (keyFilePath == null) return null;

            return File.ReadAllBytes(keyFilePath);
        }

        private static string? GetKeyFilePath(WeaverConfig config, AssemblyDefinition asm, ILogger log)
        {
            var keyFilePath = config.KeyFilePath;
            if (!string.IsNullOrEmpty(config.KeyFilePath))
            {
                keyFilePath = Path.GetFullPath(keyFilePath!);
                log.Debug("Using strong name key from KeyFilePath {KeyFilePath}.", keyFilePath);
                return keyFilePath;
            }

            var keyFileSuffix = asm
                .CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name == "AssemblyKeyFileAttribute")
                ?.ConstructorArguments
                ?.First();
            var intermediateDirectory = config.IntermediateDirectory;
            if (intermediateDirectory != null && keyFileSuffix.HasValue)
            {
                keyFilePath = Path.Combine(intermediateDirectory, (string) keyFileSuffix.Value.Value);
                log.Debug("Using strong name key from [AssemblyKeyFileAttribute({KeyFileSuffix})] {KeyFilePath}.",
                    keyFileSuffix, keyFilePath);
                return keyFilePath;
            }

            log.Debug("No strong name key was found.");
            return null;
        }
    }
}
