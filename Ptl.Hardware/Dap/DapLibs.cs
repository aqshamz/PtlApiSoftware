using System.Runtime.InteropServices;

namespace DAPCAPS
{
    sealed class CapsAPI
    {
        public static int GwCnt;

        //Structure QWAY_CCB
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct QWAY_CCB
        {
            public int ccblen;
            public byte ccbport;
            public byte ccbdnode;
            public byte ccbsnode;
            public byte ccbcmd;
            //<VBFixedArray(256)> Dim ccbdata() As Byte
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ccbdata;
        }

        //testing C#
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct QWAY_CCB_RAW
        {
            public int ccblen;
            public byte ccbport;
            public byte ccbdnode;
            public byte ccbsnode;
            public byte ccbcmd;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] ccbdata;
        }

        static public string AB_ErrMsg(int ret)
        {
            string returnValue;
            string tmpstr;
            switch (ret)
            {
                case -3:
                    tmpstr = "Parameter data is error !";
                    break;
                case -2:
                    tmpstr = "TCP is not created yet !";
                    break;
                case -1:
                    tmpstr = "DAP_ID out of range !";
                    break;
                case 0:
                    tmpstr = "Closed";
                    break;
                case 1:
                    tmpstr = "Open";
                    break;
                case 2:
                    tmpstr = "Listening";
                    break;
                case 3:
                    tmpstr = "Connection is Pending";
                    break;
                case 4:
                    tmpstr = "Resolving the host name";
                    break;
                case 5:
                    tmpstr = "Host is Resolved";
                    break;
                case 6:
                    tmpstr = "Waitting"; //"Waiting to Connect"
                    break;
                case 7:
                    tmpstr = "ok"; //"Connected ok "
                    break;
                case 8:
                    tmpstr = "Connection is closing";
                    break;
                case 9:
                    tmpstr = "State error has occurred";
                    break;
                case 10:
                    tmpstr = "Connection state is undetermined";
                    break;
                default:
                    tmpstr = "Unknown Error Code";
                    break;
            }

            returnValue = tmpstr;
            return returnValue;
        }

        //  // «Å§iDAPAPI.DLL
        //  // initial Gateway
        [DllImport("dapapi.dll")]
       public static extern int AB_API_Open();
        [DllImport("dapapi")]
       public static extern int AB_API_Close();
        [DllImport("dapapi")]
       public static extern int AB_GW_Open(int Gateway_id);
        [DllImport("dapapi")]
       public static extern int AB_GW_Close(int Gateway_id);
        [DllImport("dapapi")]
        public static extern int AB_GW_Cnt();
        [DllImport("dapapi")]
        public static extern int AB_GW_Conf(int ndx, ref int Gateway_id, ref byte ip, ref int port);  // *
        [DllImport("dapapi")]
        public static extern int AB_GW_Ndx2ID(int ndx);
        [DllImport("dapapi")]
        public static extern int AB_GW_ID2Ndx(int Gateway_id);
        [DllImport("dapapi")]
        public static extern int AB_GW_InsConf(int Gateway_id, byte ip, int port);
        [DllImport("dapapi")]
        public static extern int AB_GW_UpdConf(int Gateway_id, byte ip, int port);
        [DllImport("dapapi")]
        public static extern int AB_GW_DelConf(int Gateway_id);

        //  // Get/Send message from/to Gateway
        [DllImport("dapapi")]
       public static extern int AB_GW_RcvMsg(int Gateway_id, ref QWAY_CCB ccb);

        //testing C#
        [DllImport("dapapi", EntryPoint = "AB_GW_RcvMsg")]
        public static extern int AB_GW_RcvMsg_RAW(int Gateway_id, ref QWAY_CCB_RAW ccb);

