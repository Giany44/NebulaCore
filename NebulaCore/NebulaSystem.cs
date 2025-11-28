using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json; // Necesario para guardar en archivo

namespace NebulaCore
{
    // Clase para representar un Usuario
    public class UserData
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Guardamos el HASH, no la pass
        public DateTime RegistrationDate { get; set; }
    }

    public static class NebulaSystem
    {
        private static string dbPath = "nebula_users.json";
        public static List<UserData> Users { get; private set; }

        // Constructor estático: Carga los datos al iniciar la app
        static NebulaSystem()
        {
            LoadData();
        }

        // ENCRIPTACIÓN SHA256 (Nivel Profesional)
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Cargar usuarios del JSON
        private static void LoadData()
        {
            if (File.Exists(dbPath))
            {
                string json = File.ReadAllText(dbPath);
                Users = JsonSerializer.Deserialize<List<UserData>>(json);
            }
            else
            {
                Users = new List<UserData>();
                // Creamos un admin por defecto (Pass: 1234)
                RegisterUser("admin", "1234");
            }
        }

        // Guardar usuarios en JSON
        public static void SaveData()
        {
            string json = JsonSerializer.Serialize(Users);
            File.WriteAllText(dbPath, json);
        }

        public static bool RegisterUser(string user, string pass)
        {
            if (Users.Exists(u => u.Username == user)) return false; // Ya existe

            Users.Add(new UserData
            {
                Username = user,
                PasswordHash = HashPassword(pass),
                RegistrationDate = DateTime.Now
            });
            SaveData();
            return true;
        }

        public static bool ValidateLogin(string user, string pass)
        {
            var targetUser = Users.Find(u => u.Username == user);
            if (targetUser == null) return false;

            // Comparamos el HASH de lo que escribió con el HASH guardado
            string inputHash = HashPassword(pass);
            return targetUser.PasswordHash == inputHash;
        }
    }
}