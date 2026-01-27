using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Necesario para el MouseDown
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace NebulaCore
{
    public partial class HomeWindow : Window
    {
        private string usuarioActual;
        private List<Juego> juegosEnPantalla = new List<Juego>();
        private bool modoBiblioteca = false; // ¿Estamos en tienda o biblioteca?

        public HomeWindow(string usuario)
        {
            InitializeComponent();
            usuarioActual = usuario;
            if (txtUsuario != null) txtUsuario.Text = usuarioActual.ToUpper();

            CargarTienda(); // Empezamos en la tienda
        }

        // --- 1. MODO TIENDA (VER TODOS) ---
        private void CargarTienda()
        {
            modoBiblioteca = false;
            lblTituloSeccion.Text = "CATÁLOGO DE LA TIENDA";
            ResaltarMenu("TIENDA");

            juegosEnPantalla.Clear();
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    // Seleccionamos todo lo que tenga stock
                    string sql = "SELECT * FROM videojuegos WHERE stock > 0";
                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            juegosEnPantalla.Add(new Juego
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Titulo = reader["titulo"].ToString(),
                                Descripcion = reader["descripcion"].ToString(),
                                Precio = Convert.ToDecimal(reader["precio"]),
                                Stock = Convert.ToInt32(reader["stock"]),
                                Genero = reader["genero"].ToString(),
                                ImagenUrl = reader["imagen_url"].ToString(),
                                TextoBoton = "COMPRAR" // En tienda se compra
                            });
                        }
                    }
                }
                listaJuegos.ItemsSource = null;
                listaJuegos.ItemsSource = juegosEnPantalla;
            }
            catch (Exception ex) { MessageBox.Show("Error tienda: " + ex.Message); }
        }

        // --- 2. MODO BIBLIOTECA (VER COMPRADOS) ---
        private void CargarBiblioteca()
        {
            modoBiblioteca = true;
            lblTituloSeccion.Text = "MI BIBLIOTECA DE JUEGOS";
            ResaltarMenu("BIBLIOTECA");

            juegosEnPantalla.Clear();
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    // JOIN para traer solo lo que este usuario ha comprado
                    string sql = @"SELECT v.fecha, j.* FROM ventas v 
                                   JOIN videojuegos j ON v.juego_id = j.id 
                                   WHERE v.usuario = @u ORDER BY v.fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", usuarioActual);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            juegosEnPantalla.Add(new Juego
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Titulo = reader["titulo"].ToString(),
                                Descripcion = reader["descripcion"].ToString(),
                                Genero = reader["genero"].ToString(),
                                ImagenUrl = reader["imagen_url"].ToString(),
                                Precio = 0, // En biblioteca ya es tuyo
                                TextoBoton = "JUGAR" // En biblioteca se juega
                            });
                        }
                    }
                }
                listaJuegos.ItemsSource = null;
                listaJuegos.ItemsSource = juegosEnPantalla;

                if (juegosEnPantalla.Count == 0)
                    MessageBox.Show("Tu biblioteca está vacía. ¡Ve a la tienda a comprar algo!");
            }
            catch (Exception ex) { MessageBox.Show("Error biblioteca: " + ex.Message); }
        }

        // --- 3. BOTÓN UNIFICADO (ACCION SEGÚN MODO) ---
        private void BtnAccion_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int idJuego = (int)btn.Tag;
            Juego juego = juegosEnPantalla.Find(j => j.Id == idJuego);

            if (modoBiblioteca)
            {
                // ESTAMOS EN BIBLIOTECA -> JUGAR
                MessageBox.Show($"Iniciando {juego.Titulo}...\n\n(Simulación de lanzamiento)", "JUGANDO 🎮");
            }
            else
            {
                // ESTAMOS EN TIENDA -> COMPRAR
                SteamAlert alerta = new SteamAlert("CONFIRMAR COMPRA",
                    $"¿Añadir {juego.Titulo} a tu biblioteca por {juego.PrecioFormato}?");
                alerta.ShowDialog();

                if (alerta.Confirmado)
                {
                    RealizarCompraBD(juego);
                }
            }
        }

        private void RealizarCompraBD(Juego juego)
        {
            try
            {
                using (MySqlConnection con = ConexionDB.ObtenerConexion())
                {
                    con.Open();
                    var trans = con.BeginTransaction();
                    try
                    {
                        // 1. Restar Stock
                        new MySqlCommand($"UPDATE videojuegos SET stock = stock - 1 WHERE id={juego.Id}", con, trans).ExecuteNonQuery();

                        // 2. Insertar Venta
                        var cmd = new MySqlCommand("INSERT INTO ventas (usuario, juego_id, precio_pagado) VALUES (@u, @jid, @p)", con, trans);
                        cmd.Parameters.AddWithValue("@u", usuarioActual);
                        cmd.Parameters.AddWithValue("@jid", juego.Id);
                        cmd.Parameters.AddWithValue("@p", juego.Precio);
                        cmd.ExecuteNonQuery();

                        trans.Commit();
                        new SteamAlert("¡COMPRA EXITOSA!", "Juego añadido a tu biblioteca.", false).ShowDialog();
                        CargarTienda(); // Recargar para actualizar stock
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error compra: " + ex.Message); }
        }

        // --- 4. EVENTOS DEL MENÚ ---
        private void BtnMenuTienda_Click(object sender, MouseButtonEventArgs e)
        {
            CargarTienda();
        }

        private void BtnMenuBiblioteca_Click(object sender, MouseButtonEventArgs e)
        {
            CargarBiblioteca();
        }

        // Efecto visual para saber en qué pestaña estamos
        private void ResaltarMenu(string menu)
        {
            var azul = (Brush)new BrushConverter().ConvertFrom("#66C0F4"); // Azul Steam
            var gris = (Brush)new BrushConverter().ConvertFrom("#8F98A0"); // Gris apagado

            if (menu == "TIENDA")
            {
                menuTienda.Foreground = azul;
                menuBiblioteca.Foreground = gris;
            }
            else
            {
                menuTienda.Foreground = gris;
                menuBiblioteca.Foreground = azul;
            }
        }

        // --- FILTROS Y BÚSQUEDA ---
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtBuscar.Text.ToLower();
            var filtrados = juegosEnPantalla.Where(j => j.Titulo.ToLower().Contains(texto)).ToList();
            listaJuegos.ItemsSource = filtrados;
        }

        private void BtnFiltro_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string cat = btn.Tag.ToString();
            txtBuscar.Text = "";

            if (cat == "Todo") listaJuegos.ItemsSource = juegosEnPantalla;
            else listaJuegos.ItemsSource = juegosEnPantalla.Where(j => j.Genero == cat).ToList();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}