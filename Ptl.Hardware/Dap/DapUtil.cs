using System;
//using System.Windows.Forms;
using System.Collections.Generic;

namespace DAPCAPS
{
    public class Util
    {
        public static int[] m_DiagGWTime;
        public static bool[] m_lDiagGwOk;
        public static bool m_lSetBestPoll;
        public static bool m_lCheckTagLive;
        public static int m_CurGwID;
        private static string m_DelayItem;
        //public static long CurNode;
        public static int m_GwID;
        public static int m_GwPort;
        public static int m_NodeAddress;
        public static int m_TagCommand;
        public static string m_RcvTagData;
        public static string m_SubCommandMess;
        public static int m_max_node;
        public static int m_maxid;
        public static bool m_mnuTagDiag;
        public static m_GwConfType[] m_GwConf;
        public static string m_ListMsg;

        public struct m_GwConfType
        {
            public int gw_id;
            public string ip;
            public int port;
        }

        // 公用屬性 

        public bool[] lDiagGwOk
        {
            get { return m_lDiagGwOk; }
            set { m_lDiagGwOk = value; }
 
        }
        public bool lSetBestPoll
        {
            get { return m_lSetBestPoll; }
            set { m_lSetBestPoll = value; }
        }        
        public bool lCheckTagLive
        {
            get { return m_lCheckTagLive; }
            set { m_lCheckTagLive = value; }
        }
        public string DelayItem
        {
            get{ return m_DelayItem; }
            set { m_DelayItem = value; }
        }
        public int CurGwId
        {
            get { return m_CurGwID; }
            set { m_CurGwID = value; }
        }
        public int GwID
        {
            get { return m_GwID; }
            set { m_GwID = value; }
        }
        public int GwPort
        {
            get { return m_GwPort; }
            set { m_GwPort = value; }
        }
        public int NodeAddress
        {
            get { return m_NodeAddress; }
            set { m_NodeAddress = value; }
        }
        public int TagCommand
        {
            get { return m_TagCommand; }
            set { m_TagCommand = value; }
        }
        public string RcvTagData
        {
            get { return m_RcvTagData; }
            set { m_RcvTagData = value; }
        }
        public string SubCommandMess
        {
            get { return m_SubCommandMess; }
            set { m_SubCommandMess = value; }
        }
        public int Max_PollingNode
        {
            get { return m_max_node; }
            set { m_max_node = value; }
        }
         public bool mnuTagDiag
        {
            get { return m_mnuTagDiag; }
            set { m_mnuTagDiag = value; }
        }


        // 公用函數
        public static char Chr(int Num)
        {
            char C = Convert.ToChar(Num);
            return C;
        }

        public static int Asc(string S)
        {
            int N = Convert.ToInt32(S[0]);
            return N;
        }

        public static int ASC(char C)
        {
            int N = Convert.ToInt32(C);
            return N;
        }

        public static void Dap_Close()
        {
            int ret;
            int nGw;
            //On Error Resume Next VBConversions Warning: On Error Resume Next not supported in C#

            for (nGw = 1; nGw <= DAPCAPS.CapsAPI.GwCnt; nGw++)
            {
                ret = DAPCAPS.CapsAPI.AB_GW_Status(nGw);
                if (ret > 0)
                {
                    ret = DAPCAPS.CapsAPI.AB_LB_DspStr(nGw, - 252, "", 0, -3); //turn off all tags on port1
                    ret = DAPCAPS.CapsAPI.AB_LB_DspStr(nGw, 252, "", 0, -3); //turn off all tags on port2

                    ret = DAPCAPS.CapsAPI.AB_GW_Close(nGw); //Close the specified Gateway
                }
            }
        }

        public static void Dap_Setup()
        {
            int ret;
            if (m_CurGwID==0)
            {
                for (int i = 1; i <= DAPCAPS.CapsAPI.GwCnt; i++)
                    ret=DAPCAPS.CapsAPI.AB_GW_Open(i);

            }
            else
                ret = DAPCAPS.CapsAPI.AB_GW_Open(m_CurGwID);  // Open the specified Gateway
        }

