using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static Server.Controller.Input;
using static Server.Controller.InputExtensions;

namespace Server.Controller
{
    /// <summary>
    /// Can send mouse and keyboard input to the current window.
    /// </summary>
    public static class Input
    {
        static uint[] MOUSE_BUTTON_EVENTS = new uint[] { 0x02, 0x04, 0x0020, 0x0040, 0x08, 0x10 };

        const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        const int KEYEVENTF_KEYUP = 0x0002;

        static IntPtr loadedKeyboard = IntPtr.Zero;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        static extern bool UnloadKeyboardLayout(IntPtr hkl);


        static Input()
        {
            SetKeyboardLayout(CultureInfo.CurrentCulture);
        }

        public static void SetKeyboardLayout(CultureInfo culture)
        {
            if (loadedKeyboard != IntPtr.Zero)
                UnloadKeyboardLayout(loadedKeyboard);

            loadedKeyboard = LoadKeyboardLayout(culture.KeyboardLayoutId.ToString("X8"), 1);
        }

        public static void KeyboardEvent(VKey keycode, bool downpress)
        {
            byte key = (byte)keycode;
            if(downpress)
                keybd_event(key, 0, 0, 0);
            else
                keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
        }

        public static void KeyboardEvent(KeyEvent evt)
        {
            KeyboardEvent(evt.keycode, evt.downpress);
        }

        public static VKey GetVirtualKey(char c, out ShiftState shift)
        {
            shift = ShiftState.NONE;
            short result = VkKeyScanEx(c, loadedKeyboard);
            if (result == -1) return VKey.UNDEFINED;

            VKey key = (VKey)(result & 0xFF);
            shift = (ShiftState)(result >> 8);

            return key;
        }

        public static void MouseMove(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void MouseButton(MButton button, bool pressDown)
        {
            int pos = ((byte)button * 2) + (pressDown ? 0 : 1);
            mouse_event(MOUSE_BUTTON_EVENTS[pos], 0, 0, 0, 0);
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646332%28v=vs.85%29.aspx
        /// </summary>
        public enum ShiftState : short
        {
            NONE = 0,
            SHIFT = 1,
            CTRL = 2,
            ALT = 4,
            HANKAKU = 8,
            UNUSED1 = 16,
            UNUSED2 = 32
        }

        /// <summary>
        /// MouseButtons
        /// </summary>
        public enum MButton : byte
        {
            LEFT = 0,
            MIDDLE = 1,
            RIGHT = 2
        }

        /// <summary>
        /// Virtual Key codes
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
        /// </summary>
        public enum VKey : byte
        {
            LEFT_MOUSE = 0x01,
            RIGHT_MOUSE = 0x02,

            UNDEFINED = 0x07,

            ESC = 0x1B,
            TAB = 0x09,
            CAPS_LOCK = 0x14,
            SHIFT = 0x10,
            CONTROL = 0x11,
            ALT = 0x12,
            SPACEBAR = 0x20,
            ENTER = 0x0D,
            BACKSPACE = 0x08,

            INSERT = 0x2D,
            DELETE = 0x2E,

            END = 0x23,
            HOME = 0x24,

            PAGE_UP = 0x21,
            PAGE_DOWN = 0x22,

            LEFT_ARROW = 0x25,
            UP_ARROW = 0x26,
            RIGHT_ARROW = 0x27,
            DOWN_ARROW = 0x28,

            _0 = 0x30,
            _1 = 0x31,
            _2 = 0x32,
            _3 = 0x33,
            _4 = 0x34,
            _5 = 0x35,
            _6 = 0x36,
            _7 = 0x37,
            _8 = 0x38,
            _9 = 0x39,

            A = 0x41,
            B = 0x42,
            C = 0x43,
            D = 0x44,
            E = 0x45,
            F = 0x46,
            G = 0x47,
            H = 0x48,
            I = 0x49,
            J = 0x4A,
            K = 0x4B,
            L = 0x4C,
            M = 0x4D,
            N = 0x4E,
            O = 0x4F,
            P = 0x50,
            Q = 0x51,
            R = 0x52,
            S = 0x53,
            T = 0x54,
            U = 0x55,
            V = 0x56,
            W = 0x57,
            X = 0x58,
            Y = 0x59,
            Z = 0x5A,

            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B
        }
    }

    public static class InputExtensions
    {
        public struct KeyEvent
        {
            public VKey keycode;
            public bool downpress;
        }

        public static VKey FromChar(this VKey v, char c)
        {
            ShiftState s;
            return GetVirtualKey(c, out s);
        }

        public static KeyEvent[] ToEvents(this ShiftState shift, bool downpress)
        {
            List<KeyEvent> events = new List<KeyEvent>();

            ShiftState[] flags = new ShiftState[] { ShiftState.ALT, ShiftState.CTRL, ShiftState.HANKAKU, ShiftState.SHIFT };
            VKey[] mapto = new VKey[] { VKey.ALT, VKey.CONTROL, VKey.UNDEFINED, VKey.SHIFT };

            for (int i = 0; i < flags.Length; i++)
            {
                if ((shift | flags[i]) > 0)
                    events.Add(new KeyEvent() { keycode = mapto[i], downpress = downpress });
            }

            return events.ToArray();
        }

        public static KeyEvent[] ToKeyboardInput(this string input)
        {
            List<KeyEvent> events = new List<KeyEvent>();
            ShiftState pressedState = ShiftState.NONE;

            for (int i = 0; i < input.Length; i++)
            {
                ShiftState newState;
                VKey key = GetVirtualKey(input[i], out newState);

                if(!newState.Equals(pressedState)) //state is different from current one
                {
                    if (!pressedState.Equals(ShiftState.NONE)) //release all keys
                        events.AddRange(pressedState.ToEvents(false));

                    if (!newState.Equals(ShiftState.NONE)) //press new keys
                        events.AddRange(newState.ToEvents(true));

                   pressedState = newState;
                }

                KeyEvent down = new KeyEvent { keycode = key, downpress = true };
                KeyEvent up = new KeyEvent { keycode = key, downpress = false };

                events.Add(down);
                events.Add(up);
            }

            if (!pressedState.Equals(ShiftState.NONE)) //release last keys
                events.AddRange(pressedState.ToEvents(false));

            return events.ToArray();
        }




    }
}
