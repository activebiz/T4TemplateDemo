using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using Microsoft.VisualStudio.TextTemplating;

namespace T4TemplateDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtConnectionString.Text =
                @"Data Source=(local);Initial Catalog=MyDatabase;Integrated Security=True";
        }

        static CompilerErrorCollection ProcessTemplate(string args, string schema, string table)
        {
            string templateFileName = null;
            if (args == null)
            {
                throw new System.Exception("you must provide a text template file path");
            }
            templateFileName = args;
            if (templateFileName == null)
            {
                throw new ArgumentNullException("the file name cannot be null");
            }
            if (!File.Exists(templateFileName))
            {
                throw new FileNotFoundException("the file cannot be found");
            }
            SQLViewsHost host = new SQLViewsHost();
            Engine engine = new Engine();
            host.TemplateFileValue = templateFileName;
            //Read the text template.
            TextTemplatingSession session = new TextTemplatingSession();
            session["SchemaName"] = schema;
            session["TableName"] = table;
            session["ViewName"] = String.Format("v{0}", table);
            session["Username"] = "abuser";
            var sessionHost = (ITextTemplatingSessionHost)host;
            sessionHost.Session = session;
            string input = File.ReadAllText(templateFileName);
            //Transform the text template.
            string output = engine.ProcessTemplate(input, host);
            string outputFileName = Path.GetFileNameWithoutExtension(String.Format("v{0}", table));
            outputFileName = Path.Combine(Path.GetDirectoryName(String.Format("v{0}", table)), outputFileName);
            outputFileName = outputFileName + host.FileExtension;
            File.WriteAllText(outputFileName, output, host.FileEncoding);
            return host.Errors;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            PopulateGrid();
        }

        private void PopulateGrid()
        {
            if (!String.IsNullOrEmpty(txtConnectionString.Text))
            {
                using (SqlConnection connection = new SqlConnection(txtConnectionString.Text))
                {
                    connection.Open();
                    DataTable schema = connection.GetSchema("Tables");
                    List<string> tableNames = new List<string>();
                    dataGridTables.ItemsSource =
                        schema.Rows.Cast<DataRow>()
                            .Where(a => a.ItemArray[3].ToString() == "BASE TABLE")
                            .CopyToDataTable()
                            .AsDataView();
                }
            }
            else
            {
                MessageBox.Show("ConnectionString is Missing", "ConnectionString", MessageBoxButton.OK);
            }
        }

        private void btnGenerateViews_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTables.SelectedItems.Count > 0)
            {
                String[] columnRestrictions = new String[4];
                foreach (DataRowView selectedItem in dataGridTables.SelectedItems)
                {
                    var errors = ProcessTemplate(@"C:\practice\T4TemplateDemo\T4TemplateDemo\SQLViews.tt", selectedItem.Row[1].ToString(), selectedItem.Row[2].ToString());
                    if (errors.Count > 0)
                    {
                        throw new Exception();
                    }
                }
                
            }
            else
            {
                MessageBox.Show("Please select atleast one table.", "Select table(s).", MessageBoxButton.OK);
            }
        }
    }
}