        [DllImport("dapapi")]
        public static extern int AB_GW_SndMsg(int Gateway_id, ref QWAY_CCB ccb);
        [DllImport("dapapi")]
        public static extern int AB_GW_RcvReady(int Gateway_id);
        [DllImport("dapapi")]
       public static extern int AB_GW_RcvButton(ref byte data, ref int data_cnt);
        [DllImport("dapapi")]
        public static extern int AB_Tag_RcvMsg(ref int Gateway_id, ref int tag_addr, ref short subcmd, ref short msg_type, ref byte data, ref int data_cnt);

        //  // Get Gateway status
        [DllImport("dapapi")]
       public static extern int AB_GW_Status(int Gateway_id);
        [DllImport("dapapi")]
       public static extern int AB_GW_AllStatus(byte status);
        [DllImport("dapapi")]
        public static extern int AB_GW_TagDiag(int Gateway_id, int port);
        [DllImport("dapapi")]
       public static extern int AB_GW_SetPollRang(int Gateway_id, int max_node);
        //[DllImport("dapapi")]
        //public static extern int AB_GW_SetPollRang(int Gateway_id, int port, int max_node);  // for ABLELINK

        //  // Cable-less picking tag set up node address automatically
        [DllImport("dapapi")]
       public static extern int AB_CLTAG_DspAddr(int Gateway_id, int port, int mode);
        [DllImport("dapapi")]
       public static extern int AB_CLTAG_SetAddr(int Gateway_id, int Node_Addr, int mode);
        [DllImport("dapapi")]
       public static extern int AB_TAG_CycleEdit(int Gateway_id, int Node_Addr, byte on_off);

