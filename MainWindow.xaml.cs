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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqliteConnection conn;

        public MainWindow()
        {
            InitializeComponent();

            conn = new SqliteConnection("Data Source=contact-manager.db;");
            conn.Open();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var cmd = new SqliteCommand(txtCommand.Text, conn);

            var response = cmd.ExecuteScalar();

            MessageBox.Show(response?.ToString());
        }

        private void txtCommand_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string sqlStatement = @"
            SELECT count(name)
            FROM sqlite_master
            WHERE
            (
                type = 'table'
                AND name = 'Contacts'
            )";

            var cmdCheck = new SqliteCommand(sqlStatement, conn);

            if ((long)cmdCheck.ExecuteScalar() == 0)
            {
                using(var cmd = new SqliteCommand(sqlStatement, conn))
                {
                    sqlStatement = @"CREATE TABLE Contacts(Id INTEGER PRIMARY KEY,Name TEXT,Age INTEGER)";
                    cmd.CommandText = sqlStatement;
                    cmd.ExecuteScalar();
                    MessageBox.Show("Table Succesfully created");
                }
            }
            else
                MessageBox.Show("Table already exists");
        }
    }
}
