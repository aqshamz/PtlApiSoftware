using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;
using static DAPCAPS.CapsAPI;

public class HardwarePtlDisplay : IPtlDisplay
{
    public void Execute(PtlTxCommandDto dto)
    {
        //if (dto.CheckGateaway)
        //{
        //    GetReadyTags(dto.Gateway);
        //    return;
        //}

        if (dto.ClearHeader)
        {
            ClearHeader(dto.Gateway, dto.Tag);
            return;
        }

        if (dto.Confirm)
        {
            ConfirmTag(dto.Gateway, dto.Tag);
            return;
        }

        if (dto.Qty.HasValue)
        {
            DisplayQty(dto.Gateway, dto.Tag, dto.Qty.Value);
        }

        if (!string.IsNullOrEmpty(dto.Text))
        {
            CapsAPI.AB_AHA_DspStr(dto.Gateway, dto.Tag, dto.Text, 0, 0);
        }
    }
    public Task DisplayQty(int gateway, int tag, int qty)
    {
        CapsAPI.AB_LB_DspNum(gateway, tag, qty, 0, 0);
        CapsAPI.AB_LED_Status(gateway, tag, 0, 1);
        return Task.CompletedTask;
    }

    public Task ClearHeader(int gateway, int tag)
    {
        CapsAPI.AB_AHA_ClrDsp(gateway, tag);
        return Task.CompletedTask;
    }

    public Task ShowHeader(int gateway, int tag, string text)
    {
        CapsAPI.AB_AHA_DspStr(gateway, tag, text, 0, 0);
        return Task.CompletedTask;
    }
    public Task<IReadOnlySet<int>> GetReadyTags(int gateway)
    {
        Console.WriteLine($"[PTL][DBG] GetReadyTags START gw={gateway}");

        var ready = new HashSet<int>();
        const int PORT = 1;

        // 1️⃣ Trigger Tag Diagnostic
        int ret = CapsAPI.AB_GW_TagDiag(gateway, PORT);
        Console.WriteLine($"[PTL][DBG] AB_GW_TagDiag ret={ret}");

        if (ret != 0)
            return Task.FromResult<IReadOnlySet<int>>(ready);

        // 2️⃣ Prepare RAW CCB buffer
        var ccb = new QWAY_CCB_RAW
        {
            ccbdata = new byte[256]
        };

        bool gotDiag = false;

        // 3️⃣ RX LOOP (CRITICAL)
        for (int i = 0; i < 10; i++)
        {
            int rcv = CapsAPI.AB_GW_RcvMsg_RAW(gateway, ref ccb);

            if (rcv > 0)
            {
                Console.WriteLine(
                    $"[PTL][DBG] RX cmd=0x{ccb.ccbcmd:X2}, len={ccb.ccblen}"
                );

                // 0x09 = Tag Diagnostic response
                if (ccb.ccbcmd == 0x09)
                {
                    gotDiag = true;
                    break;
                }
            }

            // Give gateway time to respond
            Thread.Sleep(20);
        }

        if (!gotDiag)
        {
            Console.WriteLine("[PTL][DBG] TagDiag NOT received");
            return Task.FromResult<IReadOnlySet<int>>(ready);
        }

        // 4️⃣ Dump RAW bitmap for verification
        Console.WriteLine(
            $"[PTL][DBG] RAW={BitConverter.ToString(ccb.ccbdata, 11, 32)}"
        );

        // 5️⃣ Parse bytes 12–43 (32 bytes bitmap)
        for (int byteIndex = 0; byteIndex < 32; byteIndex++)
        {
            byte b = ccb.ccbdata[11 + byteIndex]; // byte 12 = index 11

            for (int bit = 0; bit < 8; bit++)
            {
                int tag = byteIndex * 8 + bit + 1;
                if (tag > 252) break;

                bool failed = ((b >> bit) & 0x01) == 1;
                if (!failed)
                    ready.Add(tag);
            }
        }

        Console.WriteLine(
            $"[PTL][DBG] READY COUNT={ready.Count}, TAGS={string.Join(",", ready.Take(20))}"
        );

        return Task.FromResult<IReadOnlySet<int>>(ready);
    }

    private void ConfirmTag(int gateway, int tag)
    {
        CapsAPI.AB_LB_DspNum(gateway, tag, 0, 0, 0);
        CapsAPI.AB_LED_Status(gateway, tag, 1, 1);
    }

}
