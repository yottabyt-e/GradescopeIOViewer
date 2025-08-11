using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;

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

        private void ButtonOpenArchive_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialogue = new OpenFileDialog()
            {
                Filter = "Zip Archives (*.zip)|*.zip"
            };
            bool? result = openFileDialogue.ShowDialog();

            if (result == true)
            {
                string tempPath = Path.GetTempPath() + "\\LsGradescopeIOViewer";
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

                ZipFile.ExtractToDirectory(openFileDialogue.FileName, tempPath);

                if (!Directory.Exists(tempPath + "\\Inputs") && Directory.GetDirectories(tempPath).Count() == 1)
                {
                    tempPath = Directory.GetDirectories(tempPath).First();
                }

                root = tempPath;
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            try
            {
                string tempPath = Path.GetTempPath() + "\\LsGradescopeIOViewer";
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            } catch { }
        }
    }
}