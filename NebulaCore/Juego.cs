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
        public string ImagenUrl { get; set; }

        // --- NUEVOS CAMPOS PARA EL 10 ---
        public string Fabricante { get; set; } // Requisito Práctica 5
        public bool Visible { get; set; }      // Requisito Práctica 5 (Admin)

        public string PrecioFormato => $"{Precio} €";
        public string TextoBoton { get; set; } = "COMPRAR";

        // Propiedad visual para saber si mostrarlo semitransparente en Admin si está oculto
        public string OpacidadVisual => Visible ? "1.0" : "0.5";
    }
}