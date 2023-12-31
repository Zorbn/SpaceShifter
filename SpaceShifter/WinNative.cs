using System.Runtime.InteropServices;

namespace SpaceShifter;

public struct WinPoint
{
    public int X;
    public int Y;
}

public struct WinKeyboardHook
{
    public uint VkCode;
    public uint ScanCode;
    public uint Flags;
    public uint Time;
    public IntPtr DwExtraInfo;
}

public struct WinMsg
{
    public IntPtr Hwnd;
    public uint Message;
    public IntPtr WParam;
    public IntPtr LParam;
    public uint Time;
    public WinPoint Pt;
}

[StructLayout(LayoutKind.Sequential)]
public struct WinInput
{
    public uint Type;
    public WinMkhInput Data;
}

[StructLayout(LayoutKind.Explicit)]
public struct WinMkhInput
{
    [FieldOffset(0)] public WinHardwareInput Hardware;
    [FieldOffset(0)] public WinKeyboardInput Keyboard;
    [FieldOffset(0)] public WinMouseInput Mouse;
}

[StructLayout(LayoutKind.Sequential)]
public struct WinHardwareInput
{
    public uint Msg;
    public ushort ParamL;
    public ushort ParamH;
}

[StructLayout(LayoutKind.Sequential)]
public struct WinKeyboardInput
{
    public ushort Vk;
    public ushort Scan;
    public uint Flags;
    public uint Time;
    public IntPtr ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct WinMouseInput
{
    public int X;
    public int Y;
    public uint MouseData;
    public uint Flags;
    public uint Time;
    public IntPtr ExtraInfo;
}

public static class WinNative
{
    public const int WhKeyboardLl = 13;

    public const int WmKeydown = 0x0100;
    public const int WmKeyup = 0x0101;

    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out WinMsg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage([In] ref WinMsg lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref WinMsg lpmsg);

    [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
        IntPtr hInstance, int threadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern bool UnhookWindowsHookEx(int idHook);

    [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int GetCurrentThreadId();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int GetLastError();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int GetModuleHandle(string module);

    [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern uint SendInput(uint inputCount, WinInput[] inputs, int sizeOfInput);
}