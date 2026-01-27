using System;

namespace NebulaCore
{
    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Genero { get; set; }

        // --- PROPIEDADES VISUALES IMPORTANTES ---
        public string ImagenUrl { get; set; } // <--- ESTA ES LA FOTO

        // Esto mostrará el precio con el símbolo de Euro
        public string PrecioFormato => $"{Precio} €";

        // Esto nos servirá para cambiar el texto del botón (Comprar vs Jugar)
        public string TextoBoton { get; set; } = "COMPRAR";

        // Para saber si activar el botón o no (si hay stock o si ya es mío)
        public bool EsComprable { get; set; } = true;
    }
}