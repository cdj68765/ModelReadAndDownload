using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Nico3D模型获取工具;

namespace WinInetDemo3
{
    internal static class NativeMethods
    {
        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InternetReadFile(IntPtr hRequest, [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer, int dwNumberOfBytesToRead, out int lpdwNumberOfBytesRead);

        [DllImport("wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InternetCloseHandle(IntPtr hInternet);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern InetHandle InternetOpen(string lpszAgent, uint dwAccessType, string lpszProxyName, string lpszProxyBypass, uint dwFlags);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr InternetOpenUrl(InetHandle hInternet, string lpszUrl, string lpszHeaders, int dwHeadersLength, uint dwFlags, IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HttpQueryInfo(IntPtr hRequest, uint dwInfoLevel, [MarshalAs(UnmanagedType.LPArray)] byte[] lpvBuffer, out int dwBufferLength, out int dwIndex);
    }
}