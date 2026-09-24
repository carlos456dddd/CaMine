using CaMine.Models;

namespace CaMine.Services
{
    public interface IVersionChecker
    {
        VersionStatus CheckVersion(string versionId);
    }
}
