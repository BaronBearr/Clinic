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

    public partial class ClientWindow : Window
    {
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";

        private int clientId;
        public ClientWindow(int clientId)
        {
            InitializeComponent();
            this.clientId = clientId;
            LoadPatientRecords(clientId);
        }

        private void LKWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                LKUserWindow lKuserWindow = new LKUserWindow(clientId);
                lKuserWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message);
            }
        }
        private void BackWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.ShowDialog();
        }

        private void LoadPatientRecords(int clientId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT r.Date, r.Time, u.FullName AS UserName
                    FROM Record r
                    JOIN [User] u ON r.UserID = u.UserID
                    WHERE r.ClientID = @ClientID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClientID", clientId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    List<Record> records = new List<Record>();

                    while (reader.Read())
                    {
                        Record record = new Record
                        {
                            Date = Convert.ToDateTime(reader["Date"]),
                            Time = TimeSpan.Parse(reader["Time"].ToString()),
                            UserName = reader["UserName"].ToString()
                        };

                        records.Add(record);
                    }

                    reader.Close();

                    dgDiagnosis.ItemsSource = records;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке записей: " + ex.Message);
                }
            }
        }


    }
}
