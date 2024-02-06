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
    public partial class EditRecordWindow : Window
    {
        private AdmWindow admWindow;
        private Record selectedRecord;
        private Employee _selectedEmployee;
        private Client _selectedClient;
        private Diagnosis _selectedDiagnosis;
        private Drug _selectedDrug;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        public EditRecordWindow(AdmWindow admWindow, Record record)
        {

            InitializeComponent();
            this.admWindow = admWindow;
            selectedRecord = record;
            LoadClients();
            LoadDiagnoses();
            LoadEmployees();
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

        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DateTime selectedDate = datePicker.SelectedDate ?? DateTime.Now;
                    TimeSpan selectedTime;
                    if (!TimeSpan.TryParse(timeTextBox.Text, out selectedTime))
                    {
                        MessageBox.Show("Используй время в формате HH:mm", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int selectedUserID = (int)userComboBox.SelectedValue;
                    int selectedClientID = (int)clientComboBox.SelectedValue;
                    int selectedDiagnosisID = (int)diagnosisComboBox.SelectedValue;

                    string updateQuery = "UPDATE Record " +
                                         "SET Date = @Date, " +
                                         "Time = @Time, " +
                                         "UserID = @UserID, " +
                                         "ClientID = @ClientID, " +
                                         "DiagnosisID = @DiagnosisID " +  
                                         "WHERE RecordID = @RecordID";

                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@RecordID", selectedRecord.RecordID);
                        updateCommand.Parameters.AddWithValue("@Date", selectedDate);
                        updateCommand.Parameters.AddWithValue("@Time", selectedTime);
                        updateCommand.Parameters.AddWithValue("@UserID", selectedUserID);
                        updateCommand.Parameters.AddWithValue("@ClientID", selectedClientID);
                        updateCommand.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosisID);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            admWindow.LoadRecords();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            datePicker.SelectedDate = selectedRecord.Date;
            DateTime baseDate = DateTime.Today;
            DateTime fullDateTime = baseDate.Add(selectedRecord.Time);
            timeTextBox.Text = fullDateTime.ToString("HH:mm");

            _selectedEmployee = userComboBox.Items.OfType<Employee>().FirstOrDefault(emp => emp.FullName == selectedRecord.UserName);
            if (_selectedEmployee != null)
            {
                userComboBox.SelectedItem = _selectedEmployee;

            }

            _selectedClient = clientComboBox.Items.OfType<Client>().FirstOrDefault(cln => cln.FullName == selectedRecord.ClientName);
            if (_selectedClient != null)
            {
                clientComboBox.SelectedItem = _selectedClient;
            }

            _selectedDiagnosis = diagnosisComboBox.Items.OfType<Diagnosis>().FirstOrDefault(dgn => dgn.DiagnosisName == selectedRecord.Diagnosis);
            if (_selectedDiagnosis != null)
            {
                diagnosisComboBox.SelectedItem = _selectedDiagnosis;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
