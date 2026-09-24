🔴 Problema 1: No pintas el fondo cuando el item está seleccionado
En tu DrawItem, cuando el usuario pasa el mouse o selecciona un item, WinForms espera que tú pintes el estado de selección. Si no lo haces, queda el fondo blanco del sistema por un instante → parpadeo blanco.
csharp
// ❌ TU CÓDIGO ACTUAL - Solo pinta tu color, ignora el estado de selección
private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
{
    if (e.Index < 0) return;
    string versionId = cmbVersiones.Items[e.Index].ToString();
    var status = _presenter?.CheckVersion(versionId);
    bool installed = status?.MinecraftInstalled ?? false;
    Color fondo = installed ? ColorInstalled : ColorNotInstalled;

    using (var brush = new SolidBrush(fondo))
        e.Graphics.FillRectangle(brush, e.Bounds);  // Solo tu color

    e.Graphics.DrawString(versionId, e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
    e.DrawFocusRectangle();  // Esto solo dibuja el borde punteado, no el fondo
}
Solución: Detectar e.State y pintar el highlight de selección antes que tu color:
csharp
private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
{
    if (e.Index < 0) return;

    string versionId = cmbVersiones.Items[e.Index].ToString();
    var status = _presenter?.CheckVersion(versionId);
    bool installed = status?.MinecraftInstalled ?? false;
    
    // Color base según instalación
    Color fondo = installed ? ColorInstalled : ColorNotInstalled;
    Color textoColor = Color.Black;

    // 🔴 CRÍTICO: Si está seleccionado o bajo el mouse, mezclar con el color de selección
    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
        (e.State & DrawItemState.HotLight) == DrawItemState.HotLight)
    {
        // Opción A: Usar color de selección del sistema (más estándar)
        // fondo = Color.FromArgb(200, SystemColors.Highlight);
        
        // Opción B: Oscurecer tu color para indicar selección
        fondo = ControlPaint.Dark(fondo, 0.9f);
        textoColor = Color.White;
    }

    // Pintar TODO el rectángulo del fondo
    using (var brush = new SolidBrush(fondo))
        e.Graphics.FillRectangle(brush, e.Bounds);

    // Dibujar el texto
    using (var brushTexto = new SolidBrush(textoColor))
    {
        e.Graphics.DrawString(versionId, e.Font, brushTexto, 
            new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
            StringFormat.GenericDefault);
    }

    // Solo dibujar foco si tiene el foco real (evita parpadeo del rectángulo punteado)
    if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        e.DrawFocusRectangle();
}
🔴 Problema 2: CheckVersion() se llama en CADA repintado
En tu DrawItem haces esto:
csharp
var status = _presenter?.CheckVersion(versionId);
Esto se ejecuta cada vez que se mueve el mouse, se hace scroll, o se redibuja. Si CheckVersion() hace cualquier operación no trivial (consultar disco, diccionario con lock, etc.), causa parpadeo por bloqueo del hilo UI.
Solución: Cachear los estados:
csharp
// Añade un diccionario privado en tu clase Form1
private Dictionary<string, bool> _versionInstalledCache = new Dictionary<string, bool>();

// Modifica SetVersions para precargar los estados
public void SetVersions(IList<string> versions)
{
    if (InvokeRequired)
    {
        BeginInvoke(new Action(() => SetVersions(versions)));
        return;
    }
    cmbVersiones.Items.Clear();
    _versionInstalledCache.Clear(); // Limpiar cache
    
    foreach (var v in versions)
    {
        cmbVersiones.Items.Add(v);
        // Precargar estado aquí, una sola vez
        var status = _presenter?.CheckVersion(v);
        _versionInstalledCache[v] = status?.MinecraftInstalled ?? false;
    }
}

// En DrawItem, usa el cache en lugar de llamar al presenter
private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
{
    if (e.Index < 0) return;
    
    string versionId = cmbVersiones.Items[e.Index].ToString();
    bool installed = _versionInstalledCache.ContainsKey(versionId) 
        ? _versionInstalledCache[versionId] 
        : false;
    
    // ... resto del código
}
Y cuando cambie el estado de una versión (instalar/desinstalar), invalidas y recargas:
csharp
// Cuando el presenter notifique que cambió el estado de una versión
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
🟡 Problema 3: InvalidateVersionList() solo hace Invalidate()
Tu método actual:
csharp
public void InvalidateVersionList()
{
    // ...
    cmbVersiones.Invalidate();  // Solo invalida, no recarga el cache
}
Si el presenter llama a esto frecuentemente, fuerza repintados sin necesidad. Asegúrate de que no se llame en un bucle o evento de mouse.
🟡 Problema 4: Posible problema con tableLayoutPanel
Veo que usas tableLayoutPanel y modificas alturas dinámicamente (RowStyles[1].Height = 0). Si el ComboBox está dentro de un TableLayoutPanel sin doble buffer, puede heredar parpadeo.
Añade en el constructor:
csharp
public Form1()
{
    InitializeComponent();
    this.DoubleBuffered = true;
    this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint, true);
            
    // 🔴 Si tus TableLayoutPanel no tienen DoubleBuffered, añádeselo
    tableLayoutPanel1.DoubleBuffered = true;  // Necesitarás extender la clase si no es pública
    tableLayoutPanel2.DoubleBuffered = true;
}
Para hacer DoubleBuffered público en TableLayoutPanel, crea una clase simple:
csharp
public class TableLayoutPanelBuffered : TableLayoutPanel
{
    public TableLayoutPanelBuffered()
    {
        this.DoubleBuffered = true;
    }
}
Y reemplaza tus TableLayoutPanel en el diseñador por TableLayoutPanelBuffered.
🟢 Problema 5 (menor): e.DrawFocusRectangle() siempre
En tu código actual dibujas el rectángulo de foco siempre, incluso cuando no tiene foco. Esto puede causar parpadeo visual del borde punteado.
Código corregido completo para DrawItem
csharp
private void cmbVersiones_DrawItem(object sender, DrawItemEventArgs e)
{
    if (e.Index < 0) return;

    string versionId = cmbVersiones.Items[e.Index].ToString();
    bool installed = _versionInstalledCache.ContainsKey(versionId) 
        ? _versionInstalledCache[versionId] 
        : false;

    Color fondo = installed ? ColorInstalled : ColorNotInstalled;
    Color textoColor = Color.Black;

    // Detectar selección o hover
    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
    bool isHot = (e.State & DrawItemState.HotLight) == DrawItemState.HotLight;

    if (isSelected || isHot)
    {
        // Mezclar con azul de selección o oscurecer
        fondo = isSelected 
            ? ControlPaint.Dark(fondo, 0.85f)  // Más oscuro si está seleccionado
            : ControlPaint.Light(fondo, 0.9f);  // Más claro si solo es hover
        textoColor = isSelected ? Color.White : Color.Black;
    }

    // Pintar fondo completo
    using (var brush = new SolidBrush(fondo))
        e.Graphics.FillRectangle(brush, e.Bounds);

    // Dibujar texto con padding
    Rectangle textBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
    using (var brushTexto = new SolidBrush(textoColor))
    {
        e.Graphics.DrawString(versionId, e.Font, brushTexto, textBounds, 
            new StringFormat { LineAlignment = StringAlignment.Center });
    }

    // Foco solo si aplica
    if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        e.DrawFocusRectangle();
}