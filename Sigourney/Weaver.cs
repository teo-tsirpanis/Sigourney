// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    /// <summary>
    /// An abstract class for a procedure that modifies (weaves) a .NET assembly.
    /// </summary>
    /// <remarks>The type's fully qualified name does not imply
    /// an endorsement or support of any kind from anyone.</remarks>
    [PublicAPI]
    public static class Weaver
    {
        /// <summary>
        /// Creates a <see cref="Weaver"/>.
        /// </summary>
        private static string GetAssemblyVersion(Assembly asm)
        {
            return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                // Is there any case where an assembly cannot have a version?
                ?.InformationalVersion ?? asm.GetName().Version!.ToString();
        }

        /// <summary>
        /// Weaves an assembly.
        /// </summary>
        /// <remarks>If weaving succeeds, a static class named "ProcessedBy<paramref name="productName"/>"
        /// will be placed to the assembly, to instruct Sigourney not to weave it again.</remarks>
        /// <param name="inputPath">The path of the assembly to weave.</param>
        /// <param name="outputPath">The path where the weaved assembly will be stored.
        /// Defaults to <paramref name="inputPath"/> if null.</param>
        /// <param name="fWeave"><see cref="MSBuildWeaver.DoWeave"/></param>
        /// <param name="log">A Serilog <see cref="ILogger"/> that will
        /// record any events that happen in the weaver.</param>
        /// <param name="config">A <see cref="WeaverConfig"/> object that
        /// further parameterizes the weaving process. If not specified,
        /// some features like strong-name signing will not be supported.</param>
        /// <param name="productName">The name of the program that weaved the assembly.
        /// If not specified, it will be the name of the assembly
        /// in which <paramref name="fWeave"/> was declared.</param>
        public static void Weave(string inputPath, string? outputPath,
            Func<AssemblyDefinition, bool> fWeave, ILogger log, WeaverConfig? config = null,
            string? productName = null)
        {
            var weaverAssembly = fWeave.Method.Module.Assembly;
            // Is there any case where an assembly cannot have a name?
            var productNameActual = productName ?? weaverAssembly.GetName().Name!;
            var assemblyVersion = GetAssemblyVersion(weaverAssembly);
            using var resultingAsembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(inputPath))
            {
                var assemblyName = asm.Name.Name;
                StrongNameKeyFinder.FindStrongNameKey(config, asm, log, out var keyPair, out var publicKey);

                if (AssemblyMarker.ShouldProcess(asm, productNameActual))
                {
                    if (!fWeave(asm))
                    {
                        log.Debug("Skipping weaving {AssemblyName} because nothing changed.", assemblyName);
                        if (outputPath != null)
                            File.Copy(inputPath, outputPath, true);
                        return;
                    }

                    AssemblyMarker.MarkAsProcessed(asm, productNameActual, assemblyVersion, log);
                    var writerParams = new WriterParameters
                    {
                        StrongNameKeyPair = keyPair
                    };
                    asm.Name.PublicKey = publicKey;
                    asm.Write(resultingAsembly, writerParams);
                }
                else
                    log.Debug("{AssemblyName} is already weaved.", assemblyName);
            }

            using var outputFile = File.Create(outputPath ?? inputPath);
            resultingAsembly.Position = 0;
            resultingAsembly.WriteTo(outputFile);
        }
    }
}
