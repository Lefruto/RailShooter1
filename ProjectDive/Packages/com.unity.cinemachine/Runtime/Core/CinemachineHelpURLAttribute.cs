using UnityEngine;
using System.Diagnostics;

namespace Unity.Cinemachine
{
    /// <summary>This must be used like
    /// [CinemachineHelpURL("api/some-page")]
    /// or
    /// [CinemachineHelpURL("manual/some-page.html")]
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    class CinemachineHelpURLAttribute : HelpURLAttribute
    {
        const string k_PackageName = "com.unity.cinemachine";
        const string k_VersionedUrlFormat = "https://docs.unity3d.com/Packages/{0}@{1}/{2}.html";
        const string k_LatestUrlFormat = "https://docs.unity3d.com/Packages/{0}@latest/index.html?subfolder=/{1}.html";

        static readonly string k_PackageVersion;

        static CinemachineHelpURLAttribute()
        {
            k_PackageVersion = string.Empty;
#if UNITY_EDITOR
            var packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForPackageName(k_PackageName);
            if (packageInfo != null)
            {
                // Extract Major.Minor from version (e.g., "6.5" from "6.5.0")
                var version = packageInfo.version;
                var lastDot = version.LastIndexOf('.');
                if (lastDot > 0)
                    k_PackageVersion = version.Substring(0, lastDot);
            }
#endif
        }

        public CinemachineHelpURLAttribute(string url)
            : base(HelpURL(url))
        {
        }

        public static string HelpURL(string pageName)
        {
            if (!string.IsNullOrEmpty(k_PackageVersion))
                return string.Format(k_VersionedUrlFormat, k_PackageName, k_PackageVersion, pageName);

            // Fallback to @latest URL format
            return string.Format(k_LatestUrlFormat, k_PackageName, pageName);
        }
    }
}
