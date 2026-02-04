using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;
using System;
using System.Threading;

public class EventLoop
{
    public event Action<int, short>? OnTagCommand;
    public event Action<PtlRxEventDto>? OnRx;
    public void Start()
    {
        new Thread(Loop)
        {
            IsBackground = true
        }.Start();
    }

    private void Loop()
    {
        byte[] buffer = new byte[255];
        int len;

        Console.WriteLine("RX LOOP ACTIVE");

        while (true)
        {
            int gw = 0;
            int node = 0;
            short cmd = -1;
            short type = -1;
            len = 0;

            int ret = CapsAPI.AB_Tag_RcvMsg(
                ref gw,
                ref node,
                ref cmd,
                ref type,
                ref buffer[0],
                ref len
            );

            if (ret > 0 && cmd != 9) // ignore diag spam
            {
                int tag = Math.Abs(node);
                var evt = new PtlRxEventDto
                {
                    Gateway = gw,
                    Tag = tag,
                    Command = cmd
                };

                OnRx?.Invoke(evt);
            }

            Thread.Sleep(50);
        }
    }
}
