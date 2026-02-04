using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Core.Interfaces;

public class HardwarePtlDisplay : IPtlDisplay
{
    public void Execute(PtlTxCommandDto dto)
    {
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
    public void DisplayQty(int gateway, int tag, int qty)
    {
        CapsAPI.AB_LB_DspNum(gateway, tag, qty, 0, 0);
        CapsAPI.AB_LED_Status(gateway, tag, 0, 1);
    }

    public void ConfirmTag(int gateway, int tag)
    {
        CapsAPI.AB_LB_DspNum(gateway, tag, 0, 0, 0);
        CapsAPI.AB_LED_Status(gateway, tag, 1, 1);
    }

    public void ShowHeader(int gatewayId, int tag, string text)
    {
        CapsAPI.AB_AHA_DspStr(gatewayId, tag, text, 0, 0);
    }

    public void ClearHeader(int gatewayId, int tag)
    {
        CapsAPI.AB_AHA_ClrDsp(gatewayId, tag);
    }
}
