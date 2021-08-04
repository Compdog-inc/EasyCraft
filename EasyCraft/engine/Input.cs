using System;
using SharpDX;
using RIN = SharpDX.RawInput;
using SharpDX.Multimedia;
using System.Windows.Forms;
using System.Collections.Generic;

namespace EasyCraft.engine
{
    public enum CursorMode {
        Normal,
        Lock
    }

    public enum KeyState
    {
        Down,
        Up,
        FrameDown,
        FrameUp
    }

    public static class Input
    {
        public static CursorMode CursorMode { get => _cursorMode; set { _cursorMode = value; UpdateCursor(); } }
        private static CursorMode _cursorMode = CursorMode.Normal;

        public static bool ShowCursor { get => _showCursor; set { _showCursor = value; UpdateCursor(); } }
        private static bool _showCursor = true;

        private static bool isDeactivated = false;

        private static bool mouseFired = false;

        private static float mouseXAxis = 0f;
        private static float mouseYAxis = 0f;
        private static float mouseZAxis = 0f;

        private static float horizontalAxis = 0f;
        private static float verticalAxis = 0f;

        private static Dictionary<Keys, KeyState> keys = new Dictionary<Keys, KeyState>();

        public static void Init()
        {
            RIN.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, RIN.DeviceFlags.None);
            RIN.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, RIN.DeviceFlags.None);
            RIN.Device.MouseInput += Device_MouseInput;
            RIN.Device.KeyboardInput += Device_KeyboardInput;
            Global.window.Load += Window_Load;
            Global.window.Deactivate += Window_Deactivate;
        }

        private static void Window_Load(object sender, EventArgs e)
        {
            isDeactivated = false;
            UpdateCursor();
        }

        private static void Window_Deactivate(object sender, EventArgs e)
        {
            isDeactivated = true;
            Cursor.Show();
        }

        private static void UpdateCursor()
        {
            if (isDeactivated) return;
            if (CursorMode == CursorMode.Lock)
                Cursor.Position = new System.Drawing.Point(Global.window.Left + Global.window.Width / 2, Global.window.Top + Global.window.Height / 2);
            if (ShowCursor)
                Cursor.Show();
            else
                Cursor.Hide();
        }

        public static void ResetAxis()
        {
            if (mouseFired)
            {
                mouseFired = false;
                mouseXAxis = 0f;
                mouseYAxis = 0f;
                mouseZAxis = 0f;
                if (!isDeactivated && CursorMode == CursorMode.Lock)
                    Cursor.Position = new System.Drawing.Point(Global.window.Left + Global.window.Width / 2, Global.window.Top + Global.window.Height / 2);
            }

            List<Keys> keyList = new List<Keys>(keys.Keys);
            foreach(Keys key in keyList)
            {
                if (keys[key] == KeyState.FrameDown) keys[key] = KeyState.Down;
                else if (keys[key] == KeyState.FrameUp) keys[key] = KeyState.Up;
            }

            if (GetKey(Keys.A))
                horizontalAxis = -1f;
            else if (GetKey(Keys.D))
                horizontalAxis = 1f;
            else
                horizontalAxis = 0f;

            if (GetKey(Keys.W))
                verticalAxis = 1f;
            else if (GetKey(Keys.S))
                verticalAxis = -1f;
            else
                verticalAxis = 0f;

            if (!isDeactivated)
            {
                if (ShowCursor)
                    Cursor.Show();
                else
                    Cursor.Hide();
            }

            else Cursor.Show();
        }

        private static void Device_MouseInput(object sender, RIN.MouseInputEventArgs e)
        {
            if(isDeactivated && e.ButtonFlags.HasFlag(RIN.MouseButtonFlags.LeftButtonDown))
            {
                isDeactivated = false;
                UpdateCursor();
            }
            mouseXAxis = e.X;
            mouseYAxis = e.Y;
            mouseZAxis = e.WheelDelta / 120f;
            mouseFired = true;
        }

        private static void Device_KeyboardInput(object sender, RIN.KeyboardInputEventArgs e)
        {
            KeyState state = KeyState.Up;
            if (keys.ContainsKey(e.Key)) state = keys[e.Key];

            if (e.State == RIN.KeyState.KeyDown && state != KeyState.Down) state = KeyState.FrameDown;
            else if (e.State == RIN.KeyState.KeyUp && state != KeyState.Up) state = KeyState.FrameUp;

            if (keys.ContainsKey(e.Key)) keys[e.Key] = state;
            else keys.Add(e.Key, state);

            if(e.Key == Keys.Escape && e.State == RIN.KeyState.KeyDown)
            {
                isDeactivated = true;
                Cursor.Show();
            }
        }

        public static float GetAxis(string name)
        {
            switch (name)
            {
                case "Mouse X":
                    return mouseXAxis;
                case "Mouse Y":
                    return mouseYAxis;
                case "Mouse ScrollWheel":
                    return mouseZAxis;
                case "Horizontal":
                    return horizontalAxis;
                case "Vertical":
                    return verticalAxis;
                default:
                    Debug.LogError($"Tried to get axis '{name}' that does not exist!");
                    break;
            }
            return 0;
        }

        public static KeyState GetKeyState(Keys key)
        {
            if (keys.ContainsKey(key)) return keys[key];
            return 0;
        }

        public static bool GetKey(Keys key)
        {
            if (keys.ContainsKey(key)) return keys[key] == KeyState.Down || keys[key] == KeyState.FrameDown;
            return false;
        }

        public static bool GetKeyDown(Keys key)
        {
            if (keys.ContainsKey(key)) return keys[key] == KeyState.FrameDown;
            return false;
        }

        public static bool GetKeyUp(Keys key)
        {
            if (keys.ContainsKey(key)) return keys[key] == KeyState.FrameUp;
            return false;
        }
    }
}