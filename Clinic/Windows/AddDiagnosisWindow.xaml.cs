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
    public partial class AddDiagnosisWindow : Window
    {
        private AdmWindow admWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public AddDiagnosisWindow(AdmWindow admWindow)
        {
            InitializeComponent();
            this.admWindow = admWindow;
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;

            }
        }
        private void AddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string diagnosisName = diagnosisNameTextBox.Text;
                string diagnosisDescription = descriptionTextBox.Text;

                if (string.IsNullOrWhiteSpace(diagnosisName))
                {
                    MessageBox.Show("Введите диагноз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO Diagnosis (DiagnosisName, Description) VALUES (@DiagnosisName, @Description)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@DiagnosisName", diagnosisName);
                        insertCommand.Parameters.AddWithValue("@Description", diagnosisDescription);

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Диагноз успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            admWindow.LoadDiagnoses();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка добавления диагноза", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
