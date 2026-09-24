using CaMine.Models;
using CaMine.Services;
using CaMine.Views;
using CmlLib.Core;
using CmlLib.Core.Installers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CaMine.Presenters
{
    public class MainPresenter
    {
        private readonly IMinecraftLauncherView _view;
        private readonly IMinecraftService _minecraftService;
        private readonly IVersionChecker _versionChecker;
        public event Action VersionsLoaded;



        public MainPresenter(IMinecraftLauncherView view, IMinecraftService minecraftService, IVersionChecker versionChecker)
        {
            _view = view;
            _minecraftService = minecraftService;
            _versionChecker = versionChecker;

            _view.LoadRequested += OnLoad;
            _view.PlayRequested += OnPlay;
            
            _view.SelectedVersionChanged += OnVersionChanged;
            _view.ForgeToggled += OnForgeToggled;
        }

        public VersionStatus CheckVersion(string versionId)
        {
            return _versionChecker.CheckVersion(versionId);
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            try
            {
                // Mostrar instantáneamente las instaladas
                var installed = _minecraftService.GetInstalledVersions();

                if (installed.Count > 0)
                {
                    _view.SetVersions(installed);
                    _view.SetLastVersionSelected();
                }

                _view.SetForgeColumnVisible(false);
                var installedSet = new HashSet<string>(installed);
                // Cargar online en segundo plano
           var online = await _minecraftService.GetOnlineVersionsAsync();
            var allVersions = installed
                .Union(online)
                .OrderByDescending(v => installedSet.Contains(v)) // instalados primero
                .ThenByDescending(v => v)                         // luego ordenar versiones
            .ToList();


                _view.SetVersions(allVersions);
                _view.SetLastVersionSelected();
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Error al cargar versiones: " + ex.Message);
            }
        }

        private void OnVersionChanged(object sender, EventArgs e)
        {
            string versionId = _view.SelectedVersion;
            UpdateStatus(versionId);
        }

        private void OnForgeToggled(object sender, EventArgs e)
        {
            
            _view.SetForgeColumnVisible(_view.ForgeEnabled);
            string versionId = _view.SelectedVersion;
            if (!string.IsNullOrEmpty(versionId))
                UpdateStatus(versionId);

            VersionsLoaded?.Invoke();
        }

        private void UpdateStatus(string versionId)
        {
            if (string.IsNullOrEmpty(versionId))
            {
                _view.SetStatus("Selecciona una versi\u00f3n", Color.Gray);
                return;
            }

            var status = _versionChecker.CheckVersion(versionId);

            if (status.MinecraftInstalled)
            {
                if (status.ForgeInstalled && _view.ForgeEnabled)
                    _view.SetStatus("Instalado + Forge", _view.ColorInstalled);
                else
                    _view.SetStatus("Instalado", _view.ColorInstalled);
            }
            else
            {
                _view.SetStatus("Sin existencia", _view.ColorNotInstalled);
            }
        }

        private async void OnPlay(object sender, EventArgs e)
        {
            string versionId = _view.SelectedVersion;
            string userName = _view.UserName;

            if (string.IsNullOrEmpty(versionId))
            {
                _view.ShowMessage("Selecciona una versi\u00f3n");
                return;
            }

            if (string.IsNullOrEmpty(userName))
            {
                _view.ShowMessage("Escribe tu nombre de usuario");
                return;
            }

            int ramMb = _view.RamMb;
            if (ramMb < 512)
            {
                _view.ShowMessage("Ingresa una cantidad de RAM v\u00e1lida (m\u00ednimo 512 MB)");
                return;
            }

            _view.SetPlayEnabled(false);

            try
            {
                var status = _versionChecker.CheckVersion(versionId);
                string versionToLaunch = versionId;

                if (_view.ForgeEnabled)
                {
                    if (!status.ForgeInstalled)
                    {
                        _view.ShowProgress();
                        _view.SetStatus("Instalando Forge...", Color.Orange);
                        _view.SetPlayText("Descargando...");

                        var fileProgress = new Progress<InstallerProgressChangedEventArgs>(ev =>
                        {
                            string fileName = Path.GetFileName(ev.Name) ?? ev.Name;
                            if (fileName.Length > 35)
                                fileName = fileName.Substring(0, 32) + "...";
                            _view.SetProgress(ev.ProgressedTasks, ev.TotalTasks);
                        });

                        var byteProgress = new Progress<ByteProgress>(ev =>
                        {
                            System.Diagnostics.Debug.WriteLine($"{ev.ToRatio() * 100:F1}%");
                        });

                        versionToLaunch = await _minecraftService.InstallForgeAsync(versionId, fileProgress, byteProgress);
                    }
                    else
                    {
                        versionToLaunch = status.ForgeVersionName;
                    }
                }
                else
                {
                    if (!status.MinecraftInstalled)
                    {
                        _view.ShowProgress();
                        _view.SetStatus("Instalando...", Color.Orange);
                        _view.SetPlayText("Descargando...");

                        var fileProgress = new Progress<InstallerProgressChangedEventArgs>(ev =>
                        {
                            string fileName = Path.GetFileName(ev.Name) ?? ev.Name;
                            if (fileName.Length > 35)
                                fileName = fileName.Substring(0, 32) + "...";
                            _view.SetProgress(ev.ProgressedTasks, ev.TotalTasks);
                        });

                        var byteProgress = new Progress<ByteProgress>(ev =>
                        {
                            System.Diagnostics.Debug.WriteLine($"{ev.ToRatio() * 100:F1}%");
                        });

                        await _minecraftService.InstallVanillaAsync(versionId, fileProgress, byteProgress);
                    }
                }
                 _view.InvalidateVersionList();
                UpdateStatus(versionId);
                _view.SetPlayText("Lanzando...");

                await _minecraftService.BuildProcessAsync(versionToLaunch, userName, ramMb);
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Error: " + ex.Message);
                _view.SetStatus("Error en instalaci\u00f3n", Color.DarkRed);
            }
            finally
            {
                _view.SetPlayEnabled(true);
                _view.SetPlayText("Jugar");
                _view.HideProgress();
            }
        }
    }
}
