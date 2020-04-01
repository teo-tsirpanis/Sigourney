using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sigourney
{
    /// <summary>
    /// An MSBuild task that creates a <c>.nuspec</c> file
    /// for Sigourney's publish mode. It is not intended for
    /// general use.
    /// </summary>
    [PublicAPI]
    public class SigourneyWriteNuspecTask: Task
    {

#pragma warning disable 1591

        [Required]
        public string? PackageId { get; set; }
        [Required]
        public string? Version { get; set; }
        public string? Title { get; set; }
        [Required]
        public string? Authors { get; set; }
        public string? Owners { get; set; }
        [Required]
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

        [Required]
        public ITaskItem[]? Files { get; set; }
        [Required]
        public string? Destination { get; set; }

#pragma warning restore 1591

        private void DoExecute()
        {
            var nuspec = new NuspecFile
            {
                Authors = Authors,
                Copyright = Copyright,
                Description = Description,
                DevelopmentDependency = DevelopmentDependency,
                IconUrl = IconUrl,
                LicenseExpression = LicenseExpression,
                LicenseFile = LicenseFile,
                Owners = Owners,
                PackageId = PackageId,
                ProjectUrl = ProjectUrl,
                ReleaseNotes = ReleaseNotes,
                RepositoryType = RepositoryType,
                RepositoryUrl = RepositoryUrl,
                RequireLicenseAcceptance = RequireLicenseAcceptance,
                Summary = Summary,
                Tags = Tags,
                Title = Title,
                Version = Version
            };
            foreach (var f in Files.Select(x => new NuspecFile.File(x)))
                nuspec.Files.Add(f);

            Log.LogMessage("Writing .nuspec file to {0}...", Destination);
            nuspec.ToXml().Save(Destination);
        }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            try
            {
                DoExecute();
                return !Log.HasLoggedErrors;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }
    }
}
