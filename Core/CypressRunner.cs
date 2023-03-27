using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Services;

public class CypressRunner : Runner
{
    public static bool Run(Test test, string directory)
    {
        var name = DateTime.Now.ToFileTimeUtc().ToString();
        var testName = test.File.Split("/").Last();
        var path = test.File.Split(testName)[0] + name + ".cy.js";

        File.WriteAllText(path, test.Body);

        var process = new Process()
        {
            StartInfo =
            {
                WorkingDirectory = directory,
                Arguments = "cypress run --spec '" + path.Split(directory)[1].Substring(1) + "'",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            process.StartInfo.FileName = "npx";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            process.StartInfo.FileName = "npx";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "npx";
        }

        try
        {
            process.Start();
            test.Progress = 2;
            var output = process.StandardOutput.ReadToEnd();
            test.Progressing = output;
            process.WaitForExit();
            test.Progress = 3;
            process.Close();

            if (output.Contains("failed"))
            {
                File.Delete(path);
                return false;
            }

            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            File.Delete(path);
            return false;
        }
    }

    public static async void RunAll(List<Test> tests, string directory, int workers)
    {
        // if (tests.Count() >= workers)
        // {
        //     workers = tests.Count();
        // }
        foreach (var test in tests)
        {
            await Task.Run(() => test.Result = Run(test, directory));
            Console.WriteLine(test.Name + " " + test.Result);
        }
        for (var i = 0; i < tests.Count(); i += workers)
        {
            // for (var j = 0; j < workers; j++)
            // {
                // await Task.Run(() => tests[i + j].Result = Run(tests[i], directory));
            // }
        }
    }
}