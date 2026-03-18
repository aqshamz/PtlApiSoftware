using System.Net.Http.Json;

public class HardwareEventRetryWorker
{
    private readonly HttpClient _api;
    private readonly HardwareEventStore _store;

    public HardwareEventRetryWorker(HttpClient api, HardwareEventStore store)
    {
        _api = api;
        _store = store;
    }

    public void Start()
    {
        new Thread(async () =>
        {
            await Task.Delay(2000);

            while (true)
            {
                var events = _store.GetAll();

                if (events.Count == 0)
                {
                    await Task.Delay(3000);
                    continue;
                }

                PtlLog.Hw($"Pending events: {events.Count}");

                var evt = events.First();

                try
                {
                    var res = await _api.PostAsJsonAsync("/ptl/rx", evt);

                    if (res.IsSuccessStatusCode)
                    {
                        _store.Remove(evt);
                        PtlLog.Hw("Event processed");
                    }
                    else
                    {
                        PtlLog.Hw($"API rejected event ({res.StatusCode})");
                    }
                }
                catch
                {
                    PtlLog.Hw("API unavailable");
                }

                await Task.Delay(3000);
            }

        })
        { IsBackground = true }.Start();
    }
}