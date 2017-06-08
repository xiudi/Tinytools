﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TinyToolsLite
{
    public partial class TinyToolsLite : Form
    {
        public static HidDevice HidDevice;
        ushort vid = 0xdddd;
        ushort pid = 0x5678;
        ushort eepromsize = 511;
        public void Clear()
        {
            Box1.Text = "";
        }
        public void Print(Object str)
        {
            Box1.Text += str.ToString() + "\r\n";
        }
        public TinyToolsLite()
        {
            InitializeComponent();
        }
        string[] encodingtype =
       {
            "GBK",
            "BigEndianUnicode",
            "Default",
            "Unicode",
            "UTF32",
            "UTF7",
            "UTF8"
        };
        public ushort ConvertChinese1(char str, string code)
        {
            string str2 = Convert.ToString(str);
            byte[] data; ushort a3;
            if (code == encodingtype[0])
            {
                return ConvertChinese2(str, code);
            }
            else if (code == encodingtype[1])
            {
                data = Encoding.BigEndianUnicode.GetBytes(str2);
            }
            else if (code == encodingtype[2])
            {
                data = Encoding.Default.GetBytes(str2);
                byte temple = data[0];
                data[0] = data[1]; data[1] = temple;
                a3 = BitConverter.ToUInt16(data, 0);
                return a3;
            }
            else if (code == encodingtype[3])
            {
                data = Encoding.Unicode.GetBytes(str2);
            }
            else if (code == encodingtype[4]) data = Encoding.UTF32.GetBytes(str2);
            else if (code == encodingtype[5]) data = Encoding.UTF7.GetBytes(str2);
            else if (code == encodingtype[6])
            {
                data = Encoding.UTF8.GetBytes(str2);
                byte temple = data[0];
                data[0] = data[1]; data[1] = temple;
                a3 = BitConverter.ToUInt16(data, 0);
                return a3;
            }
            else { Print("encoding error"); return 0; }
            str2 = data[1].ToString("x") + data[0].ToString("x");
            a3 = Convert.ToUInt16(str2, 16);
            return a3;
        }
        public ushort ConvertChinese2(char str, string code)
        {
            string str2 = Convert.ToString(str);
            byte[] data = Encoding.GetEncoding(code).GetBytes(str2);
            byte temple = data[0];
            data[0] = data[1]; data[1] = temple;
            ushort a3 = BitConverter.ToUInt16(data, 0);
            return a3;
        }
        int encode_index = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            encode_index--;
            if (encode_index == -1) encode_index = 6;
            textBox3.Text = encodingtype[encode_index];
            convertToolStripMenuItem.Text = encodingtype[encode_index];
        }

        private void button5_Click(object sender, EventArgs e)
        {
            encode_index++;
            if (encode_index == 7) encode_index = 0;
            textBox3.Text = encodingtype[encode_index];
            convertToolStripMenuItem.Text = encodingtype[encode_index];
        }
       
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                string[] str = textBox4.Text.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (str.Length != 2) return;
                 vid = (ushort)Convert.ToInt32(str[0], 16);
                 pid = (ushort)Convert.ToInt32(str[1], 16);
            }
            Clear();
            Print("0x"+vid.ToString("x"));
            Print("0x" + pid.ToString("x"));
            try
            {
                HidDevice[] HidDeviceList = HidDevices.Enumerate(vid, pid,Convert.ToUInt16(0xFF31)).ToArray();
                if (HidDeviceList == null || HidDeviceList.Length == 0)
                {
                    Print("Connect usb device and install driver. Try open again");
                    return;
                }
                for (int i = 0; i < HidDeviceList.Length; i++)
                {
                    Print(HidDeviceList[i].DevicePath);
                    HidDevice = HidDeviceList[0]; break; 
                }
                if (HidDevice == null)
                {
                    Print("Connect usb device and install driver. Try open again");
                    return;
                }
                Print("Device OK");
            }
            catch (Exception ex)
            {
                Print(ex.ToString());
            }
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Text == "")
                {
                    Clear();
                    Print("Nothing to upload");
                    return;
                }
                string[] str = textBox2.Text.Split(',');
                if (HidDevice == null)
                {
                    Clear();
                    Print("Invalid device");
                    return;
                }
                Clear();
                Print("Uploading");
                byte[] outdata = new byte[9]; outdata[0] = 0;
                byte[] a = new byte[2];

                outdata[1] = 0xFF; outdata[2] = 0xF1;
                HidDevice.Write(outdata); Thread.Sleep(60);

                for (ushort i = 0; (i * 2) < Convert.ToInt32(eepromsize); i += 3)
                {
                    a = BitConverter.GetBytes((ushort)(i * 2));
                    outdata[1] = a[0]; outdata[2] = a[1];
                    if ((i + 2) < str.Length)
                    {
                        ushort data3 = Convert.ToUInt16(str[i + 2]);
                        //Print(data3);
                        a = BitConverter.GetBytes(data3);
                        outdata[7] = a[0]; outdata[8] = a[1];
                    }
                    if ((i + 1) < str.Length)
                    {
                        ushort data2 = Convert.ToUInt16(str[i + 1]);
                        //Print(data2);
                        a = BitConverter.GetBytes(data2);
                        outdata[5] = a[0]; outdata[6] = a[1];
                    }
                    if (i < str.Length)
                    {
                        ushort data1 = Convert.ToUInt16(str[i]);
                        //Print(data1);
                        a = BitConverter.GetBytes(data1);
                        outdata[3] = a[0]; outdata[4] = a[1];
                    }
                    else { break; }
                    HidDevice.Write(outdata);
                    string outdatastr = "";
                    for (int k = 1; k < outdata.Length; k++)
                    {
                        outdatastr += outdata[k].ToString() + "/";
                    }
                    Print(outdatastr);
                    Thread.Sleep(60);
                }
                outdata[1] = 0xFF; outdata[2] = 0xF2;
                HidDevice.Write(outdata); Thread.Sleep(60);
                Print("Upload finished");
            }
            catch (Exception ex) { Print(ex.ToString()); }
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                char[] ch = textBox1.Text.ToArray();

                if (ch == null || ch.Length == 0)
                {
                    Clear();
                    Print("Nothing to Convert");
                    return;
                }
                Clear();
                Print("English 0-127 GBK > " + 0x8080);

                int length = Convert.ToInt32(eepromsize) / 2 - 1;
                if (ch.Length < length) length = ch.Length;
                string output = "";
                int length2 = length;
                for (int j = 0; j < length; j++)
                {
                    if (ch[j] < 127 && ch[j] >= 0)
                    {
                        int code = Program.ascii_to_scan_code_table[(int)ch[j]];
                        if (code != 0)
                        {
                            output += code.ToString();
                            if (j != length - 1) output += ",";
                        }
                        else
                        {
                            length2--;
                        }
                    }
                    else if (ch[j] <= 0xFFFF)
                    {
                        //汉字                     
                        ushort a3 = ConvertChinese1(ch[j], textBox3.Text);
                        output += a3.ToString();
                        //Printhex((int)a3);
                        if (j != length - 1) output += ",";
                    }
                }
                textBox2.Text = length2.ToString() + ",";
                textBox2.Text += output;
            }
            catch (Exception ex) { Print(ex.ToString()); }
        }
    }
}