        static public string Bin2Str(byte[] bufbin, int start, int cnt)
        {
            string returnValue;
            int i;
            string bufstr;
            bufstr = "";
            //UPGRADE_WARNING: 無法解析物件 cnt 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            for (i = 0; i <= cnt - 1; i++)
            {
                bufstr = bufstr + Chr(bufbin[i]);
            }

            returnValue = bufstr;
            return returnValue;

        }
        static public int AB_LoadConf()
        {
            int returnValue=0;
            int ret;
            int i;
            int j;
            int id=0;
            byte[] ip = new byte[21];
            int port=0;
            string tmpstr;

            DAPCAPS.CapsAPI.GwCnt = DAPCAPS.CapsAPI.AB_GW_Cnt();
            if (DAPCAPS.CapsAPI.GwCnt == 0)
            {
                return returnValue;
            }
            m_GwConf = new m_GwConfType[DAPCAPS.CapsAPI.GwCnt];

            for (i = 0; i <= DAPCAPS.CapsAPI.GwCnt - 1; i++)
            {
                ret = DAPCAPS.CapsAPI.AB_GW_Conf(i, ref id, ref ip[0], ref port);
                if (ret >= 0)
                {
                    m_GwConf[i].gw_id = id;
                    m_GwConf[i].port = port;
                    tmpstr = "";
                    for (j = 0; j <= 20; j++)
                    {
                        if (ip[j] == 0)
                        {
                            break;
                        }
                        tmpstr = tmpstr + Chr(ip[j]);
                    }
                    m_GwConf[i].ip = tmpstr;
                }
                else
                {
                    m_GwConf[i].gw_id = 0;
                    m_GwConf[i].port = 0;
                    m_GwConf[i].ip = "";
                }
            }
            returnValue = DAPCAPS.CapsAPI.GwCnt;
            return returnValue;
        }

        static public int BinCopy(ref byte[] dstbuf, ref int dst_ndx, byte[] srcbuf, int src_ndx, int cnt)
        {
            int returnValue;
            int i;

            for (i = 0; i <= cnt - 1; i++)
            {
                dstbuf[dst_ndx + i] = srcbuf[src_ndx + i];
            }

            returnValue = 0;
            return returnValue;
        }

        
        static public int Str2Bin(string strdata, ref byte[] bufbin, int start)
        {
            int returnValue;
            //// change string to byte array
            int i;
            int strcnt;
            int ndx;
            int data;
            int val1,val2;

            strcnt = strdata.Length;
            ndx = start;
            for (i = 1; i <= strcnt; i++)
            {
                data = Asc(strdata.Substring(i - 1, 1));
                val1 = (data % 256) & 0xFF;
                bufbin[ndx] = (byte)val1;
                ndx++;
                if (data >= 256)
                {
                    val2 = (int)(data / 256) & 0xFF;
                    bufbin[ndx] = (byte)val2;
                    ndx++;
                }
            }

            returnValue = ndx;
            return returnValue;
        }
        

        //static public int QwayGetDataB(ListBox ListBox2,Button Button7,Timer Timer2)
        //{
        //    int returnValue;
        //    //ret>=0:ok,else:err
        //    int ret;
        //    byte[] ccbdata = new byte[255];
        //    int ccblen;
        //    int nRcvbun;
        //    int k;
        //    string tmpstr;
        //    string tmpstr1;
        //    string m_RcvTagData="";
        //    byte tmp=0;
        //    int nGwId,nNode;
        //    short TagCommand, nMsg_type;
        //    nGwId = 0;
        //    nNode = 0;
        //    TagCommand = -1;
        //    nMsg_type = -1;
        //    m_SubCommandMess = "";
        //    tmpstr1 = "";
        //    ccblen = 0;

        //    //================================='
        //    //By API COMMAND
        //    ret = DAPCAPS.CapsAPI.AB_Tag_RcvMsg(ref nGwId, ref nNode, ref TagCommand, ref nMsg_type, ref ccbdata[0], ref ccblen);
        //    if (ret > 0)
        //    {
        //        m_GwID = nGwId;
        //        m_NodeAddress = nNode;
        //        m_TagCommand = TagCommand;

        //        if (m_NodeAddress < 0)
        //            m_GwPort = 1;
        //        else
        //            m_GwPort = 2;
        //        m_NodeAddress = Math.Abs(m_NodeAddress);
                
