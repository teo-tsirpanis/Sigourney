// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;

namespace Sigourney
{
    /// <summary>
    /// A <c>.nuspec</c> file.
    /// </summary>
    /// <remarks>This class is not a complete specification of <c>.nuspec</c> files.
    /// It only allows setting metadata and files, neither references nor dependencies,
    /// and does not support reading <c>.nuspec</c> files, only writing.</remarks>
    [PublicAPI]
    public class NuspecFile
    {
#pragma warning disable 1591

        public string? PackageId { get; set; }
        public string? Version { get; set; }
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Owners { get; set; }
        public string? Description { get; set; }
        public string? ReleaseNotes { get; set; }
        public string? Summary { get; set; }
        public string? ProjectUrl { get; set; }
        public string? IconUrl { get; set; }
        public string? LicenseExpression { get; set; }
        public string? LicenseFile { get; set; }
        public string? Copyright { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string? Tags { get; set; }
        public bool DevelopmentDependency { get; set; }
        public string? RepositoryType { get; set; }
        public string? RepositoryUrl { get; set; }

        public List<File> Files { get; } = new List<File>();

        private static void AddMetadata(XContainer metadata, string name, string? value, string? defaultValue = null)
        {
            value = string.IsNullOrEmpty(value) ? defaultValue : value;
            if (!string.IsNullOrEmpty(value))
                metadata.Add(new XElement(XName.Get(name), value));
        }

        [PublicAPI]
        public class File
        {
            public string Source { get; set; }
            public string Target { get; set; }

            public File(string source, string target)
            {
                Source = source;
                Target = target;
            }

            public File(ITaskItem item) : this(item.ItemSpec, item.GetMetadata("Target"))
            {
            }

            internal XNode ToXml()
            {
                var node = new XElement(XName.Get("file"));
                node.Add(new XAttribute(XName.Get("src"), Source));
                node.Add(new XAttribute(XName.Get("target"), Target));
                return node;
            }
        }

        private static string BoolToStr(bool x) => x.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

#pragma warning restore 1591

        /// <returns>An XML document describing this <c>.nuspec</c> file.</returns>
        public XDocument ToXml()
        {
            var pkg = new XElement(XName.Get("package"));
            var metadata = new XElement(XName.Get("metadata"));

            AddMetadata(metadata, "id", PackageId);
            AddMetadata(metadata, "version", Version, "1.0.0");
            AddMetadata(metadata, "title", Title);
            AddMetadata(metadata, "authors", Authors);
            AddMetadata(metadata, "owners", Owners);
            AddMetadata(metadata, "description", Description);
            AddMetadata(metadata, "releaseNotes", ReleaseNotes);
            AddMetadata(metadata, "summary", Summary);
            AddMetadata(metadata, "projectUrl", ProjectUrl);
            AddMetadata(metadata, "iconUrl", IconUrl);
            if (!string.IsNullOrEmpty(LicenseExpression))
                metadata.Add(new XElement(XName.Get("license"),
                    new XAttribute(XName.Get("type"), "expression"), LicenseExpression));
            else if (!string.IsNullOrEmpty(LicenseFile))
                metadata.Add(new XElement(XName.Get("license"),
                    new XAttribute(XName.Get("type"), "file"), LicenseFile));
            AddMetadata(metadata, "copyright", Copyright);
            AddMetadata(metadata, "requireLicenseAcceptance",
                BoolToStr(RequireLicenseAcceptance));
            AddMetadata(metadata, "tags", Tags);
            AddMetadata(metadata, "developmentDependency",
                BoolToStr(DevelopmentDependency));
            pkg.Add(metadata);

            pkg.Add(new XElement(XName.Get("files"), Files.Select(x => x.ToXml())));

            return new XDocument(pkg);
        }
    }
}
