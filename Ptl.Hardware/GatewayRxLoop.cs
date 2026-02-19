using DAPCAPS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class GatewayRxLoop
{
    private readonly ConcurrentDictionary<int, IReadOnlySet<int>> _readyCache
        = new();

    public void Start()
    {
        new Thread(Loop)
        {
            IsBackground = true,
            Name = "PTL-Gateway-RX"
        }.Start();

        Console.WriteLine("GW RX LOOP ACTIVE");
    }

    private void Loop()
    {
        var ccb = new CapsAPI.QWAY_CCB_RAW
        {
            ccbdata = new byte[256]
        };

        while (true)
        {
            int ret = CapsAPI.AB_GW_RcvMsg_RAW(0, ref ccb); // 0 = any gateway

            if (ret > 0)
            {
                // 0x09 = TagDiag response
                if (ccb.ccbcmd == 0x09)
                {
                    var ready = ParseReadyTags(ccb);
                    _readyCache[ccb.ccbport] = ready;

                    Console.WriteLine(
                        $"[PTL][GW] ReadyTags gw={ccb.ccbport}, count={ready.Count}"
                    );
                }
            }

            Thread.Sleep(20);
        }
    }

    private IReadOnlySet<int> ParseReadyTags(CapsAPI.QWAY_CCB_RAW ccb)
    {
        var ready = new HashSet<int>();

        for (int byteIndex = 0; byteIndex < 32; byteIndex++)
        {
            byte b = ccb.ccbdata[11 + byteIndex];

            for (int bit = 0; bit < 8; bit++)
            {
                int tag = byteIndex * 8 + bit + 1;
                if (tag > 252) break;

                bool failed = ((b >> bit) & 1) == 1;
                if (!failed)
                    ready.Add(tag);
            }
        }

        return ready;
    }

    // 🔑 This is what everyone else uses
    public IReadOnlySet<int> GetReadyTags(int gateway)
    {
        return _readyCache.TryGetValue(gateway, out var tags)
            ? tags
            : new HashSet<int>();
    }
}
