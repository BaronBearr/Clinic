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
    public partial class AddConclusionWindow : Window
    {
        private AdmWindow admWindow;
        private Conclusion conclusion;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public AddConclusionWindow(AdmWindow admWindow, Conclusion conclusion)
        {
            InitializeComponent();
            this.conclusion = conclusion;
            this.admWindow = admWindow;
            LoadDiagnoses();
            LoadDrugs();
            txtRecordID.DataContext = this.conclusion;
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
            cmbDrugs.ItemsSource = drugs;
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

            cmbDiagnosis.ItemsSource = diagnoses;
        }

        private void SaveConclusion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbDrugs.SelectedItem != null && cmbDiagnosis.SelectedItem != null)
                {
                    int selectedDrugsID = (int)cmbDrugs.SelectedValue;
                    int selectedDiagnosisID = (int)cmbDiagnosis.SelectedValue;

                    conclusion.DrugsID = selectedDrugsID;
                    conclusion.DiagnosisID = selectedDiagnosisID;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = "INSERT INTO Conclusion (RecordID, DrugsID, DiagnosisID) VALUES (@RecordID, @DrugsID, @DiagnosisID)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@RecordID", conclusion.RecordID);
                            command.Parameters.AddWithValue("@DrugsID", conclusion.DrugsID);
                            command.Parameters.AddWithValue("@DiagnosisID", conclusion.DiagnosisID);

                            command.ExecuteNonQuery();
                        }
                    }
                    admWindow.LoadConclusions();
                    MessageBox.Show("Заключение успешно добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Выберите лекарство и диагноз перед сохранением.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
