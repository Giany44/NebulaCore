using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaCore
{
    public partial class MainWindow : Window
    {
        private int intentos = 0;
        private bool isBloqueado = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- BARRA DE TÍTULO CUSTOM ---
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        // --- LÓGICA VER / OCULTAR CONTRASEÑA ---
        private void BtnShowPass_Click(object sender, RoutedEventArgs e)
        {
            if (btnShowPass.IsChecked == true)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
            }
        }
        // Sincronizar cambios manuales
        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible) txtPasswordVisible.Text = txtPassword.Password;
        }
        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Visible) txtPassword.Password = txtPasswordVisible.Text;
        }

        // --- LÓGICA DE LOGIN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (isBloqueado) { lblMensaje.Text = "TERMINAL BLOQUEADO"; return; }

            string u = txtUsuario.Text;
            string p = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                lblMensaje.Text = "DATOS REQUERIDOS";
                return;
            }

            // Usamos nuestro sistema NebulaSystem (JSON + HASH)
            if (NebulaSystem.ValidateLogin(u, p))
            {
                intentos = 0;
                HomeWindow home = new HomeWindow(u);
                home.Show();
                this.Close();
            }
            else
            {
                intentos++;
                lblMensaje.Text = "CREDENCIALES INVÁLIDAS";

                // Efecto visual de error (sacudir ventana sería top, pero dejemos el mensaje)
                if (intentos >= 3)
                {
                    isBloqueado = true;
                    lblMensaje.Text = "SISTEMA DE SEGURIDAD ACTIVADO";
                    btnLogin.IsEnabled = false;
                    btnLogin.Opacity = 0.5;
                }
            }
        }

        // --- SISTEMA DE REGISTRO RÁPIDO ---
        private void BtnRegister_Click(object sender, MouseButtonEventArgs e)
        {
            string u = txtUsuario.Text;
            string p = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                lblMensaje.Text = "INTRODUZCA USUARIO Y PASS PARA REGISTRAR";
                return;
            }

            bool exito = NebulaSystem.RegisterUser(u, p);
            if (exito)
            {
                lblMensaje.Foreground = System.Windows.Media.Brushes.LightGreen;
                lblMensaje.Text = "USUARIO REGISTRADO. INICIE SESIÓN.";
            }
            else
            {
                lblMensaje.Foreground = System.Windows.Media.Brushes.Red;
                lblMensaje.Text = "EL USUARIO YA EXISTE.";
            }
        }
    }
}