using System;
using System.Runtime.InteropServices;

namespace SanCoder.Widget.DesktopIconPosBackupAndRecovery.Util
{
    public static class WinAPI
    {

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx", SetLastError = true)]
        public extern static IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out Point lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true)]
        public static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        public static int ListView_GetItemPosition(IntPtr hwndLV, int i, IntPtr ppt)
        {
            return SendMessage(hwndLV, LVM_GETITEMPOSITION, i, ppt);
        }

        public static int ListView_SetItemPosition(IntPtr hwndLV, int i, int x, int y)
        {
            return SendMessage(hwndLV, LVM_SETITEMPOSITION, i, x | ((UInt16)y << 16));
        }

        public static int ListView_GetItemCount(IntPtr hwndLV)
        {
            return SendMessage(hwndLV, LVM_GETITEMCOUNT, 0, 0);
        }

        public static int ListView_GetItemText(IntPtr hwndLV, int i, IntPtr hProcess, IntPtr plvitem, int pszText_, int cchTextMax)
        {
            //建立结构
            tagLVITEMW _macro_lvi = new tagLVITEMW();
            _macro_lvi.iSubItem = 0;
            _macro_lvi.cchTextMax = cchTextMax;
            _macro_lvi.pszText = pszText_;

            //转换到非托管区
            IntPtr lv = Marshal.AllocCoTaskMem(GetLvItemSize());
            Marshal.StructureToPtr(_macro_lvi, lv, false);

            //目标进程分配空间填充
            int lpNumberOfBytesWritten;
            int result = WriteProcessMemory(hProcess, plvitem, lv, GetLvItemSize(), out lpNumberOfBytesWritten);
            Marshal.FreeCoTaskMem(lv);
            //Console.Out.WriteLine(result);
            return SendMessage(hwndLV, LVM_GETITEMTEXT, i, plvitem);
        }

        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_WRITE = 0x0020;

        public const int MEM_COMMIT = 0x1000;
        public const int PAGE_READWRITE = 0x04;

        public const int LVM_GETITEMPOSITION = 0x1010;
        public const int LVM_SETITEMPOSITION = 0x100F;
        public const int LVM_GETITEMCOUNT = 0x1004;
        public const int LVM_GETITEMTEXT = 0x1073;

        public const int MAX_PATH = 260;

        public struct tagLVITEMW
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public Int64 pszText;
            public int cchTextMax;
            public int iImage;
            public int lParam;
            public int iIndent;
            public int iGroupId;
            public uint cColumns; // tile view columns
            public IntPtr puColumns;
            public IntPtr piColFmt;
            public int iGroup; // readonly. only valid for owner data.
        }

        public static int GetLvItemSize()
        {
            return Marshal.SizeOf(typeof(tagLVITEMW));
        }
    }
}