using ReactiveUI;

namespace Services;

public class Test : ReactiveObject
{
    public string Name { get; set; }
    public string Body { get; set; }
    public string File { get; set; }
    public string Path { get; set; }

    private int progreess;

    public int Progress
    {
        get => progreess;
        set => this.RaiseAndSetIfChanged(ref progreess, value);
    }

    private string result;
    public string Result
    {
        get => result;
        set => this.RaiseAndSetIfChanged(ref result, value);
    }
    public string Progressing { get; set; } = "";
}