        //  // Send Message to picking tag
        [DllImport("dapapi")]
       public static extern int AB_LB_DspNum(int Gateway_id, int Node_Addr, int Disp_Int, byte Dot, int interval);
        [DllImport("dapapi")]
       public static extern int AB_LB_DspStr(int Gateway_id, int Node_Addr, string Disp_Str, byte Dot, int interval);
        [DllImport("dapapi")]
        public static extern int AB_LB8_DspStr(int Gateway_id, int Node_Addr, string Disp_Str, byte Dot, int interval);
        [DllImport("dapapi")]
        public static extern int AB_LB_DspAddr(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
       public static extern int AB_LED_Dsp(int Gateway_id, int Node_Addr, byte Lamp_STA, int interval);
        [DllImport("dapapi")]
       public static extern int AB_LED_Status(int Gateway_id, int Node_Addr, byte Lamp_color, byte Lamp_STA);
        [DllImport("dapapi")]
       public static extern int AB_BUZ_On(int Gateway_id, int Node_Addr, byte Buzzer_Type);
        [DllImport("dapapi")]
       public static extern int AB_LB_SetMode(int Gateway_id, int Node_Addr, byte pick_mode);
        [DllImport("dapapi")]
       public static extern int AB_LB_Simulate(int Gateway_id, int Node_Addr, byte simulate_mode);
        [DllImport("dapapi")]
       public static extern int AB_LB_SetLock(int Gateway_id, int Node_Addr, byte Lock_State, byte Lock_key);
        [DllImport("dapapi")]
       public static extern int AB_TAG_Reset(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
       public static extern int AB_TAG_mode(int Gateway_id, int Node_Addr, int Save_mode, byte mode_type);
        [DllImport("dapapi")]
       public static extern int AB_TAG_CountDigit(int Gateway_id, int Node_Addr, byte Digit);
        [DllImport("dapapi")]
       public static extern int AB_TAG_ButtonDelay(int Gateway_id, int Node_Addr, byte DelayTime);
        [DllImport("dapapi")]
        public static extern int AB_Tag_ChgAddr(int Gateway_id, int Node_Addr, int new_addr);

        //  // Send message to 12-digits Alphanumerical display (AT50C)
        [DllImport("dapapi")]
       public static extern int AB_AHA_DspStr(int Gateway_id, int Node_Addr, string Disp_Str, byte BeConfirm, byte DigitSta);
        [DllImport("dapapi")]
       public static extern int AB_AHA_ClrDsp(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
       public static extern int AB_AHA_ReDsp(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
       public static extern int AB_AHA_LED_Dsp(int Gateway_id, int Node_Addr, byte Lamp_Type, int Lamp_STA);
        [DllImport("dapapi")]
       public static extern int AB_AHA_BUZ_On(int Gateway_id, int Node_Addr, byte Buz_STA);
        // Only for model AT50C-20
        [DllImport("dapapi")]
       public static extern int AB_AHA_DEMO_Control(int Gateway_id, int Node_Addr, byte Con_STA);
        [DllImport("dapapi")]
       public static extern int AB_AHA_DEMO_Interval(int Gateway_id, int Node_Addr, byte Interval);

        //  // Send message to 3-digit directional and 2-digit vertical & directional pick (AT503A & AT502V)
        [DllImport("dapapi")]
       public static extern int AB_AV_LB_DspNum(int Gateway_id, int Node_Addr, int Disp_Int, byte Arrow, byte Dot, int interval);
        //  // Send message to 6-digit,3 separated windows pick tag (AT506-3W-123)
        [DllImport("dapapi")]
       public static extern int AB_3W_LB_DspNum(int Gateway_id, int Node_Addr, string Row, string Col, int Disp_Int, byte Dot, int interval);
        //  // Send message to Melody completion indicator (AT510M)
        [DllImport("dapapi")]
       public static extern int AB_Melody_On(int Gateway_id, int Node_Addr, byte Song, byte Buzzer_Type);
        [DllImport("dapapi")]
       public static extern int AB_Melody_Volume(int Gateway_id, int Node_Addr, byte Volume);
        //  // Send message to 6-digit, 2 separated windows pick tag (AT506-2W-33)
        [DllImport("dapapi")]
       public static extern int AB_2W_LB_DspNum(int Gateway_id, int Node_Addr, int Case_unit, int Piece_unit, byte Dot, int Interval);
        //  // Send message to 10-digit, 3 separated windows Alphanumerical pick tag (AT50A-3W-523)
        [DllImport("dapapi")]
       public static extern int AB_3W_CP_DspNum(int Gateway_id, int Node_Addr, string Lot, string Loc, int Disp_Int, int interval);
        //  // Send message to 10-digit, 3 separated windows pick tag (AT50A-3W-334K)
        [DllImport("dapapi")]
       public static extern int AB_3W334_LB_DspNum(int Gateway_id, int Node_Addr, int Case_Unit, int Pack_Unit, int Piece_Unit, 
                                                    byte Dot1, byte Dot2, int Interval);
        //  // Send message to 8-digit, 2 separated windows pick tag(AT508-2W-44K)
        [DllImport("dapapi")]
       public static extern int AB_2W44_LB_DspNum(int Gateway_id, int Node_Addr, int Case_Unit, int Piece_Unit, byte Dot, int Interval);
       //  // Send message to 3-digit, 4-lightable button pick tag (AT503-4K)
        [DllImport("dapapi")]
       public static extern int AB_4K_DspNum(int Gateway_id, int Node_Addr, int Qty, byte Buf_Idx, byte Auto_Rtv, byte LED_Sta, byte LED_Sta2 , byte Dot);
        [DllImport("dapapi")]
       public static extern int AB_4K_DspLED(int Gateway_id, int Node_Addr, byte LED_Idx, byte BeConfirm, byte LED_Sta);
        [DllImport("dapapi")]
       public static extern int AB_4K_Demo(int Gateway_id, int Node_Addr, byte Buf_Idx, string Disp_Str, byte LED_Sta, byte Swap_Time, byte Dot);


        //   // DI/DO Operation
        [DllImport("dapapi")]
       public static extern int AB_DIO_ReadIoStatus(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
       public static extern int AB_DIO_SetDO(int Gateway_id, int Node_Addr, int channel , int status);
        [DllImport("dapapi")]
       public static extern int AB_DIO_SetDiRspMode(int Gateway_id, int Node_Addr, int mode);

        //  //-----DCS-19
        [DllImport("dapapi")]
        public static extern int AB_DCS_InputMode(int Gateway_id, int Node_Addr, byte input_mode);
        [DllImport("dapapi")]
        public static extern int AB_DCS_BufSize(int Gateway_id, int Node_Addr, byte buf_size);
        [DllImport("dapapi")]
        public static extern int AB_DCS_SetConf(int Gateway_id, int Node_Addr, byte enable_status, byte disable_status);
      //[DllImport("dapapi")]
      //public static extern int AB_DCS_ReqConf(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DCS_GetVer(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DCS_Reset(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DCS_SetRows(int Gateway_id, int Node_Addr, byte rows);
        [DllImport("dapapi")]
        public static extern int AB_DCS_SimulateKey(int Gateway_id, int Node_Addr, byte key_code);
        [DllImport("dapapi")]
        public static extern int AB_DCS_Cls(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DCS_Buzzer(int Gateway_id, int Node_Addr, int alarm_time, int alarm_cnt);
        [DllImport("dapapi")]
        public static extern int AB_DCS_ScrollUp(int Gateway_id, int Node_Addr, byte up_rows);
        [DllImport("dapapi")]
        public static extern int AB_DCS_ScrollDown(int Gateway_id, int Node_Addr, byte down_rows);
      //[DllImport("dapapi")]
      //public static extern int AB_DCS_ScrollHome(int Gateway_id, int Node_Addr);
      //[DllImport("dapapi")]
      //public static extern int AB_DCS_ScrollEnd(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DCS_SetCursor(int Gateway_id, int Node_Addr, byte row, byte column);
        [DllImport("dapapi")]
        public static extern int AB_DCS_DspStrE(int Gateway_id, int Node_Addr, string dsp_str, int dsp_cnt);
        [DllImport("dapapi")]
        public static extern int AB_DCS_DspStrC(int Gateway_id, int Node_Addr, string dsp_str, int dsp_cnt);

        //  //-----DT200
        [DllImport("dapapi")]
        public static extern int AB_DT2_DspStr(int Gateway_id, int Node_Addr, string dsp_str, int dsp_cnt);
        [DllImport("dapapi")]
        public static extern int AB_DT2_EnableCounter(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DT2_DisableCounter(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DT2_ReadCounter(int Gateway_id, int Node_Addr);
        [DllImport("dapapi")]
        public static extern int AB_DT2_SetCounter(int Gateway_id, int Node_Addr, int count);
        //[DllImport("dapapi")]
        //public static extern int AB_DT2_Buzzer(int Gateway_id, int Node_Addr, byte alarm_time, byte alarm_cnt);
        //[DllImport("dapapi")]
        //public static extern int AB_DT2_AutoReport(int Gateway_id, int Node_Addr, byte enable_flag);
        //[DllImport("dapapi")]
        //public static extern int AB_DT2_FunKeyConfig(int Gateway_id, int Node_Addr, byte config);
        //[DllImport("dapapi")]
        //public static extern int AB_DT2_Terminator(int Gateway_id, int Node_Addr, byte config);

        //  // RS-232 Converter Operation
        [DllImport("dapapi")]
        public static extern int AB_CNV_SendData(int Gateway_id, int Node_Addr, string dps_str, int dsp_cnt);
        [DllImport("dapapi")]
        public static extern int AB_CNV_SetTerminator(int Gateway_id, int Node_Addr, int terminator);

        //  // Fixed CCD Operation
        [DllImport("dapapi")]
        public static extern int AB_CCD_Rescan(int Gateway_id, int Node_Addr);
        //[DllImport("dapapi")]
        //public static extern int AB_CCD_Action(int Gateway_id, int Node_Addr, int Action);
   }
}