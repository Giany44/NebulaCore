using System.Windows;
using System.Windows.Input;

namespace NebulaCore
{
    public partial class HomeWindow : Window
    {
        public HomeWindow(string nombreUsuario)
        {
            InitializeComponent();
            if (lblUser != null)
                lblUser.Text = nombreUsuario.ToUpper();
        }

        // Mover ventana
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // Minimizar
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Cerrar Aplicación
        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Cerrar Sesión (Volver al Login)
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}