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
            LoadConclusions();
            LoadDiagnoses();
            LoadDrugs();
            LoadRecords();

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
        private void Search4TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if ((textBox.Text + e.Text).Length > 30)
            {
                e.Handled = true;
            }
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

        private void ContextMenu_AddConclusion_Click(object sender, RoutedEventArgs e)
        {

        }

    }

}
