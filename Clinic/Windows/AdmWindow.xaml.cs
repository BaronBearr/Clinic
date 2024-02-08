using Clinic.AllClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Clinic.Windows
{
    public partial class AdmWindow : Window
    {
        private ObservableCollection<Employee> employeesList;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        private int userId;

        public AdmWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadEmployees();
            LoadCategories();
            LoadClients();
            LoadDrugs();
            LoadDiagnoses();
            LoadRecords();
            LoadConclusions();
        }
        // ОКНО СОТРУДНИКИ
        public void LoadEmployees()
        {
            employeesList = new ObservableCollection<Employee>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT U.FullName, U.DoB, U.Email, JT.Name AS JobTitle, U.Phone, R.Name AS Role, U.Expirience, C.Name AS Category, U.Login " +
                               "FROM [User] U " +
                               "JOIN JobTitle JT ON U.JobTitleID = JT.JobTitleID " +
                               "JOIN Role R ON U.RoleID = R.RoleID " +
                               "JOIN Category C ON U.CategoryID = C.CategoryID";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            FullName = reader["FullName"].ToString(),
                            DoB = Convert.ToDateTime(reader["DoB"]),
                            Email = reader["Email"].ToString(),
                            JobTitle = reader["JobTitle"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Role = reader["Role"].ToString(),
                            Experience = Convert.ToInt32(reader["Expirience"]),
                            Category = reader["Category"].ToString(),
                            Login = reader["Login"].ToString()
                        };

                        employeesList.Add(employee);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }

            dgEmployees.ItemsSource = employeesList;
        }
        private void LKWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LKWindow lKWindow = new LKWindow(userId);
                lKWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message);
            }
        }
        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow(this);
            addEmployeeWindow.Show();
        }
        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmployees.SelectedItem != null)
            {
                Employee selectedEmployee = (Employee)dgEmployees.SelectedItem;

                EditEmployeeWindow editEmployeeWindow = new EditEmployeeWindow(this, selectedEmployee);
                editEmployeeWindow.ShowDialog();

                LoadEmployees();
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsEmployeeInRecordTable(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkRecordQuery = "SELECT COUNT(*) FROM Record WHERE UserID = (SELECT UserID FROM [User] WHERE Login = @Login)";
                try
                {
                    connection.Open();

                    using (SqlCommand checkRecordCommand = new SqlCommand(checkRecordQuery, connection))
                    {
                        checkRecordCommand.Parameters.AddWithValue("@Login", login);
                        int recordCount = (int)checkRecordCommand.ExecuteScalar();

                        return recordCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке наличия сотрудника в таблице Record: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void DeleteEmployeeFromDatabase(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string deleteLoginsQuery = "DELETE FROM UserLogins WHERE UserID = (SELECT UserID FROM [User] WHERE Login = @Login)";

                string deleteUserQuery = "DELETE FROM [User] WHERE Login = @Login";

                try
                {
                    connection.Open();

                    using (SqlCommand deleteLoginsCommand = new SqlCommand(deleteLoginsQuery, connection))
                    {
                        deleteLoginsCommand.Parameters.AddWithValue("@Login", login);
                        deleteLoginsCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand deleteUserCommand = new SqlCommand(deleteUserQuery, connection))
                    {
                        deleteUserCommand.Parameters.AddWithValue("@Login", login);
                        int rowsAffected = deleteUserCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Сотрудник успешно удален из базы данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить сотрудника из базы данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении из базы данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Employee selectedEmployee = (Employee)dgEmployees.SelectedItem;

            if (selectedEmployee.Role == "Администратор" || selectedEmployee.JobTitle == "Главный врач")
            {
                MessageBox.Show("Нельзя удалить пользователя с ролью 'Администратор' или должностью 'Главный врач'.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить сотрудника?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (IsEmployeeInRecordTable(selectedEmployee.Login))
                {
                    MessageBox.Show("Невозможно удалить.\nУ сотрудника имеется актуальная запись.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                employeesList.Remove(selectedEmployee);
                DeleteEmployeeFromDatabase(selectedEmployee.Login);
            }

        }
        private void BackWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.ShowDialog();
        }
        public void LoadCategories()
        {
            List<Category> categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Category";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    categories.Add(new Category { CategoryID = 0, Name_Category = "Все" });

                    while (reader.Read())
                    {
                        Category category = new Category
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            Name_Category = reader["Name"].ToString(),
                        };
                        categories.Add(category);
                    }
                }
            }
            categoryComboBox.ItemsSource = categories;
            categoryComboBox.DisplayMemberPath = "Name_Category";
            categoryComboBox.SelectedIndex = 0;
        }
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category selectedCategory = (categoryComboBox.SelectedItem as Category);
            if (selectedCategory != null && selectedCategory.Name_Category != "Все")
            {
                var filteredEmployees = employeesList.Where(emp => emp.Category == selectedCategory.Name_Category).ToList();
                dgEmployees.ItemsSource = filteredEmployees;
            }
            else
            {
                dgEmployees.ItemsSource = employeesList;
            }
        }
        private void SearchEmployees(string searchString)
        {
            List<Employee> filteredEmployees = new List<Employee>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT U.*, JT.Name AS JobTitle, R.Name AS Role, C.Name AS Category FROM [User] U " +
                       "JOIN JobTitle JT ON U.JobTitleID = JT.JobTitleID " +
                       "JOIN Role R ON U.RoleID = R.RoleID " +
                       "JOIN Category C ON U.CategoryID = C.CategoryID " +
                       "WHERE " +
                       "U.FullName LIKE @searchString OR " +
                       "U.DoB LIKE @searchString OR " +
                       "U.Login LIKE @searchString OR " +
                       "JT.Name LIKE @searchString OR " +
                       "U.Email LIKE @searchString OR " +
                       "U.Phone LIKE @searchString OR " +
                       "R.Name LIKE @searchString OR " +
                       "U.Expirience LIKE @searchString OR " +
                       "C.Name LIKE @searchString";



                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Employee employee = new Employee
                                {
                                    FullName = reader["FullName"].ToString(),
                                    DoB = Convert.ToDateTime(reader["DoB"]),
                                    Login = reader["Login"].ToString(),
                                    JobTitle = reader["JobTitle"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Experience = Convert.ToInt32(reader["Expirience"]),
                                    Category = reader["Category"].ToString()
                                };
                                filteredEmployees.Add(employee);
                            }
                        }
                    }
                }

                dgEmployees.ItemsSource = filteredEmployees;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при выполнении поиска: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = SearchTextBox.Text;
            SearchEmployees(searchString);
        }
        private void SearchTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
        }

        // ОКНО КЛИЕНТЫ

        private void AddClient_Click(object sender, EventArgs e)
        {
            AddClientWindow addClientWindow = new AddClientWindow(this);
            addClientWindow.Show();
        }
        private void EditClient_Click(object sender, EventArgs e)
        {
            if (dgClients.SelectedItem != null)
            {
                Client selectedClient = (Client)dgClients.SelectedItem;

                EditClientWindow editClientWindow = new EditClientWindow(this, selectedClient);
                editClientWindow.Show();
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsClientInRecordTable(int clientID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkRecordQuery = "SELECT COUNT(*) FROM Record WHERE ClientID = @ClientID";
                try
                {
                    connection.Open();

                    using (SqlCommand checkRecordCommand = new SqlCommand(checkRecordQuery, connection))
                    {
                        checkRecordCommand.Parameters.AddWithValue("@ClientID", clientID);
                        int recordCount = (int)checkRecordCommand.ExecuteScalar();

                        return recordCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке наличия клиента в таблице Record: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Client selectedClient = (Client)dgClients.SelectedItem;

                        if (selectedClient != null)
                        {
                            if (IsClientInRecordTable(selectedClient.ClientID))
                            {
                                MessageBox.Show("Невозможно удалить.\nУ клиента имеется актуальная запись.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                string deleteQuery = "DELETE FROM Client WHERE ClientID = @ClientID";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                                {
                                    deleteCommand.Parameters.AddWithValue("@ClientID", selectedClient.ClientID);
                                    int rowsAffected = deleteCommand.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("Клиент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                        LoadClients();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Не удалось удалить клиента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка при удалении клиента: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchClients(string searchString)
        {
            List<Client> filteredClients = new List<Client>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT c.ClientID, c.FullName, c.DoB, c.Phone, c.Adress, c.PolicyNumber, c.ClientEmail, c.GenderID, c.Login, g.Name AS GenderName " +
                                   "FROM Client c " +
                                   "JOIN Gender g ON c.GenderID = g.GenderID " +
                                   "WHERE " +
                                   "c.FullName LIKE @searchString OR " +
                                   "c.DoB LIKE @searchString OR " +
                                   "c.Phone LIKE @searchString OR " +
                                   "c.Adress LIKE @searchString OR " +
                                   "c.PolicyNumber LIKE @searchString OR " +
                                   "c.ClientEmail LIKE @searchString OR " +
                                   "g.Name LIKE @searchString OR " +
                                   "c.Login LIKE @searchString";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Client client = new Client
                                {
                                    ClientID = (int)reader["ClientID"],
                                    FullName = reader["FullName"].ToString(),
                                    DoB = Convert.ToDateTime(reader["DoB"]),
                                    Phone = reader["Phone"].ToString(),
                                    Adress = reader["Adress"].ToString(),
                                    PolicyNumber = reader["PolicyNumber"].ToString(),
                                    ClientEmail = reader["ClientEmail"].ToString(),
                                    GenderID = (int)reader["GenderID"],
                                    Login = reader["Login"].ToString(),
                                    GenderName = reader["GenderName"].ToString()
                                };
                                filteredClients.Add(client);
                            }
                        }
                    }
                }

                dgClients.ItemsSource = filteredClients;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при выполнении поиска: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Search2TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = Search2TextBox.Text;
            SearchClients(searchString);
        }
        private void Search2TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
        }
        public void LoadClients()
        {
            List<Client> clients = new List<Client>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT c.ClientID, c.FullName, c.DoB, c.Phone, c.Adress, c.PolicyNumber, c.ClientEmail, c.GenderID, c.Login, g.Name AS GenderName " +
                               "FROM Client c " +
                               "JOIN Gender g ON c.GenderID = g.GenderID";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Client client = new Client
                        {
                            ClientID = (int)reader["ClientID"],
                            FullName = reader["FullName"].ToString(),
                            DoB = (DateTime)reader["DoB"],
                            Phone = reader["Phone"].ToString(),
                            Adress = reader["Adress"].ToString(),
                            PolicyNumber = reader["PolicyNumber"].ToString(),
                            ClientEmail = reader["ClientEmail"].ToString(),
                            GenderID = (int)reader["GenderID"],
                            Login = reader["Login"].ToString(),
                            GenderName = reader["GenderName"].ToString()
                        };
                        clients.Add(client);
                    }
                }
            }

            dgClients.ItemsSource = clients;
        }

        // ОКНО ЛЕКАРСТВА
        private bool IsDrugUsedInConclusion(int drugID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkConclusionQuery = "SELECT COUNT(*) FROM Conclusion WHERE DrugsID = @DrugsID";
                try
                {
                    connection.Open();

                    using (SqlCommand checkConclusionCommand = new SqlCommand(checkConclusionQuery, connection))
                    {
                        checkConclusionCommand.Parameters.AddWithValue("@DrugsID", drugID);
                        int conclusionCount = (int)checkConclusionCommand.ExecuteScalar();

                        return conclusionCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке использования лекарства в заключениях: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public void LoadDrugs()
        {
            List<Drug> drugs = new List<Drug>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Drugs";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Drug drug = new Drug
                        {
                            DrugsID = (int)reader["DrugsID"],
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString()
                        };
                        drugs.Add(drug);
                    }
                }
            }

            dgDrugs.ItemsSource = drugs;
        }
        private void AddDrug_Click(object sender, EventArgs e)
        {
            AddDrugWindow addDrugWindow = new AddDrugWindow(this);
            addDrugWindow.ShowDialog();
        }
        private void EditDrug_Click(object sender, EventArgs e)
        {
            if (dgDrugs.SelectedItem != null)
            {
                Drug selectedDrug = (Drug)dgDrugs.SelectedItem;
                EditDrugWindow editDrugWindow = new EditDrugWindow(this, selectedDrug);
                editDrugWindow.Show();
            }
            else
            {
                MessageBox.Show("Выберите лекарство для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteDrug_Click(object sender, RoutedEventArgs e)
        {
            if (dgDrugs.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить это лекарство?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Drug selectedDrug = (Drug)dgDrugs.SelectedItem;

                        if (IsDrugUsedInConclusion(selectedDrug.DrugsID))
                        {
                            MessageBox.Show("Лекарство нельзя удалить, так как есть заключение, которое использует это лекарство", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            string deleteQuery = "DELETE FROM Drugs WHERE DrugsID = @DrugsID";
                            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@DrugsID", selectedDrug.DrugsID);
                                int rowsAffected = deleteCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Лекарство удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadDrugs();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите лекарство для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchDrugs(string searchString)
        {
            List<Drug> filteredDrugs = new List<Drug>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Drugs " +
                                   "WHERE " +
                                   "Name LIKE @searchString";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Drug drug = new Drug
                                {
                                    DrugsID = (int)reader["DrugsID"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                };
                                filteredDrugs.Add(drug);
                            }
                        }
                    }
                }

                dgDrugs.ItemsSource = filteredDrugs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска лекарства: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Search3TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
        }
        private void Search3TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = Search3TextBox.Text;
            SearchDrugs(searchString);
        }


        // ОКНО ДИАГНОЗ

        private bool IsDiagnosisUsedInConclusion(int diagnosisID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkConclusionQuery = "SELECT COUNT(*) FROM Conclusion WHERE DiagnosisID = @DiagnosisID";
                try
                {
                    connection.Open();

                    using (SqlCommand checkConclusionCommand = new SqlCommand(checkConclusionQuery, connection))
                    {
                        checkConclusionCommand.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                        int conclusionCount = (int)checkConclusionCommand.ExecuteScalar();

                        return conclusionCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке использования диагноза в заключениях: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public void LoadDiagnoses()
        {
            List<Diagnosis> diagnoses = new List<Diagnosis>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT DiagnosisID, DiagnosisName, Description FROM Diagnosis";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Diagnosis diagnosis = new Diagnosis
                        {
                            DiagnosisID = (int)reader["DiagnosisID"],
                            DiagnosisName = reader["DiagnosisName"].ToString(),
                            Description = reader["Description"].ToString()
                        };
                        diagnoses.Add(diagnosis);
                    }
                }
            }

            dgDiagnosis.ItemsSource = diagnoses;
        }
        private void SearchDiagnosis(string searchString)
        {
            List<Diagnosis> filteredDiagnosis = new List<Diagnosis>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT DiagnosisID, DiagnosisName, Description " +
                                   "FROM Diagnosis " +
                                   "WHERE DiagnosisName LIKE @searchString";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Diagnosis diagnosis = new Diagnosis
                                {
                                    DiagnosisID = (int)reader["DiagnosisID"],
                                    DiagnosisName = reader["DiagnosisName"].ToString(),
                                    Description = reader["Description"].ToString()
                                };
                                filteredDiagnosis.Add(diagnosis);
                            }
                        }
                    }
                }

                dgDiagnosis.ItemsSource = filteredDiagnosis;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска диагноза: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Search4TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = Search4TextBox.Text;
            SearchDiagnosis(searchString);
        }
        private void AddDiagnosis_Click(object sender, EventArgs e)
        {
            AddDiagnosisWindow addDiagnosisWindow = new AddDiagnosisWindow(this);
            addDiagnosisWindow.Show();
        }
        private void EditDiagnosis_Click(object sender, EventArgs e)
        {
            if (dgDiagnosis.SelectedItem != null)
            {
                Diagnosis selectedDiagnosis = (Diagnosis)dgDiagnosis.SelectedItem;
                EditDiagnosisWindow editDiagnosisWindow = new EditDiagnosisWindow(this, selectedDiagnosis);
                editDiagnosisWindow.Show();
            }
            else
            {
                MessageBox.Show("Выберите диагноз для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Search4TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
        }
        private void DeleteDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiagnosis.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот диагноз?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Diagnosis selectedDiagnosis = (Diagnosis)dgDiagnosis.SelectedItem;

                        if (IsDiagnosisUsedInConclusion(selectedDiagnosis.DiagnosisID))
                        {
                            MessageBox.Show("Диагноз нельзя удалить, так как есть заключение, которое использует этот диагноз", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            string deleteQuery = "DELETE FROM Diagnosis WHERE DiagnosisID = @DiagnosisID";
                            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosis.DiagnosisID);
                                int rowsAffected = deleteCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Диагноз успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadDiagnoses();
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить диагноз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка при удалении диагноза: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите диагноз для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // ОКНО ЗАПИСЬ

        public void LoadRecords()
        {
            List<Record> records = new List<Record>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT R.RecordID, R.Date, R.Time, U.FullName AS UserName, C.FullName AS ClientName " +
                               "FROM Record R " +
                               "JOIN [User] U ON R.UserID = U.UserID " +
                               "JOIN Client C ON R.ClientID = C.ClientID";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Record record = new Record
                        {
                            RecordID = Convert.ToInt32(reader["RecordID"]),
                            Date = Convert.ToDateTime(reader["Date"]),
                            Time = TimeSpan.Parse(reader["Time"].ToString()),
                            UserName = reader["UserName"].ToString(),
                            ClientName = reader["ClientName"].ToString(),
                        };

                        records.Add(record);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }

            dgRecords.ItemsSource = records;
        }
        private bool IsRecordUsedInConclusion(int recordID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkConclusionQuery = "SELECT COUNT(*) FROM Conclusion WHERE RecordID = @RecordID";
                try
                {
                    connection.Open();

                    using (SqlCommand checkConclusionCommand = new SqlCommand(checkConclusionQuery, connection))
                    {
                        checkConclusionCommand.Parameters.AddWithValue("@RecordID", recordID);
                        int conclusionCount = (int)checkConclusionCommand.ExecuteScalar();

                        return conclusionCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке использования записи в заключениях: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
        private void ContextMenu_AddConclusion_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecords.SelectedItem != null)
            {
                Record selectedRecord = (Record)dgRecords.SelectedItem;

                Conclusion newConclusion = new Conclusion
                {
                    RecordID = selectedRecord.RecordID,
                    DrugsID = 0,
                    DiagnosisID = 0
                };

                AddConclusionWindow addConclusionWindow = new AddConclusionWindow(this, newConclusion);
                addConclusionWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите запись для добавления заключения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddRecord_Click(object sender, EventArgs e)
        {
            AddRecordWindow addRecordWindow = new AddRecordWindow(this);
            addRecordWindow.Show();
        }
        private void EditRecord_Click(object sender, EventArgs e)
        {

            if (dgRecords.SelectedItem != null)
            {
                Record selectedRecord = (Record)dgRecords.SelectedItem;
                EditRecordWindow editRecordWindow = new EditRecordWindow(this, selectedRecord);
                editRecordWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecords.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Record selectedRecord = (Record)dgRecords.SelectedItem;

                        if (IsRecordUsedInConclusion(selectedRecord.RecordID))
                        {
                            MessageBox.Show("Запись нельзя удалить, так как есть заключение, которое использует эту запись", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            string deleteQuery = "DELETE FROM Record WHERE RecordID = @RecordID";
                            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@RecordID", selectedRecord.RecordID);
                                int rowsAffected = deleteCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Запись успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadRecords();
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка при удалении записи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchRecords(string searchString)
        {
            List<Record> filteredRecords = new List<Record>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT R.RecordID, R.Date, R.Time, U.FullName AS UserName, C.FullName AS ClientName " +
                                   "FROM Record R " +
                                   "JOIN [User] U ON R.UserID = U.UserID " +
                                   "JOIN Client C ON R.ClientID = C.ClientID " +
                                   "WHERE " +
                                   "R.Date LIKE @searchString OR " +
                                   "R.Time LIKE @searchString OR " +
                                   "C.FullName LIKE @searchString OR " +
                                   "U.FullName LIKE @searchString " +
                                   "GROUP BY R.RecordID, R.Date, R.Time, U.FullName, C.FullName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Record record = new Record
                                {
                                    RecordID = Convert.ToInt32(reader["RecordID"]),
                                    Date = Convert.ToDateTime(reader["Date"]),
                                    Time = TimeSpan.Parse(reader["Time"].ToString()),
                                    UserName = reader["UserName"].ToString(),
                                    ClientName = reader["ClientName"].ToString()
                                };
                                filteredRecords.Add(record);
                            }
                        }
                    }
                }

                dgRecords.ItemsSource = filteredRecords;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search5TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = Search5TextBox.Text;
            SearchRecords(searchString);
        }
        private void Search5TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
        }

        // ОКНО ЗАКЛЮЧЕНИЯ

        public void LoadConclusions()
        {
            List<ConclusionData> conclusions = new List<ConclusionData>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT c.RecordID, d.Name AS DrugName, diag.DiagnosisName " +
                               "FROM Conclusion c " +
                               "JOIN Drugs d ON c.DrugsID = d.DrugsID " +
                               "JOIN Diagnosis diag ON c.DiagnosisID = diag.DiagnosisID";

                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ConclusionData conclusion = new ConclusionData
                        {
                            RecordID = (int)reader["RecordID"],
                            DrugName = reader["DrugName"].ToString(),
                            Diagnosis = reader["DiagnosisName"].ToString()
                        };
                        conclusions.Add(conclusion);
                    }
                }
            }

            dgList.ItemsSource = conclusions;
        }
    }
}
