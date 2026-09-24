using System;
using System.Collections.Generic;
using System.Drawing;

namespace CaMine.Views
{
    public interface IMinecraftLauncherView
    {
        string SelectedVersion { get; }
        string UserName { get; }
        int RamMb { get; }
        bool ForgeEnabled { get; }

        void SetVersions(IList<string> versions);
        void SetLastVersionSelected();
        void SetStatus(string text, Color color);
        void ShowProgress();
        void HideProgress();
        void SetProgress(int value, int maximum);
        void SetPlayEnabled(bool enabled);
        void SetPlayText(string text);
        void InvalidateVersionList();
        void ShowMessage(string text);
        void SetForgeColumnVisible(bool visible);
        Color ColorInstalled { get; }
        Color ColorNotInstalled { get; }

        event EventHandler LoadRequested;
        event EventHandler PlayRequested;
        event EventHandler SelectedVersionChanged;
        event EventHandler ForgeToggled;
    }
}
