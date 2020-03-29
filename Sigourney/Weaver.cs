using System;
using Mono.Cecil;
using Serilog;
using Serilog.Core;

namespace Sigourney
{
    public abstract partial class Weaver
    {
        protected readonly ILogger Log;

        public Weaver(ILogger? log = null)
        {
            Log = log ?? Logger.None;
        }

        protected abstract bool DoWeave(AssemblyDefinition asm);
    }
}
