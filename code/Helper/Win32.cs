using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Forms;
using System.IO.Compression;
using System.Windows.Controls;
using System.ComponentModel;
using System.Web;
using Microsoft.Win32;
namespace doru
{
	public static class Win32
	{

        

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private static LowLevelKeyboardProc _proc = HookCallback;
		private static IntPtr _hookID = IntPtr.Zero;


		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
					GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private delegate IntPtr LowLevelKeyboardProc(
			int nCode, IntPtr wParam, IntPtr lParam);

		public delegate bool OnKeyDown(Keys key, bool down);
		private static OnKeyDown onKeyDown;
		public static OnKeyDown _OnKeyDown
		{
			get { return onKeyDown; }
			set
			{
				if (value != null && _hookID != null) _hookID = SetHook(_proc);
				else UnhookWindowsHookEx(_hookID);
				onKeyDown = value;
			}
		}
		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{

			if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
			{
				int vkCode = Marshal.ReadInt32(lParam);
				if (onKeyDown != null) onKeyDown((Keys)vkCode, wParam == (IntPtr)WM_KEYDOWN);
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook,
			LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
			IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);


		public static String GetSelectedText(bool all)
		{
			IntPtr hWnd = GetForegroundWindow();
			uint processId;
			uint activeThreadId = GetWindowThreadProcessId(hWnd, out processId);
			uint currentThreadId = GetCurrentThreadId();
			AttachThreadInput(activeThreadId, currentThreadId, true);
			IntPtr focusedHandle = GetFocus();
			AttachThreadInput(activeThreadId, currentThreadId, false);
			int len = SendMessage(focusedHandle, WM_GETTEXTLENGTH, 0, null);
			StringBuilder sb = new StringBuilder(len);
			int numChars = SendMessage(focusedHandle, WM_GETTEXT, len + 1, sb);
			if (all)
				return sb.ToString();
			else
			{
				int start, next;
				SendMessage(focusedHandle, EM_GETSEL, out start, out next);
				return sb.ToString().Substring(start, next - start);
			}

		}
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();
		[DllImport("user32.dll")]
		static extern bool AttachThreadInput(uint idAttach, uint idAttachTo,
		bool fAttach);
		[DllImport("user32.dll")]
		static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
		[DllImport("user32.dll")]
		static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);
		// second overload of SendMessage
		[DllImport("user32.dll")]
		static extern int SendMessage(IntPtr hWnd, uint Msg, out int wParam, out int lParam);
		const uint WM_GETTEXT = 0x0D;
		const uint WM_GETTEXTLENGTH = 0x0E;
		const uint EM_GETSEL = 0xB0;

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		public static void SetConsoleWindowVisibility(bool visible, string title)
		{
			IntPtr hWnd = FindWindow(null, title);
			if (hWnd != IntPtr.Zero)
			{
				if (!visible)
					ShowWindow(hWnd, 0);
				else
					ShowWindow(hWnd, 1);
			}
		}
		public static void HideConsoleBar(IntPtr hWnd)
		{
			if (hWnd != IntPtr.Zero)
			{
				int style = GetWindowLong(hWnd, GWL_EXSTYLE);
				style &= ~WS_EX_APPWINDOW;
				SetWindowLong(hWnd, GWL_EXSTYLE, style);
			} else Debugger.Break();
		}

		public static void HideConsoleBar(string title)
		{
			IntPtr hWnd = FindWindow(null, title);
			HideConsoleBar(hWnd);
		}
		public static int WS_EX_APPWINDOW = 0x40000;
		public static int GWL_EXSTYLE = -20;

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("kernel32.dll")]
		private static extern IntPtr CreateWaitableTimer(IntPtr
		lpTimerAttributes,
		bool bManualReset, string lpTimerName);

		[DllImport("kernel32.dll")]
		private static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long
		pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr
		lpArgToCompletionRoutine, bool fResume);

		[DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
		private static extern Int32 WaitForSingleObject(IntPtr handle, uint
		milliseconds);

		static IntPtr handle;
		public static void SetWaitForWakeUpTime(int secconds)
		{
			long duetime = -10000000 * secconds;
			handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer");
			SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
			IntPtr.Zero, true);
			//duetime = -t;
			Console.WriteLine("{0:x}", duetime);
			handle = CreateWaitableTimer(IntPtr.Zero, true,
			"MyWaitabletimer");
			SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
			IntPtr.Zero, true);
			uint INFINITE = 0xFFFFFFFF;
			int ret = WaitForSingleObject(handle, INFINITE);
			//MessageBox.Show("Wake up call");
		}


		public struct LASTINPUTINFO
		{
			public uint cbSize;
			public uint dwTime;
		}
		[DllImport("User32.dll")]
		public static extern bool LockWorkStation();

		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		[DllImport("Kernel32.dll")]
		private static extern uint GetLastError();

		public static uint GetIdleTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
			GetLastInputInfo(ref lastInPut);

			return ((uint)Environment.TickCount - lastInPut.dwTime);
		}

		public static long GetTickCount()
		{
			return Environment.TickCount;
		}

		public static long GetLastInputTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
			if (!GetLastInputInfo(ref lastInPut))
			{
				throw new Exception(GetLastError().ToString());
			}

			return lastInPut.dwTime;
		}
	}
}
