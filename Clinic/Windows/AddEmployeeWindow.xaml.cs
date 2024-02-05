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
using Clinic.AllClass;
using Clinic.Windows;

namespace Clinic.Windows
{
    public partial class AddEmployeeWindow : Window
    {
        private AdmWindow admWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        public AddEmployeeWindow(AdmWindow admWindow)
        {
            InitializeComponent();
            LoadComboBoxData();
            this.admWindow = admWindow;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }
        private void LoadComboBoxData()
        {
            string roleQuery = "SELECT Name FROM Role";
            LoadDataToComboBox(roleComboBox, roleQuery);

            string categoryQuery = "SELECT Name FROM Category";
            LoadDataToComboBox(categoryComboBox, categoryQuery);

            string jobTitleQuery = "SELECT Name FROM JobTitle";
            LoadDataToComboBox(jobTitleComboBox, jobTitleQuery);
        }

        private void LoadDataToComboBox(ComboBox comboBox, string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        comboBox.Items.Add(reader.GetString(0));
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCategory = categoryComboBox.SelectedItem?.ToString();

            if (selectedCategory != null)
            {
                jobTitleComboBox.Items.Clear(); 

                switch (selectedCategory)
                {
                    case "Администрация":
                        jobTitleComboBox.Items.Add("Главный врач");
                        break;
                    case "Общие":
                        jobTitleComboBox.Items.Add("Терапевт");
                        jobTitleComboBox.Items.Add("Кардиолог");
                        jobTitleComboBox.Items.Add("Невролог");
                        break;
                    case "Хирургия":
                        jobTitleComboBox.Items.Add("Хирург");
                        jobTitleComboBox.Items.Add("Акушер");
                        jobTitleComboBox.Items.Add("Стоматолог");
                        break;
                    case "Другие":
                        jobTitleComboBox.Items.Add("Диетолог");
                        jobTitleComboBox.Items.Add("Логопед");
                        jobTitleComboBox.Items.Add("Психиатр");
                        break;
                    default:
                        break;
                }
            }
        }

