public class ApiHealthChecker
{
    private readonly HttpClient _client = new();

    public async Task<bool> WaitUntilReady(string url, Action<string> log)
    {
        for (int i = 0; i < 20; i++)
        {
            try
            {
                var res = await _client.GetAsync(url);

                if (res.IsSuccessStatusCode)
                    return true;
            }
            catch { }

            log("API not ready yet...");
            await Task.Delay(1000);
        }

        return false;
    }
}