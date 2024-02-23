﻿using Clinic.AllClass;
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
            LoadClients();
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
       
        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (datePicker.SelectedDate == null ||
                    timeComboBox.SelectedItem == null ||
                    userComboBox.SelectedValue == null ||
                    clientComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DateTime selectedDate = datePicker.SelectedDate.Value;
                string selectedTimeString = ((ComboBoxItem)timeComboBox.SelectedItem).Content.ToString();
                TimeSpan selectedTime = TimeSpan.Parse(selectedTimeString);

                int selectedUserID = (int)userComboBox.SelectedValue;

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
                            MessageBox.Show("Нельзя выбрать администратора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                int selectedClientID = (int)clientComboBox.SelectedValue;

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
                            MessageBox.Show("У сотрудника уже есть запись на это время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Record (Date, Time, UserID, ClientID) " +
                                 "VALUES (@Date, @Time, @UserID, @ClientID)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Date", selectedDate);
                        insertCommand.Parameters.AddWithValue("@Time", selectedTime);
                        insertCommand.Parameters.AddWithValue("@UserID", selectedUserID);
                        insertCommand.Parameters.AddWithValue("@ClientID", selectedClientID);

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
