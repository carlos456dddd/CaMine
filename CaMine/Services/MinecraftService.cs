using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CaMine.Services
{
    public class MinecraftService : IMinecraftService
    {
        private readonly MinecraftLauncher _launcher;
        private readonly ForgeInstaller _forgeInstaller;
        private readonly string _minecraftPath;

        public MinecraftService(string minecraftPath)
        {
            _minecraftPath = minecraftPath;
            var path = new MinecraftPath(minecraftPath);
            _launcher = new MinecraftLauncher(path);
            _forgeInstaller = new ForgeInstaller(_launcher);
        }

        public MinecraftLauncher Launcher => _launcher;

        public List<string> GetInstalledVersions()
        {
            string versionsPath = Path.Combine(_minecraftPath, "versions");

            if (!Directory.Exists(versionsPath))
                return new List<string>();

            return Directory.GetDirectories(versionsPath)
                .Select(Path.GetFileName)
                .ToList();
        }

      public async Task<List<string>> GetOnlineVersionsAsync()
       {
          var versions = await _launcher.GetAllVersionsAsync();
          return versions.Select(v => v.Name).ToList();
       }

        public async Task<string> InstallForgeAsync(string versionId,
            IProgress<InstallerProgressChangedEventArgs> fileProgress,
            IProgress<ByteProgress> byteProgress)
        {
            var versionName = await _forgeInstaller.Install(versionId, new ForgeInstallOptions
            {
                FileProgress = fileProgress,
                ByteProgress = byteProgress,
                InstallerOutput = new Progress<string>(ev => System.Diagnostics.Debug.WriteLine(ev)),
            });

            await _launcher.InstallAsync(versionName, fileProgress, byteProgress);
            return versionName;
        }

        public async Task InstallVanillaAsync(string versionId,
            IProgress<InstallerProgressChangedEventArgs> fileProgress,
            IProgress<ByteProgress> byteProgress)
        {
            await _launcher.InstallAsync(versionId, fileProgress, byteProgress);
        }

        public async Task<System.Diagnostics.Process> BuildProcessAsync(string versionId, string userName, int ramMb)
        {
            var launchOption = new MLaunchOption
            {
                Session = MSession.CreateOfflineSession(userName),
                MaximumRamMb = ramMb,
            };

            var process = await _launcher.BuildProcessAsync(versionId, launchOption);

            var processWrapper = new ProcessWrapper(process);
            processWrapper.OutputReceived += (s, log) => System.Diagnostics.Debug.WriteLine($"[Game] {log}");
            processWrapper.StartWithEvents();

            return process;
        }
    }
}
