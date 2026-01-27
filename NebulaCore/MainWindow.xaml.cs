using System;
using System.Windows;
using System.Windows.Input; // IMPORTANTE PARA ARRASTRAR
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NebulaCore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string u = txtUsuario.Text;
            string p = HashPasswordSimple(txtPassword.Password);

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(txtPassword.Password))
            {
                lblMensaje.Text = "Por favor, rellena todos los campos.";
                return;
            }

            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    if (con == null) return;

                    con.Open();
                    string sql = "SELECT rol, estado FROM usuarios WHERE nombre_usuario=@u AND password=@p";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", u);
                    cmd.Parameters.AddWithValue("@p", p);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string rol = reader["rol"].ToString();
                            string estado = reader["estado"].ToString();

                            if (estado == "Baneado")
                            {
                                MessageBox.Show("Acceso denegado.\nTu cuenta ha sido suspendida.", "Cuenta Baneada", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            if (rol == "admin" || rol == "Admin")
                            {
                                AdminWindow adminWin = new AdminWindow();
                                adminWin.Show();
                            }
                            else
                            {
                                HomeWindow home = new HomeWindow(u);
                                home.Show();
                            }
                            this.Close();
                        }
                        else
                        {
                            lblMensaje.Text = "Usuario o contraseña incorrectos";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }

        private string HashPasswordSimple(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        private void BtnRegister_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.ShowDialog();
        }

        // ===============================================
        //  MÉTODOS NUEVOS PARA LA VENTANA SIN BORDES
        // ===============================================

        // 1. Cerrar la aplicación
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // 2. Mover la ventana al hacer clic y arrastrar
        private void BarraTitulo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}