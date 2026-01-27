using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace NebulaCore
{
    public static class ConexionDB
    {
        // AJUSTA AQUÍ: Pon tu contraseña de MySQL donde dice 'tu_password'
        private static string cadenaConexion = "Server=localhost;Database=nebula_games;Uid=root;Pwd=;";

        public static MySqlConnection ObtenerConexion()
        {
            try
            {
                return new MySqlConnection(cadenaConexion);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
                return null;
            }
        }
    }
}