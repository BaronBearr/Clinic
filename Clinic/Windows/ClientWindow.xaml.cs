using System;
using System.Collections.Generic;
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
    }
}
