using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;

namespace ContactManager
{
    public partial class MainWindow : Window
    {
        int currentContactIndex;
        public MainWindow()
        {
            InitializeComponent();
            LoadContacts();
        }
        //make a connection to the database
        public static SqliteConnection GetFileDatabaseConnection()
        {
            var connection = new SqliteConnection("Data Source=contact-manager.db;");
            connection.Open();
            return connection;
        }
        //import contacts from csv file
        public void ImportFromCSV(string filePath)
        {
            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields != null && fields.Length >= 3)
                        {
                            if (int.TryParse(fields[0], out int id) && int.TryParse(fields[2], out int age))
                            {
                                string name = fields[1];
                                if (!ContactExists(id, name))
                                {
                                    AddContact(id, name, age);
                                }
                                else
                                {
                                    MessageBox.Show("Contact already exists");
                                }
                            }
                        }
                    }
                    MessageBox.Show($"Successfully imported from {filePath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while importing: {ex.Message}");
            }
        }

        //Export contacts to csv file
        public void ExportToCSV(string filepath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                using (var conn = GetFileDatabaseConnection())
                {
                    var command = new SqliteCommand("SELECT Id, Name, Age FROM Contacts", conn);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sb.AppendLine($"{reader["Id"]}, {reader["Name"]}, {reader["Age"]}");
                        }
                    }
                }
                File.WriteAllText(filepath, sb.ToString());
                MessageBox.Show($"Successfully exported all contacts to {filepath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while importing: {ex.Message}");
            }

        }

        //-----------------------------------------------CRUD---------------------------------------------------//
        //add a contact to database
        private void AddContact(int id, string name, int age)
        {
            using (var conn = GetFileDatabaseConnection())
            {
                var insertCommand = conn.CreateCommand();
                insertCommand.CommandText = "INSERT INTO Contacts (Id, Name, Age) VALUES (@Id, @Name, @Age)";
                insertCommand.Parameters.AddWithValue("@Id", id);
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Age", age);
                insertCommand.ExecuteNonQuery();
            }
        }
        //load all contacts in the databes to the screen
        private void LoadContacts()
        {
            lstContacts.Items.Clear(); 
            using (var conn = GetFileDatabaseConnection())
            {
                var command = new SqliteCommand("SELECT Name FROM Contacts", conn);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstContacts.Items.Add(reader["Name"].ToString());
                    }
                }
            }
            if (lstContacts.Items.Count > 0)
            {
                currentContactIndex = 0;
                lstContacts.SelectedIndex = currentContactIndex;
            }
        }
        //edit/ update the selected contact
        public static async void editContact(int id, string name, int age)
        {
            using (var conn = GetFileDatabaseConnection())
            {
                var updateCommand = conn.CreateCommand();
                updateCommand.CommandText = "UPDATE Contacts SET Name = @Name, Age = @Age WHERE Id = @Id";
                updateCommand.Parameters.AddWithValue("@Name", name);
                updateCommand.Parameters.AddWithValue("@Age", age);
                updateCommand.Parameters.AddWithValue("@Id", id);

                //add delay to not get "database is locked" error
                await Task.Delay(1000);
                updateCommand.ExecuteNonQuery();
                MessageBox.Show("Contact has been changed. (refresh needed)");
            }
        }
        //delete selected contact
        private void DeleteContact()
        {
            if (currentContactIndex >= 0 && lstContacts.Items.Count > 0)
            {
                using (var conn = GetFileDatabaseConnection())
                {
                    var command = conn.CreateCommand();
                    command.CommandText = "SELECT Id FROM Contacts LIMIT 1 OFFSET @Index";
                    command.Parameters.AddWithValue("@Index", currentContactIndex);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        int contactId = Convert.ToInt32(result);

                        var deleteCommand = conn.CreateCommand();
                        deleteCommand.CommandText = "DELETE FROM Contacts WHERE Id = @ContactId";
                        deleteCommand.Parameters.AddWithValue("@ContactId", contactId);

                        deleteCommand.ExecuteNonQuery();
                        LoadContacts();
                    }
                }
            }
        }
        //--------------------------------------------END OF CRUD----------------------------------------------------//

        //view full selected contactt info
        private void SelectContact()
        {
            if (currentContactIndex >= 0 && lstContacts.Items.Count > 0)
            {
                using (var conn = GetFileDatabaseConnection())
                {
                    var command = conn.CreateCommand();
                    command.CommandText = "SELECT Id, Name, Age FROM Contacts LIMIT 1 OFFSET @Index";
                    command.Parameters.AddWithValue("@Index", currentContactIndex);

                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                        {
                            int Id = result.GetInt32(0);
                            string Name = result.GetString(1);
                            int Age = result.GetInt32(2);

                            FullContactWindow fullContactWindow = new FullContactWindow(Id, Name, Age);
                            fullContactWindow.ShowDialog();
                        }
                    }
                }
            }
        }
        // Check if contact exists
        private bool ContactExists(int id, string name)
        {
            using (var conn = GetFileDatabaseConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Contacts WHERE Id = @Id OR Name = @Name";
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", name);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        //--------------------------------------Buttons and TextBoxes-------------------------------------//

        //previous select button
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        { 
            if (currentContactIndex > 0)
            {
                currentContactIndex--;
                lstContacts.SelectedIndex = currentContactIndex;
            }
        }
        //next select button
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentContactIndex < lstContacts.Items.Count - 1)
            {
                currentContactIndex++;
                lstContacts.SelectedIndex = currentContactIndex;
            }

        }
        //button to add the contact
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(IDtxtb.Text, out var id);
            int.TryParse(Agetxtb.Text, out var age);

            string newName = Nametxtb.Text.Trim();

            if (id > 0 && age > 0 && !string.IsNullOrEmpty(newName))
            {
                if (!ContactExists(id, newName))
                {
                    AddContact(id, newName, age);
                    LoadContacts();
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
        //delete selected contact
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteContact();
            LoadContacts();
        }
        //clear id, name, and age txtBoxes
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IDtxtb.Text = "";
            Nametxtb.Text = "";
            Agetxtb.Text = "";
        }
        //view full selected conatct info
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SelectContact();
        }

        //to refresh contacts
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            LoadContacts();
        }
        //import button
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string filePath = filepath.Text.Trim();

            if (!string.IsNullOrEmpty(filePath))
            {
                ImportFromCSV(filePath);
                LoadContacts();
            }
            else
            {
                MessageBox.Show("enter a file path");
            }
            
        }
        //Export button
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string filePath = filepath.Text.Trim();

            if (!string.IsNullOrEmpty(filePath))
            {
                ExportToCSV(filePath);
            }
            else
            {
                MessageBox.Show("enter a file path");
            }
        }
        private void Age_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void filepath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
