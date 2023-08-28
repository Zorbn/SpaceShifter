namespace ModalEverything;

internal static class Program
{
    private static void Main()
    {
        var moduleHandle = WinNative.GetModuleHandle("");
        var keyHandler = new KeyHandler();
        keyHandler.HHook = WinNative.SetWindowsHookEx(WinNative.WhKeyboardLl, keyHandler.KeyboardHookProc, moduleHandle, 0);

        while (WinNative.GetMessage(out var msg, nint.Zero, 0, 0) > 0)
        {
            WinNative.TranslateMessage(ref msg);
            WinNative.DispatchMessage(ref msg);
        }

        WinNative.UnhookWindowsHookEx(keyHandler.HHook);
    }
}