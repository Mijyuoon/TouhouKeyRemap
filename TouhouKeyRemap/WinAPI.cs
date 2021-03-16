using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TouhouKeyRemap.Extern {
    class WinAPI {
        public enum MSGType : uint {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
        }

        public enum WINDOWSHOOKType : int {
            WH_KEYBOARD_LL = 13
        }

        #region struct KBDLLHOOK

        [Flags]
        public enum KBDLLHOOKFlags : uint {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOK {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKFlags flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region struct INPUT

        public enum INPUTType : uint {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT {
            public INPUTType type;
            public INPUTUnion ds;

            [StructLayout(LayoutKind.Explicit)]
            public struct INPUTUnion {
                [FieldOffset(0)]
                public MOUSEINPUT mi;

                [FieldOffset(0)]
                public KEYBDINPUT ki;

                [FieldOffset(0)]
                public HARDWAREINPUT hi;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT {
            public int dx;
            public int dy;
            public int mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KEYBDINPUTFlags : uint {
            KEYEVENTF_None = 0x00,
            KEYEVENTF_EXTENDEDKEY = 0x01,
            KEYEVENTF_KEYUP = 0x02,
            KEYEVENTF_UNICODE = 0x04,
            KEYEVENTF_SCANCODE = 0x08,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        #endregion

        public enum MAPVKType : uint {
            MAPVK_VK_TO_VSC = 0,
            MAPVK_VSC_TO_VK = 1,
            MAPVK_VK_TO_CHAR = 2,
            MAPVK_VSC_TO_VK_EX = 3,
        }

        [Flags]
        public enum SETWINDOWPOSFlags : uint {
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(WINDOWSHOOKType idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MAPVKType uMapType);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int w, int h, SETWINDOWPOSFlags flags);
    }
}
