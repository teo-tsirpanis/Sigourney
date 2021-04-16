// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Mono.Cecil;
using Serilog;

namespace Sigourney
{
    /// <summary>
    /// An object representing an assembly file name, with
    /// some convenience methods to access information about it.
    /// </summary>
    /// <remarks>Two <see cref="AssemblyReference"/> objects are
    /// considered as equal if they point to the same file name.</remarks>
    [PublicAPI]
    public sealed class AssemblyReference : IEquatable<AssemblyReference>, IComparable<AssemblyReference>, IComparable
    {
        /// <summary>
        /// The path to the assembly.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Creates an <see cref="AssemblyReference"/>.
        /// </summary>
        /// <param name="fileName">The path to the assembly.</param>
        public AssemblyReference(string fileName)
        {
            FileName = fileName;

            using var asm = AssemblyDefinition.ReadAssembly(fileName);
            // https://github.com/dotnet/runtime/issues/35449#issuecomment-620156856
            IsReferenceAssembly =
                asm.Name.Attributes.HasFlag((AssemblyAttributes) 0x70)
                || asm.CustomAttributes.Any(attr =>
                    attr.AttributeType.FullName.Equals(typeof(ReferenceAssemblyAttribute).FullName,
                        StringComparison.Ordinal));
            AssemblyName = asm.Name;
        }

        /// <summary>
        /// Whether the assembly is recognized by either .NET
        /// Framework or .NET Core/.NET 5+ as a reference assembly.
        /// </summary>
        public bool IsReferenceAssembly { get; }

        /// <summary>
        /// An <see cref="AssemblyNameDefinition"/> object that corresponds to this assembly.
        /// </summary>
        public AssemblyNameDefinition AssemblyName { get; }

        #region Interface implementation boilerplate

        /// <inheritdoc/>
        public bool Equals(AssemblyReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FileName == other.FileName;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is AssemblyReference other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }

        /// <inheritdoc/>
        public int CompareTo(AssemblyReference? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(FileName, other.FileName, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is AssemblyReference other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(AssemblyReference)}", nameof(obj));
        }

        /// <inheritdoc/>
        public override string ToString() => FileName;

        #endregion
    }

    /// <summary>
    /// An <see cref="IAssemblyResolver"/> implementation that
    /// takes into consideration the <see cref="AssemblyReference"/>
    /// objects that were passed to its constructor.
    /// </summary>
    internal sealed class AssemblyReferenceResolver : BaseAssemblyResolver
    {
        private readonly Dictionary<string, string> _assemblyNameLookup;
        private readonly ILogger _log;

        // The DefaultAssemblyResolver's cache is not appropriate because we
        // can't check whether it has the assembly we ask for without triggering
        // a resolve that throws an exception if the assembly is not found. So
        // we use our own cache and inherit directly from BaseAssemblyResolver.
        private readonly ConcurrentDictionary<string, AssemblyDefinition> _assemblyCache =
            new ConcurrentDictionary<string, AssemblyDefinition>(StringComparer.Ordinal);

        internal AssemblyReferenceResolver(IEnumerable<AssemblyReference> assemblies, ILogger log)
        {
            _assemblyNameLookup = assemblies.ToDictionary(x => x.AssemblyName.FullName,
                x => x.FileName, StringComparer.Ordinal);
            _log = log;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            _log.Verbose("Cecil requested to resolve assembly {AssemblyName}", name);
            return _assemblyCache.GetOrAdd(name.FullName, key => {
                if (_assemblyNameLookup.TryGetValue(key, out var path)) {
                    _log.Verbose("Assembly recognized as project reference, loading it for the first time");
                    return AssemblyDefinition.ReadAssembly(path);
                }

                _log.Verbose("Unrecognized assembly, falling back to Cecil's resolver");
                return base.Resolve(name);
            });
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var asm in _assemblyCache.Values)
                asm.Dispose();
            _assemblyCache.Clear();

            base.Dispose(disposing);
        }
    }
}
