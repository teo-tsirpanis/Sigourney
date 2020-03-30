using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Serilog;

namespace Sigourney
{
    internal static class AssemblyMarker
    {
        private static string GetProcessedByClassName(string productName) =>
            "ProcessedBy" + productName.Replace('.', '_');

        internal static bool ShouldProcess(AssemblyDefinition asm, string productName)
        {
            var name = GetProcessedByClassName(productName);
            return asm.MainModule.GetAllTypes().Any(x => x.FullName == name);
        }

        internal static void MarkAsProcessed(AssemblyDefinition asm, string productName, string version, ILogger log)
        {
            var name = GetProcessedByClassName(productName);

            log.Debug("Adding the {ProcessedBy} class", name);
            const TypeAttributes typeAttributes =
                TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed;
            var td = new TypeDefinition("", name, typeAttributes, asm.MainModule.TypeSystem.Object);

            const FieldAttributes fieldAttributes = FieldAttributes.Assembly |
                                                    FieldAttributes.Literal |
                                                    FieldAttributes.Static |
                                                    FieldAttributes.HasDefault;
            var fieldDefinition =
                new FieldDefinition("Version", fieldAttributes, asm.MainModule.TypeSystem.String)
                {
                    Constant = version
                };
            td.Fields.Add(fieldDefinition);

            asm.MainModule.Types.Add(td);
        }
    }
}
