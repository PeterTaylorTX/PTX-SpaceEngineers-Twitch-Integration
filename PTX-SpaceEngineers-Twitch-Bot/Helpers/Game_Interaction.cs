using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace PTX_SpaceEngineers_Twitch_Bot.Helpers
{
    public class Game_Interaction
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        public static void Interact(string key, Int32 duration = 0)
        {
            Process[] spaceEngineers = Process.GetProcessesByName("SpaceEngineers");
            foreach (Process process in spaceEngineers)
            {
                SetForegroundWindow(process.MainWindowHandle);

                // Duration
                if (duration == 0) { duration = 1000; }
                // Duration
                WindowsInput.Native.VirtualKeyCode convertedKey = ConvertKey(key);

                if (key.ToLower().Contains("mouse"))
                {
                    Mouse_Click(key, duration);
                    return;
                }

                PressKey(convertedKey, duration);
            }
        }

        protected static void Mouse_Click(string key, Int32 duration)
        {
            InputSimulator inputSim = new InputSimulator();
            MouseSimulator mse = new WindowsInput.MouseSimulator(inputSim);

            if (key.ToLower() == "mouse_left") { mse.Sleep(duration); mse.LeftButtonDown(); mse.Sleep(duration); mse.LeftButtonUp(); }
            else if (key.ToLower() == "mouse_right") { mse.Sleep(duration); mse.RightButtonDown(); mse.Sleep(duration); mse.RightButtonDown(); }
        }
        protected static void PressKey(WindowsInput.Native.VirtualKeyCode key, Int32 duration)
        {
            InputSimulator inputSim = new InputSimulator();
            KeyboardSimulator kbd = new WindowsInput.KeyboardSimulator(inputSim);

            kbd.Sleep(duration);
            kbd.KeyDown(key); // Single Press
            kbd.Sleep(duration);
            kbd.KeyUp(key); // Single Press
        }
        private static WindowsInput.Native.VirtualKeyCode ConvertKey(string key)
        {
            switch (key.ToLower())
            {
                case "a": return WindowsInput.Native.VirtualKeyCode.VK_A;
                case "b": return WindowsInput.Native.VirtualKeyCode.VK_B;
                case "c": return WindowsInput.Native.VirtualKeyCode.VK_C;
                case "d": return WindowsInput.Native.VirtualKeyCode.VK_D;
                case "e": return WindowsInput.Native.VirtualKeyCode.VK_E;
                case "f": return WindowsInput.Native.VirtualKeyCode.VK_F;
                case "g": return WindowsInput.Native.VirtualKeyCode.VK_G;
                case "h": return WindowsInput.Native.VirtualKeyCode.VK_H;
                case "i": return WindowsInput.Native.VirtualKeyCode.VK_I;
                case "j": return WindowsInput.Native.VirtualKeyCode.VK_J;
                case "k": return WindowsInput.Native.VirtualKeyCode.VK_K;
                case "l": return WindowsInput.Native.VirtualKeyCode.VK_L;
                case "m": return WindowsInput.Native.VirtualKeyCode.VK_M;
                case "n": return WindowsInput.Native.VirtualKeyCode.VK_N;
                case "o": return WindowsInput.Native.VirtualKeyCode.VK_O;
                case "p": return WindowsInput.Native.VirtualKeyCode.VK_P;
                case "q": return WindowsInput.Native.VirtualKeyCode.VK_Q;
                case "r": return WindowsInput.Native.VirtualKeyCode.VK_R;
                case "s": return WindowsInput.Native.VirtualKeyCode.VK_S;
                case "t": return WindowsInput.Native.VirtualKeyCode.VK_T;
                case "u": return WindowsInput.Native.VirtualKeyCode.VK_U;
                case "v": return WindowsInput.Native.VirtualKeyCode.VK_V;
                case "w": return WindowsInput.Native.VirtualKeyCode.VK_W;
                case "x": return WindowsInput.Native.VirtualKeyCode.VK_X;
                case "y": return WindowsInput.Native.VirtualKeyCode.VK_Y;
                case "z": return WindowsInput.Native.VirtualKeyCode.VK_Z;
                case "1": return WindowsInput.Native.VirtualKeyCode.VK_1;
                case "2": return WindowsInput.Native.VirtualKeyCode.VK_2;
                case "3": return WindowsInput.Native.VirtualKeyCode.VK_3;
                case "4": return WindowsInput.Native.VirtualKeyCode.VK_4;
                case "5": return WindowsInput.Native.VirtualKeyCode.VK_5;
                case "6": return WindowsInput.Native.VirtualKeyCode.VK_6;
                case "7": return WindowsInput.Native.VirtualKeyCode.VK_7;
                case "8": return WindowsInput.Native.VirtualKeyCode.VK_8;
                case "9": return WindowsInput.Native.VirtualKeyCode.VK_9;
                case "0": return WindowsInput.Native.VirtualKeyCode.VK_0;
                case " ": return WindowsInput.Native.VirtualKeyCode.SPACE;
                case ",": return WindowsInput.Native.VirtualKeyCode.OEM_COMMA;
                case ".": return WindowsInput.Native.VirtualKeyCode.OEM_PERIOD;
                default: return WindowsInput.Native.VirtualKeyCode.MEDIA_NEXT_TRACK;
            }
        }
    }
}
