﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TouhouKeyRemap.Config;
using TouhouKeyRemap.Extern;

namespace TouhouKeyRemap {
    class KeyProcessor {
        private readonly ConfigData _config;

        private IntPtr _hookHandle;
        private WinAPI.LowLevelKeyboardProc _hookFunc;

        public KeyProcessor(ConfigData config) {
            _config = config;
        }

        public void Initialize() {
            if(_hookHandle != IntPtr.Zero) return;
            _hookFunc = new WinAPI.LowLevelKeyboardProc(WindowsHookProc);
            _hookHandle = WinAPI.SetWindowsHookEx(WinAPI.WINDOWSHOOKType.WH_KEYBOARD_LL, _hookFunc, IntPtr.Zero, 0u);
        }

        public void Shutdown() {
            if(_hookHandle == IntPtr.Zero) return;
            WinAPI.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
            _hookFunc = null;
        }

        private IntPtr WindowsHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode < 0) goto exit;

            uint vkCode;
            RemapData remap;

            switch((WinAPI.MSGType)wParam) {
            case WinAPI.MSGType.WM_KEYDOWN:
                if(!CheckCurrentProcess()) goto exit;

                ReadHookVkCode(lParam, out vkCode);
                if(!_config.KeyRemap.TryGetValue(vkCode, out remap)) goto exit;

                SimulateKeyInput(remap.Vk, true);
                return (IntPtr)(-1);

            case WinAPI.MSGType.WM_KEYUP:
                if(!CheckCurrentProcess()) goto exit;

                ReadHookVkCode(lParam, out vkCode);
                if(!_config.KeyRemap.TryGetValue(vkCode, out remap)) goto exit;

                SimulateKeyInput(remap.Vk, false);
                return (IntPtr)(-1);
            }

        exit:
            return WinAPI.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private bool CheckCurrentProcess() {
            IntPtr hWnd = WinAPI.GetForegroundWindow();
            WinAPI.GetWindowThreadProcessId(hWnd, out uint pid);

            var process = Process.GetProcessById((int)pid);
            return _config.EnableFor.Contains(process.ProcessName);
        }

        private void ReadHookVkCode(IntPtr lParam, out uint vkCode) {
            var info = (WinAPI.KBDLLHOOK)Marshal.PtrToStructure(lParam, typeof(WinAPI.KBDLLHOOK));
            vkCode = info.vkCode;
        }

        private void SimulateKeyInput(uint vkCode, bool keyDown) {
            var flags = WinAPI.KEYBDINPUTFlags.KEYEVENTF_SCANCODE;

            if(!keyDown) {
                flags |= WinAPI.KEYBDINPUTFlags.KEYEVENTF_KEYUP;
            }

            if((vkCode & 0x100) > 0) {
                flags |= WinAPI.KEYBDINPUTFlags.KEYEVENTF_EXTENDEDKEY;
            }

            var scCode = WinAPI.MapVirtualKey(vkCode, WinAPI.MAPVKType.MAPVK_VK_TO_VSC);

            var input = new WinAPI.INPUT();
            input.type = WinAPI.INPUTType.INPUT_KEYBOARD;
            input.ds.ki = new WinAPI.KEYBDINPUT {
                dwFlags = (uint)flags,
                wScan = (ushort)scCode,
            };

            WinAPI.SendInput(1, new[] { input }, Marshal.SizeOf<WinAPI.INPUT>());
        }
    }
}