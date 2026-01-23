using System.Runtime.InteropServices;
using System;

public static class WindowUtil
{

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowText(IntPtr hWnd, string text);

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    private const string RuntimeWindowTitlePrefix = "Card Game - ";

    public static void SetWindowTitle(IntPtr hWnd, string text)
    {
        SetWindowText(hWnd, RuntimeWindowTitlePrefix + text);
    }
}