using System;
using UnityEditor;

namespace Cc83.Utils
{
    public static class PathUtils
    {
        public static string SelectAssetsPath(string filePath)
        {
            var index = filePath.IndexOf("Assets", StringComparison.Ordinal);
            if (index == -1)
            {
                EditorUtility.DisplayDialog("Path Error", $"Invalid asset path at {filePath}", "Close");
                return null;
            }

            return filePath[index..];
        }
    }
}
