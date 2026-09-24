using CaMine.Presenters;
using CaMine.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace CaMine
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string minecraftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft-launcher-amigos");
            Directory.CreateDirectory(minecraftPath);

            var path = new CmlLib.Core.MinecraftPath(minecraftPath);
            var minecraftService = new MinecraftService(minecraftPath);
            var versionChecker = new VersionChecker(path);

            var form = new Form1();
            var presenter = new MainPresenter(form, minecraftService, versionChecker);
            form.SetPresenter(presenter);

            Application.Run(form);
        }
    }
}
