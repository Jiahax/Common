namespace Microsoft.OpenPublishing.Build.Common.Files
{
    using System;
    using System.IO;

    public class SelfCleaningFolder : IDisposable
    {
        public string FullPath { get; }

        public SelfCleaningFolder(string path)
        {
            Guard.ArgumentNotNullOrEmpty(path, nameof(path));

            path = Path.GetFullPath(path);
            Guard.Argument(() => !Directory.Exists(path), nameof(path), $"Directory already exists. Full path: {path}");

            Directory.CreateDirectory(path);
            FullPath = path;
        }

        public void Dispose()
        {
            if (Directory.Exists(FullPath))
            {
                FolderUtility.ForceDeleteDirectoryWithAllSubDirectories(FullPath);
            }
        }
    }
}