        private void phoneTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "+7 xxx xxx xx xx")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void phoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "+7 xxx xxx xx xx";
                textBox.Foreground = Brushes.LightGray;
            }
        }

        private void phoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
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
            catch (Exception ex)
            {

            }
        }

        private void ClearFields()
        {
            fullNameTextBox.Text = "";
            dobDatePicker.SelectedDate = null;
            emailTextBox.Text = "";
            loginTextBox.Text = "";
            passwordBox.Password = "";
            phoneTextBox.Text = "";
            experienceTextBox.Text = "";
            roleComboBox.SelectedItem = null;
            categoryComboBox.SelectedItem = null;
            jobTitleComboBox.SelectedItem = null;
        }
        private bool IsLoginUnique(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string userQuery = "SELECT COUNT(*) FROM [User] WHERE Login = @Login";
                string clientQuery = "SELECT COUNT(*) FROM Client WHERE Login = @Login";

                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                SqlCommand clientCommand = new SqlCommand(clientQuery, connection);

                userCommand.Parameters.AddWithValue("@Login", login);
                clientCommand.Parameters.AddWithValue("@Login", login);

                connection.Open();

                int userCount = (int)userCommand.ExecuteScalar();
                int clientCount = (int)clientCommand.ExecuteScalar();

                return userCount == 0 && clientCount == 0;
            }
        }

        private void addEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = fullNameTextBox.Text;
                DateTime dob = dobDatePicker.SelectedDate ?? DateTime.Now;
                string email = emailTextBox.Text;
                string login = loginTextBox.Text;
                string password = passwordBox.Password;
                string phone = phoneTextBox.Text;
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                string phonePattern = @"^\+7 \d{3} \d{3} \d{2} \d{2}$";
                int experience = 0;
                string selectedRole = roleComboBox.SelectedItem?.ToString();
                string selectedCategory = categoryComboBox.SelectedItem?.ToString();
                string selectedJobTitle = jobTitleComboBox.SelectedItem?.ToString();

                if ((selectedRole == "Администратор" && selectedCategory != "Администрация") ||
            (selectedJobTitle == "Главный врач" && selectedRole == "Сотрудник" && selectedCategory == "Администрация") ||
            (selectedRole == "Администратор" && selectedCategory == "Администрация" && selectedJobTitle == "Главный врач"))

                {
                    MessageBox.Show("Администратор уже существует.");
                    return;
                }

                if (!string.IsNullOrEmpty(experienceTextBox.Text))
                {
                    int.TryParse(experienceTextBox.Text, out experience);
                }


                if (experience <= 0)
                {
                    MessageBox.Show("Стаж должен быть больше 0.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("Введи ФИО.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(login) || login.Contains(" ") || login.Any(char.IsWhiteSpace))
                {
                    MessageBox.Show("Введи логин.");
                    return;
                }

                if (login.Length < 4)
                {
                    MessageBox.Show("Логин должен быть больше 4 символов.");
                    return;
                }

                if (password.Length < 4)
                {
                    MessageBox.Show("Пароль должен быть больше 4 символов.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(password) || password.Contains(" ") || password.Any(char.IsWhiteSpace))
                {
                    MessageBox.Show("Введи пароль.");
                    return;
                }

                if (email.Contains(" ") || email.Any(char.IsWhiteSpace))
                {
                    MessageBox.Show("Некорретный формат электронной почты");
                    return;
                }

                if (jobTitleComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите вашу должность");
                    return;
                }

                if (roleComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите вашу роль");
                    return;
                }

                if (categoryComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите вашу категорию");
                    return;
                }

                if (dobDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату рождения");
                    return;
                }

                if (dob > DateTime.Now)
                {
                    MessageBox.Show("Дата рождения не может быть в будущем");
                    return;
                }

                if (!Regex.IsMatch(email, emailPattern))
                {
                    MessageBox.Show("Некорректный формат электронной почты");
                    return;
                }

                if (!Regex.IsMatch(phone, phonePattern))
                {
                    MessageBox.Show("Некорректный формат номера телефона. Пожалуйста, введите номер в формате +7 xxx xxx xx xx");
                    return;
                }

                if (Regex.IsMatch(fullName, @"\d"))
                {
                    MessageBox.Show("ФИО не может содержать цифры");
                    return;
                }

                if (Regex.IsMatch(fullName, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
                {
                    MessageBox.Show("ФИО не может содержать специальные символы");
                    return;
                }

                if (Regex.IsMatch(login, @"[!@#$%^&*()+=\[{\]};:<>|./?,-]"))
                {
                    MessageBox.Show("Логин не может содержать специальные символы, кроме '_' ");
                    return;
                }

                if (!IsLoginUnique(login))
                {
                    MessageBox.Show("Логин уже существует. Пожалуйста, выберите другой логин.");
                    return;
                }

                int roleId = GetSelectedRoleId(roleComboBox.SelectedItem?.ToString());
                int categoryId = GetSelectedCategoryId(categoryComboBox.SelectedItem?.ToString());
                int jobTitleId = GetSelectedJobTitleId(jobTitleComboBox.SelectedItem?.ToString());

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO [User] (FullName, DoB, JobTitleID, Email, Login, Password, Phone, RoleID, Expirience, CategoryID) 
                         VALUES (@FullName, @DoB, @JobTitleID, @Email, @Login, @Password, @Phone, @RoleID, @Expirience, @CategoryID)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@DoB", dob);
                    command.Parameters.AddWithValue("@JobTitleID", jobTitleId);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@RoleID", roleId);
                    command.Parameters.AddWithValue("@Expirience", experience);
                    command.Parameters.AddWithValue("@CategoryID", categoryId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные успешно сохранены.");
                            ClearFields();
                            admWindow.LoadEmployees();

                        }
                        else
                        {
                            MessageBox.Show("Не удалось сохранить данные.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private int GetSelectedRoleId(string roleName)
        {
            int roleId = 0;
            string query = "SELECT RoleID FROM Role WHERE Name = @RoleName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoleName", roleName);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        roleId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID роли: " + ex.Message);
                }
            }

            return roleId;
        }

        private int GetSelectedCategoryId(string categoryName)
        {
            int categoryId = 0;
            string query = "SELECT CategoryID FROM Category WHERE Name = @CategoryName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryName", categoryName);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        categoryId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID категории: " + ex.Message);
                }
            }

            return categoryId;
        }

        private int GetSelectedJobTitleId(string jobTitleName)
        {
            int jobTitleId = 0;
            string query = "SELECT JobTitleID FROM JobTitle WHERE Name = @JobTitleName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@JobTitleName", jobTitleName);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        jobTitleId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID должности: " + ex.Message);
                }
            }
            return jobTitleId;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
