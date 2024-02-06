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

    public partial class EditDiagnosisWindow : Window
    {
        private AdmWindow admWindow;
        private Diagnosis selectedDiagnosis;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public EditDiagnosisWindow(AdmWindow admWindow, Diagnosis diagnosis)
        {
            InitializeComponent();
            this.admWindow = admWindow;
            selectedDiagnosis = diagnosis;
            diagnosisNameTextBox.Text = selectedDiagnosis.DiagnosisName;
            descriptionTextBox.Text = diagnosis.Description;
            
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }
        private void SaveChanges()
        {
            try
            {
                string diagnosisName = diagnosisNameTextBox.Text;
                string description = descriptionTextBox.Text;

                if (string.IsNullOrWhiteSpace(diagnosisName))
                {
                    MessageBox.Show("Введите диагноз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = "UPDATE Diagnosis SET DiagnosisName = @DiagnosisName, Description = @Description WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@DiagnosisName", diagnosisName);
                        updateCommand.Parameters.AddWithValue("@Description", description);
                        updateCommand.Parameters.AddWithValue("@DiagnosisID", selectedDiagnosis.DiagnosisID);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            admWindow.LoadDiagnoses();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