        //        m_RcvTagData = Bin2Str(ccbdata, 0, ccblen).Trim();
        //            switch (m_TagCommand)
        //            {
        //                case 100:
        //                    switch (nMsg_type)
        //                    {
        //                        case 0:
        //                            nRcvbun = DAPCAPS.CapsAPI.AB_GW_RcvButton(ref ccbdata[0], ref ccblen);
        //                            //1   && Only CONFIRM button
        //                            //2   && Only UP button
        //                            //3   && Only DOWN button
        //                            //4   && First UP, then CONFIRM
        //                            //5   && First DOWN , then CONFIRM
        //                            //6   && First UP and DOWN , then CONFIRM
        //                            //7   && First DOWN , then UP
        //                            //8   && First CONFIRM , then UP
        //                            //9   && First DOWN and CONFIRM , then UP
        //                            //10  && First UP , then DOWN
        //                            //11  && First CONFIRM , then DOWN
        //                            //12  && First UP and CONFIRM , then DOWN
        //                            //---------------------'

        //                            m_SubCommandMess = "keycode return/receive Button value:" + nRcvbun.ToString() + ")";
        //                            break;
        //                        case 1:
        //                            m_SubCommandMess = "tag Busy)";
        //                            break;
        //                        case 2:
        //                            m_SubCommandMess = "first record disappear)";
        //                            break;
        //                        case 3:
        //                            m_SubCommandMess = "last record confirm)";
        //                            break;
        //                    }
        //                    if (nMsg_type == -1)
        //                    {
        //                        m_SubCommandMess = "Null";
        //                    }
        //                    else
        //                    {
        //                        m_SubCommandMess = nMsg_type.ToString() + " (" + m_SubCommandMess;
        //                    }

        //                    m_RcvTagData = Bin2Str(ccbdata, 0, ccblen).Trim();

        //                //ListBox2.Items.Add(DateTime.Today.ToString() + " [" + m_GwID.ToString() + "," + m_GwPort.ToString() + "," + m_NodeAddress.ToString() + "," + m_TagCommand.ToString() + "," + m_SubCommandMess + "]," + m_RcvTagData.Trim());
        //                ListBox2.SelectedIndex = ListBox2.Items.Count - 1;
        //                    break;

        //                case 9:
        //                    m_lDiagGwOk[m_GwID] = true; //check gateway connectting
        //                    tmpstr = ldump3(ccbdata, 0, ccblen);

        //                    tmpstr = "";
        //                    if (m_mnuTagDiag == true)
        //                    {
        //                        if (m_CurGwID == m_GwID)
        //                        {
        //                            MsgBox2(" Tag Diagnosis as below ===>");
        //                        }
        //                    }
        //                    for (k = 1; k <= 250; k++)
        //                    {
        //                        if (((k - 1) % 8) == 0)
        //                        {
        //                            tmp = ccbdata[3 + ((k - 1) / 8)];
        //                        }
        //                        if ((tmp % 2) == 1)
        //                        {
        //                            tmpstr = tmpstr + "0";
        //                        }
        //                        else
        //                        {
        //                            tmpstr = tmpstr + "1";
        //                            m_maxid = k;
        //                        }
        //                        tmp = (byte)(tmp / 2);
        //                    }

        //                    //easy read
        //                    if (m_mnuTagDiag == true)
        //                    {
        //                        if (m_CurGwID == m_GwID)
        //                        {
        //                            for (k = 1; k <= m_NodeAddress; k++)
        //                            {
        //                                tmpstr1 = tmpstr1 + tmpstr.Substring(k - 1, 1);
        //                                if (k > 1 && (k % 10) == 0)
        //                                {
        //                                    tmpstr1 = tmpstr1 + ",";
        //                                }
        //                                if ((k % 50) == 0)
        //                                {
        //                                    //ListBox2.Items.Add(tmpstr1.PadLeft(18, ' ') + '\n');
        //                                    tmpstr1 = "";
        //                                }
        //                            }
        //                            if (tmpstr1.Trim() != "")
        //                            {
        //                                //ListBox2.Items.Add(tmpstr1.PadLeft(18, ' ') + '\n');
        //                            }
        //                            //ListBox2.Items.Add(" ".PadLeft(18, ' ') + " <<GW id:" + m_GwID.ToString() + ",Max Node = " + m_maxid + ",Current polling range = " + m_NodeAddress + ">>");
        //                            ListBox2.SelectedIndex = ListBox2.Items.Count - 1;
        //                            m_mnuTagDiag = false;
        //                        }
        //                        else
        //                        {
        //                            if (m_lDiagGwOk[m_CurGwID] == false) //connect fail
        //                            {
        //                                m_mnuTagDiag = false;
        //                            }
        //                        }
        //                    }

