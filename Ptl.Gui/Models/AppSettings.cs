public class AppSettings
{
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public PtlSettings PtlSettings { get; set; } = new();
}

public class ConnectionStrings
{
    public string PgDb { get; set; } = "";
}

public class PtlSettings
{
    public int GroupZona { get; set; }
}