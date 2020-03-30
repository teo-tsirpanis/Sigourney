using System.IO;
using System.Reflection;
using Mono.Cecil;
using Serilog;
using Serilog.Core;

namespace Sigourney
{
    public abstract partial class Weaver
    {
        protected readonly ILogger Log;
        private readonly string _version;

        public Weaver(ILogger? log = null)
        {
            Log = log ?? Logger.None;
            var ownerAssembly = GetType().Assembly;
            _version = ownerAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? ownerAssembly.GetName().Version.ToString();
        }

        public void Weave(string inputPath, string? outputPath = null, WeaverConfig? config = null)
        {
            using var resultingAsembly = new MemoryStream();
            using (var asm = AssemblyDefinition.ReadAssembly(inputPath))
            {
                if (config != null)
                    FindStrongNameKey(config, asm);

                if (AssemblyMarker.ShouldProcess(asm, ProductName))
                {
                    if (!DoWeave(asm)) return;

                    AssemblyMarker.MarkAsProcessed(asm, ProductName, _version, Log);
                    var writerParams = new WriterParameters
                    {
                        StrongNameKeyPair = _keyPair
                    };
                    asm.Name.PublicKey = _publicKey;
                    asm.Write(resultingAsembly, writerParams);
                }
            }

            using var outputFile = File.Create(outputPath ?? inputPath);
            resultingAsembly.Position = 0;
            resultingAsembly.CopyTo(outputFile);
        }

        protected abstract string ProductName { get; }
        protected abstract bool DoWeave(AssemblyDefinition asm);
    }
}
