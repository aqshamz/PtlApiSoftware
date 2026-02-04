
namespace Ptl.Hardware
{
    public class ConsoleNotifier : ICoreNotifier
    {
        public void Info(string message)
            => Console.WriteLine(message);

        public void Warn(string message)
            => Console.WriteLine($"WARN: {message}");
    }
}
