namespace Services;

public class SearchService
{
    private static readonly string[] Extensions =
    {
        ".cy",
        ".spec",
        ".test"
    };

    public static IEnumerable<string> FindTestFiles(IEnumerable<string> files)
    {
        var result = new List<string>();
        foreach (var file in files)
        {
            foreach (var extension in Extensions)
            {
                if (file.Contains(extension))
                {
                    result.Add(file);
                }
            }
        }

        return result;
    }

    public static IEnumerable<string> FindTestFiles(DirectoryInfo directory)
    {
        var result = new List<string>();

        var files = directory.GetFiles("*", SearchOption.AllDirectories)
                .Select((f) => f.FullName)
                .Where((l) => l.Contains("node_modules") == false);

        foreach (var file in files)
        {
            foreach (var extension in Extensions)
            {
                if (file.Contains(extension))
                {
                    result.Add(file);
                }
            }
        }

        return result;
    }

    public static IEnumerable<Test> GetTests(IEnumerable<string> files)
    {
        var tests = new List<Test>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            text = text.Replace("\r", "\n").Replace("\n", "\r\n");

            var lines = text.Split("\n").Where((line) => line.Contains(" it('"));
            foreach (var line in lines)
            {
                var body = text;
                var newLine = line.Replace(" it('", " it.only('");
                body = body.Replace(line, newLine);
                tests.Add(new Test
                {
                    File = file,
                    Body = body,
                    Name = line.Split("it('")[1].Split("',")[0],
                });
            }
        }

        return tests;
    }

    public static IEnumerable<Test> GetTests(string file)
    {
        var text = File.ReadAllText(file);
        text = text.Replace("\r", "\n").Replace("\n", "\r\n");
        var tests = new List<Test>();

        var lines = text.Split("\n").Where((line) => line.Contains(" it('"));
        foreach (var line in lines)
        {
            var body = text;
            var name = line.Split("it('")[1].Split("',")[0];
            var newLine = line.Replace(" it('", " it.only('");
            body = body.Replace(line, newLine);
            tests.Add(new Test
            {
                File = file,
                Body = body,
                Name = name,
            });
        }

        return tests;
    }
}