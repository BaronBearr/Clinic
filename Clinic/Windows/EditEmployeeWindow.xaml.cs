using Clinic.AllClass;
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
    public partial class EditEmployeeWindow : Window
    {
        private AdmWindow admWindow;
        private Employee selectedEmployee;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public EditEmployeeWindow(AdmWindow admWindow, Employee employee)
        {
            InitializeComponent();
            this.admWindow = admWindow;
            selectedEmployee = employee;
            fullNameTextBox.Text = selectedEmployee.FullName;
            dobDatePicker.SelectedDate = selectedEmployee.DoB;
            loginTextBox.Text = selectedEmployee.Login;
            passwordBox.Visibility = Visibility.Visible;
            LoadComboBoxData();
            jobTitleComboBox.Text = selectedEmployee.JobTitle;
            emailTextBox.Text = selectedEmployee.Email;
            phoneTextBox.Text = selectedEmployee.Phone;
            experienceTextBox.Text = employee.Experience.ToString();
            SelectComboBoxItem(roleComboBox, employee.Role);
            SelectComboBoxItem(categoryComboBox, employee.Category);
            SelectComboBoxItem(jobTitleComboBox, employee.JobTitle);

        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

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
        private void SelectComboBoxItem(ComboBox comboBox, string selectedItem)
        {
            if (comboBox.Items.Contains(selectedItem))
            {
                comboBox.SelectedItem = selectedItem;
            }
            else
            {
                MessageBox.Show($"Элемент '{selectedItem}' не найден в списке.");
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

            string passwordQuery = "SELECT Password FROM [User] WHERE Login = @login";
            LoadPassword(passwordBox, passwordQuery, selectedEmployee.Login);
        }
        private void LoadPassword(PasswordBox passwordBox, string query, string login)
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
                        passwordBox.Password = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке пароля: " + ex.Message);
            }
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
        private int GetUserId(SqlConnection connection, string login)
        {
            int userId = 0;
            string query = "SELECT UserID FROM [User] WHERE Login = @Login";

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
        private bool IsUserExists(string selectedRole)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM [User] u " +
                               "JOIN [Role] r ON u.RoleID = r.RoleID " +
                               "WHERE r.Name = @Role";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Role", selectedRole);

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке наличия пользователя: " + ex.Message);
                    return false;
                }
            }
        }
        private void addEmployee_Click(object sender, RoutedEventArgs e)
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

            if (selectedRole == "Администратор" && IsUserExists("Администратор") && selectedEmployee.Role != "Администратор")
            {
                MessageBox.Show("Администратор уже существует.");
                return;
            }

            if (selectedRole == "Администратор")
            {
                if (selectedCategory != "Администрация" || selectedJobTitle != "Главный врач")
                {
                    MessageBox.Show("Неверные категория и(или) должность");
                    return;
                }
            }

            if (selectedRole == "Сотрудник" && selectedCategory == "Администрация")
            {
                MessageBox.Show("Роль администратора должна быть 'Администратор'.");
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

            if (email.Contains(" ") || email.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Некорретный формат электронной почты");
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

            int roleId = GetSelectedRoleId(selectedRole);
            int categoryId = GetSelectedCategoryId(selectedCategory);
            int jobTitleId = GetSelectedJobTitleId(selectedJobTitle);



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();  

                string newLogin = loginTextBox.Text;
                int userId = GetUserId(connection, selectedEmployee.Login);

                if (newLogin != selectedEmployee.Login)
                {
                    if (!IsLoginUnique(newLogin, userId))
                    {
                        MessageBox.Show("Логин уже существует. Пожалуйста, выберите другой логин.");
                        return;
                    }
                }

                string queryUpdate = @"UPDATE [User] SET FullName = @FullName, DoB = @DoB, JobTitleID = @JobTitleID, 
        Email = @Email, Login = @NewLogin, Password = @Password, Phone = @Phone, 
        RoleID = @RoleID, Expirience = @Expirience, CategoryID = @CategoryID 
        WHERE UserID = @UserID";

                SqlCommand command = new SqlCommand(queryUpdate, connection);
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@DoB", dob);
                command.Parameters.AddWithValue("@JobTitleID", jobTitleId);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@NewLogin", newLogin);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@RoleID", roleId);
                command.Parameters.AddWithValue("@Expirience", experience);
                command.Parameters.AddWithValue("@CategoryID", categoryId);

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно сохранены.");
                        admWindow.LoadEmployees();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить данные." + command.CommandText);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении данных: " + ex.ToString());
                }
            }


        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}