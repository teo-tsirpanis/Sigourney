using Mono.Cecil;
using Sigourney;

namespace testweaver_1
{
    public class TestWeaver1 : MSBuildWeaver
    {
        protected override bool DoWeave(AssemblyDefinition asm)
        {
            asm.MainModule.Types.Add(new TypeDefinition("", "TestWeaver1Rulez",
                TypeAttributes.Class | TypeAttributes.Sealed, asm.MainModule.TypeSystem.Object));
            return true;
        }
    }
}
