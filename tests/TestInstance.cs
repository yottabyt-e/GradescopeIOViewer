using System.Diagnostics;
using System.IO;
using System.Text;

namespace GradescopeIOViewer.tests
{

    // Credit to (Shah, 2016) for template - https://stackoverflow.com/a/39624153/12964643
    public class TestInstance
    {

        Process process;
        TaskCompletionSource<string> processExitedTaskSource = new TaskCompletionSource<string>();

        public StringBuilder output = new StringBuilder();

        private TestInstance(string executable, string input)
        {
            process = new Process();

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
            process.Exited += new EventHandler(process_Exited);

            process.StartInfo.FileName = executable;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();

            if (!string.IsNullOrEmpty(input))
            {
                using (StreamWriter sw = process.StandardInput)
                {
                    string[] lines = input.Split('\n');
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

        }

        public static async Task<TestInstance> Spawn(string executable, string input)
        {
            TestInstance test = new TestInstance(executable, input);
            await Task.WhenAny(test.processExitedTaskSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            return test;
        }

        void process_Exited(object? sender, EventArgs e)
        {
            processExitedTaskSource.SetResult(output.ToString());
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            output.AppendLine(e.Data);
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            output.AppendLine(e.Data);
        }

    }
}
