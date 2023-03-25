namespace Services;

public class TestSearchService
{
    private readonly string[] _testExtensions =
    {
        ".cy.",
        ".spec.",
        ".test.",
    };
    
    public string[] FindTestFiles(string[] files)
    {
        var result = new List<string>();
        foreach (var file in files)
        {
            foreach (var ext in _testExtensions)
            {
                if (file.Contains(ext))
                {
                    result.Add(file);
                }
            }
        }
        return result.ToArray();
    }
}