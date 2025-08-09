using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;

namespace GradescopeIOViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string root;
        public ObservableCollection<string> names = new ObservableCollection<string> { };
        List<string> inputs = new List<string> { };
        List<string> outputs = new List<string> { };

        public MainWindow()
        {
            InitializeComponent();

            CasesBox.ItemsSource = names;
        }

        private void OpenCase(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                inputText.Text = "";
                outputText.Text = "";

                return;
            }

            string name = (string)e.AddedItems[0];
            int index = names.IndexOf(name);

            inputText.Text = inputs[index];
            outputText.Text = outputs[index];
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            bool? result = openFolderDialog.ShowDialog();

            if (result == true)
            {
                root = openFolderDialog.FolderName;

                UpdateData();
            }
        }

        private void UpdateData()
        {
            if (root == null)
            {
                // error dialog
                MessageBox.Show("No file path was provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(root))
            {
                MessageBox.Show("The selected folder does not exist, or is unable to be accessed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(root + "\\Inputs") || !Directory.Exists(root + "\\RefOutputs"))
            {
                MessageBox.Show("The selected folder does not contain the \"Inputs\" and \"RefOutputs\" subfolder. Ensure you have selected the right folder, then try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            names.Clear();
            inputs.Clear();
            outputs.Clear();

            string[] files = Directory.GetFiles(root + "\\Inputs");

            foreach (string file in files)
            {
                string fileName = file.Substring((root + "\\Inputs\\").Length);
                string name = fileName.Substring(0, fileName.Length - 4);

                string refOutputPath = root + "\\RefOutputs\\RefOutput_" + fileName;

                if (!File.Exists(refOutputPath))
                {
                    continue;
                }

                names.Add(name);
                inputs.Add(File.ReadAllText(file));
                outputs.Add(File.ReadAllText(refOutputPath));
            }

            string[] pathArray = root.Split("\\");
            string windowTitle = $"\"{pathArray[pathArray.Length - 1]}\" - L's Gradescope I/O Viewer";

            this.Title = windowTitle;

            folderLabel.Content = $"Folder: \"{root}\"";
        }
    }
}