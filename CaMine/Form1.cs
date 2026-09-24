using CaMine.Presenters;
using CaMine.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaMine
{
   
    public partial class Form1 : Form, IMinecraftLauncherView
    {
        private MainPresenter _presenter;

        public Form1()
        {
            InitializeComponent();
            this.Opacity = 0;

        }
       

        public void SetPresenter(MainPresenter presenter)
        {
            _presenter = presenter;
        }

        public string SelectedVersion => cmbVersiones.SelectedItem?.ToString();
        public string UserName => txtNombre.Text.Trim();

        public int RamMb
        {
            get
            {
                if (string.IsNullOrWhiteSpace(txtRam.Text)) return 4096;
                if (int.TryParse(txtRam.Text, out int ram)) return ram;
                return -1;
            }
        }

        public bool ForgeEnabled => checkBox1.Checked;
        public Color ColorInstalled => Color.FromArgb(113, 222, 117);
        public Color ColorNotInstalled => Color.FromArgb(240, 87, 65);

        public event EventHandler LoadRequested;
        public event EventHandler PlayRequested;
        public event EventHandler SelectedVersionChanged;
        public event EventHandler ForgeToggled;

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //cmbVersiones.DrawItem += cmbVersiones_DrawItem;
            //tableLayoutPanel2.RowStyles[1].Height = 0;
            //progressBar1.Visible = false;
            //LoadRequested?.Invoke(this, EventArgs.Empty);
        }
        

        private Dictionary<string, bool> _versionInstalledCache = new Dictionary<string, bool>();
        public void SetVersions(IList<string> versions)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetVersions(versions)));
                return;
            }

            cmbVersiones.Items.Clear();
            _versionInstalledCache.Clear();
                

            foreach (var v in versions)
            {

                cmbVersiones.Items.Add(v);
                var status = _presenter.CheckVersion(v);
                _versionInstalledCache[v] = status?.MinecraftInstalled ?? false;
            }
           
        }

        public void SetLastVersionSelected()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(SetLastVersionSelected));
                return;
            }
            if (cmbVersiones.Items.Count > 0)
                cmbVersiones.SelectedIndex = cmbVersiones.Items.Count - 1;
        }

        public void SetStatus(string text, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetStatus(text, color)));
                return;
            }
            label3.Text = text;
            label3.ForeColor = color;
            label3.BackColor = Color.White;
        }

        public void ShowProgress()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowProgress));
                return;
            }
            tableLayoutPanel2.RowStyles[1].Height = 15;
            progressBar1.Visible = true;
        }

        public void HideProgress()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideProgress));
                return;
            }
            tableLayoutPanel2.RowStyles[1].Height = 0;
            progressBar1.Visible = false;
        }

        public void SetProgress(int value, int maximum)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetProgress(value, maximum)));
                return;
            }
            progressBar1.Maximum = Math.Max(maximum, 1);
            progressBar1.Value = Math.Min(value, progressBar1.Maximum);
        }

        public void SetPlayEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetPlayEnabled(enabled)));
                return;
            }
            btnJugar.Enabled = enabled;
        }

        public void SetPlayText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetPlayText(text)));
                return;
            }
            btnJugar.Text = text;
        }

        public void InvalidateVersionList()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(InvalidateVersionList));
                return;
            }
            // Recargar cache
            _versionInstalledCache.Clear();
            for (int i = 0; i < cmbVersiones.Items.Count; i++)
            {
                string v = cmbVersiones.Items[i].ToString();
                var status = _presenter?.CheckVersion(v);
                _versionInstalledCache[v] = status?.MinecraftInstalled ?? false;
            }
            cmbVersiones.Invalidate();
        }

        public void ShowMessage(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowMessage(text)));
                return;
            }
            MessageBox.Show(text);
        }

        public void SetForgeColumnVisible(bool visible)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetForgeColumnVisible(visible)));
                return;
            }
            if (visible)
            {
                tableLayoutPanel1.ColumnStyles[4].Width = 80;
                btnMods.Size = new Size(80, btnMods.Height);
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[4].Width = 0;
                btnMods.Size = new Size(1, btnMods.Height);
            }
        }

        private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string versionId = cmbVersiones.Items[e.Index].ToString();
            bool installed = _versionInstalledCache.ContainsKey(versionId)? _versionInstalledCache[versionId]: false;
            Color fondo = installed ? ColorInstalled : ColorNotInstalled;
            Color texto = Color.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {

                fondo = SystemColors.Highlight;


            }

            using (var brush = new SolidBrush(fondo))
                e.Graphics.FillRectangle(brush, e.Bounds);
            using (var brushTexto = new SolidBrush(texto))
            {
                e.Graphics.DrawString(versionId, e.Font, brushTexto,
                    new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                    StringFormat.GenericDefault);
            }

            // Solo dibujar foco si tiene el foco real (evita parpadeo del rectángulo punteado)
            //if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
              //  e.DrawFocusRectangle();
        }

        private void btnJugar_Click(object sender, EventArgs e)
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        private void cmbVersiones_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedVersionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ForgeToggled?.Invoke(this, EventArgs.Empty);
        }

        private void BtnMods_Click(object sender, EventArgs e)
        {
            var minecraftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft-launcher-amigos");
            string rutaMods = Path.Combine(minecraftPath, "mods");

            if (!Directory.Exists(rutaMods))
                Directory.CreateDirectory(rutaMods);

            var psi = new ProcessStartInfo
            {
                FileName = rutaMods,
                UseShellExecute = true,
                Verb = "open"
            };
            var proceso = Process.Start(psi);
            proceso?.WaitForInputIdle(2000);
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void txtNombre_TextChanged(object sender, EventArgs e) { }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            cmbVersiones.DrawItem += cmbVersiones_DrawItem;
            tableLayoutPanel2.RowStyles[1].Height = 0;
            progressBar1.Visible = false;

            LoadRequested?.Invoke(this, EventArgs.Empty);
            await Task.Delay(500);
            this.Opacity = 1;
        }
    }
}
