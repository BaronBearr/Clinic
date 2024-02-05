using Clinic.AllClass;
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
using System.Windows.Shapes;

namespace Clinic.Windows
{
    public partial class AddRecordWindow : Window
    {
        private AdmWindow admWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        public AddRecordWindow(AdmWindow admWindow)
        {
            InitializeComponent();
            this.admWindow = admWindow;
            LoadEmployees();
            LoadDiagnoses();
            LoadClients();
            LoadDrugs();
        }

        private void LoadDrugs()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT DrugsID, Description, Name FROM Drugs";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Drug drug = new Drug
                            {
                                DrugsID = (int)reader["DrugsID"],
                                Description = reader["Description"].ToString(),
                                Name = reader["Name"].ToString(),
                            };
                            drugsComboBox.Items.Add(drug);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки лекарств: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }
        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID, FullName FROM [User] ";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employee employee = new Employee
                            {
                                UserID = (int)reader["UserID"],
                                FullName = reader["FullName"].ToString(),
                            };
                            userComboBox.Items.Add(employee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadClients()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ClientID, FullName FROM Client";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Client client = new Client
                            {
                                ClientID = (int)reader["ClientID"],
                                FullName = reader["FullName"].ToString()
                            };
                            clientComboBox.Items.Add(client);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadDiagnoses()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT DiagnosisID, DiagnosisName FROM Diagnosis";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Diagnosis diagnosis = new Diagnosis
                            {
                                DiagnosisID = (int)reader["DiagnosisID"],
                                DiagnosisName = reader["DiagnosisName"].ToString()
                            };
                            diagnosisComboBox.Items.Add(diagnosis);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TimeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "12:00")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }
        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "12:00";
                textBox.Foreground = Brushes.LightGray;
            }
        }
        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (char.IsDigit(e.Text, 0) && textBox.Text.Length < 5)
            {
                textBox.Text += e.Text;

                if (textBox.Text.Length == 2)
                {
                    textBox.Text += ":";
                }
                else if (textBox.Text.Length == 5)
                {
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            e.Handled = true;
        }
        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (datePicker.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(timeTextBox.Text) ||
                    userComboBox.SelectedValue == null ||
                    clientComboBox.SelectedValue == null ||
                    diagnosisComboBox.SelectedValue == null ||
                    drugsComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Заполни все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime selectedDate = datePicker.SelectedDate.Value;
                if (selectedDate < DateTime.Today)
                {
                    MessageBox.Show("Дата не может быть раньше текущей", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(timeTextBox.Text, out TimeSpan selectedTime))
                {
                    MessageBox.Show("Используй время в формате HH:MM", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int selectedUserID = (int)userComboBox.SelectedValue;
                int selectedDrugsID = (int)drugsComboBox.SelectedValue;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string roleQuery = "SELECT RoleID FROM [User] WHERE UserID = @UserID";

                    using (SqlCommand roleCommand = new SqlCommand(roleQuery, connection))
                    {
                        roleCommand.Parameters.AddWithValue("@UserID", selectedUserID);

                        int roleID = (int)roleCommand.ExecuteScalar();

                        if (roleID == 1)
                        {
                            MessageBox.Show("Нельзя выбрать администратора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                int selectedClientID = (int)clientComboBox.SelectedValue;
                int selectedDiagnosisID = (int)diagnosisComboBox.SelectedValue;
                int minutes = (int)selectedTime.TotalMinutes;
                int roundedMinutes = (minutes / 30) * 30;
                selectedTime = TimeSpan.FromMinutes(roundedMinutes);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Record " +
                                "WHERE Date = @Date AND Time = @Time AND UserID = @UserID";

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Date", selectedDate);
                        checkCommand.Parameters.AddWithValue("@Time", selectedTime);
                        checkCommand.Parameters.AddWithValue("@UserID", selectedUserID);

                        int existingRecords = (int)checkCommand.ExecuteScalar();

                        if (existingRecords > 0)
                        {
                            MessageBox.Show("У сотрудника уже есть запись на это время\nЗапись доступна на каждые 30 минут", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Record (Date, Time, UserID, ClientID, DiagnosisID, DrugsID) " +
                                 "VALUES (@Date, @Time, @UserID, @ClientID, @DiagnosisID, @DrugsID)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Date", selectedDate);
                        insertCommand.Parameters.AddWithValue("@Time", selectedTime);
                        insertCommand.Parameters.AddWithValue("@UserID", selectedUserID);
                        insertCommand.Parameters.AddWithValue("@ClientID", selectedClientID);
                        insertCommand.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosisID);
                        insertCommand.Parameters.AddWithValue("@DrugsID", selectedDrugsID);

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            admWindow.LoadRecords();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка добавления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
