using Mono.Cecil;
using Sigourney;

namespace testweaver_2
{
    public class TestWeaver2 : MSBuildWeaver
    {
        protected override bool DoWeave(AssemblyDefinition asm)
        {
            asm.MainModule.Types.Add(new TypeDefinition("", "TestWeaver2IzBetter",
                TypeAttributes.Class | TypeAttributes.Sealed, asm.MainModule.TypeSystem.Object));
            return true;
        }
    }
}
