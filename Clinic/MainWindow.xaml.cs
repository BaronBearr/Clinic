using Clinic.Windows;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Clinic.AllClass;

namespace Clinic
{
    public partial class MainWindow : Window
    {
        private RegistrationWindow registrationWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();

            if (Application.Current.Properties.Contains("LastLoggedInUsername"))
            {
                txtUsername.Text = (string)Application.Current.Properties["LastLoggedInUsername"];
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }
        private void LogUserLogin(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO UserLogins (UserId, LoginTime) VALUES (@UserId, @LoginTime)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@LoginTime", DateTime.Now);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при записи статистики входа: " + ex.Message);
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            bool isAuthenticated = AuthenticateUser(username, password);
            bool isClientAuthenticated = AuthenticateClient(username, password);

            if (isAuthenticated)
            {
                Application.Current.Properties["LastLoggedInUsername"] = username;
                int userId = GetUserId(username);

                LogUserLogin(userId);
                int roleId = GetUserRoleId(username);

                if (roleId == 1)
                {
                    AdmWindow admWindow = new AdmWindow(userId);
                    admWindow.Show();
                    this.Close();
                }
                else if (roleId == 2)
                {
                    UserWindow userWindow = new UserWindow(userId);
                    userWindow.Show();
                    this.Close();
                }
            }
            else if (isClientAuthenticated)
            {
                Application.Current.Properties["LastLoggedInUsername"] = username;
                int clientId = GetClientId(username);
                ClientWindow clientWindow = new ClientWindow(clientId);
                clientWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private int GetUserId(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID FROM [User] WHERE Login = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                object result = command.ExecuteScalar();

                return result == null ? -1 : Convert.ToInt32(result);
            }
        }

        private int GetClientId(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID FROM Client WHERE Login = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                object result = command.ExecuteScalar();

                return result == null ? -1 : Convert.ToInt32(result);
            }
        }

        private bool AuthenticateClient(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Password FROM Client WHERE Login = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string decryptedPassword = EncryptionHelper.Decrypt(result.ToString(), "6uH8#bgZpE$@2sD1");

                        if (decryptedPassword == password)
                        {
                            return true; 
                        }
                    }

                    return false; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при аутентификации: " + ex.Message);
                    return false;
                }
            }
        }
        private bool AuthenticateUser(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Password FROM [User] WHERE Login = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string decryptedPassword = EncryptionHelper.Decrypt(result.ToString(), "6uH8#bgZpE$@2sD1");

                        if (decryptedPassword == password)
                        {
                            return true; 
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при аутентификации: " + ex.Message);
                    return false;
                }
            }
        }

        private int GetUserRoleId(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT RoleID FROM [User] WHERE Login = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                object result = command.ExecuteScalar();

                return result == null ? -1 : Convert.ToInt32(result);
            }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Red;
            ((TextBlock)sender).TextDecorations = null;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Blue;
            ((TextBlock)sender).TextDecorations = TextDecorations.Underline; 
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (registrationWindow == null || !registrationWindow.IsVisible)
            {
                registrationWindow = new RegistrationWindow();
                registrationWindow.Show();
            }
            else
            {
                registrationWindow.Activate();
            }
        }
    }
}
