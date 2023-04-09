using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Services;

public class CypressRunner : Runner
{
    public static bool Run(Test test, string directory)
    {

        directory = directory.Replace("\\", "/");
        test.File = test.File.Replace("\\", "/");

        var name = DateTime.Now.ToFileTimeUtc().ToString() + DateTime.Now.Microsecond;
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
            test.OnRan();

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

    public static async Task<bool> RunAll(List<Test> tests, string directory)
    {
        foreach (var test in tests)
        {
            await Task.Run(() =>
            {
                var res = Run(test, directory);
                test.Result = res ? "Пройден" : "Провален";
            });
        }
        return true;
    }

    public static bool RunAllParallel(List<Test> tests, string directory, int threadCount) 
    {
        Parallel.ForEach(tests, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, test =>
        {
            var res = Run(test, directory);
            test.Result = res ? "Пройден" : "Провален";
        });

        return true;
    }
}