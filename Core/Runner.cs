namespace Services;

public interface Runner
{
    public static bool Run(Test test, string directory)
    {
        return false;
    }
    
    public static bool RunAll(IEnumerable<Test> tests, int workers) { return false; }
}