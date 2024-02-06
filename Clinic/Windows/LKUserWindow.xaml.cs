using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
    public partial class LKUserWindow : Window
    {
        private int clientId;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";
        public LKUserWindow(int clientId)
        {
            InitializeComponent();
            this.clientId = clientId;
            LoadClientData(this.clientId);

        }

        private void LoadClientData(int clientId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    c.Login, 
                    c.FullName, 
                    c.Phone, 
                    c.ClientEmail, 
                    c.DoB,
                    g.Name AS Gender,
                    c.PolicyNumber,
                    c.Photo
                FROM 
                    Client c
                LEFT JOIN
                    Gender g ON c.GenderID = g.GenderID
                WHERE 
                    c.ClientID = @ClientID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClientID", clientId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        loginTextBox.Text = reader["Login"].ToString();
                        TextBoxFullName.Text = reader["FullName"].ToString();
                        TelephoneTextBox.Text = reader["Phone"].ToString();
                        emailTextBox.Text = reader["ClientEmail"].ToString();
                        polisTextBox.Text = reader["PolicyNumber"].ToString();

                        if (DateTime.TryParse(reader["DoB"].ToString(), out DateTime dob))
                        {
                            dobTextBox.Text = dob.ToShortDateString();
                        }

                        genderTextBox.Text = reader["Gender"].ToString();

                        byte[] photoBytes = reader["Photo"] as byte[];
                        if (photoBytes != null && photoBytes.Length > 0)
                        {
                            BitmapImage bitmapImage = LoadBitmapImage(photoBytes);
                            userPhoto.Source = bitmapImage;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Данные клиента не найдены.");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        private BitmapImage LoadBitmapImage(byte[] photoBytes)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return null;

            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(photoBytes))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                SaveClientPhotoToDatabase(clientId, photoBytes);

                BitmapImage bitmapImage = LoadBitmapImage(photoBytes);
                userPhoto.Source = bitmapImage;
            }
        }
        private void SaveClientPhotoToDatabase(int clientId, byte[] photoBytes)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Client SET Photo = @Photo WHERE ClientID = @ClientID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientID", clientId);
                    command.Parameters.AddWithValue("@Photo", photoBytes);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
