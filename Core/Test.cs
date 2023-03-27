namespace Services;

public class Test
{
    public string Name { get; set; }
    public string Body { get; set; }
    public string File { get; set; }

    public int Progress { get; set; } = 0;

    public bool Result { get; set; } = false;
    public string Progressing { get; set; } = "";
}