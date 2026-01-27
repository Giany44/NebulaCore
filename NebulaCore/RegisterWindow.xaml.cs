using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NebulaCore
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtRegUsuario.Text;
            string email = txtRegEmail.Text;
            // ENCRIPTAMOS AQUÍ PARA QUE SE GUARDE EL CÓDIGO LARGO EN LA DB
            string passEncriptada = HashPasswordSimple(txtRegPassword.Password);

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(txtRegPassword.Password))
            {
                MessageBox.Show("Completa los campos obligatorios.");
                return;
            }

            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "INSERT INTO usuarios (nombre_usuario, email, password, rol, estado) VALUES (@u, @e, @p, 'user', 'activo')";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.AddWithValue("@p", passEncriptada);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("¡Usuario '" + usuario + "' creado con éxito!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar: " + ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // FUNCIÓN IDÉNTICA A LA DEL LOGIN
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
    }
}