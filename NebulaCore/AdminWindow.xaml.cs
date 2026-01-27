using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Diagnostics; // <--- IMPRESCINDIBLE PARA EL BOTÓN DE GOOGLE

namespace NebulaCore
{
    public partial class AdminWindow : Window
    {
        // Variables para controlar qué estamos editando
        private int idUsuarioSeleccionado = 0;
        private int idJuegoSeleccionado = 0;

        public AdminWindow()
        {
            InitializeComponent();
            CargarUsuarios();
            CargarJuegos(); // Carga inicial de juegos
            CargarLogs();   // Carga inicial de logs
        }

        // =========================================================
        //                 PESTAÑA 1: USUARIOS
        // =========================================================
        private void CargarUsuarios(string busqueda = "")
        {
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "SELECT * FROM usuarios";
                    if (!string.IsNullOrEmpty(busqueda)) sql += " WHERE nombre_usuario LIKE @s OR id = @idBusqueda";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        cmd.Parameters.AddWithValue("@s", "%" + busqueda + "%");
                        int.TryParse(busqueda, out int idB);
                        cmd.Parameters.AddWithValue("@idBusqueda", idB);
                    }

                    MySqlDataAdapter adaptador = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);
                    gridUsuarios.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error usuarios: " + ex.Message); }
        }

        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)gridUsuarios.SelectedItem;
            if (row != null)
            {
                idUsuarioSeleccionado = Convert.ToInt32(row["id"]);
                txtID.Text = row["id"].ToString();
                txtNombre.Text = row["nombre_usuario"].ToString();
                txtEmail.Text = row["email"].ToString();
                txtEstado.Text = row["estado"].ToString();
                cmbRol.Text = row["rol"].ToString();
                txtMotivo.Text = row["motivo_baneo"] != DBNull.Value ? row["motivo_baneo"].ToString() : "";
                txtPass.Password = "";
            }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtPass.Password))
            { MessageBox.Show("Nombre y contraseña obligatorios"); return; }

            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "INSERT INTO usuarios (nombre_usuario, password, email, rol, estado) VALUES (@u, @p, @e, @r, 'Activo')";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@p", HashPassword(txtPass.Password));
                    cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@r", cmbRol.Text);
                    cmd.ExecuteNonQuery();

                    RegistrarLog("Creó usuario: " + txtNombre.Text);
                    MessageBox.Show("Usuario creado.");
                    LimpiarFormularioUsuario();
                    CargarUsuarios();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == 0) return;
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = string.IsNullOrEmpty(txtPass.Password) ?
                        "UPDATE usuarios SET nombre_usuario=@u, email=@e, rol=@r WHERE id=@id" :
                        "UPDATE usuarios SET nombre_usuario=@u, password=@p, email=@e, rol=@r WHERE id=@id";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@r", cmbRol.Text);
                    cmd.Parameters.AddWithValue("@id", idUsuarioSeleccionado);
                    if (!string.IsNullOrEmpty(txtPass.Password)) cmd.Parameters.AddWithValue("@p", HashPassword(txtPass.Password));

                    cmd.ExecuteNonQuery();
                    RegistrarLog("Editó usuario ID: " + idUsuarioSeleccionado);
                    MessageBox.Show("Actualizado.");
                    CargarUsuarios();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnBanear_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == 0) return;
            string nuevoEstado = (txtEstado.Text == "Baneado") ? "Activo" : "Baneado";

            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "UPDATE usuarios SET estado=@est, motivo_baneo=@mot WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@est", nuevoEstado);
                    cmd.Parameters.AddWithValue("@mot", txtMotivo.Text);
                    cmd.Parameters.AddWithValue("@id", idUsuarioSeleccionado);
                    cmd.ExecuteNonQuery();

                    RegistrarLog($"Cambió estado a {nuevoEstado} (ID: {idUsuarioSeleccionado})");
                    MessageBox.Show("Estado cambiado a: " + nuevoEstado);
                    CargarUsuarios();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == 0) return;
            if (MessageBox.Show("¿Eliminar usuario?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = ConexionDB.ObtenerConexion())
                    {
                        con.Open();
                        new MySqlCommand($"DELETE FROM usuarios WHERE id={idUsuarioSeleccionado}", con).ExecuteNonQuery();
                        RegistrarLog("Eliminó usuario ID: " + idUsuarioSeleccionado);
                        LimpiarFormularioUsuario();
                        CargarUsuarios();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e) => CargarUsuarios(txtBusqueda.Text);
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormularioUsuario();

        private void LimpiarFormularioUsuario()
        {
            idUsuarioSeleccionado = 0;
            txtID.Clear(); txtNombre.Clear(); txtEmail.Clear(); txtPass.Clear(); txtMotivo.Clear();
            gridUsuarios.SelectedIndex = -1;
        }

        // =========================================================
        //                 PESTAÑA 2: JUEGOS
        // =========================================================
        private void CargarJuegos()
        {
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM videojuegos", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gridJuegos.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error cargando juegos: " + ex.Message); }
        }

        private void GridJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)gridJuegos.SelectedItem;
            if (row != null)
            {
                idJuegoSeleccionado = Convert.ToInt32(row["id"]);
                txtJuegoID.Text = row["id"].ToString();
                txtJuegoTitulo.Text = row["titulo"].ToString();
                txtJuegoDesc.Text = row["descripcion"].ToString();
                txtJuegoPrecio.Text = row["precio"].ToString();
                txtJuegoStock.Text = row["stock"].ToString();
                cmbJuegoGenero.Text = row["genero"].ToString();
                txtJuegoImagen.Text = row["imagen_url"].ToString();
            }
        }

        // --- ESTE ES EL BOTÓN QUE TE DABA ERROR ---
        private void BtnBuscarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJuegoTitulo.Text))
            {
                MessageBox.Show("Escribe primero el título del juego para buscarlo.");
                return;
            }

            // Busca en Google: "NombreJuego steam library cover 600x900"
            string busqueda = txtJuegoTitulo.Text.Replace(" ", "+") + "+steam+library+cover+600x900";
            string urlGoogle = $"https://www.google.com/search?q={busqueda}&tbm=isch";

            // Abre el navegador predeterminado
            Process.Start(new ProcessStartInfo
            {
                FileName = urlGoogle,
                UseShellExecute = true
            });
        }

        private void BtnJuegoGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJuegoTitulo.Text) || string.IsNullOrWhiteSpace(txtJuegoPrecio.Text))
            { MessageBox.Show("Título y Precio obligatorios"); return; }

            // Lógica de Foto Automática
            string fotoFinal = txtJuegoImagen.Text;
            if (string.IsNullOrWhiteSpace(fotoFinal))
            {
                // Foto genérica si no pones nada
                fotoFinal = "https://cdn.cloudflare.steamstatic.com/steam/apps/105600/library_600x900_2x.jpg";
            }

            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;

                    if (idJuegoSeleccionado == 0)
                    {
                        cmd.CommandText = "INSERT INTO videojuegos (titulo, descripcion, precio, stock, genero, imagen_url) VALUES (@t, @d, @p, @s, @g, @img)";
                        RegistrarLog("Añadió juego: " + txtJuegoTitulo.Text);
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE videojuegos SET titulo=@t, descripcion=@d, precio=@p, stock=@s, genero=@g, imagen_url=@img WHERE id=@id";
                        cmd.Parameters.AddWithValue("@id", idJuegoSeleccionado);
                        RegistrarLog("Editó juego: " + txtJuegoTitulo.Text);
                    }

                    cmd.Parameters.AddWithValue("@t", txtJuegoTitulo.Text);
                    cmd.Parameters.AddWithValue("@d", txtJuegoDesc.Text);
                    cmd.Parameters.AddWithValue("@p", Convert.ToDecimal(txtJuegoPrecio.Text));
                    cmd.Parameters.AddWithValue("@s", Convert.ToInt32(txtJuegoStock.Text));
                    cmd.Parameters.AddWithValue("@g", cmbJuegoGenero.Text);
                    cmd.Parameters.AddWithValue("@img", fotoFinal);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Juego guardado correctamente.");
                    LimpiarFormularioJuego();
                    CargarJuegos();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error guardando juego: " + ex.Message); }
        }

        private void BtnJuegoEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idJuegoSeleccionado == 0) return;
            if (MessageBox.Show("¿Borrar este juego?", "Alerta", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = ConexionDB.ObtenerConexion())
                    {
                        con.Open();
                        new MySqlCommand($"DELETE FROM videojuegos WHERE id={idJuegoSeleccionado}", con).ExecuteNonQuery();
                        RegistrarLog("Eliminó juego ID: " + idJuegoSeleccionado);
                        LimpiarFormularioJuego();
                        CargarJuegos();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void BtnJuegoLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormularioJuego();
        private void BtnRefrescarJuegos_Click(object sender, RoutedEventArgs e) => CargarJuegos();

        private void LimpiarFormularioJuego()
        {
            idJuegoSeleccionado = 0;
            txtJuegoID.Clear(); txtJuegoTitulo.Clear(); txtJuegoDesc.Clear();
            txtJuegoPrecio.Clear(); txtJuegoStock.Clear(); txtJuegoImagen.Clear();
            gridJuegos.SelectedIndex = -1;
        }

        // =========================================================
        //                 PESTAÑA 3: LOGS
        // =========================================================
        private void CargarLogs()
        {
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "SELECT * FROM log_actividad ORDER BY fecha DESC";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gridLogs.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error logs: " + ex.Message); }
        }

        private void BtnRefrescarLogs_Click(object sender, RoutedEventArgs e) => CargarLogs();

        private void RegistrarLog(string accion)
        {
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    string sql = "INSERT INTO log_actividad (admin_responsable, accion_realizada) VALUES ('admin', @a)";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@a", accion);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* Ignorar errores de log */ }
            CargarLogs();
        }

        // =========================================================
        //                 UTILIDADES
        // =========================================================
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private string HashPassword(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}