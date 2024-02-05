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

    public partial class LKWindow : Window
    {

        private int userId;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RaionnayaPoliklinika;Integrated Security=True";


        public LKWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData(this.userId);
        }

        // ЗАГРУЗКА ДАННЫХ ПОЛЬЗОВАТЕЛЯ И ФОТО 
        private void LoadUserData(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    u.Login, 
                    u.FullName, 
                    jt.Name AS JobTitle, 
                    u.Phone,
                    u.Email,
                    u.DoB, 
                    c.Name AS Category,
                    u.Photo
                FROM 
                    [User] u
                LEFT JOIN 
                    JobTitle jt ON u.JobTitleID = jt.JobTitleID
                LEFT JOIN 
                    Category c ON u.CategoryID = c.CategoryID
                WHERE 
                    u.UserID = @UserID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        loginTextBox.Text = reader["Login"].ToString();
                        TextBoxFullName.Text = reader["FullName"].ToString();
                        JobTitleTextBox.Text = reader["JobTitle"].ToString();
                        TelephoneTextBox.Text = reader["Phone"].ToString();
                        emailTextBox.Text = reader["Email"].ToString();

                        if (DateTime.TryParse(reader["DoB"].ToString(), out DateTime dob))
                        {
                            dobTextBox.Text = dob.ToShortDateString();
                        }

                        categoryTextBox.Text = reader["Category"].ToString();

                        byte[] photoBytes = reader["Photo"] as byte[];
                        if (photoBytes != null && photoBytes.Length > 0)
                        {
                            BitmapImage bitmapImage = LoadBitmapImage(photoBytes);
                            userPhoto.Source = bitmapImage;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Данные пользователя не найдены.");
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
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                SaveUserPhotoToDatabase(userId, photoBytes); 

                BitmapImage bitmapImage = LoadBitmapImage(photoBytes);
                userPhoto.Source = bitmapImage;
            }
        }
        private void SaveUserPhotoToDatabase(int userId, byte[] photoBytes)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE [User] SET Photo = @Photo WHERE UserID = @UserID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@Photo", photoBytes);

                    command.ExecuteNonQuery();
                }
            }
        }

        // СТАТИСТИКА
        private void ShowLoginStats(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*) AS TotalLogins, MIN(LoginTime) AS FirstLogin, MAX(LoginTime) AS LastLogin
                    FROM UserLogins
                    WHERE UserId = @UserId AND LoginTime >= DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int totalLogins = Convert.ToInt32(reader["TotalLogins"]);
                        DateTime firstLogin = Convert.ToDateTime(reader["FirstLogin"]);
                        DateTime lastLogin = Convert.ToDateTime(reader["LastLogin"]);

                        reader.Close();

                        string loginHistoryQuery = "SELECT LoginTime FROM UserLogins WHERE UserId = @UserId ORDER BY LoginTime DESC";
                        using (SqlCommand historyCommand = new SqlCommand(loginHistoryQuery, connection))
                        {
                            historyCommand.Parameters.AddWithValue("@UserId", userId);

                            SqlDataReader historyReader = historyCommand.ExecuteReader();
                            StringBuilder loginHistory = new StringBuilder();
                            loginHistory.AppendLine("История входов:");

                            while (historyReader.Read())
                            {
                                DateTime loginTime = Convert.ToDateTime(historyReader["LoginTime"]);
                                loginHistory.AppendLine(loginTime.ToString());
                            }

                            historyReader.Close();

                            Window statsWindow = new Window
                            {
                                Title = "Статистика входов за сутки",
                                Width = 400,
                                Height = 300
                            };

                            ScrollViewer scrollViewer = new ScrollViewer
                            {
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Content = new TextBox
                                {
                                    Text = $"Количество входов: {totalLogins}\nПервый вход: {firstLogin}\nПоследний вход: {lastLogin}\n\n{loginHistory.ToString()}",
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true
                                }
                            };

                            statsWindow.Content = scrollViewer;

                            statsWindow.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Статистика входов не найдена.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении статистики входов: " + ex.Message);
                }
            }
        }
        private void Statistic_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginStats(userId);
        }
    }
}