        //                    //set best polling range
        //                    if (m_lSetBestPoll == true)
        //                    {
        //                        if (m_CurGwID == m_GwID)
        //                        {
        //                            ret = DAPCAPS.CapsAPI.AB_GW_SetPollRang(m_GwID, 0 - m_maxid);
        //                            Timer2.Enabled = false;
        //                            m_lSetBestPoll = false;
        //                        //ListBox2.Items.Add(DateTime.Today.ToString() + " Gateway = " + m_GwID + " " + ",Max Node = " + m_maxid);
        //                            ListBox2.SelectedIndex = ListBox2.Items.Count - 1;
        //                            Button7.Text = "Set best polling range";
        //                            Button7.Enabled = true;
        //                        }
        //                    }
        //                    break;

        //                case 10:
        //                    if (m_lCheckTagLive == true)
        //                    {
        //                        if (m_CurGwID == m_GwID)
        //                        {
        //                            if (m_GwPort == 1)
        //                            {
        //                                m_NodeAddress = m_NodeAddress * (-1);
        //                            }
        //                            MsgBox2("Tag has died or not existed [" + m_GwID.ToString() + "," + m_NodeAddress + "]");
        //                            MessageBox.Show("Tag has died or not existed [" + m_GwID.ToString() + "," + m_NodeAddress + "]");
        //                            m_lCheckTagLive = false;
        //                            Timer2.Enabled = false;
        //                        }
        //                    }
        //                    break;

        //                case 60:
        //                    m_RcvTagData = Bin2Str(ccbdata, 0, ccblen).Trim();
        //                    ListBox2.Items.Add(DateTime.Today.ToString() + " [" + m_GwID.ToString() + "," + m_GwPort.ToString() + "," + m_NodeAddress.ToString() + "," + m_TagCommand.ToString() + "," + m_SubCommandMess + "]," + m_RcvTagData.Substring(m_RcvTagData.Length - 8, 8));
        //                    ListBox2.SelectedIndex = ListBox2.Items.Count - 1;
        //                    break;

        //                default:
        //                    m_RcvTagData = Bin2Str(ccbdata, 0, ccblen).Trim();
        //                    ListBox2.Items.Add(DateTime.Today.ToString() + " [" + m_GwID.ToString() + "," + m_GwPort.ToString() + "," + m_NodeAddress.ToString() + "," + m_TagCommand.ToString() + "," + m_SubCommandMess + "]," + m_RcvTagData.Trim());
        //                    ListBox2.SelectedIndex = ListBox2.Items.Count - 1;
        //                    break;

        //            }
        //    }
        //    returnValue = ret;
        //    return returnValue;
        //}
        public static void DisplayNum(int val)
        {
            int nGw;
            int ret;
            for (nGw = 1; nGw <= DAPCAPS.CapsAPI.GwCnt; nGw++)
            {
                ret = DAPCAPS.CapsAPI.AB_LB_DspNum(nGw, - 252, val, 0, 0);
            }
        }
        public static void clearTags()
        {
            int nGw;
            int ret;
            for (nGw = 1; nGw <= DAPCAPS.CapsAPI.GwCnt; nGw++)
            {
                ret = DAPCAPS.CapsAPI.AB_LB_DspNum(nGw, - 252, 0, 0, -2);
                ret = DAPCAPS.CapsAPI.AB_LB_DspNum(nGw, 252, 0, 0, -2);
                ret = DAPCAPS.CapsAPI.AB_BUZ_On(nGw, 252, 0);
                ret = DAPCAPS.CapsAPI.AB_BUZ_On(nGw, -252, 0);
                ret = DAPCAPS.CapsAPI.AB_LED_Status(nGw, 252, 0, 0);
                ret = DAPCAPS.CapsAPI.AB_LED_Status(nGw, -252, 0, 0);

                ret = DAPCAPS.CapsAPI.AB_AHA_ClrDsp(nGw, 252);
                ret = DAPCAPS.CapsAPI.AB_AHA_ClrDsp(nGw, -252);
                ret = DAPCAPS.CapsAPI.AB_AHA_BUZ_On(nGw, 252, 0);
                ret = DAPCAPS.CapsAPI.AB_AHA_BUZ_On(nGw, -252, 0);
                ret = DAPCAPS.CapsAPI.AB_AHA_LED_Dsp(nGw, 252, 0, 0);
                ret = DAPCAPS.CapsAPI.AB_AHA_LED_Dsp(nGw, -252, 0, 0);

                ret = DAPCAPS.CapsAPI.AB_DCS_Cls(nGw, 252);
                ret = DAPCAPS.CapsAPI.AB_DCS_Cls(nGw, -252);
            }
        }
        public static void TimeDelay(int DT)
        {
            int StartTick;
            StartTick = Environment.TickCount; //在這邊記下一個數字
            do
            {
                if (Environment.TickCount - StartTick >= DT)
                {
                    break; //這段是判斷是否經過了DT個微秒數, 如果是就離開, DT是你給的傳入值
                }
                //Application.DoEvents() '如果還沒到你希望的延遲時間, 先讓系統去處理其他等待中的工作
            } while (true);
        }

