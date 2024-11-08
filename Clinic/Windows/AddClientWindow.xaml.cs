﻿using Clinic.AllClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
    public partial class AddClientWindow : Window
    {
        private AdmWindow admWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public AddClientWindow(AdmWindow admWindow)
        {
            InitializeComponent();
            this.admWindow = admWindow;
        }

        private void PhoneNumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "+7 xxx xxx xx xx")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void PhoneNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "+7 xxx xxx xx xx";
                textBox.Foreground = Brushes.LightGray;
            }
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (char.IsDigit(e.Text, 0) && textBox.Text.Length < 16)
            {
                if (textBox.Text.Length == 0)
                {
                    textBox.Text = "+7 ";
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else if (textBox.Text.Length == 6 || textBox.Text.Length == 10 || textBox.Text.Length == 13)
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

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
                    genderComboBox.Items.Insert(0, "Не выбрано");

                    while (reader.Read())
                    {
                        genderComboBox.Items.Add(reader["Name"].ToString());
                    }

                    reader.Close();
                    genderComboBox.SelectedIndex = 0;
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

        private bool CheckExistingLogin(string login)
        {
            string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string clientQuery = "SELECT COUNT(*) FROM Client WHERE Login = @Login";
                SqlCommand clientCommand = new SqlCommand(clientQuery, connection);
                clientCommand.Parameters.AddWithValue("@Login", login);

                string userQuery = "SELECT COUNT(*) FROM [User] WHERE Login = @Login";
                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@Login", login);

                try
                {
                    connection.Open();

                    int clientCount = (int)clientCommand.ExecuteScalar();
                    if (clientCount > 0)
                    {
                        MessageBox.Show("Логин уже существует");
                        return true;
                    }

                    int userCount = (int)userCommand.ExecuteScalar();
                    if (userCount > 0)
                    {
                        MessageBox.Show("Логин уже существует");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке логина: " + ex.Message);
                }
            }

            return false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

            string fullName = fullNameTextBox.Text;
            string phoneNumber = phoneTextBox.Text;
            string address = adressTextBox.Text;
            string policyNumber = policyNumberTextBox.Text;
            DateTime dob = dobDatePicker.SelectedDate ?? DateTime.Now;
            int age = DateTime.Now.Year - dob.Year;
            string gender = genderComboBox.SelectedItem?.ToString();
            string login = loginTextBox.Text;
            string password = passwordBox.Password;
            string email = emailTextBox.Text;
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            string phonePattern = @"^\+7 \d{3} \d{3} \d{2} \d{2}$";
            string policyPattern = @"^\d{4} \d{4} \d{4} \d{4}$";

            if (string.IsNullOrWhiteSpace(login) || login.Contains(" ") || login.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Введи логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (fullName.Contains(" ") && fullName.Length >= 10)
            {

            }
            else
            {
                MessageBox.Show("Введи фамилию и имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (genderComboBox.Text == "Не выбрано")
            {
                MessageBox.Show("Выбери пол", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Contains(" ") || password.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Введи пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (email.Contains(" ") || email.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Некорретный формат электронной почты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Введите адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(policyNumber))
            {
                MessageBox.Show("Введите полис", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CheckExistingLogin(login))
            {
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

            if (!Regex.IsMatch(policyNumber, policyPattern))
            {
                MessageBox.Show("Некорректный формат номера полиса. Введите номер в формате xxxx xxxx xxxx xxxx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error );
                return;
            }

            if (!Regex.IsMatch(phoneNumber, phonePattern))
            {
                MessageBox.Show("Некорректный формат номера телефона. Пожалуйста, введите номер в формате +7 xxx xxx xx xx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Regex.IsMatch(fullName, @"\d"))
            {
                MessageBox.Show("ФИО не может содержать цифры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Regex.IsMatch(fullName, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
            {
                MessageBox.Show("ФИО не может содержать специальные символы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (login.Length < 4)
            {
                MessageBox.Show("Логин должен быть больше 4 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть больше 4 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string encryptedPassword = EncryptionHelper.Encrypt(password, "6uH8#bgZpE$@2sD1");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Client (FullName, DoB, Phone, Adress, PolicyNumber, ClientEmail, GenderID, Login, Password) " +
                               "VALUES (@FullName, @DoB, @Phone, @Address, @PolicyNumber, @ClientEmail, @GenderID, @Login, @Password)";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@DoB", dob);
                command.Parameters.AddWithValue("@Phone", phoneNumber);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@PolicyNumber", policyNumber);
                command.Parameters.AddWithValue("@ClientEmail", email);
                command.Parameters.AddWithValue("@GenderID", GetGenderId(gender));
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", encryptedPassword);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Вы успешно зарегистрировали клиента", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        admWindow.LoadClients();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось зарегистрироваться", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
                }
            }
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


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            admWindow.Show();
        }
    }
}
