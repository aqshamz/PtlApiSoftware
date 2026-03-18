public static class PtlLog
{
    private static void Write(string level, string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
    }

    public static void Info(string msg) => Write("INFO", msg);
    public static void Hw(string msg) => Write("HW", msg);
    public static void Rx(string msg) => Write("RX", msg);
    public static void Eng(string msg) => Write("ENGINE", msg);
    public static void Db(string msg) => Write("DB", msg);
    public static void Warn(string msg) => Write("WARN", msg);
    public static void Error(string msg) => Write("ERROR", msg);
}