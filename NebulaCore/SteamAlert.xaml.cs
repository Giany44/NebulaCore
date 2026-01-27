using System.Windows;

namespace NebulaCore
{
    public partial class SteamAlert : Window
    {
        public bool Confirmado { get; private set; } = false;

        public SteamAlert(string titulo, string mensaje, bool esPregunta = true)
        {
            InitializeComponent();
            lblTitulo.Text = titulo.ToUpper();
            lblMensaje.Text = mensaje;

            // Si no es una pregunta (es solo información), ocultamos el botón cancelar
            if (!esPregunta)
            {
                btnCancelar.Visibility = Visibility.Collapsed;
                btnAceptar.Content = "ENTENDIDO";
            }
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.Close();
        }
    }
}