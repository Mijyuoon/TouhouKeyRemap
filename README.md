# TouhouKeyRemap
Key remapping tool, mainly created so I can play Touhou with WASD (because my laptop's arrow keys are shit).

## Usage
The tool reads config file `keyremap.txt` in its working directory.
Config file is line-based and uses `option: value` syntax, lines starting with `#` are treated as comments.

The following options are supported:
- `for` — space-separated list of process names (without `.exe`) for which to enable the tool, this works by checking process name of currently focused window.
  Example value: `for: th06 th07 th08`, this would enable the remapping for any window belonging to a process `th06.exe`, `th07.exe` or `th08.exe`.
  (And yes, if your process name contains whitespace you're SOL.)
- `map` — space-separated list of number pairs, specifying source and target [virtual key codes](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)
  for which to perform the remapping. Supports both decimal and hexadecimal (by suffixing with `h`) number formats. Using the value `0` for the target key will suppress
  all key down and up events for the source key, effectively disabling it.
  Example value: `map: 57h,26h 53h,28h 41h,25h 44h,27h`, this would remap *W*, *S*, *A*, *D* to *up*, *down*, *left* and *right* arrows respectively.
- `tmap` – same as `map` but alternates the key state between pressed and released states on each trigger.
  Example value: `tmap: BAh,5Ah`, this would toggle the state of *Z* key when *semicolon* is pressed.
- `scale` — space-separated list of number triplets, where the first number specifies the hotkey (same way as in `map` option) and the remaining two specify the desired
  width and height. When the hotkey is triggered the active window will be resized to the specified size.
  Example value: `scale: 7Bh,1280,960`, this would resize the active window to 1280×960 when *F12* is pressed.
- `scancode` — boolean flag that enables or disables injecting raw key scancodes instead of virtual key codes. Scancode mode is preferred and enabled by default, however
  some games might require disabling it when trying to remap certain keys. In particular, this was observed when remapping arrow keys in some Unity games.
  Example value: `scancode: false`, this would switch keystroke injector to using virtual key codes.

Example config file (can also be found in the repo itself):
```
for: th06 th06e th07 th08 th09 th10 th11 th12 th13 th14 th15 th16 th17

#Mapping: W→up  S→down  A→left  D→right  L→Z  '→X  ,→Z  /→X  Space→LShift
map: 57h,26h 53h,28h 41h,25h 44h,27h 4Ch,5Ah DEh,58h BCh,5Ah BFh,58h 20h,A0h

#Toggle: ;→Z  .→Z
tmap: BAh,5Ah BEh,5Ah

#Resize: F12→1280×960  F11→960×720
scale: 7Bh,1280,960 7Ah,960,720

scancode: true
```
