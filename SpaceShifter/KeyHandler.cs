using System.Runtime.InteropServices;

namespace SpaceShifter;

public class KeyHandler
{
    public int HHook
    {
        get => _hHook;

        set
        {
            _hHook = value;

            if (_hHook == 0)
            {
                Console.WriteLine($"Error: {WinNative.GetLastError()}");
            }
        }
    }

    private int _hHook;

    private enum State
    {
        NoSpace,
        SpaceUnknown,
        SpaceAsShift,
        SpaceAsSpace
    }

    private State _state = State.NoSpace;
    private bool _dontIntercept;
    private bool _isCapsPressed;
    private bool _wasCapsUsedAsToggle;
    private bool _isEnabled;

    public int KeyboardHookProc(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0 || _dontIntercept)
        {
            return WinNative.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        var keyboard = Marshal.PtrToStructure<WinKeyboardHook>(lParam);
        var isSpace = keyboard.VkCode == 0x20;
        var isCaps = keyboard.VkCode == 0x14;

        if (isCaps)
        {
            switch (wParam)
            {
                case WinNative.WmKeydown:
                    _isCapsPressed = true;
                    break;
                case WinNative.WmKeyup:
                    _isCapsPressed = false;
                    if (!_wasCapsUsedAsToggle)
                    {
                        _dontIntercept = true;
                        KeyPress(0x14);
                        KeyPress(0x14, isRelease: true);
                        _dontIntercept = false;
                    }
                    break;
            }

            _wasCapsUsedAsToggle = false;
            return 1;
        }

        // SpaceShifter was toggled.
        if (isSpace && wParam == WinNative.WmKeydown && _state == State.NoSpace && _isCapsPressed)
        {
            _wasCapsUsedAsToggle = true;
            _isEnabled = !_isEnabled;
            return 1;
        }

        if (!_isEnabled)
        {
            return WinNative.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        // First space press -> start tracking & holding shift.
        if (isSpace && wParam == WinNative.WmKeydown && _state == State.NoSpace)
        {
            KeyPress(0x10);

            // Console.WriteLine("keydown");

            _state = State.SpaceUnknown;

            return 1;
        }

        // Space release -> stop holding shift +
        if (isSpace && wParam == WinNative.WmKeyup)
        {
            KeyPress(0x10, isRelease: true);

            // Space wasn't used as shift -> send a real space press.
            if (_state == State.SpaceUnknown)
            {
                // Console.WriteLine("keyup->space");
                _state = State.SpaceAsSpace;
                KeyPress(0x20);
                KeyPress(0x20, isRelease: true);
                _state = State.NoSpace;
            }
            // Space was used as a shift -> reset state.
            else
            {
                // Console.WriteLine("keyup");
                _state = State.NoSpace;
            }

            return 1;
        }

        // A non-space key was pressed while space was held -> space was used as a shift key.
        if (!isSpace && wParam == WinNative.WmKeydown && _state is State.SpaceUnknown or State.SpaceAsShift)
        {
            _state = State.SpaceAsShift;
            return WinNative.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        // // We received the real space we sent earlier -> reset state and let it be processed as normal.
        // if (isSpace && (wParam == WinNative.WmKeydown || wParam == WinNative.WmKeyup) && _state == State.SpaceAsSpace)
        // {
        //     if (wParam == WinNative.WmKeyup)
        //     {
        //         _state = State.NoSpace;
        //     }
        //
        //     return WinNative.CallNextHookEx(_hHook, nCode, wParam, lParam);
        // }

        // Space was pressed but didn't fall into any of the other categories -> it was a repeat, ignore it.
        if (isSpace)
        {
            // Console.WriteLine("blocked");
            return 1;
        }

        return WinNative.CallNextHookEx(_hHook, nCode, wParam, lParam);
    }

    private void KeyPress(ushort key, bool isRelease = false)
    {

        var inputs = new WinInput[1];
        inputs[0].Type = 1;
        inputs[0].Data.Keyboard = new WinKeyboardInput
        {
            Vk = key,
            Scan = 0,
            Flags = isRelease ? 2u : 0u,
            Time = 0,
            ExtraInfo = nint.Zero
        };

        _dontIntercept = true;
        SendInput(inputs);
        _dontIntercept = false;
    }

    private void SendInput(WinInput[] inputs)
    {
        WinNative.SendInput(Convert.ToUInt32(inputs.Length), inputs, Marshal.SizeOf(typeof(WinInput)));
    }
}