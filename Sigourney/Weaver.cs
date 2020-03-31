// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Serilog;
using Serilog.Core;

namespace Sigourney
{
    /// <summary>
    /// An abstract class for a process that modifies (weaves) a .NET assembly.
    /// </summary>
    /// <remarks>Yes, I know about the type's fully qualified name...</remarks>
    [PublicAPI]
    public abstract partial class Weaver
    {
        /// <summary>
        /// A Serilog <see cref="ILogger"/> that will
        /// record any events that happen in the weaver.
        /// </summary>
        protected readonly ILogger Log;
        private readonly string _version;

        /// <summary>
        /// Creates a <see cref="Weaver"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to
        /// be put to the <see cref="Log"/> field.</param>
        protected Weaver(ILogger? log = null)
        {
            Log = log ?? Logger.None;
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
        /// Defaults to <paramref name="inputPath"/>.</param>
        /// <param name="config">An optional <see cref="WeaverConfig"/> object to
        /// further parameterize the weaving process.</param>
        public void Weave(string inputPath, string? outputPath = null, WeaverConfig? config = null)
        {
            using var resultingAsembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(inputPath))
            {
                var assemblyName = asm.Name.Name;
                if (config != null)
                    FindStrongNameKey(config, asm);

                if (AssemblyMarker.ShouldProcess(asm, ProductName))
                {
                    if (!DoWeave(asm, config))
                    {
                        Log.Debug("Skipping weaving {AssemblyName} as requested.", assemblyName);
                        return;
                    }

                    AssemblyMarker.MarkAsProcessed(asm, ProductName, _version, Log);
                    var writerParams = new WriterParameters
                    {
                        StrongNameKeyPair = _keyPair
                    };
                    asm.Name.PublicKey = _publicKey;
                    asm.Write(resultingAsembly, writerParams);
                }
                else
                {
                    Log.Debug("{AssemblyName} is already weaved.", assemblyName);
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
        /// <param name="config"></param>
        /// <returns></returns>
        protected abstract bool DoWeave(AssemblyDefinition asm, WeaverConfig? config);
    }
}
