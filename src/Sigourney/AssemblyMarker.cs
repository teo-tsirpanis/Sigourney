using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    internal static class AssemblyMarker
    {
        private static string GetProcessedByClassName(string weaverName) =>
            "ProcessedBy" + weaverName.Replace('.', '_');

        internal static bool ShouldProcess(AssemblyDefinition asm, string weaverName)
        {
            var name = GetProcessedByClassName(weaverName);
            return asm.MainModule.GetType(name) == null;
        }

        internal static void MarkAsProcessed(AssemblyDefinition asm, string weaverName, string version, ILogger log)
        {
            var name = GetProcessedByClassName(weaverName);

            log.Debug("Adding the {ProcessedBy:l} class.", name);
            const TypeAttributes typeAttributes =
                TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed;
            var td = new TypeDefinition("", name, typeAttributes, asm.MainModule.TypeSystem.Object);

            const FieldAttributes fieldAttributes = FieldAttributes.Assembly |
                                                    FieldAttributes.Literal |
                                                    FieldAttributes.Static |
                                                    FieldAttributes.HasDefault;
            var fieldDefinition =
                new FieldDefinition(weaverName + "Version", fieldAttributes, asm.MainModule.TypeSystem.String)
                {
                    Constant = version
                };
            td.Fields.Add(fieldDefinition);

            asm.MainModule.Types.Add(td);
        }
    }
}
