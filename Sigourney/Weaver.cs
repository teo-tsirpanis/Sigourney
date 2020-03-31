// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    /// <summary>
    /// An abstract class for a process that modifies (weaves) a .NET assembly.
    /// </summary>
    /// <remarks>Yes, I know about the type's fully qualified name...</remarks>
    [PublicAPI]
    public abstract partial class Weaver
    {
        private readonly string _version;

        /// <summary>
        /// Creates a <see cref="Weaver"/>.
        /// </summary>
        protected Weaver()
        {
            var ownerAssembly = GetType().Assembly;
            _version = ownerAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? ownerAssembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Weaves an assembly.
        /// </summary>
        /// <remarks>If weaving succeeds, a static class named "ProcessedBy<see cref="ProductName"/>"
        /// will be presented to instruct Sigourney not to weave it again.</remarks>
        /// <param name="inputPath">The path of the assembly to weave.</param>
        /// <param name="outputPath">The path where the weaved assembly will be stored.
        /// Defaults to <paramref name="inputPath"/> if null.</param>
        /// <param name="log">A Serilog <see cref="ILogger"/> that will
        /// record any events that happen in the weaver.</param>
        /// <param name="config">An <see cref="WeaverConfig"/> object to
        /// further parameterize the weaving process.</param>
        public void Weave(string inputPath, string? outputPath, ILogger log, WeaverConfig config)
        {
            using var resultingAsembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(inputPath))
            {
                var assemblyName = asm.Name.Name;
                FindStrongNameKey(config, asm, log);

                if (AssemblyMarker.ShouldProcess(asm, ProductName))
                {
                    if (!DoWeave(asm, log, config))
                    {
                        log.Debug("Skipping weaving {AssemblyName} as requested.", assemblyName);
                        return;
                    }

                    AssemblyMarker.MarkAsProcessed(asm, ProductName, _version, log);
                    var writerParams = new WriterParameters
                    {
                        StrongNameKeyPair = _keyPair
                    };
                    asm.Name.PublicKey = _publicKey;
                    asm.Write(resultingAsembly, writerParams);
                }
                else
                {
                    log.Debug("{AssemblyName} is already weaved.", assemblyName);
                }
            }

            using var outputFile = File.Create(outputPath ?? inputPath);
            resultingAsembly.Position = 0;
            resultingAsembly.CopyTo(outputFile);
        }

        /// <summary>
        /// Gets an informative name of the weaver.
        /// </summary>
        /// <remarks>Defaults to the name of the assembly
        /// the weaver's class was defined.</remarks>
        protected virtual string ProductName => GetType().Assembly.GetName().Name;

        /// <summary>
        /// Performs the actual weaving using Mono.Cecil.
        /// </summary>
        /// <param name="asm">The assembly to modify in an <see cref="AssemblyDefinition"/> format.</param>
        /// <param name="log">A Serilog <see cref="ILogger"/> that will
        /// record any events that happen in the weaver.</param>
        /// <param name="config">An <see cref="WeaverConfig"/> object to
        /// further parameterize the weaving process.</param>
        /// <returns>Whether weaving actually happened. The weaver might
        /// determine that weaving is unnecessary for this assembly
        /// and Sigourney will skip modifying it entirely.</returns>
        protected abstract bool DoWeave(AssemblyDefinition asm, ILogger log, WeaverConfig config);
    }
}
