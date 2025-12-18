using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;


/*
  Proyecto: Sistema Agua a Granel - Windows Forms (single-file)
  Descripción: App completa en un único archivo C# (sin designer) para facilitar
  pegado en Visual Studio. Guarda datos en archivos de texto dentro de la carpeta "data".

  Credenciales por defecto: usuario=admin contraseña=1234

  Cómo usar:
  - Crear un nuevo proyecto Windows Forms (.NET Framework) en Visual Studio
  - Reemplazar Program.cs con este archivo, o añadirlo al proyecto y setear como inicio
  - Ejecutar

  Nota: Este código crea formularios en tiempo de ejecución (no usa archivos .Designer)
*/

namespace AguaGranelApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DataStore.ProbarConexion();
            DataStore.Initialize();

            Application.Run(new LoginForm());
        }
    }

    #region Models
    class Cliente
    {
        public int Id; public string Nombre; public string Tipo; public string Direccion; public string Telefono; public string Credito; public string Estado;
        public override string ToString() => $"{Id}|{Nombre}|{Tipo}|{Direccion}|{Telefono}|{Credito}|{Estado}";
        public static Cliente FromLine(string line)
        {
            var p = line.Split('|');
            return new Cliente { Id = int.Parse(p[0]), Nombre = p[1], Tipo = p[2], Direccion = p[3], Telefono = p[4], Credito = p[5], Estado = p[6] };
        }
    }

    class Producto
    {
        public int Id; public string TipoAgua; public string Unidad; public decimal Precio; public string Stock; public override string ToString() => $"{Id}|{TipoAgua}|{Unidad}|{Precio}|{Stock}";
        public static Producto FromLine(string line)
        {
            var p = line.Split('|');
            return new Producto { Id = int.Parse(p[0]), TipoAgua = p[1], Unidad = p[2], Precio = decimal.Parse(p[3]), Stock = p[4] };
        }
    }

    class Camion
    {
        public int Id; public string Placa; public string Capacidad; public string Chofer; public string Estado;
        public override string ToString() => $"{Id}|{Placa}|{Capacidad}|{Chofer}|{Estado}";
        public static Camion FromLine(string line)
        {
            var p = line.Split('|');
            return new Camion { Id = int.Parse(p[0]), Placa = p[1], Capacidad = p[2], Chofer = p[3], Estado = p[4] };
        }
    }

    class Venta
    {
        public int Id; public int ClienteId; public DateTime Fecha; public string TipoEntrega; public int? CamionId; public decimal Cantidad; public decimal PrecioUnidad; public decimal Total; public string FormaPago;
        public override string ToString() => $"{Id}|{ClienteId}|{Fecha:yyyy-MM-dd HH:mm}|{TipoEntrega}|{(CamionId.HasValue ? CamionId.Value.ToString() : "-")}|{Cantidad}|{PrecioUnidad}|{Total}|{FormaPago}";
        public static Venta FromLine(string line)
        {
            var p = line.Split('|');
            return new Venta { Id = int.Parse(p[0]), ClienteId = int.Parse(p[1]), Fecha = DateTime.Parse(p[2]), TipoEntrega = p[3], CamionId = p[4] == "-" ? (int?)null : int.Parse(p[4]), Cantidad = decimal.Parse(p[5]), PrecioUnidad = decimal.Parse(p[6]), Total = decimal.Parse(p[7]), FormaPago = p[8] };
        }
    }

    class Factura
    {
        public int Id; public int VentaId; public string Detalle; public decimal Total; public decimal Impuestos; public string Firma;
        public override string ToString() => $"{Id}|{VentaId}|{Detalle}|{Total}|{Impuestos}|{Firma}";
        public static Factura FromLine(string line)
        {
            var p = line.Split('|');
            return new Factura { Id = int.Parse(p[0]), VentaId = int.Parse(p[1]), Detalle = p[2], Total = decimal.Parse(p[3]), Impuestos = decimal.Parse(p[4]), Firma = p[5] };
        }
    }
    #endregion

    #region DataStore
    static class DataStore
    {
        public static void InsertCliente(Cliente c)
        {
            using (SqlConnection cn = new SqlConnection(Cadena))
            {
                string sql = @"INSERT INTO Clientes
        (Nombre, Tipo, Direccion, Telefono, Credito, Estado)
        VALUES
        (@Nombre, @Tipo, @Direccion, @Telefono, @Credito, @Estado)";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", c.Tipo);
                cmd.Parameters.AddWithValue("@Direccion", c.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", c.Telefono);
                cmd.Parameters.AddWithValue("@Credito", c.Credito);
                cmd.Parameters.AddWithValue("@Estado", c.Estado);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateCliente(Cliente c)
        {
            using (SqlConnection cn = new SqlConnection(Cadena))
            {
                string sql = @"UPDATE Clientes SET
            Nombre=@Nombre,
            Tipo=@Tipo,
            Direccion=@Direccion,
            Telefono=@Telefono,
            Credito=@Credito,
            Estado=@Estado
            WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@Id", c.Id);
                cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", c.Tipo);
                cmd.Parameters.AddWithValue("@Direccion", c.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", c.Telefono);
                cmd.Parameters.AddWithValue("@Credito", c.Credito);
                cmd.Parameters.AddWithValue("@Estado", c.Estado);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteCliente(int id)
        {
            using (SqlConnection cn = new SqlConnection(Cadena))
            {
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Clientes WHERE Id=@Id", cn);
                cmd.Parameters.AddWithValue("@Id", id);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static string Cadena =
            "Server=(local);Database=AguaGranelDB;Trusted_Connection=True;";

        public static void ProbarConexion()
        {
            using (SqlConnection cn = new SqlConnection(Cadena))
            {
                cn.Open();
                MessageBox.Show("Conexión exitosa con SQL Server");
            }
        }


        public static string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        public static string ClientesFile => Path.Combine(DataDir, "clientes.txt");
        public static string ProductosFile => Path.Combine(DataDir, "productos.txt");
        public static string CamionesFile => Path.Combine(DataDir, "camiones.txt");
        public static string VentasFile => Path.Combine(DataDir, "ventas.txt");
        public static string FacturasFile => Path.Combine(DataDir, "facturas.txt");
        public static string UsuariosFile => Path.Combine(DataDir, "usuarios.txt");

        public static List<Cliente> Clientes = new List<Cliente>();
        public static List<Producto> Productos = new List<Producto>();
        public static List<Camion> Camiones = new List<Camion>();
        public static List<Venta> Ventas = new List<Venta>();
        public static List<Factura> Facturas = new List<Factura>();

        public static string Usuario = "admin";
        public static string Contrasena = "1234";

        public static void Initialize()
        {
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);

            if (!File.Exists(UsuariosFile)) File.WriteAllText(UsuariosFile, "admin|1234");
            LoadUsers();

            if (!File.Exists(ClientesFile)) File.WriteAllText(ClientesFile, "");
            if (!File.Exists(ProductosFile)) File.WriteAllText(ProductosFile, "");
            if (!File.Exists(CamionesFile)) File.WriteAllText(CamionesFile, "");
            if (!File.Exists(VentasFile)) File.WriteAllText(VentasFile, "");
            if (!File.Exists(FacturasFile)) File.WriteAllText(FacturasFile, "");

            LoadAll();

            // seed basic producto if none
            if (Productos.Count == 0)
            {
                Productos.Add(new Producto { Id = 1, TipoAgua = "Purificada", Unidad = "Galón", Precio = 30m, Stock = "1000" });
                SaveProductos();
            }
        }

        public static void LoadUsers()
        {
            try
            {
                var line = File.ReadAllText(UsuariosFile).Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    var p = line.Split('|');
                    Usuario = p[0]; Contrasena = p[1];
                }
            }
            catch { }
        }

        public static void SaveUsers()
        {
            File.WriteAllText(UsuariosFile, Usuario + "|" + Contrasena);
        }

        public static void LoadAll()
        {
            //Clientes = File.ReadAllLines(ClientesFile).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Cliente.FromLine).ToList();

            Productos = File.ReadAllLines(ProductosFile).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Producto.FromLine).ToList();
            Camiones = File.ReadAllLines(CamionesFile).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Camion.FromLine).ToList();
            Ventas = File.ReadAllLines(VentasFile).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Venta.FromLine).ToList();
            Facturas = File.ReadAllLines(FacturasFile).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Factura.FromLine).ToList();
        }

        public static void LoadClientes()
        {
            Clientes.Clear();

            using (SqlConnection cn = new SqlConnection(Cadena))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Clientes", cn);
                cn.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Clientes.Add(new Cliente
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Tipo = dr["Tipo"].ToString(),
                        Direccion = dr["Direccion"].ToString(),
                        Telefono = dr["Telefono"].ToString(),
                        Credito = dr["Credito"].ToString(),
                        Estado = dr["Estado"].ToString()
                    });
                }
            }
        }

        public static void SaveClientes() => File.WriteAllLines(ClientesFile, Clientes.Select(c => c.ToString()));
        public static void SaveProductos() => File.WriteAllLines(ProductosFile, Productos.Select(p => p.ToString()));
        public static void SaveCamiones() => File.WriteAllLines(CamionesFile, Camiones.Select(c => c.ToString()));
        public static void SaveVentas() => File.WriteAllLines(VentasFile, Ventas.Select(v => v.ToString()));
        public static void SaveFacturas() => File.WriteAllLines(FacturasFile, Facturas.Select(f => f.ToString()));

        public static int NextId<T>(List<T> list) where T : class
        {
            if (typeof(T) == typeof(Cliente)) return Clientes.Count == 0 ? 1 : Clientes.Max(c => c.Id) + 1;
            if (typeof(T) == typeof(Producto)) return Productos.Count == 0 ? 1 : Productos.Max(p => p.Id) + 1;
            if (typeof(T) == typeof(Camion)) return Camiones.Count == 0 ? 1 : Camiones.Max(c => c.Id) + 1;
            if (typeof(T) == typeof(Venta)) return Ventas.Count == 0 ? 1 : Ventas.Max(v => v.Id) + 1;
            if (typeof(T) == typeof(Factura)) return Facturas.Count == 0 ? 1 : Facturas.Max(f => f.Id) + 1;
            return 1;
        }
    }
    #endregion

    #region Forms
    class LoginForm : Form
    {
        TextBox txtUser; TextBox txtPass; Button btnLogin; Label lblInfo;
        public LoginForm()
        {
            Text = "Login - Sistema Agua a Granel"; Width = 360; Height = 220; StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;

            var lbl1 = new Label { Text = "Usuario:", Left = 20, Top = 20, Width = 80 };
            txtUser = new TextBox { Left = 110, Top = 18, Width = 200 };
            var lbl2 = new Label { Text = "Contraseña:", Left = 20, Top = 60, Width = 80 };
            txtPass = new TextBox { Left = 110, Top = 58, Width = 200, UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Entrar", Left = 110, Top = 100, Width = 100 };
            btnLogin.Click += BtnLogin_Click;

            lblInfo = new Label { Left = 110, Top = 140, Width = 220, Height = 30, Text = "Usuario por defecto: admin / 1234" };

            Controls.AddRange(new Control[] { lbl1, txtUser, lbl2, txtPass, btnLogin, lblInfo });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            DataStore.LoadUsers();
            if (txtUser.Text == DataStore.Usuario && txtPass.Text == DataStore.Contrasena)
            {
                var menu = new MainForm(txtUser.Text);
                menu.FormClosed += (s, ev) => { Application.Exit(); };
                menu.Show();
                Hide();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    class MainForm : Form
    {
        string currentUser;
        TabControl tabs;
        public MainForm(string user)
        {
            currentUser = user;
            Text = "Sistema Agua a Granel - Menú Principal"; Width = 1000; Height = 600; StartPosition = FormStartPosition.CenterScreen;

            tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add("Clientes");
            tabs.TabPages.Add("Productos");
            tabs.TabPages.Add("Camiones");
            tabs.TabPages.Add("Ventas");
            tabs.TabPages.Add("Facturas");
            tabs.TabPages.Add("Reportes");
            tabs.TabPages.Add("Configuración");

            CreateClientesTab(tabs.TabPages[0]);
            CreateProductosTab(tabs.TabPages[1]);
            CreateCamionesTab(tabs.TabPages[2]);
            CreateVentasTab(tabs.TabPages[3]);
            CreateFacturasTab(tabs.TabPages[4]);
            CreateReportesTab(tabs.TabPages[5]);
            CreateConfigTab(tabs.TabPages[6]);

            Controls.Add(tabs);
        }

        #region Clientes Tab
        ListView lvClientes;
        void CreateClientesTab(TabPage page)
        {
            lvClientes = new ListView { View = View.Details, FullRowSelect = true, Dock = DockStyle.Top, Height = 320 };
            lvClientes.Columns.Add("Id", 50); lvClientes.Columns.Add("Nombre", 200); lvClientes.Columns.Add("Tipo", 100); lvClientes.Columns.Add("Direccion", 200); lvClientes.Columns.Add("Telefono", 100); lvClientes.Columns.Add("Credito", 80); lvClientes.Columns.Add("Estado", 80);

            var btnAdd = new Button { Text = "Agregar", Left = 20, Top = 340, Width = 100 };
            var btnEdit = new Button { Text = "Editar", Left = 140, Top = 340, Width = 100 };
            var btnDel = new Button { Text = "Eliminar", Left = 260, Top = 340, Width = 100 };

            btnAdd.Click += (s, e) => { ShowClienteForm(null); };
            btnEdit.Click += (s, e) => { if (lvClientes.SelectedItems.Count > 0) { int id = int.Parse(lvClientes.SelectedItems[0].SubItems[0].Text); ShowClienteForm(DataStore.Clientes.First(c => c.Id == id)); } };
            btnDel.Click += (s, e) => { if (lvClientes.SelectedItems.Count > 0) { int id = int.Parse(lvClientes.SelectedItems[0].SubItems[0].Text); if (MessageBox.Show("Eliminar cliente?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) 
                    {
                        DataStore.DeleteCliente(id);
                        DataStore.LoadClientes();
                        ;
                    } } };

            page.Controls.AddRange(new Control[] { lvClientes, btnAdd, btnEdit, btnDel });
            LoadClientes();
        }

        void LoadClientes()
        {
            DataStore.LoadAll();
            lvClientes.Items.Clear();
            foreach (var c in DataStore.Clientes)
            {
                var it = new ListViewItem(c.Id.ToString());
                it.SubItems.Add(c.Nombre); it.SubItems.Add(c.Tipo); it.SubItems.Add(c.Direccion); it.SubItems.Add(c.Telefono); it.SubItems.Add(c.Credito); it.SubItems.Add(c.Estado);
                lvClientes.Items.Add(it);
            }
        }

        void ShowClienteForm(Cliente cliente)
        {
            var f = new Form { Text = cliente == null ? "Agregar Cliente" : "Editar Cliente", Width = 420, Height = 360, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lblNombre = new Label { Text = "Nombre:", Left = 20, Top = 20, Width = 100 };
            var txtNombre = new TextBox { Left = 120, Top = 18, Width = 250 };
            var lblTipo = new Label { Text = "Tipo:", Left = 20, Top = 60, Width = 100 };
            var cmbTipo = new ComboBox { Left = 120, Top = 58, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList }; cmbTipo.Items.AddRange(new string[] { "residencial", "comercial", "industrial" });
            var lblDireccion = new Label { Text = "Dirección:", Left = 20, Top = 100, Width = 100 };
            var txtDireccion = new TextBox { Left = 120, Top = 98, Width = 250 };
            var lblTelefono = new Label { Text = "Teléfono:", Left = 20, Top = 140, Width = 100 };
            var txtTelefono = new TextBox { Left = 120, Top = 138, Width = 250 };
            var lblCredito = new Label { Text = "Crédito:", Left = 20, Top = 180, Width = 100 };
            var txtCredito = new TextBox { Left = 120, Top = 178, Width = 250 };
            var lblEstado = new Label { Text = "Estado:", Left = 20, Top = 220, Width = 100 };
            var cmbEstado = new ComboBox { Left = 120, Top = 218, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList }; cmbEstado.Items.AddRange(new string[] { "activo", "inactivo" });
            var btnSave = new Button { Text = "Guardar", Left = 120, Top = 260, Width = 120 };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Nombre requerido"); return; }
                if (cliente == null)
                {
                    var id = DataStore.NextId<Cliente>(null);
                    var c = new Cliente { Id = id, Nombre = txtNombre.Text, Tipo = cmbTipo.Text, Direccion = txtDireccion.Text, Telefono = txtTelefono.Text, Credito = txtCredito.Text, Estado = cmbEstado.Text };
                    
                    DataStore.InsertCliente(c);
                    DataStore.LoadClientes();

                }
                else
                {
                    cliente.Nombre = txtNombre.Text;
                    cliente.Tipo = cmbTipo.Text;
                    cliente.Direccion = txtDireccion.Text;
                    cliente.Telefono = txtTelefono.Text;
                    cliente.Credito = txtCredito.Text;
                    cliente.Estado = cmbEstado.Text;

                    DataStore.UpdateCliente(cliente);
                    DataStore.LoadClientes();

                }
                LoadClientes(); f.Close();
            };

            if (cliente != null)
            {
                txtNombre.Text = cliente.Nombre; cmbTipo.Text = cliente.Tipo; txtDireccion.Text = cliente.Direccion; txtTelefono.Text = cliente.Telefono; txtCredito.Text = cliente.Credito; cmbEstado.Text = cliente.Estado;
            }

            f.Controls.AddRange(new Control[] { lblNombre, txtNombre, lblTipo, cmbTipo, lblDireccion, txtDireccion, lblTelefono, txtTelefono, lblCredito, txtCredito, lblEstado, cmbEstado, btnSave });
            f.ShowDialog();
        }
        #endregion

        #region Productos Tab
        ListView lvProductos;
        void CreateProductosTab(TabPage page)
        {
            lvProductos = new ListView { View = View.Details, FullRowSelect = true, Dock = DockStyle.Top, Height = 320 };
            lvProductos.Columns.Add("Id", 50); lvProductos.Columns.Add("Tipo Agua", 200); lvProductos.Columns.Add("Unidad", 100); lvProductos.Columns.Add("Precio", 80); lvProductos.Columns.Add("Stock", 80);

            var btnAdd = new Button { Text = "Agregar", Left = 20, Top = 340, Width = 100 };
            var btnEdit = new Button { Text = "Editar", Left = 140, Top = 340, Width = 100 };
            var btnDel = new Button { Text = "Eliminar", Left = 260, Top = 340, Width = 100 };

            btnAdd.Click += (s, e) => { ShowProductoForm(null); };
            btnEdit.Click += (s, e) => { if (lvProductos.SelectedItems.Count > 0) { int id = int.Parse(lvProductos.SelectedItems[0].SubItems[0].Text); ShowProductoForm(DataStore.Productos.First(p => p.Id == id)); } };
            btnDel.Click += (s, e) => { if (lvProductos.SelectedItems.Count > 0) { int id = int.Parse(lvProductos.SelectedItems[0].SubItems[0].Text); if (MessageBox.Show("Eliminar producto?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { DataStore.Productos.RemoveAll(p => p.Id == id); DataStore.SaveProductos(); LoadProductos(); } } };

            page.Controls.AddRange(new Control[] { lvProductos, btnAdd, btnEdit, btnDel });
            LoadProductos();
        }

        void LoadProductos()
        {
            DataStore.LoadAll();
            lvProductos.Items.Clear();
            foreach (var p in DataStore.Productos)
            {
                var it = new ListViewItem(p.Id.ToString()); it.SubItems.Add(p.TipoAgua); it.SubItems.Add(p.Unidad); it.SubItems.Add(p.Precio.ToString("F2")); it.SubItems.Add(p.Stock);
                lvProductos.Items.Add(it);
            }
        }

        void ShowProductoForm(Producto prod)
        {
            var f = new Form { Text = prod == null ? "Agregar Producto" : "Editar Producto", Width = 420, Height = 320, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lblTipo = new Label { Text = "Tipo de agua:", Left = 20, Top = 20, Width = 100 };
            var txtTipo = new TextBox { Left = 120, Top = 18, Width = 250 };
            var lblUnidad = new Label { Text = "Unidad:", Left = 20, Top = 60, Width = 100 };
            var txtUnidad = new TextBox { Left = 120, Top = 58, Width = 250 };
            var lblPrecio = new Label { Text = "Precio por unidad:", Left = 20, Top = 100, Width = 100 };
            var txtPrecio = new TextBox { Left = 120, Top = 98, Width = 250 };
            var lblStock = new Label { Text = "Stock/capacidad:", Left = 20, Top = 140, Width = 100 };
            var txtStock = new TextBox { Left = 120, Top = 138, Width = 250 };
            var btnSave = new Button { Text = "Guardar", Left = 120, Top = 180, Width = 120 };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtTipo.Text)) { MessageBox.Show("Tipo de agua requerido"); return; }
                if (prod == null)
                {
                    var id = DataStore.NextId<Producto>(null);
                    var p = new Producto { Id = id, TipoAgua = txtTipo.Text, Unidad = txtUnidad.Text, Precio = decimal.Parse(txtPrecio.Text), Stock = txtStock.Text };
                    DataStore.Productos.Add(p); DataStore.SaveProductos();
                }
                else
                {
                    prod.TipoAgua = txtTipo.Text; prod.Unidad = txtUnidad.Text; prod.Precio = decimal.Parse(txtPrecio.Text); prod.Stock = txtStock.Text; DataStore.SaveProductos();
                }
                LoadProductos(); f.Close();
            };

            if (prod != null) { txtTipo.Text = prod.TipoAgua; txtUnidad.Text = prod.Unidad; txtPrecio.Text = prod.Precio.ToString(); txtStock.Text = prod.Stock; }
            f.Controls.AddRange(new Control[] { lblTipo, txtTipo, lblUnidad, txtUnidad, lblPrecio, txtPrecio, lblStock, txtStock, btnSave });
            f.ShowDialog();
        }
        #endregion

        #region Camiones Tab
        ListView lvCamiones;
        void CreateCamionesTab(TabPage page)
        {
            lvCamiones = new ListView { View = View.Details, FullRowSelect = true, Dock = DockStyle.Top, Height = 320 };
            lvCamiones.Columns.Add("Id", 50); lvCamiones.Columns.Add("Placa", 150); lvCamiones.Columns.Add("Capacidad", 100); lvCamiones.Columns.Add("Chofer", 150); lvCamiones.Columns.Add("Estado", 100);

            var btnAdd = new Button { Text = "Agregar", Left = 20, Top = 340, Width = 100 };
            var btnEdit = new Button { Text = "Editar", Left = 140, Top = 340, Width = 100 };
            var btnDel = new Button { Text = "Eliminar", Left = 260, Top = 340, Width = 100 };

            btnAdd.Click += (s, e) => { ShowCamionForm(null); };
            btnEdit.Click += (s, e) => { if (lvCamiones.SelectedItems.Count > 0) { int id = int.Parse(lvCamiones.SelectedItems[0].SubItems[0].Text); ShowCamionForm(DataStore.Camiones.First(c => c.Id == id)); } };
            btnDel.Click += (s, e) => { if (lvCamiones.SelectedItems.Count > 0) { int id = int.Parse(lvCamiones.SelectedItems[0].SubItems[0].Text); if (MessageBox.Show("Eliminar camión?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { DataStore.Camiones.RemoveAll(c => c.Id == id); DataStore.SaveCamiones(); LoadCamiones(); } } };

            page.Controls.AddRange(new Control[] { lvCamiones, btnAdd, btnEdit, btnDel });
            LoadCamiones();
        }

        void LoadCamiones()
        {
            DataStore.LoadAll();
            lvCamiones.Items.Clear();
            foreach (var c in DataStore.Camiones)
            {
                var it = new ListViewItem(c.Id.ToString()); it.SubItems.Add(c.Placa); it.SubItems.Add(c.Capacidad); it.SubItems.Add(c.Chofer); it.SubItems.Add(c.Estado);
                lvCamiones.Items.Add(it);
            }
        }

        void ShowCamionForm(Camion cam)
        {
            var f = new Form { Text = cam == null ? "Agregar Camión" : "Editar Camión", Width = 420, Height = 320, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lblPlaca = new Label { Text = "Placa:", Left = 20, Top = 20, Width = 100 };
            var txtPlaca = new TextBox { Left = 120, Top = 18, Width = 250 };
            var lblCap = new Label { Text = "Capacidad:", Left = 20, Top = 60, Width = 100 };
            var txtCap = new TextBox { Left = 120, Top = 58, Width = 250 };
            var lblChofer = new Label { Text = "Chofer:", Left = 20, Top = 100, Width = 100 };
            var txtChofer = new TextBox { Left = 120, Top = 98, Width = 250 };
            var lblEstado = new Label { Text = "Estado:", Left = 20, Top = 140, Width = 100 };
            var cmbEstado = new ComboBox { Left = 120, Top = 138, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList }; cmbEstado.Items.AddRange(new string[] { "disponible", "repartiendo", "mantenimiento" });
            var btnSave = new Button { Text = "Guardar", Left = 120, Top = 180, Width = 120 };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtPlaca.Text)) { MessageBox.Show("Placa requerida"); return; }
                if (cam == null) { var id = DataStore.NextId<Camion>(null); var c = new Camion { Id = id, Placa = txtPlaca.Text, Capacidad = txtCap.Text, Chofer = txtChofer.Text, Estado = cmbEstado.Text }; DataStore.Camiones.Add(c); DataStore.SaveCamiones(); }
                else { cam.Placa = txtPlaca.Text; cam.Capacidad = txtCap.Text; cam.Chofer = txtChofer.Text; cam.Estado = cmbEstado.Text; DataStore.SaveCamiones(); }
                LoadCamiones(); f.Close();
            };
            if (cam != null) { txtPlaca.Text = cam.Placa; txtCap.Text = cam.Capacidad; txtChofer.Text = cam.Chofer; cmbEstado.Text = cam.Estado; }
            f.Controls.AddRange(new Control[] { lblPlaca, txtPlaca, lblCap, txtCap, lblChofer, txtChofer, lblEstado, cmbEstado, btnSave }); f.ShowDialog();
        }
        #endregion

        #region Ventas Tab
        ListView lvVentas;
        void CreateVentasTab(TabPage page)
        {
            lvVentas = new ListView { View = View.Details, FullRowSelect = true, Dock = DockStyle.Top, Height = 320 };
            lvVentas.Columns.Add("Id", 50); lvVentas.Columns.Add("ClienteId", 80); lvVentas.Columns.Add("Fecha", 140); lvVentas.Columns.Add("Entrega", 80); lvVentas.Columns.Add("CamionId", 80); lvVentas.Columns.Add("Cantidad", 80); lvVentas.Columns.Add("PrecioU", 80); lvVentas.Columns.Add("Total", 80); lvVentas.Columns.Add("Pago", 80);

            var btnAdd = new Button { Text = "Nueva Venta", Left = 20, Top = 340, Width = 120 };
            var btnVer = new Button { Text = "Ver Detalle", Left = 160, Top = 340, Width = 120 };

            btnAdd.Click += (s, e) => { ShowVentaForm(); };
            btnVer.Click += (s, e) => { if (lvVentas.SelectedItems.Count > 0) { int id = int.Parse(lvVentas.SelectedItems[0].SubItems[0].Text); var v = DataStore.Ventas.First(x => x.Id == id); MessageBox.Show($"Venta #{v.Id}\nClienteId:{v.ClienteId}\nFecha:{v.Fecha}\nCantidad:{v.Cantidad}\nTotal:{v.Total}"); } };

            page.Controls.AddRange(new Control[] { lvVentas, btnAdd, btnVer }); LoadVentas();
        }

        void LoadVentas()
        {
            DataStore.LoadAll();
            lvVentas.Items.Clear();
            foreach (var v in DataStore.Ventas)
            {
                var it = new ListViewItem(v.Id.ToString()); it.SubItems.Add(v.ClienteId.ToString()); it.SubItems.Add(v.Fecha.ToString("yyyy-MM-dd HH:mm")); it.SubItems.Add(v.TipoEntrega); it.SubItems.Add(v.CamionId.HasValue ? v.CamionId.Value.ToString() : "-"); it.SubItems.Add(v.Cantidad.ToString()); it.SubItems.Add(v.PrecioUnidad.ToString("F2")); it.SubItems.Add(v.Total.ToString("F2")); it.SubItems.Add(v.FormaPago);
                lvVentas.Items.Add(it);
            }
        }

        void ShowVentaForm()
        {
            var f = new Form { Text = "Nueva Venta", Width = 520, Height = 420, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lblCliente = new Label { Text = "Cliente:", Left = 20, Top = 20, Width = 100 };
            var cmbCliente = new ComboBox { Left = 130, Top = 18, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var c in DataStore.Clientes) cmbCliente.Items.Add($"{c.Id} - {c.Nombre}");
            var lblFecha = new Label { Text = "Fecha:", Left = 20, Top = 60, Width = 100 };
            var dtFecha = new DateTimePicker { Left = 130, Top = 58, Width = 340, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };
            var lblEntrega = new Label { Text = "Tipo entrega:", Left = 20, Top = 100, Width = 100 };
            var cmbEntrega = new ComboBox { Left = 130, Top = 98, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList }; cmbEntrega.Items.AddRange(new string[] { "retiro", "envio" });
            var lblCamion = new Label { Text = "Camión (opcional):", Left = 20, Top = 140, Width = 100 };
            var cmbCamion = new ComboBox { Left = 130, Top = 138, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList }; cmbCamion.Items.Add("-"); foreach (var m in DataStore.Camiones) cmbCamion.Items.Add($"{m.Id} - {m.Placa}"); cmbCamion.SelectedIndex = 0;
            var lblCantidad = new Label { Text = "Cantidad:", Left = 20, Top = 180, Width = 100 };
            var txtCantidad = new TextBox { Left = 130, Top = 178, Width = 140 };
            var lblPrecio = new Label { Text = "Precio/u:", Left = 300, Top = 180, Width = 80 };
            var txtPrecio = new TextBox { Left = 380, Top = 178, Width = 90 };
            var lblTotal = new Label { Text = "Total:", Left = 20, Top = 220, Width = 100 };
            var txtTotal = new TextBox { Left = 130, Top = 218, Width = 140, ReadOnly = true };
            var lblPago = new Label { Text = "Forma pago:", Left = 20, Top = 260, Width = 100 };
            var cmbPago = new ComboBox { Left = 130, Top = 258, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList }; cmbPago.Items.AddRange(new string[] { "efectivo", "transferencia", "credito" });
            var btnCalc = new Button { Text = "Calcular", Left = 300, Top = 216, Width = 80 };
            var btnSave = new Button { Text = "Guardar Venta", Left = 130, Top = 300, Width = 140 };

            btnCalc.Click += (s, e) => { if (decimal.TryParse(txtCantidad.Text, out decimal cant) && decimal.TryParse(txtPrecio.Text, out decimal pu)) txtTotal.Text = (cant * pu).ToString("F2"); else MessageBox.Show("Cantidad o precio inválido"); };
            btnSave.Click += (s, e) => {
                if (cmbCliente.SelectedIndex == -1) { MessageBox.Show("Seleccione un cliente"); return; }
                var clienteId = int.Parse(cmbCliente.SelectedItem.ToString().Split('-')[0].Trim());
                var fecha = dtFecha.Value; var entrega = cmbEntrega.Text; int? camionId = null; if (cmbCamion.SelectedIndex > 0) camionId = int.Parse(cmbCamion.SelectedItem.ToString().Split('-')[0].Trim());
                if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad)) { MessageBox.Show("Cantidad inválida"); return; }
                if (!decimal.TryParse(txtPrecio.Text, out decimal preciou)) { MessageBox.Show("Precio inválido"); return; }
                if (!decimal.TryParse(txtTotal.Text, out decimal total)) { MessageBox.Show("Calcule el total"); return; }
                var forma = cmbPago.Text;
                var id = DataStore.NextId<Venta>(null);
                var venta = new Venta { Id = id, ClienteId = clienteId, Fecha = fecha, TipoEntrega = entrega, CamionId = camionId, Cantidad = cantidad, PrecioUnidad = preciou, Total = total, FormaPago = forma };
                DataStore.Ventas.Add(venta); DataStore.SaveVentas(); LoadVentas();
                MessageBox.Show($"Venta registrada. ID: {venta.Id}");
                f.Close();
            };

            f.Controls.AddRange(new Control[] { lblCliente, cmbCliente, lblFecha, dtFecha, lblEntrega, cmbEntrega, lblCamion, cmbCamion, lblCantidad, txtCantidad, lblPrecio, txtPrecio, lblTotal, txtTotal, lblPago, cmbPago, btnCalc, btnSave });
            f.ShowDialog();
        }
        #endregion

        #region Facturas Tab
        ListView lvFacturas;
        void CreateFacturasTab(TabPage page)
        {
            lvFacturas = new ListView { View = View.Details, FullRowSelect = true, Dock = DockStyle.Top, Height = 320 };
            lvFacturas.Columns.Add("Id", 50); lvFacturas.Columns.Add("VentaId", 80); lvFacturas.Columns.Add("Detalle", 300); lvFacturas.Columns.Add("Total", 80); lvFacturas.Columns.Add("Impuestos", 80); lvFacturas.Columns.Add("Firma", 120);

            var btnGen = new Button { Text = "Generar Factura", Left = 20, Top = 340, Width = 140 };
            var btnView = new Button { Text = "Ver Factura", Left = 180, Top = 340, Width = 140 };

            btnGen.Click += (s, e) => { ShowGenerarFacturaForm(); };
            btnView.Click += (s, e) => { if (lvFacturas.SelectedItems.Count > 0) { int id = int.Parse(lvFacturas.SelectedItems[0].SubItems[0].Text); var fac = DataStore.Facturas.First(x => x.Id == id); MessageBox.Show($"Factura #{fac.Id}\nVenta:{fac.VentaId}\nDetalle:{fac.Detalle}\nTotal:{fac.Total}\nImpuestos:{fac.Impuestos}\nFirma:{fac.Firma}"); } };

            page.Controls.AddRange(new Control[] { lvFacturas, btnGen, btnView }); LoadFacturas();
        }

        void LoadFacturas()
        {
            DataStore.LoadAll();
            lvFacturas.Items.Clear();
            foreach (var f in DataStore.Facturas)
            {
                var it = new ListViewItem(f.Id.ToString()); it.SubItems.Add(f.VentaId.ToString()); it.SubItems.Add(f.Detalle); it.SubItems.Add(f.Total.ToString("F2")); it.SubItems.Add(f.Impuestos.ToString("F2")); it.SubItems.Add(f.Firma);
                lvFacturas.Items.Add(it);
            }
        }

        void ShowGenerarFacturaForm()
        {
            var f = new Form { Text = "Generar Factura", Width = 520, Height = 360, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lblVenta = new Label { Text = "Venta:", Left = 20, Top = 20, Width = 100 };
            var cmbVenta = new ComboBox { Left = 130, Top = 18, Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var v in DataStore.Ventas) cmbVenta.Items.Add($"{v.Id} - Cliente {v.ClienteId} - Total {v.Total:F2}");
            var lblDetalle = new Label { Text = "Detalle:", Left = 20, Top = 60, Width = 100 };
            var txtDetalle = new TextBox { Left = 130, Top = 58, Width = 360 };
            var lblImpuestos = new Label { Text = "Impuestos:", Left = 20, Top = 100, Width = 100 };
            var txtImpuestos = new TextBox { Left = 130, Top = 98, Width = 360, Text = "0" };
            var lblFirma = new Label { Text = "Firma/Autorización:", Left = 20, Top = 140, Width = 100 };
            var txtFirma = new TextBox { Left = 130, Top = 138, Width = 360 };
            var btnGen = new Button { Text = "Generar", Left = 130, Top = 200, Width = 120 };

            btnGen.Click += (s, e) => {
                if (cmbVenta.SelectedIndex == -1) { MessageBox.Show("Seleccione una venta"); return; }
                var ventaId = int.Parse(cmbVenta.SelectedItem.ToString().Split('-')[0].Trim());
                if (!decimal.TryParse(txtImpuestos.Text, out decimal imp)) { MessageBox.Show("Impuestos inválidos"); return; }
                var id = DataStore.NextId<Factura>(null);
                var venta = DataStore.Ventas.First(x => x.Id == ventaId);
                var total = venta.Total + imp;
                var fac = new Factura { Id = id, VentaId = ventaId, Detalle = txtDetalle.Text, Total = total, Impuestos = imp, Firma = txtFirma.Text };
                DataStore.Facturas.Add(fac); DataStore.SaveFacturas(); LoadFacturas();
                MessageBox.Show($"Factura generada. ID: {fac.Id}"); f.Close();
            };

            f.Controls.AddRange(new Control[] { lblVenta, cmbVenta, lblDetalle, txtDetalle, lblImpuestos, txtImpuestos, lblFirma, txtFirma, btnGen }); f.ShowDialog();
        }
        #endregion

        #region Reportes Tab
        void CreateReportesTab(TabPage page)
        {
            var btnVentasDia = new Button { Text = "Ventas del día", Left = 20, Top = 20, Width = 160 };
            var btnVentasCliente = new Button { Text = "Ventas por cliente", Left = 200, Top = 20, Width = 160 };
            var btnConsumoCamion = new Button { Text = "Consumo por camión", Left = 380, Top = 20, Width = 160 };
            var btnProdVendidos = new Button { Text = "Productos vendidos (unidad)", Left = 560, Top = 20, Width = 200 };
            var btnFormaPago = new Button { Text = "Ventas por forma de pago", Left = 20, Top = 80, Width = 220 };

            btnVentasDia.Click += (s, e) => {
                var hoy = DateTime.Today; var ventas = DataStore.Ventas.Where(v => v.Fecha.Date == hoy).ToList();
                var sb = new StringBuilder(); sb.AppendLine($"Ventas del día ({hoy:yyyy-MM-dd}) - Total: {ventas.Sum(v => v.Total):F2}");
                foreach (var v in ventas) sb.AppendLine($"ID:{v.Id} Cliente:{v.ClienteId} Total:{v.Total:F2}");
                MessageBox.Show(sb.ToString());
            };

            btnVentasCliente.Click += (s, e) => {
                var dic = DataStore.Ventas.GroupBy(v => v.ClienteId).Select(g => new { Cliente = DataStore.Clientes.FirstOrDefault(c => c.Id == g.Key)?.Nombre ?? g.Key.ToString(), Total = g.Sum(x => x.Total) });
                var sb = new StringBuilder(); foreach (var d in dic) sb.AppendLine($"{d.Cliente} => {d.Total:F2}"); MessageBox.Show(sb.ToString());
            };

            btnConsumoCamion.Click += (s, e) => {
                var dic = DataStore.Ventas.Where(v => v.CamionId.HasValue).GroupBy(v => v.CamionId.Value).Select(g => new { Camion = DataStore.Camiones.FirstOrDefault(c => c.Id == g.Key)?.Placa ?? g.Key.ToString(), Consumo = g.Sum(x => x.Cantidad) });
                var sb = new StringBuilder(); foreach (var d in dic) sb.AppendLine($"{d.Camion} => {d.Consumo}"); MessageBox.Show(sb.ToString());
            };

            btnProdVendidos.Click += (s, e) => {
                var sb = new StringBuilder(); sb.AppendLine($"Total unidades vendidas (sum cantidad): {DataStore.Ventas.Sum(v => v.Cantidad)}"); MessageBox.Show(sb.ToString());
            };

            btnFormaPago.Click += (s, e) => {
                var dic = DataStore.Ventas.GroupBy(v => v.FormaPago).Select(g => new { Forma = g.Key, Total = g.Sum(x => x.Total), Count = g.Count() }); var sb = new StringBuilder(); foreach (var d in dic) sb.AppendLine($"{d.Forma} => {d.Count} ventas / {d.Total:F2}"); MessageBox.Show(sb.ToString());
            };

            page.Controls.AddRange(new Control[] { btnVentasDia, btnVentasCliente, btnConsumoCamion, btnProdVendidos, btnFormaPago });
        }
        #endregion

        #region Config Tab (solo admin)
        void CreateConfigTab(TabPage page)
        {
            var lblUser = new Label { Text = "Usuario actual:", Left = 20, Top = 20, Width = 120 };
            var txtUser = new TextBox { Left = 150, Top = 18, Width = 200, ReadOnly = true, Text = DataStore.Usuario };
            var lblPass = new Label { Text = "Contraseña actual:", Left = 20, Top = 60, Width = 120 };
            var txtPass = new TextBox { Left = 150, Top = 58, Width = 200, ReadOnly = true, Text = DataStore.Contrasena };
            var lblNewUser = new Label { Text = "Nuevo usuario:", Left = 20, Top = 110, Width = 120 };
            var txtNewUser = new TextBox { Left = 150, Top = 108, Width = 200 };
            var lblNewPass = new Label { Text = "Nueva contraseña:", Left = 20, Top = 150, Width = 120 };
            var txtNewPass = new TextBox { Left = 150, Top = 148, Width = 200 };
            var btnSave = new Button { Text = "Guardar credenciales (ADMIN)", Left = 150, Top = 190, Width = 200 };

            btnSave.Click += (s, e) => {
                if (currentUser != "admin")
                {
                    MessageBox.Show("Solo el administrador puede cambiar credenciales."); return;
                }
                if (string.IsNullOrWhiteSpace(txtNewUser.Text) || string.IsNullOrWhiteSpace(txtNewPass.Text)) { MessageBox.Show("Complete usuario y contraseña"); return; }
                DataStore.Usuario = txtNewUser.Text.Trim(); DataStore.Contrasena = txtNewPass.Text.Trim(); DataStore.SaveUsers(); MessageBox.Show("Credenciales actualizadas. Reinicie sesión para aplicar.");
                txtUser.Text = DataStore.Usuario; txtPass.Text = DataStore.Contrasena;
            };

            page.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, lblNewUser, txtNewUser, lblNewPass, txtNewPass, btnSave });
        }
        #endregion
    }
    #endregion
}
