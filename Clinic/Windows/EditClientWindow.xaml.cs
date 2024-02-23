using Clinic.AllClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clinic.Windows
{
    public partial class EditClientWindow : Window
    {

        private AdmWindow admWindow;
        private Client selectedClient;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public EditClientWindow(AdmWindow admWindow, Client client)
        {
            InitializeComponent();
            this.admWindow = admWindow;
            selectedClient = client;
            fullNameTextBox.Text = selectedClient.FullName;
            dobDatePicker.SelectedDate = selectedClient.DoB;
            phoneTextBox.Text = selectedClient.Phone;
            adressTextBox.Text = selectedClient.Adress;
            policyNumberTextBox.Text = selectedClient.PolicyNumber;
            loginTextBox.Text = selectedClient.Login;
            passwordBox.Visibility = Visibility.Visible;
            emailTextBox.Text = selectedClient.ClientEmail;

            string passwordQuery = "SELECT Password FROM Client WHERE Login = @login";
            LoadPassword(passwordBox, passwordQuery, selectedClient.Login);
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

            string fullname = fullNameTextBox.Text;
            DateTime dob = dobDatePicker.SelectedDate ?? DateTime.Now;
            string phone = phoneTextBox.Text;
            string adress = adressTextBox.Text;
            string polis = policyNumberTextBox.Text;
            string gender = genderComboBox.SelectedItem?.ToString();
            string login = loginTextBox.Text;
            int age = DateTime.Now.Year - dob.Year;
            string newpassword = passwordBox.Password;
            string email = emailTextBox.Text;
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            string phonePattern = @"^\+7 \d{3} \d{3} \d{2} \d{2}$";
            string policyPattern = @"^\d{4} \d{4} \d{4} \d{4}$";

            if (string.IsNullOrWhiteSpace(login) || login.Contains(" ") || login.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Введи логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (fullname.Contains(" "))
            {
                string[] nameParts = fullname.Split(' ');

                if (nameParts.Length >= 2 && nameParts[0].Length >= 2 && nameParts[1].Length >= 4)
                {

                }
                else
                {
                    MessageBox.Show("Имя должно иметь минимум 2 символа, фамилия должна иметь минимум 4 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Введи фамилию и имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DateTime.Now.Month < dob.Month || (DateTime.Now.Month == dob.Month && DateTime.Now.Day < dob.Day))
            {
                age--;
            }

            if (age < 18 || age > 99)
            {
                MessageBox.Show("Возраст должен быть от 18 до 99 лет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(newpassword) || newpassword.Contains(" ") || newpassword.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Введи пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (email.Contains(" ") || email.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Некорретный формат электронной почты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(adress))
            {
                MessageBox.Show("Введите адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(polis))
            {
                MessageBox.Show("Введите полис", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullname))
            {
                MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (genderComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите ваш пол", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dobDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dob > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Некорректный формат электронной почты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(polis, policyPattern))
            {
                MessageBox.Show("Некорректный формат номера полиса. Введите номер в формате xxxx xxxx xxxx xxxx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(phone, phonePattern))
            {
                MessageBox.Show("Некорректный формат номера телефона. Пожалуйста, введите номер в формате +7 xxx xxx xx xx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Regex.IsMatch(fullname, @"\d"))
            {
                MessageBox.Show("ФИО не может содержать цифры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Regex.IsMatch(fullname, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
            {
                MessageBox.Show("ФИО не может содержать специальные символы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (login.Length < 4)
            {
                MessageBox.Show("Логин должен быть больше 4 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newpassword.Length < 4 || !ContainsDigit(newpassword) || !ContainsUppercase(newpassword) || CountLowercaseLetters(newpassword) < 2)
            {
                MessageBox.Show("Пароль должен быть больше 4 символов и соответствовать условиям:\n (минимум 1 цифра, 1 заглавная буква, 2 строчные буквы).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string newLogin = loginTextBox.Text;
                int userId = GetClientID(connection, selectedClient.Login);

                if (newLogin != selectedClient.Login)
                {
                    if (!IsLoginUnique(newLogin, userId))
                    {
                        MessageBox.Show("Логин уже существует. Пожалуйста, выберите другой логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                string encryptedPassword = EncryptionHelper.Encrypt(newpassword, "6uH8#bgZpE$@2sD1");


                string query = "UPDATE Client SET " +
                               "FullName = @FullName, " +
                               "DoB = @DoB, " +
                               "Phone = @Phone, " +
                               "Adress = @Address, " +
                               "PolicyNumber = @PolicyNumber, " +
                               "ClientEmail = @ClientEmail, " +
                               "GenderID = @GenderID, " +
                               "Login = @NewLogin, " +
                               "Password = @Password " +
                               "WHERE ClientID = @ClientID";  

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@FullName", fullname);
                command.Parameters.AddWithValue("@DoB", dob);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Address", adress);
                command.Parameters.AddWithValue("@PolicyNumber", polis);
                command.Parameters.AddWithValue("@ClientEmail", email);
                command.Parameters.AddWithValue("@GenderID", GetGenderId(gender));
                command.Parameters.AddWithValue("@NewLogin", newLogin);
                command.Parameters.AddWithValue("@Password", encryptedPassword);
                command.Parameters.AddWithValue("@ClientID", selectedClient.ClientID); 

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        admWindow.LoadClients();

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
                }
            }

        }

        private bool ContainsDigit(string password)
        {
            foreach (char c in password)
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ContainsUppercase(string password)
        {
            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    return true;
                }
            }
            return false;
        }

        private int CountLowercaseLetters(string password)
        {
            int count = 0;
            foreach (char c in password)
            {
                if (char.IsLower(c))
                {
                    count++;
                }
            }
            return count;
        }


        private bool IsLoginUnique(string login, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string queryUser = "SELECT COUNT(*) FROM [User] WHERE Login = @Login AND UserID <> @UserID";
                string queryClient = "SELECT COUNT(*) FROM [Client] WHERE Login = @Login";

                SqlCommand commandUser = new SqlCommand(queryUser, connection);
                commandUser.Parameters.AddWithValue("@Login", login);
                commandUser.Parameters.AddWithValue("@UserID", userId);

                SqlCommand commandClient = new SqlCommand(queryClient, connection);
                commandClient.Parameters.AddWithValue("@Login", login);

                connection.Open();

                int countUser = (int)commandUser.ExecuteScalar();
                int countClient = (int)commandClient.ExecuteScalar();

                return countUser == 0 && countClient == 0;
            }
        }

        private int GetClientID(SqlConnection connection, string login)
        {
            int userId = 0;
            string query = "SELECT ClientID FROM Client WHERE Login = @Login";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Login", login);

                try
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID пользователя: " + ex.Message);
                }
            }
            return userId;
        }

        private int GetGenderId(string genderName)
        {
            int genderId = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT GenderID FROM Gender WHERE Name = @Name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", genderName);

                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        genderId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID пола: " + ex.Message);
                }
            }

            return genderId;
        }

        private void LoadPassword(PasswordBox passwordTextBox, string query, string login)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@login", login);
                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string decryptedPassword = EncryptionHelper.Decrypt(result.ToString(), "6uH8#bgZpE$@2sD1");
                        passwordBox.Password = decryptedPassword;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке пароля: " + ex.Message);
            }
        }

        private void LoadGenders()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Name FROM Gender";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        genderComboBox.Items.Add(reader["Name"].ToString());
                    }

                    reader.Close();

                    genderComboBox.SelectedItem = selectedClient.GenderName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        private void genderComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenders();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PolisTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (char.IsDigit(e.Text, 0) && textBox.Text.Length < 19)
            {
                if (textBox.Text.Length == 4 || textBox.Text.Length == 9 || textBox.Text.Length == 14)
                {
                    textBox.Text += " " + e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else
                {
                    textBox.Text += e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            e.Handled = true;
        }

    }
}
