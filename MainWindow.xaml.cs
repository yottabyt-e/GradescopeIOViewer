using GradescopeIOViewer.tests;
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

        string openedPath;
        string selectedExe;
        string[] roots;
        bool[] rootsShown;
        public ObservableCollection<string> names = new ObservableCollection<string> { };
        List<string> inputs = new List<string> { };
        List<string> outputs = new List<string> { };
        string?[]? testResults;

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

            if (name.StartsWith("➕") || name.StartsWith("➖"))
            {
                // Project folded / unfolded
                int projectIndex = names.Where(n => n.StartsWith("➕") || n.StartsWith("➖")).ToList().IndexOf(name);
                rootsShown[projectIndex] = !rootsShown[projectIndex];
                UpdateData();

                if (e.RemovedItems.Count != 0) CasesBox.SelectedIndex = names.IndexOf((string)e.RemovedItems[0]);
                else CasesBox.SelectedIndex = -1;
                return;
            }

            inputText.Text = inputs[index];
            outputText.Text = outputs[index];
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            bool? result = openFolderDialog.ShowDialog();

            if (result == true)
            {
                LoadFolder(openFolderDialog.FolderName, openFolderDialog.FolderName, "folder");
            }
        }

        private void ButtonOpenArchive_Click(object sender, RoutedEventArgs e)
        {
            if (testResults != null && testResults.Count(e => e == null) > 0) return;

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

                LoadFolder(tempPath, openFileDialogue.FileName, "archive");
            }
        }

        private void ButtonSelectExe_Click(object sender, RoutedEventArgs e)
        {
            if (testResults != null && testResults.Count(e => e == null) > 0) return;

            OpenFileDialog openFileDialogue = new OpenFileDialog()
            {
                Filter = "Executables (*.exe)|*.exe"
            };
            bool? result = openFileDialogue.ShowDialog();

            if (result == true)
            {
                selectedExe = openFileDialogue.FileName;
                executableLabel.Content = "Executable: \"" + selectedExe + "\"";
                UpdateTestStatus();
            }
        }

        private void UpdateTestStatus()
        {
            btnRunTests.IsEnabled = selectedExe != null && outputs.Count > 0;
        }

        private void LoadFolder(string path, string name, string type)
        {
            List<string> validDirectories = (new string[] { path }.Concat(Directory.GetDirectories(path)))
                    .Where(dir => Directory.Exists(dir + "\\Inputs") && Directory.Exists(dir + "\\RefOutputs"))
                    .ToList();
            if (validDirectories.Count == 0)
            {
                MessageBox.Show($"The selected {type} does not contain any valid test cases.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            roots = validDirectories.ToArray();
            rootsShown = Enumerable.Repeat(false, roots.Length).ToArray();
            openedPath = name;
            testResults = null;
            UpdateData();
            UpdateTestStatus();
        }

        private void UpdateData()
        {
            if (roots == null || roots.Length == 0)
            {
                // error dialog
                MessageBox.Show("No file path was provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            roots = roots.Where(Directory.Exists).ToArray();
            if (roots.Length == 0)
            {
                MessageBox.Show("The selected folder does not exist, or is unable to be accessed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            roots = roots
                .Where(path => Directory.Exists(Path.Join(path, "Inputs")) && Directory.Exists(Path.Join(path, "RefOutputs")))
                .ToArray();
            if (roots.Length == 0)
            {
                MessageBox.Show("The selected folder does not contain the \"Inputs\" and \"RefOutputs\" subfolder. Ensure you have selected the right folder, then try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            names.Clear();
            inputs.Clear();
            outputs.Clear();

            int i = 0;
            foreach (string root in roots)
            {
                bool isShown = roots.Length == 1 || rootsShown[i++];
                if (roots.Length != 1)
                {
                    names.Add((isShown ? "➖ " : "➕ ") + root.Split("\\").Last());
                    inputs.Add("");
                    outputs.Add("");
                }
                if (!isShown) continue;

                string[] files = Directory.GetFiles(root + "\\Inputs");

                foreach (string file in files)
                {
                    string fileName = file.Substring((root + "\\Inputs\\").Length);
                    string name = fileName.Substring(0, fileName.Length - 4);
                    if (roots.Length != 1) name = "└─ " + name;

                    string refOutputPath = root + "\\RefOutputs\\RefOutput_" + fileName;

                    if (!File.Exists(refOutputPath))
                    {
                        continue;
                    }

                    names.Add(name);
                    inputs.Add(File.ReadAllText(file));
                    outputs.Add(File.ReadAllText(refOutputPath));
                }
            }

            string[] pathArray = openedPath.Split("\\");
            string windowTitle = $"\"{pathArray[pathArray.Length - 1]}\" - L's Gradescope I/O Viewer";

            this.Title = windowTitle;

            folderLabel.Content = $"Folder: \"{openedPath}\"";
        }

        private void ButtonRunTests_Click(object sender, RoutedEventArgs e)
        {
            if (selectedExe == null || outputs.Count == 0) return;
            if (testResults != null && testResults.Count(e => e == null) > 0) return;

            testResults = new string[outputs.Count];
            btnOpenFolder.IsEnabled = false;
            btnOpenArchive.IsEnabled = false;
            btnChangeExeLoc.IsEnabled = false;
            btnRunTests.IsEnabled = false;

            TestManager.runTests(testResults, selectedExe, inputs, outputs, i => {
                System.Diagnostics.Debug.WriteLine(i + " complete.");
                Dispatcher.Invoke(() => {
                    if (testResults != null && testResults.Count(e => e == null) == 0)
                    {
                        btnOpenFolder.IsEnabled = true;
                        btnOpenArchive.IsEnabled = true;
                        btnChangeExeLoc.IsEnabled = true;
                        btnRunTests.IsEnabled = true;
                    }
                });
            });
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