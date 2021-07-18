// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    /// <summary>
    /// Provides methods to modify (weave) a .NET assembly.
    /// </summary>
    /// <remarks>The type's fully qualified name does not imply
    /// an endorsement or support of any kind from anyone.</remarks>
    [PublicAPI]
    public static class Weaver
    {
        private static string GetAssemblyVersion(Assembly asm)
        {
            return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                // Is there any case where an assembly cannot have a version?
                ?.InformationalVersion ?? asm.GetName().Version!.ToString();
        }

        private static bool TryReadSymbols(AssemblyDefinition asm, ILogger log)
        {
            try
            {
                asm.MainModule.ReadSymbols();
                log.Debug("Debug symbols found and loaded");
                return true;
            }
            catch (Exception e)
            {
                log.Debug(e, "Reading debug symbols failed");
                return false;
            }
        }

        /// <summary>
        /// Weaves an assembly.
        /// </summary>
        /// <remarks>If weaving succeeds, a static class named "ProcessedBy<paramref name="weaverName"/>"
        /// will be placed to the assembly, to instruct Sigourney not to weave it again.</remarks>
        /// <param name="inputPath">The path of the assembly to weave.</param>
        /// <param name="outputPath">The path where the weaved assembly will be stored.
        /// Defaults to <paramref name="inputPath"/> if null.</param>
        /// <param name="fWeave">A delegate that performs the actual weaving. If it returns
        /// <see langword="false"/>, weaving will stop and the assembly will not be modified.</param>
        /// <param name="log">A Serilog <see cref="ILogger"/> that will
        /// record any events that happen in the weaver.</param>
        /// <param name="config">A <see cref="WeaverConfig"/> object that
        /// further parameterizes the weaving process. If not specified,
        /// some features like strong-name signing will not be supported.</param>
        /// <param name="weaverName">The name of the program that weaved the assembly.
        /// If not specified, it will be the name of the assembly
        /// in which <paramref name="fWeave"/> was declared.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inputPath"/>,
        /// <paramref name="fWeave"/> or <paramref name="log"/> is <see langword="null"/>.</exception>
        public static void Weave(string inputPath, string? outputPath,
            Func<AssemblyDefinition, bool> fWeave, ILogger log, WeaverConfig? config = null,
            string? weaverName = null)
        {
            if (inputPath == null) throw new ArgumentNullException(nameof(inputPath));
            if (fWeave == null) throw new ArgumentNullException(nameof(fWeave));
            if (log == null) throw new ArgumentNullException(nameof(log));
            
            var weaverAssembly = fWeave.Method.Module.Assembly;
            // string.IsNullOrEmpty does not propagate nullability information on .NET Standard 2.0
            if (weaverName == null || weaverName.Length == 0)
            {
                // Is there any case where an assembly cannot have a name?
                weaverName = weaverAssembly.GetName().Name!;
                log.Debug(
                    "No weaver name was supplied; it is inferred from " +
                    "the weaving delegate's assembly to be {WeaverName}", weaverName);
            }

            var assemblyVersion = GetAssemblyVersion(weaverAssembly);

            // If the output path is specified (i.e. it's not the same as the input path),
            // we first copy the input assembly there, and then weave that new copy.
            if (outputPath != null)
                File.Copy(inputPath, outputPath, true);
            using var assemblyResolver =
                new AssemblyReferenceResolver(config?.References ?? Enumerable.Empty<AssemblyReference>(), log);
            var readerParams = new ReaderParameters()
            {
                ReadWrite = true,
                AssemblyResolver = assemblyResolver
            };
            using (var asm = AssemblyDefinition.ReadAssembly(outputPath ?? inputPath, readerParams))
            {
                var assemblyName = asm.Name.Name;
                var hasSymbols = TryReadSymbols(asm, log);
                StrongNameKeyFinder.FindStrongNameKey(config, asm, log, out var keyPair, out var publicKey);

                if (AssemblyMarker.ShouldProcess(asm, weaverName))
                {
                    if (!fWeave(asm))
                    {
                        log.Debug("Skipping weaving {AssemblyName} because the weaving function returned false",
                            assemblyName);
                        return;
                    }

                    log.Debug("Weaving {AssemblyName} succeeded", assemblyName);

                    AssemblyMarker.MarkAsProcessed(asm, weaverName, assemblyVersion, log);
                    var writerParams = new WriterParameters
                    {
                        StrongNameKeyPair = keyPair,
                        WriteSymbols = hasSymbols
                    };
                    asm.Name.PublicKey = publicKey;
                    asm.Write(writerParams);
                }
                else
                    log.Debug(
                        "Skipping weaving {AssemblyName} because it already" +
                        "has a type named ProcessedBy{WeaverName}", assemblyName, weaverName);
            }
        }
    }
}
