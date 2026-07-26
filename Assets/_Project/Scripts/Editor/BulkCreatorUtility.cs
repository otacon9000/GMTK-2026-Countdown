using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace GmtkCountdown.Editor
{
    /// <summary>
    /// Shared helpers for the bulk content-generation editor tools (TaskDataBulkCreator,
    /// ContentBulkCreator): filename sanitization and collision-safe asset path resolution.
    /// </summary>
    internal static class BulkCreatorUtility
    {
        public static string SanitizeName(string source, int maxLength)
        {
            var builder = new StringBuilder();
            foreach (char c in source)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    builder.Append('_');
                }
            }

            string sanitized = builder.ToString();
            while (sanitized.Contains("__"))
            {
                sanitized = sanitized.Replace("__", "_");
            }

            sanitized = sanitized.Trim('_');

            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength).TrimEnd('_');
            }

            return sanitized;
        }

        /// <summary>
        /// Builds an asset path from folder/baseFileName.asset, appending a numeric suffix if
        /// another entry generated in this same run already claimed that path (two different
        /// source strings sanitizing to the same base name). Does not consult the AssetDatabase —
        /// callers are responsible for deciding whether a path that already exists on disk should
        /// be skipped instead of (re)created.
        /// </summary>
        public static string ResolveUniquePath(string folder, string baseFileName, HashSet<string> pathsUsedThisRun)
        {
            string path = $"{folder}/{baseFileName}.asset";
            if (pathsUsedThisRun.Add(path))
            {
                return path;
            }

            int suffix = 2;
            string candidate;
            do
            {
                candidate = $"{folder}/{baseFileName}_{suffix}.asset";
                suffix++;
            } while (!pathsUsedThisRun.Add(candidate));

            return candidate;
        }

        public static void EnsureFolder(string parentFolder, string folderName)
        {
            string fullPath = $"{parentFolder}/{folderName}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
    }
}
