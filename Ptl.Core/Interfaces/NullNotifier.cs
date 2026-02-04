using Ptl.Core.Interfaces;

public class NullNotifier : ICoreNotifier
{
    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void Warn(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }
}
