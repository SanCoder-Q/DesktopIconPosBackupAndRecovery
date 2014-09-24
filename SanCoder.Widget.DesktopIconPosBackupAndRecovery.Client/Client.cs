using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanCoder.Widget.DesktopIconPosBackupAndRecovery.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            DesktopIconControl.ShowHelp();

            if (args.Length < 1)
            {
                Console.Out.WriteLine("按任意键结束...");
                Console.In.Read();
                return;
            }

            bool isBackup = true;
            bool hasFilePath = false;
            string path = "";

            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i].Trim().ToLower();
                if (s[0] != '-') continue;

                hasFilePath = (i + 1 < args.Length) && (args[i + 1].Trim()[0] != '-') ? true : false;
                s = s.Substring(1);
                switch (s[0])
                {
                    case 'r':
                        //恢复
                        isBackup = false;
                        break;
                    case 'b':
                        //备份
                        isBackup = true;
                        break;
                    default:
                        Console.Out.WriteLine("Error: 未知参数！");
                        break;
                }

                if (!hasFilePath) continue;
                path = args[i + 1].Trim();
                i++;
            }

            if (isBackup)
            {
                if (hasFilePath)
                    DesktopIconControl.BackupIconInfo(path);
                else
                    DesktopIconControl.BackupIconInfo();
            }
            else
            {
                if (hasFilePath)
                    DesktopIconControl.RecoveryIconInfo(path);
                else
                    DesktopIconControl.RecoveryIconInfo();
            }

        }


    }
}
