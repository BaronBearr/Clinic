using Clinic.AllClass;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class UserWindow : Window
    {

        private int userId;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        public UserWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadRecords();
            LoadDiagnoses();
        }

        // ЗАПИСИ
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

        private void BackWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.ShowDialog();
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
                                   "U.FullName LIKE @searchString";

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
                MessageBox.Show("Error searching records: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = SearchTextBox.Text;
            SearchRecords(searchString);
        }

        // ДИАГНОЗЫ

        public void LoadDiagnoses()
        {
            List<Diagnosis> diagnoses = new List<Diagnosis>();

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
                    string query = "SELECT DiagnosisID, DiagnosisName " +
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
                                    DiagnosisName = reader["DiagnosisName"].ToString()
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
        private void Search2TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = Search2TextBox.Text;
            SearchDiagnosis(searchString);
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
    }

}
