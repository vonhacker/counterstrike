using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ChatBox2;
using doru;

namespace IcqCommandLine2
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        Process cmd;
        ICQAPP _ICQAPP;
        public Program()
        {
            cmd = new Process();
            cmd.StartInfo.FileName = @"cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.Start();
            StartIcq();
        }

        private void StartIcq()
        {
            _ICQAPP = new ICQAPP();
            _ICQAPP._uin = "451589443";
            _ICQAPP._passw = "er54s4";
            _ICQAPP.StartAsync();
            _ICQAPP._onMessage += new ICQAPP.Message(ICQAPP__onMessage);
            while (true)
            {
                if (uin != null)
                {
                    char[] buffer = new char[999999];
                    int c = cmd.StandardOutput.Read(buffer, 0, buffer.Length);                    
                    string s = new string(buffer, 0, c);
                    _ICQAPP.SendMessage(new Im { uin = uin, msg = s });
                }
            }
        }
        public string uin;
        void ICQAPP__onMessage(Im im)
        {
            cmd.StandardInput.WriteLine(im.msg);
            uin = im.uin;
        }
    }
}
