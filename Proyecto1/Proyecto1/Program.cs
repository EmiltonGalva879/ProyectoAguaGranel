using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Proyecto1;

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
    internal static class Program
    {
        public static object ApplicationConfiguration { get; private set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }

    // ===========================
    //    FORMULARIO DE LOGIN
    // ===========================
    public class LoginForm : Form
    {
        TextBox txtUser = new TextBox();
        TextBox txtPass = new TextBox();
        Button btnLogin = new Button();

        public LoginForm()
        {
            this.Text = "Login";
            this.Size = new Size(300, 200);

            Label l1 = new Label() { Text = "Usuario:", Top = 20, Left = 20 };
            Label l2 = new Label() { Text = "Contraseña:", Top = 60, Left = 20 };

            txtUser.Top = 20; txtUser.Left = 120;
            txtPass.Top = 60; txtPass.Left = 120; txtPass.PasswordChar = '*';

            btnLogin.Text = "Entrar";
            btnLogin.Top = 110; btnLogin.Left = 100;
            btnLogin.Click += BtnLogin_Click;

            this.Controls.Add(l1);
            this.Controls.Add(l2);
            this.Controls.Add(txtUser);
            this.Controls.Add(txtPass);
            this.Controls.Add(btnLogin);

            // Crear archivo de credenciales si no existe
            if (!File.Exists("usuarios.txt"))
                File.WriteAllText("usuarios.txt", "admin;1234");
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string[] cred = File.ReadAllText("usuarios.txt").Split(';');

            if (txtUser.Text == cred[0] && txtPass.Text == cred[1])
            {
                new MainForm().Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas");
            }
        }
    }

    // ===========================
    //   FORM PRINCIPAL (MENÚ)
    // ===========================
    public class MainForm : Form
    {
        public MainForm()
        {
            this.Text = "Sistema Completo";
            this.Size = new Size(600, 400);

            MenuStrip menu = new MenuStrip();

            var clientes = new ToolStripMenuItem("Clientes");
            clientes.Click += (s, e) => new SimpleTextForm("clientes.txt", "Clientes").Show();

            var productos = new ToolStripMenuItem("Productos");
            productos.Click += (s, e) => new SimpleTextForm("productos.txt", "Productos").Show();

            var camiones = new ToolStripMenuItem("Camiones");
            camiones.Click += (s, e) => new SimpleTextForm("camiones.txt", "Camiones").Show();

            var configuracion = new ToolStripMenuItem("Configuración");
            configuracion.Click += (s, e) => new ConfigForm().Show();

            var backup = new ToolStripMenuItem("Crear Backup");
            backup.Click += (s, e) =>
            {
                try
                {
                    BackupRestore.CrearBackup();
                    MessageBox.Show("Backup creado correctamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            var restaurar = new ToolStripMenuItem("Restaurar Backup");
            restaurar.Click += (s, e) =>
            {
                try
                {
                    BackupRestore.RestaurarBackup();
                    MessageBox.Show("Restauración completada.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            menu.Items.Add(clientes);
            menu.Items.Add(productos);
            menu.Items.Add(camiones);
            menu.Items.Add(configuracion);
            menu.Items.Add(backup);
            menu.Items.Add(restaurar);

            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
        }
    }

    // ================================
    //  FORMULARIO SIMPLE PARA TXT
    // ================================
    public class SimpleTextForm : Form
    {
        TextBox txt = new TextBox();
        string ruta;

        public SimpleTextForm(string archivo, string titulo)
        {
            ruta = archivo;

            this.Text = titulo;
            this.Size = new Size(500, 400);

            txt.Multiline = true;
            txt.ScrollBars = ScrollBars.Vertical;
            txt.Dock = DockStyle.Fill;

            if (File.Exists(ruta))
                txt.Text = File.ReadAllText(ruta);

            Button btnGuardar = new Button() { Text = "Guardar", Dock = DockStyle.Bottom };
            btnGuardar.Click += (s, e) =>
            {
                File.WriteAllText(ruta, txt.Text);
                MessageBox.Show("Guardado");
            };

            this.Controls.Add(txt);
            this.Controls.Add(btnGuardar);
        }
    }

    // ================================
    //      CONFIGURACIÓN
    // ================================
    public class ConfigForm : Form
    {
        TextBox userActual = new TextBox();
        TextBox passActual = new TextBox();
        TextBox nuevoUser = new TextBox();
        TextBox nuevaPass = new TextBox();

        public ConfigForm()
        {
            this.Text = "Configuración";
            this.Size = new Size(350, 300);

            Label la = new Label() { Text = "Usuario actual:", Top = 20, Left = 20 };
            Label lb = new Label() { Text = "Contraseña actual:", Top = 60, Left = 20 };
            Label lc = new Label() { Text = "Nuevo Usuario:", Top = 120, Left = 20 };
            Label ld = new Label() { Text = "Nueva Contraseña:", Top = 160, Left = 20 };

            userActual.SetBounds(150, 20, 150, 20);
            passActual.SetBounds(150, 60, 150, 20);
            passActual.PasswordChar = '*';

            nuevoUser.SetBounds(150, 120, 150, 20);
            nuevaPass.SetBounds(150, 160, 150, 20);
            nuevaPass.PasswordChar = '*';

            Button btnCambiar = new Button() { Text = "Cambiar", Top = 210, Left = 120 };
            btnCambiar.Click += BtnCambiar_Click;

            this.Controls.AddRange(new Control[] {
                la, lb, lc, ld,
                userActual, passActual,
                nuevoUser, nuevaPass,
                btnCambiar
            });
        }

        private void BtnCambiar_Click(object sender, EventArgs e)
        {
            try
            {
                Credenciales.CambiarCredencialesAdmin(
                    userActual.Text,
                    passActual.Text,
                    nuevoUser.Text,
                    nuevaPass.Text
                );

                MessageBox.Show("Credenciales actualizadas.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    // ================================
    //  (4) CAMBIAR CREDENCIALES
    // ================================
    public static class Credenciales
    {
        public static void CambiarCredencialesAdmin(string usuarioActual, string passActual, string nuevoUsuario, string nuevaPass)
        {
            string ruta = "usuarios.txt";

            if (!File.Exists(ruta))
                File.WriteAllText(ruta, "admin;1234");

            string[] partes = File.ReadAllText(ruta).Split(';');

            string user = partes[0];
            string pass = partes[1];

            if (usuarioActual != user || passActual != pass)
                throw new Exception("Usuario o contraseña actual incorrectos.");

            if (nuevoUsuario.Trim() == "" || nuevaPass.Trim() == "")
                throw new Exception("Ningún campo puede estar vacío.");

            File.WriteAllText(ruta, nuevoUsuario + ";" + nuevaPass);
        }
    }

    // ================================
    //    (5) BACKUP & (6) RESTAURAR
    // ================================
    public static class BackupRestore
    {
        public static void CrearBackup()
        {
            string carpeta = "backup";
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            foreach (string file in Directory.GetFiles(".", "*.txt"))
            {
                string name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(carpeta, name), true);
            }
        }

        public static void RestaurarBackup()
        {
            string carpeta = "backup";

            if (!Directory.Exists(carpeta))
                throw new Exception("No hay backup creado.");

            foreach (string file in Directory.GetFiles(carpeta, "*.txt"))
            {
                string name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(".", name), true);
            }
        }
    }
}
