using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaMine
{
    public partial class Form1 : Form
    {
        private MinecraftPath path;
        private MinecraftLauncher launcher;
        private ForgeInstaller forgeInstaller;
        private string minecraftPath;

        private readonly Color ColorInstalado = Color.FromArgb(113, 222, 117);
        private readonly Color ColorNoInstalado = Color.FromArgb(240, 87, 65);
        private readonly Color ColorTexto = Color.Black;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            minecraftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft-launcher-amigos");
            Directory.CreateDirectory(minecraftPath);

            path = new MinecraftPath(minecraftPath);
            launcher = new MinecraftLauncher(path);
            forgeInstaller = new ForgeInstaller(launcher);

            cmbVersiones.DrawMode = DrawMode.OwnerDrawFixed;
            cmbVersiones.DrawItem += cmbVersiones_DrawItem;

            // OCULTAR fila del progressBar al inicio (Height = 0)
            tableLayoutPanel2.RowStyles[1].Height = 0;
            progressBar1.Visible = false;

            var versions = await launcher.GetAllVersionsAsync();
            foreach (var version in versions)
            {
                cmbVersiones.Items.Add(version.Name);
            }

            if (cmbVersiones.Items.Count > 0)
                cmbVersiones.SelectedIndex = cmbVersiones.Items.Count - 1;
        }

        // ============================================================
        // MOSTRAR fila de progreso (descarga activa)
        // ============================================================
        private void MostrarFilaProgreso()
        {
            tableLayoutPanel2.RowStyles[1].Height = 15; // o el tamaño que necesites
            progressBar1.Visible = true;
            
        }

        // ============================================================
        // OCULTAR fila de progreso (sin descarga)
        // ============================================================
        private void OcultarFilaProgreso()
        {
            tableLayoutPanel2.RowStyles[1].Height = 0;
            progressBar1.Visible = false;
        }

        // ============================================================
        // ACTUALIZAR progreso con mensaje útil
        // ============================================================
        private void ActualizarProgresoUI(int valor, int maximo, string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ActualizarProgresoUI(valor, maximo, mensaje)));
                return;
            }

            progressBar1.Maximum = Math.Max(maximo, 1);
            progressBar1.Value = Math.Min(valor, progressBar1.Maximum);
        }

        // ... (DrawItem, VersionMinecraftInstalada, ForgeInstalado, ObtenerNombreVersionForge igual)

        private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            string versionId = cmbVersiones.Items[e.Index].ToString();
            bool instalada = VersionMinecraftInstalada(versionId);
            Color fondo = instalada ? ColorInstalado : ColorNoInstalado;
            e.Graphics.FillRectangle(new SolidBrush(fondo), e.Bounds);
            e.Graphics.DrawString(versionId, e.Font, new SolidBrush(ColorTexto),
                e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private bool VersionMinecraftInstalada(string versionId)
        {
            string versionPath = Path.Combine(path.Versions, versionId);
            string versionJson = Path.Combine(versionPath, $"{versionId}.json");
            string versionJar = Path.Combine(versionPath, $"{versionId}.jar");
            return Directory.Exists(versionPath) && File.Exists(versionJson) && File.Exists(versionJar);
        }

        private bool ForgeInstalado(string versionId)
        {
            string versionsDir = path.Versions;
            if (!Directory.Exists(versionsDir)) return false;
            var directorios = Directory.GetDirectories(versionsDir);
            foreach (var dir in directorios)
            {
                string nombreDir = Path.GetFileName(dir);
                if (nombreDir.StartsWith(versionId) && nombreDir.Contains("forge"))
                {
                    string jsonPath = Path.Combine(dir, $"{nombreDir}.json");
                    string jarPath = Path.Combine(dir, $"{nombreDir}.jar");
                    if (File.Exists(jsonPath) && File.Exists(jarPath)) return true;
                }
            }
            return false;
        }

        private string ObtenerNombreVersionForge(string versionId)
        {
            string versionsDir = path.Versions;
            if (!Directory.Exists(versionsDir)) return null;
            var directorios = Directory.GetDirectories(versionsDir);
            foreach (var dir in directorios)
            {
                string nombreDir = Path.GetFileName(dir);
                if (nombreDir.StartsWith(versionId) && nombreDir.Contains("forge"))
                {
                    string jsonPath = Path.Combine(dir, $"{nombreDir}.json");
                    string jarPath = Path.Combine(dir, $"{nombreDir}.jar");
                    if (File.Exists(jsonPath) && File.Exists(jarPath)) return nombreDir;
                }
            }
            return null;
        }

        // ============================================================
        // ACTUALIZAR LABEL3
        // ============================================================
        private void ActualizarEstadoLabel(string versionId)
        {
            label3.BackColor = Color.White;
            if (string.IsNullOrEmpty(versionId))
            {
                label3.Text = "Selecciona una versión";
                label3.ForeColor = Color.Gray;
                return;
            }

            bool minecraftInstalado = VersionMinecraftInstalada(versionId);
            bool forgeInstalado = ForgeInstalado(versionId);

            if (minecraftInstalado)
            {
                if (forgeInstalado && checkBox1.Checked)
                    label3.Text = "Instalado + Forge";
               
                else
                    label3.Text = "Instalado";
                label3.ForeColor = ColorInstalado;
            }
            else
            {
                label3.Text = "Sin existencia";
                label3.ForeColor = ColorNoInstalado;
            }

            label3.Font = new Font("AdwaitaMono Nerd Font", 10.2F,
                FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        // ============================================================
        // BOTÓN JUGAR - ProgressBar SOLO si descarga
        // ============================================================
        private async void btnJugar_Click(object sender, EventArgs e)
        {
            string versionId = cmbVersiones.SelectedItem?.ToString();
            string nombreUsuario = txtNombre.Text.Trim();

            if (string.IsNullOrEmpty(versionId))
            {
                MessageBox.Show("Selecciona una versión");
                return;
            }

            if (string.IsNullOrEmpty(nombreUsuario))
            {
                MessageBox.Show("Escribe tu nombre de usuario");
                return;
            }

            int ramMb = 4096;
            if (!string.IsNullOrEmpty(txtRam.Text))
            {
                if (!int.TryParse(txtRam.Text, out ramMb) || ramMb < 512)
                {
                    MessageBox.Show("Ingresa una cantidad de RAM válida (mínimo 512 MB)");
                    return;
                }
            }

            btnJugar.Enabled = false;
            string versionALanzar = versionId;
            bool necesitaDescarga = false;

            try
            {
                bool minecraftInstalado = VersionMinecraftInstalada(versionId);

                // ═══════════════════════════════════════════════════════
                // CASO: FORGE ACTIVADO
                // ═══════════════════════════════════════════════════════
                if (checkBox1.Checked)
                {

                   
                    bool forgeYaInstalado = ForgeInstalado(versionId);

                    if (!forgeYaInstalado)
                    {
                        necesitaDescarga = true;
                        MostrarFilaProgreso();

                        label3.Text = "Instalando Forge...";
                        label3.ForeColor = Color.Orange;
                        btnJugar.Text = "Descargando...";

                        var fileProgress = new Progress< InstallerProgressChangedEventArgs > (ev =>
                        {
                            // SOLO mensajes útiles, una sola línea
                            string accion = ev.EventType.ToString(); // Download, Extract, Install
                            string archivo = Path.GetFileName(ev.Name) ?? ev.Name;

                            // Acortar si es muy largo
                            if (archivo.Length > 35)
                                archivo = archivo.Substring(0, 32) + "...";

                            string mensaje = $"{accion}: {archivo}";

                            ActualizarProgresoUI(ev.ProgressedTasks, ev.TotalTasks, mensaje);
                            Console.WriteLine($"[{ev.EventType}] {ev.ProgressedTasks}/{ev.TotalTasks} {ev.Name}");
                        });

                        var byteProgress = new Progress<ByteProgress>(ev =>
                        {
                            Console.WriteLine($"{ev.ToRatio() * 100:F1}%");
                        });

                        versionALanzar = await forgeInstaller.Install(versionId, new ForgeInstallOptions
                        {
                            FileProgress = fileProgress,
                            ByteProgress = byteProgress,
                            InstallerOutput = new Progress<string>(ev => Console.WriteLine(ev)),
                        });

                        ActualizarProgresoUI(1, 1, "Finalizando Forge...");
                        await launcher.InstallAsync(versionALanzar, fileProgress, byteProgress);
                    }
                    else
                    {
                        versionALanzar = ObtenerNombreVersionForge(versionId);
                        // Forge ya existe, NO mostrar ProgressBar
                    }
                }
                // ═══════════════════════════════════════════════════════
                // CASO: VANILLA
                // ═══════════════════════════════════════════════════════
                else
                {
                    tableLayoutPanel1.ColumnStyles[4].Width = 0;

                    if (!minecraftInstalado)
                    {
                        necesitaDescarga = true;
                        MostrarFilaProgreso();

                        label3.Text = "Instalando...";
                        label3.ForeColor = Color.Orange;
                        btnJugar.Text = "Descargando...";

                        var fileProgress = new Progress< InstallerProgressChangedEventArgs > (ev =>
                        {
                            string accion = ev.EventType.ToString();
                            string archivo = Path.GetFileName(ev.Name) ?? ev.Name;

                            if (archivo.Length > 35)
                                archivo = archivo.Substring(0, 32) + "...";

                            string mensaje = $"{accion}: {archivo}";

                            ActualizarProgresoUI(ev.ProgressedTasks, ev.TotalTasks, mensaje);
                        });

                        var byteProgress = new Progress<ByteProgress>(ev =>
                        {
                            Console.WriteLine($"{ev.ToRatio() * 100:F1}%");
                        });

                        await launcher.InstallAsync(versionId, fileProgress, byteProgress);
                    }
                    else
                    {
                        // Ya instalado, NO mostrar ProgressBar
                        btnJugar.Text = "Versión lista ✓";
                        await Task.Delay(500);
                    }
                }

                // Lanzar
                if (!necesitaDescarga)
                {
                    btnJugar.Text = "Lanzando...";
                }
                else
                {
                    ActualizarProgresoUI(1, 1, "Listo!");
                    await Task.Delay(300);
                }

                cmbVersiones.Invalidate();
                ActualizarEstadoLabel(versionId);

                btnJugar.Text = "Lanzando...";

                var launchOption = new MLaunchOption
                {
                    Session = MSession.CreateOfflineSession(nombreUsuario),
                    MaximumRamMb = ramMb,
                };

                var process = await launcher.BuildProcessAsync(versionALanzar, launchOption);

                var processWrapper = new ProcessWrapper(process);
                processWrapper.OutputReceived += (s, log) => Console.WriteLine($"[Game] {log}");
                processWrapper.StartWithEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                label3.Text = "Error en instalación";
                label3.ForeColor = Color.DarkRed;
            }
            finally
            {
                btnJugar.Text = "Jugar";
                btnJugar.Enabled = true;
                OcultarFilaProgreso();
            }
        }

        private void cmbVersiones_SelectedIndexChanged(object sender, EventArgs e)
        {
            string versionId = cmbVersiones.SelectedItem?.ToString();
            ActualizarEstadoLabel(versionId);
        }
      
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string versionId = cmbVersiones.SelectedItem?.ToString();

            if (checkBox1.Checked)
            {
                tableLayoutPanel1.ColumnStyles[4].Width = 80;
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[4].Width = 0;
            }
            ActualizarEstadoLabel(versionId);
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void txtNombre_TextChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }


        private void Abrir_Carpeta() {

            string rutaMods = Path.Combine(minecraftPath, "mods");

            if (!Directory.Exists(rutaMods))
                Directory.CreateDirectory(rutaMods);

            var psi = new ProcessStartInfo()
            {
                FileName = rutaMods,  // Abrir directamente la carpeta
                UseShellExecute = true,
                Verb = "open"
            };

            var proceso = Process.Start(psi);

            if (proceso != null)
            {
                proceso.WaitForInputIdle(2000);
            }
        }
        private void BtnMods_Click(object sender, System.EventArgs e)
        {
            Abrir_Carpeta();
        }
    }
}