// Copyright (c) 2020 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

// Source code based on Fody.
// https://github.com/Fody/Fody/blob/6.1.0/FodyIsolated/StrongNameKeyFinder.cs

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    public partial class Weaver
    {
        private StrongNameKeyPair? _keyPair;
        private byte[]? _publicKey;

        private void FindStrongNameKey(WeaverConfig config, AssemblyDefinition asm, ILogger log)
        {
            if (!config.SignAssembly)
            {
                return;
            }

            var keyFilePath = GetKeyFilePath(config, asm, log);
            if (keyFilePath == null) return;

            if (!File.Exists(keyFilePath))
                throw new FileNotFoundException("KeyFilePath was defined but file does not exist.", keyFilePath);

            var fileBytes = File.ReadAllBytes(keyFilePath);
            _keyPair = new StrongNameKeyPair(fileBytes);

            try
            {
                _publicKey = _keyPair.PublicKey;
            }
            catch (ArgumentException e)
            {
                log.Debug(e, "Exception while trying to load strong-name key pair.");
                _keyPair = null;
                _publicKey = fileBytes;
            }
        }

        private string? GetKeyFilePath(WeaverConfig config, AssemblyDefinition asm, ILogger log)
        {
            var keyFilePath = config.KeyFilePath;
            if (keyFilePath != null)
            {
                keyFilePath = Path.GetFullPath(keyFilePath);
                log.Debug("Using strong name key from KeyFilePath '{KeyFilePath}'.", keyFilePath);
                return keyFilePath;
            }

            var keyFileSuffix = asm
                .CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name == "AssemblyKeyFileAttribute")
                ?.ConstructorArguments
                ?.First();
            if (keyFileSuffix.HasValue)
            {
                keyFilePath = Path.Combine(config.IntermediateDirectory, (string) keyFileSuffix.Value.Value);
                log.Debug("Using strong name key from [AssemblyKeyFileAttribute(\"{KeyFileSuffix}\")] '{KeyFilePath}'",
                    keyFileSuffix, keyFilePath);
                return keyFilePath;
            }

            log.Debug("No strong name key found");
            return null;
        }
    }
}