        public static void MsgBox2(string msg)
        {
          
        }

        /*
        static public short HexIp(string ip_str, ref string op_str)
        {
            short returnValue;
            int start;
            short cnt;
            string tmpstr;

            op_str = "";
            start = 1;
            cnt = 0;
            do
            {
                tmpstr = ip_str.Substring(start - 1, 1);
                start++;
                if (tmpstr == "")
                {
                    break;
                }
                if (tmpstr == "\\")
                {
                    tmpstr = "&H";
                    tmpstr = tmpstr + ip_str.Substring(start - 1, 2);
                    start = start + 2;
                    tmpstr = Strings.Chr((int)(Conversion.Val(tmpstr))); // 轉換成字元
                }
                op_str = op_str + tmpstr;
                cnt++;
            } while (true);
            returnValue = cnt;
            return returnValue;
        }

        static public short HexIpB(string ip_str, ref byte[] op_str, short op_start)
        {
            short returnValue;
            int start;
            short cnt;
            string tmpstr;
            short data;

            start = 1;
            cnt = op_start;
            do
            {
                tmpstr = ip_str.Substring(start - 1, 1);
                start++;
                if (tmpstr == "")
                {
                    break;
                }
                if (tmpstr == "\\")
                {
                    tmpstr = "&H" + ip_str.Substring(start - 1, 2);
                    start = start + 2;
                    op_str[cnt] = byte.Parse(tmpstr);
                    cnt++;
                }
                else
                {
                    data = Asc(tmpstr);
                    op_str[cnt] = (data % 256) & 0xFF;
                    cnt++;
                    if ((data & 0xFF00) == 1)
                    {
                        op_str[cnt] = (data / 256) & 0xFF;
                        cnt++;
                    }
                }
            } while (true);
            returnValue = cnt;

            return returnValue;
        }


        static public short HexOp0(object ip_str, int cnt, ref string op_str)
        {
            short start;
            string tmpstr;
            short ch;

            op_str = "";
            for (start = 1; start <= cnt; start++)
            {
                //UPGRADE_WARNING: 無法解析物件 ip_str 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                ch = Asc(Strings.Mid(ip_str, start, 1));
                //If ch = 0 Or ch = &H5C Or ch > &H80 Then
                if (ch < 0x20 || ch == 0x5C || ch >= 0x80)
                {
                    tmpstr = Hex(ch);
                    if (tmpstr.Length < 2)
                    {
                        op_str = op_str + "\\0" + tmpstr;
                    }
                    else
                    {
                        op_str = op_str + "\\" + tmpstr;
                    }
                }
                else
                {
                    op_str = op_str + Chr((int)ch);
                }
            }
        }

        static public string HexOp2(ref string ip_str, ref int cnt)
        {
            string returnValue;
            short ret;
            string op_str = "";

            if (cnt < 0)
            {
                cnt = ip_str.Length;
            }

            ret = HexOp0(ip_str, cnt, ref op_str);
            returnValue = op_str;
            return returnValue;
        }

        static public short ldump(object ip_str, ref object op_str)
        {
            short start;
            string tmpstr;

            //UPGRADE_WARNING: 無法解析物件 op_str 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            op_str = "";
            start = 1;
            do
            {
                //UPGRADE_WARNING: 無法解析物件 ip_str 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                tmpstr = Strings.Mid(ip_str, start, 1);
                start++;
                if (tmpstr == "")
                {
                    break;
                }
                tmpstr = Conversion.Hex(Strings.Asc(tmpstr)) + " ";
                if (tmpstr.Length < 3)
                {
                    tmpstr = "0" + tmpstr;
                }
                //UPGRADE_WARNING: 無法解析物件 op_str 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                op_str = op_str + tmpstr;
            } while (true);

        }
        static public string ldump2(string ip_str, ref short cnt)
        {
            string returnValue;
            short ret;
            short start;
            string tmpstr;
            string op_str;

            op_str = "";
            if (cnt < 0)
            {
                cnt = (byte)ip_str.Length;
            }
            start = 1;
            do
            {
                tmpstr = ip_str.Substring(start - 1, 1);
                start++;
                if (tmpstr == "")
                {
                    break;
                }
                tmpstr = Conversion.Hex(Strings.Asc(tmpstr)) + " ";
                if (tmpstr.Length < 3)
                {
                    tmpstr = "0" + tmpstr;
                }
                op_str = op_str + tmpstr;

                if (start > cnt)
                {
                    break;
                }
            } while (true);

            returnValue = op_str;
            return returnValue;
        }
        */

