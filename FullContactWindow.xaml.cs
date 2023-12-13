using Microsoft.Data.Sqlite;
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

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for FullContactWindow.xaml
    /// </summary>
    public partial class FullContactWindow : Window
    {
        public FullContactWindow(int id, string name, int age)
        {
            InitializeComponent();
            IDTextBox.Text = id.ToString();
            NameTextBox.Text = name.ToString();
            AgeTextBox.Text = age.ToString();
        }

        public FullContactWindow() { }

        //make a connection to the database
        public static SqliteConnection GetFileDatabaseConnection()
        {
            var connection = new SqliteConnection("Data Source=contact-manager.db;");
            connection.Open();
            return connection;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(IDTextBox.Text, out var id);
            int.TryParse(AgeTextBox.Text, out var age);

            string newName = NameTextBox.Text.Trim();

            if (id > 0 && age > 0 && !string.IsNullOrEmpty(newName))
            {
                if (!ContactExists(newName, age))
                {
                    MainWindow.editContact(id, newName, age);
                    Close();
                }
                else
                {
                    MessageBox.Show("Contact already exists!");
                }
            }
            else
            {
                MessageBox.Show("Enter a valid id, name, and age.");
            }
        }
        //button to close this window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Check if contact exists
        private bool ContactExists(string name, int age)
        {
            using (var conn = GetFileDatabaseConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Contacts WHERE Name = @Name AND Age = @Age";
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Age", age);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
