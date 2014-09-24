using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using SanCoder.Widget.DesktopIconPosBackupAndRecovery.Util;

namespace SanCoder.Widget.DesktopIconPosBackupAndRecovery
{
    public static class DesktopIconControl
    {
        private static List<Icon> GetIconInfo()
        {
            List<Icon> aIcons = new List<Icon>();

            //取桌面ListView句柄
            IntPtr hDeskTop = WinAPI.FindWindow("progman", null);
            hDeskTop = WinAPI.FindWindowEx(hDeskTop, IntPtr.Zero, "shelldll_defview", null);
            hDeskTop = WinAPI.FindWindowEx(hDeskTop, IntPtr.Zero, "syslistview32", null);

            //取explorer.exe句柄
            int dwProcessId;
            WinAPI.GetWindowThreadProcessId(hDeskTop, out dwProcessId);
            IntPtr hProcess = WinAPI.OpenProcess(WinAPI.PROCESS_VM_OPERATION | WinAPI.PROCESS_VM_READ | WinAPI.PROCESS_VM_WRITE, false, dwProcessId);


            unsafe
            {
                //在 explorer.exe 空间内分配内存
                IntPtr pv = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, sizeof(Point), WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);
                IntPtr pn = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, WinAPI.MAX_PATH * 2, WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);
                IntPtr plvitem = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, WinAPI.GetLvItemSize(), WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);

                //发送获取坐标消息
                int count = WinAPI.ListView_GetItemCount(hDeskTop);

                for (int i = 0; i < count; i++)
                {
                    Icon icon = new Icon();
                    WinAPI.ListView_GetItemText(hDeskTop, i, hProcess, plvitem, (int)pn, WinAPI.MAX_PATH * 2);

                    WinAPI.ListView_GetItemPosition(hDeskTop, i, pv);

                    //读取坐标数据
                    int lpNumberOfBytesRead;
                    WinAPI.ReadProcessMemory(hProcess, pv, out icon.Position, sizeof(Point), out lpNumberOfBytesRead);

                    //读取文本数据
                    IntPtr pszName = Marshal.AllocCoTaskMem(WinAPI.MAX_PATH*2);
                    WinAPI.ReadProcessMemory(hProcess, pn, pszName, WinAPI.MAX_PATH * 2, out lpNumberOfBytesRead);
                    icon.Text = Marshal.PtrToStringUni(pszName);
                    Marshal.FreeCoTaskMem(pszName);

                    //加入列表
                    aIcons.Add(icon);

                    Console.Out.WriteLine(icon.Text);
                    //int result = Marshal.GetLastWin32Error();
                }

                //释放内存
                WinAPI.VirtualFreeEx(hProcess, pv, sizeof(Point), 0);
                WinAPI.VirtualFreeEx(hProcess, pn, WinAPI.MAX_PATH * 2, 0);
                WinAPI.VirtualFreeEx(hProcess, plvitem, WinAPI.GetLvItemSize(), 0);

                //释放句柄
                WinAPI.CloseHandle(hProcess);
            }
            return aIcons;
        }

        private static void SetIconInfo(List<Icon> aIcons)
        {
            if (aIcons == null) throw new ArgumentNullException("aIcons");
            if (aIcons.Count == 0) return;

            //取桌面ListView句柄
            IntPtr hDeskTop = WinAPI.FindWindow("progman", null);
            hDeskTop = WinAPI.FindWindowEx(hDeskTop, IntPtr.Zero, "shelldll_defview", null);
            hDeskTop = WinAPI.FindWindowEx(hDeskTop, IntPtr.Zero, "syslistview32", null);

            //取explorer.exe句柄
            int dwProcessId;
            WinAPI.GetWindowThreadProcessId(hDeskTop, out dwProcessId);
            IntPtr hProcess = WinAPI.OpenProcess(WinAPI.PROCESS_VM_OPERATION | WinAPI.PROCESS_VM_READ | WinAPI.PROCESS_VM_WRITE, false, dwProcessId);

            unsafe
            {
                //在 explorer.exe 空间内分配内存
                IntPtr pv = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, sizeof(Point), WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);
                IntPtr pn = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, WinAPI.MAX_PATH * 2, WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);
                IntPtr plvitem = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, WinAPI.GetLvItemSize(), WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);

                //发送获取坐标消息
                int count = WinAPI.ListView_GetItemCount(hDeskTop);

                for (int i = 0; i < count; i++)
                {
                    WinAPI.ListView_GetItemText(hDeskTop, i, hProcess, plvitem, (int)pn, WinAPI.MAX_PATH * 2);

                    //读取文本数据
                    IntPtr pszName = Marshal.AllocCoTaskMem(WinAPI.MAX_PATH * 2);
                    int lpNumberOfBytesRead;
                    WinAPI.ReadProcessMemory(hProcess, pn, pszName, WinAPI.MAX_PATH * 2, out lpNumberOfBytesRead);
                    string text = Marshal.PtrToStringUni(pszName);
                    Marshal.FreeCoTaskMem(pszName);

                    //判断包含列表
                    Icon icon = aIcons.SingleOrDefault(item => item.Text == text);
                    if (icon == null)
                        continue;

                    //设置坐标
                    WinAPI.ListView_SetItemPosition(hDeskTop, i, icon.Position.x, icon.Position.y);
                }

                //释放内存
                WinAPI.VirtualFreeEx(hProcess, pv, sizeof(Point), 0);
                WinAPI.VirtualFreeEx(hProcess, pn, WinAPI.MAX_PATH * 2, 0);
                WinAPI.VirtualFreeEx(hProcess, plvitem, WinAPI.GetLvItemSize(), 0);

                //释放句柄
                WinAPI.CloseHandle(hProcess);
            }

        }


        public static void BackupIconInfo(string backupfilepath)
        {
            try
            {
                if (!Directory.Exists(backupfilepath.Substring(0, backupfilepath.LastIndexOf('\\'))))
                {
                    Console.Out.WriteLine("Error: 备份文件地址<" + backupfilepath + ">不存在");
                    Console.In.ReadLine();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

            //二进制序列化
            FileStream fs = File.Open(backupfilepath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, GetIconInfo());
            fs.Close();
        }

        public static void BackupIconInfo()
        {
            //获取文档路径
            BackupIconInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\DesktopIconPositionSave.dat");
        }

        public static void RecoveryIconInfo()
        {
            //获取文档路径
            RecoveryIconInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\DesktopIconPositionSave.dat");
        }

        public static void RecoveryIconInfo(string backupfilepath)
        {
            try
            {
                if (!File.Exists(backupfilepath))
                {
                    Console.Out.WriteLine("Error: 备份文件地址<" + backupfilepath + ">不存在");
                    Console.In.ReadLine();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

            FileStream fs = File.Open(backupfilepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            BinaryFormatter bf = new BinaryFormatter();

            SetIconInfo((List<Icon>) bf.Deserialize(fs));
        }

        public static void ShowHelp()
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("===============================================");
            Console.Out.WriteLine("桌面图标位置备份与恢复程序, SanCoder 2014-09-24");
            Console.Out.WriteLine("-----------------------------------------------");
            Console.Out.WriteLine("命令格式:");
            Console.Out.WriteLine("\t -b [BackupFilePath]  备份图标位置");
            Console.Out.WriteLine("\t -r [BackupFilePath]  恢复图标位置");
            Console.Out.WriteLine("===============================================");
            Console.Out.WriteLine("");
        }

    }

    [Serializable]
    public struct Point
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class Icon
    {
        public string Text;
        public Point Position;
    }
}