        static public string ldump3(byte[] ip_str, int start, int cnt)
        {
            string returnValue;
            short i;
            string tmpstr;
            string op_str;

            op_str = "";
            for (i = 0; i <= cnt - 1; i++)
            {
                tmpstr = Convert.ToString(ip_str[start + i], 16) + " "; // 傳回代表數字十六進位值的字串
                if (tmpstr.Length < 3)
                {
                    tmpstr = "0" + tmpstr;
                }
                op_str = op_str + tmpstr;
            }

            returnValue = op_str;
            return returnValue;
        }
        //Function CreateBitStr(ByRef nAsc As Byte) As String
        //    Dim cBitStr As String
        //    Dim aBinary(8) As Integer
        //    Dim lmore As Boolean
        //    Dim aBitAry(8) As Boolean
        //    Dim I As Integer

        //    aBinary(1) = 1
        //    aBinary(2) = 2
        //    aBinary(3) = 4
        //    aBinary(4) = 8
        //    aBinary(5) = 16
        //    aBinary(6) = 32
        //    aBinary(7) = 64
        //    aBinary(8) = 128
        //    lmore = True
        //    Do While lmore = True
        //        If nAsc = 0 Then
        //            lmore = False
        //        Else
        //            For I = 1 To 8
        //                If nAsc >= aBinary(8 - I + 1) Then
        //                    nAsc = nAsc - aBinary(8 - I + 1)
        //                    aBitAry(8 - I + 1) = True
        //                    Exit Do
        //                End If
        //            Next I
        //        End If
        //    Loop
        //    cBitStr = ""
        //    For I = 1 To 8
        //        If aBitAry(I) = True Then
        //            cBitStr = cBitStr & "1"
        //        Else
        //            cBitStr = cBitStr & "0"
        //        End If
        //    Next I
        //    CreateBitStr = cBitStr
        //End Function

        /*
        static public string str2hex(string tmpstr)
        {
            string returnValue;
            //// change string to Hex format string (Double-Byte Code)
            int cnt;
            short i;
            string tmpstr2;
            short data;

            cnt = tmpstr.Length;
            tmpstr2 = "";
            for (i = 1; i <= cnt; i++)
            {
                //UPGRADE_WARNING: 無法解析物件 tmpstr 的預設屬性。 按一下以取得詳細資訊: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                data = Strings.Asc(Strings.Mid(tmpstr, i, 1)); //double byte code
                if ((data & 0xFF00) == 0)
                {
                    tmpstr2 = tmpstr2 + Conversion.Hex(0x100 + data).Substring(1);
                }
                else
                {
                    tmpstr2 = tmpstr2 + Conversion.Hex(0x10000 + data & 0xFFFF).Substring(1);
                }
            }
            returnValue = tmpstr2;
            return returnValue;
        }
        */

    }
}