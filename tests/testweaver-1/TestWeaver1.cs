using Mono.Cecil;
using Sigourney;
using System;

namespace testweaver_1
{
    // This task inherits the MSBuildWeaver class, itself a descendant of
    // Microsoft.Build.Utilities.Task. This task does not define any
    // additional parameters but we can if necessary.
    public class TestWeaver1 : MSBuildWeaver
    {
        public override bool Execute()
        {
            // In more advanced cases we can override the Execute method as well.
            // We can perform additional checks, or even entirely bypass weaving
            // by not calling the base Execute method.
            if (new Random().Next(0, 10) == 11)
            {
                // Log2 is a property introduced in MSBuildWeaver.
                // It exposes a Serilog ILogger that logs any event to MSBuild.
                // An error raised either by Log2 or Log will fail the build
                // if the base Execute method is called.
                Log2.Error("Sorry, something went wrong with the laws of math :-(");
                return false;
            }
            // Calling the base method will eventually call the DoWeave method below.
            return base.Execute();
        }

        // The MSBuildWeaver class has an abstract method that accepts a Mono.Cecil
        // assembly definition, modifies it, and returns a boolean value. If false
        // is returned, weaving is skipped and the assembly file on the disk is not
        // changed. In that case, any changes to the assembly definition are discarded.
        protected override bool DoWeave(AssemblyDefinition asm)
        {
            asm.MainModule.Types.Add(new TypeDefinition("", "TestWeaver1Rulez",
                TypeAttributes.Class | TypeAttributes.Sealed, asm.MainModule.TypeSystem.Object));
            return true;
        }
    }
}
