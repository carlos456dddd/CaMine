using CmlLib.Core;
using System.IO;
using CaMine.Models;

namespace CaMine.Services
{
    public class VersionChecker : IVersionChecker
    {
        private readonly MinecraftPath _path;

        public VersionChecker(MinecraftPath path)
        {
            _path = path;
        }

        public VersionStatus CheckVersion(string versionId)
        {
            bool minecraftInstalled = IsMinecraftInstalled(versionId);
            string forgeVersionName = GetForgeVersionName(versionId);
            bool forgeInstalled = forgeVersionName != null;

            return new VersionStatus
            {
                MinecraftInstalled = minecraftInstalled,
                ForgeInstalled = forgeInstalled,
                ForgeVersionName = forgeVersionName
            };
        }

        private bool IsMinecraftInstalled(string versionId)
        {
            string versionPath = Path.Combine(_path.Versions, versionId);
            string versionJson = Path.Combine(versionPath, $"{versionId}.json");
            string versionJar = Path.Combine(versionPath, $"{versionId}.jar");
            return Directory.Exists(versionPath) && File.Exists(versionJson) && File.Exists(versionJar);
        }

        private string GetForgeVersionName(string versionId)
        {
            string versionsDir = _path.Versions;
            if (!Directory.Exists(versionsDir)) return null;

            foreach (var dir in Directory.GetDirectories(versionsDir))
            {
                string nombreDir = Path.GetFileName(dir);
                if (nombreDir.StartsWith(versionId) && nombreDir.Contains("forge"))
                {
                    string jsonPath = Path.Combine(dir, $"{nombreDir}.json");
                    string jarPath = Path.Combine(dir, $"{nombreDir}.jar");
                    if (File.Exists(jsonPath) && File.Exists(jarPath))
                        return nombreDir;
                }
            }
            return null;
        }
    }
}
