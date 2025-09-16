namespace GradescopeIOViewer.tests
{
    internal struct TestManager
    {

        public static async void runTests(string?[] results, string executable, List<string> inputs, List<string> outputs, Action<int> onTestComplete)
        {
            System.Diagnostics.Debug.WriteLine(">>> " + results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int tempI = i;
                System.Diagnostics.Debug.WriteLine("> " + tempI);
                _ = Task.Run(async () => {
                    System.Diagnostics.Debug.WriteLine(tempI + " started...");
                    TestInstance testInstance = await TestInstance.Spawn(executable, inputs[tempI]);
                    string output = testInstance.output.ToString();
                    results[tempI] = output;
                    onTestComplete(tempI);
                });
            }
        }

    }
}
