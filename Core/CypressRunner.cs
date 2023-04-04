using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Services;

public class CypressRunner : Runner
{
    public static bool Run(Test test, string directory)
    {

        directory = directory.Replace("\\", "/");
        test.File = test.File.Replace("\\", "/");

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
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "/C npx " + process.StartInfo.Arguments;
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
        }

        try
        {
            process.Start();
            test.Result = "";
            test.Progress = 2;
            var output = process.StandardOutput.ReadToEnd();
            test.Progressing = output;
            process.WaitForExit();
            test.Progress = 3;
            process.Close();

            File.Delete(path);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return output.Contains("✓ " + test.Name);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return output.Contains("√ " + test.Name);
            }
            return output.Contains("✓ " + test.Name);
        }
        catch (Exception ex)
        {
            File.Delete(path);
            return false;
        }
    }

    public static async void RunAll(List<Test> tests, string directory, int workers)
    {
        foreach (var test in tests)
        {
            await Task.Run(() =>
            {
                bool res = Run(test, directory);
                if (res) test.Result = "Пройден";
                else test.Result = "Провален";
            });
        }
    }

    //run all test in one run
    public static async void RunMany(List<Test> tests, string directory)
    {
        foreach (var test in tests)
        {
            var name = DateTime.Now.ToFileTimeUtc().ToString();
            var testName = test.File.Split("/").Last();
            var path = test.File.Split(testName)[0] + name + ".cy.js";

            //File.WriteAllText(path, test.Body);
            test.Path = path;
        }

        var paths = tests.Select((test) => test.Path).Aggregate((a, b) =>
            a.Split(directory)[1].Substring(1) + "," + b.Split(directory)[1].Substring(1));
        int a = 5;
        foreach (var s in tests.Select((test) => test.Path))
        {
            //File.Delete(s);
        }
    }
}