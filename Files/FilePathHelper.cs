namespace Common.Files
{
    using System;
    using System.IO;
    using System.Linq;

    public static class FilePathHelper
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] AdditionalInvalidPathChars = new char[] { '*' };

        /// <summary>
        /// Validate absolute path. Check whether it includes any invalid char
        /// </summary>
        /// <param name="path">Absolute path</param>
        public static void ValidatePath(string path)
        {
            Guard.ArgumentNotNullOrEmpty(path, nameof(path));

            CheckInvalidCharInPath(path, InvalidPathChars);
            var fileName = Path.GetFileName(path);
            CheckInvalidCharInPath(fileName, InvalidFileNameChars);
            CheckInvalidCharInPath(path, AdditionalInvalidPathChars);
        }

        /// <summary>
        /// Return path to output parent folder
        /// </summary>
        /// <param name="pathUnderSubfolder">relative file path under sub folder</param>
        /// <param name="subfolderToOutputFolder">relative folder path under output folder</param>
        /// <returns>relative path to output folder</returns>
        public static string ToOutputFolderRelativePath(this string pathUnderSubfolder, string subfolderToOutputFolder)
        {
            Guard.ArgumentNotNullOrEmpty(pathUnderSubfolder, nameof(pathUnderSubfolder));
            Guard.ArgumentNotNullOrEmpty(subfolderToOutputFolder, nameof(subfolderToOutputFolder));

            return CombineRelativePathsAndNormalize(subfolderToOutputFolder, pathUnderSubfolder);
        }

        /// <summary>
        /// Return pathToConvert to basePath's relative path
        /// </summary>
        /// <param name="pathToConvert">Path need to be relative</param>
        /// <param name="basePath">Base Path</param>
        /// <param name="uriKind">indicate what kind of paths you want to make to relative</param>
        /// <returns>A relative path based on base path</returns>
        public static string ToRelativePath(this string pathToConvert, string basePath, UriKind uriKind)
        {
            Guard.ArgumentNotNullOrEmpty(pathToConvert, nameof(pathToConvert));
            Guard.ArgumentNotNullOrEmpty(basePath, nameof(basePath));

            if (uriKind == UriKind.Relative)
            {
                var normalizedPathToConvert = NormalizeRelativePath(pathToConvert);
                var normalizedReferencePath = NormalizeRelativePath(basePath);
                if (!normalizedPathToConvert.StartsWith(normalizedReferencePath))
                {
                    throw new InvalidRelativePathException($"Path '{pathToConvert}' is not a subfolder of reference path '{basePath}'.", pathToConvert);
                }

                // Empty means that the base path is currently folder symbol ".", after normalize it become to strimg.Empty.
                return normalizedReferencePath == string.Empty ? normalizedPathToConvert : normalizedPathToConvert.Substring(normalizedReferencePath.TrimEnd(Path.AltDirectorySeparatorChar).Length + 1);
            }
            else if (uriKind == UriKind.Absolute)
            {
                Uri pathToConvertUri = new Uri(pathToConvert);
                Uri basePathUri = new Uri(basePath);
                Uri relativePathToBaseUri = basePathUri.MakeRelativeUri(pathToConvertUri);
                return relativePathToBaseUri.OriginalString.Length == 0 ? Path.GetFileName(pathToConvert) : relativePathToBaseUri.OriginalString;
            }
            else
            {
                throw new NotSupportedException($"UriKind: {uriKind} doesn't support");
            }
        }

        public static string NormalizeFilePathToRelative(string path)
        {
            if (path == null)
            {
                return null;
            }
            var normalizedPath = path.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return string.IsNullOrEmpty(normalizedPath) ? "." : normalizedPath;
        }

        public static bool IsFilePathEquals(string filePath1, string filePath2) => string.Equals(filePath1.BackSlashToForwardSlash(), filePath2.BackSlashToForwardSlash(), StringComparison.OrdinalIgnoreCase);

        #region Methods should be used with FileAccessor

        /// <summary>
        /// Validate relative path. Check whether it includes any invalid char
        /// Also verify the relative path that can't start above the repository root.
        /// </summary>
        /// <param name="relativePath">Path relative to the git repository</param>
        public static void ValidateRelativePath(string relativePath)
        {
            ValidatePath(relativePath);

            if (Path.IsPathRooted(relativePath))
            {
                throw new InvalidRelativePathException($"relativePath: '{relativePath}' can't be a rooted path", relativePath);
            }

            var mockPrefix = GenerateMockPrefix();
            var normalizedPath = Path.GetFullPath(Path.Combine(mockPrefix, relativePath));
            if (!normalizedPath.StartsWith(mockPrefix))
            {
                throw new InvalidRelativePathException($"relativePath: '{relativePath}' is above the root path", relativePath);
            }
        }

        /// <summary>
        /// Normalize relative path, remove all ".", ".." to normal relative path
        /// </summary>
        /// <param name="relativePath">Path relative to the git repository</param>
        /// <returns>Return normalized path without ".", ".."</returns>
        public static string NormalizeRelativePath(string relativePath)
        {
            try
            {
                ValidateRelativePath(relativePath);
                var mockPrefix = GenerateMockPrefix();
                var fullPath = Path.GetFullPath(Path.Combine(mockPrefix, relativePath));
                var normalizedRelativePath = fullPath.Length > mockPrefix.Length ? fullPath.Substring(mockPrefix.Length + 1) : string.Empty;
                return normalizedRelativePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidRelativePathException(ae.Message, relativePath, ae);
            }
        }

        /// <summary>
        /// Combine relative path to a new relative path which is relative to the repository
        /// </summary>
        /// <param name="relativePaths">Relative path</param>
        /// <returns>Path relative to the git repository</returns>
        public static string CombineRelativePathsAndNormalize(params string[] relativePaths)
        {
            try
            {
                Guard.ArgumentNotNull(relativePaths, nameof(relativePaths));
                Guard.Argument(() => relativePaths.All(p => p != null), nameof(relativePaths), $"{nameof(relativePaths)} can't contain any null value");

                var combinedRelativePath = Path.Combine(relativePaths);
                return NormalizeRelativePath(combinedRelativePath);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidRelativePathException(ae.Message, string.Join(",", relativePaths), ae);
            }
        }

        #endregion

        #region Private Methods

        private static void CheckInvalidCharInPath(string path, char[] invalidChars)
        {
            var invalidCharIndex = path.IndexOfAny(invalidChars);
            if (invalidCharIndex != -1)
            {
                throw new ArgumentException($"path: '{path}' contains invalid path char: {path[invalidCharIndex]}");
            }
        }

        private static string GenerateMockPrefix() => Path.Combine("c:\\", Guid.NewGuid().ToString("N").Substring(0, 8));

        #endregion
    }
}
