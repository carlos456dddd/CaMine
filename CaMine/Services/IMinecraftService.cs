using CmlLib.Core;
using CmlLib.Core.Installers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaMine.Services
{
    public interface IMinecraftService
    {
        List<string> GetInstalledVersions();
        Task<List<string>> GetOnlineVersionsAsync();

        Task<string> InstallForgeAsync(string versionId,
            IProgress<InstallerProgressChangedEventArgs> fileProgress,
            IProgress<ByteProgress> byteProgress);
        Task InstallVanillaAsync(string versionId,
            IProgress<InstallerProgressChangedEventArgs> fileProgress,
            IProgress<ByteProgress> byteProgress);
        Task<System.Diagnostics.Process> BuildProcessAsync(string versionId, string userName, int ramMb);
    }
}
