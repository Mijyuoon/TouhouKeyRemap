using System;
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
        private readonly IntPtr HookIgnore = (IntPtr)(-1);

        private readonly ConfigData _config;
        private readonly HashSet<uint> _downKeys;

        private IntPtr _hookHandle;
        private WinAPI.LowLevelKeyboardProc _hookFunc;

        public KeyProcessor(ConfigData config) {
            _config = config;
            _downKeys = new HashSet<uint>();
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

            switch((WinAPI.MSGType)wParam) {
            case WinAPI.MSGType.WM_KEYDOWN:
                if(HandleKeyDown(lParam)) return HookIgnore;
                break;

            case WinAPI.MSGType.WM_KEYUP:
                if(HandleKeyUp(lParam)) return HookIgnore;
                break;
            }

        exit:
            return WinAPI.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private bool HandleKeyDown(IntPtr lParam) {
            if(!CheckCurrentProcess()) return false;

            ReadHookVkCode(lParam, out uint vkCode);

            if(_config.KeyRemap.TryGetValue(vkCode, out RemapData remap)) {
                if(remap.Vk == 0x00) return true;
                
                if(!remap.Toggle) {
                    SimulateKeyInput(remap.Vk, true);
                    return true;
                }

                if(_downKeys.Contains(remap.Vk)) {
                    SimulateKeyInput(remap.Vk, false);
                    _downKeys.Remove(remap.Vk);
                } else {
                    SimulateKeyInput(remap.Vk, true);
                    _downKeys.Add(remap.Vk);
                }

                return true;
            }

            if(_config.KeyRescale.TryGetValue(vkCode, out RescaleData rescale)) {
                SetWindowSize(rescale.X, rescale.Y);
                return true;
            }

            return false;
        }

        private bool HandleKeyUp(IntPtr lParam) {
            if(!CheckCurrentProcess()) return false;

            ReadHookVkCode(lParam, out uint vkCode);

            if(_config.KeyRemap.TryGetValue(vkCode, out RemapData remap)) {
                if(remap.Vk == 0x00) return true;
                
                if(!remap.Toggle) {
                    SimulateKeyInput(remap.Vk, false);
                    _downKeys.Remove(remap.Vk);
                }

                return true;
            }

            _downKeys.Remove(vkCode);
            return false;
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
            var flags = _config.UseScancode
                ? WinAPI.KEYBDINPUTFlags.KEYEVENTF_SCANCODE
                : WinAPI.KEYBDINPUTFlags.KEYEVENTF_None;

            if(!keyDown) {
                flags |= WinAPI.KEYBDINPUTFlags.KEYEVENTF_KEYUP;
            }

            if((vkCode & 0x100) > 0) {
                flags |= WinAPI.KEYBDINPUTFlags.KEYEVENTF_EXTENDEDKEY;
                vkCode &= 0xFF;
            }

            var scCode = WinAPI.MapVirtualKey(vkCode & 0x7FFF, WinAPI.MAPVKType.MAPVK_VK_TO_VSC);

            var input = new WinAPI.INPUT();
            input.type = WinAPI.INPUTType.INPUT_KEYBOARD;
            input.ds.ki = new WinAPI.KEYBDINPUT {
                dwFlags = (uint)flags,
                wScan = (ushort)scCode,
                wVk = (ushort)vkCode,
            };

            WinAPI.SendInput(1, new[] { input }, Marshal.SizeOf<WinAPI.INPUT>());
        }

        private void SetWindowSize(uint x, uint y) {
            IntPtr hWnd = WinAPI.GetForegroundWindow();

            WinAPI.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, (int)x, (int)y,
                WinAPI.SETWINDOWPOSFlags.SWP_NOMOVE | WinAPI.SETWINDOWPOSFlags.SWP_NOZORDER);
        }
    }
}
