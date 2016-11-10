using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Timers;

namespace OpenKeyboard
{
    public struct KeyboardCommand
    {
        public string[] KBKeys;
        public string[] KBShKeys;
        public string SendString;
        public string shSendString;
    }//struct

    public struct KeyItem
    {
        public int code;
        public bool isUpLast;
        public string extendCode;

        public KeyItem(int c, bool isLast) { code = c; isUpLast = isLast; extendCode = null; }//func
        public KeyItem(int c) { code = c; isUpLast = false; extendCode = null; }//func
        public KeyItem(string exCode) { code = 0; isUpLast = false; extendCode = exCode; }//func
    }//func

    public abstract class vKeyboard
    {
        #region Keyboard Code Dictionary - NOTE:Maybe remove and make it part of the XML
        //http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731%28v=vs.85%29.aspx
        //http://www.kbdedit.com/manual/low_level_vk_list.html
        private static Dictionary<string, KeyItem> KeyDict = new Dictionary<string, KeyItem>(){
            {"LSHIFT",new KeyItem(0xA0,true)}   ,{"RSHIFT",new KeyItem(0xA1,true)}  ,{"SHIFT",new KeyItem(0x10,true)}
            ,{"ALT",new KeyItem(0x12,true)}     ,{"LALT",new KeyItem(0xA4,true)}    ,{"RALT",new KeyItem(0xA5,true)}
            ,{"LCTRL",new KeyItem(0xA2,true)}   ,{"RCTRL",new KeyItem(0xA3,true)}
            ,{"PGUP",new KeyItem(0x21,true)}    ,{"PGDOWN",new KeyItem(0x22,true)}
            ,{"HOME",new KeyItem(0x24,true)}    ,{"END",new KeyItem(0x23,true)}
            ,{"LWIN",new KeyItem(0x5B,true)}    ,{"RWIN",new KeyItem(0x5C,true)}

            ,{"NUM0",new KeyItem(0x60)}
            ,{"NUM1",new KeyItem(0x61)}
            ,{"NUM2",new KeyItem(0x62)}
            ,{"NUM3",new KeyItem(0x63)}
            ,{"NUM4",new KeyItem(0x64)}
            ,{"NUM5",new KeyItem(0x65)}
            ,{"NUM6",new KeyItem(0x66)}
            ,{"NUM7",new KeyItem(0x67)}
            ,{"NUM8",new KeyItem(0x68)}
            ,{"NUM9",new KeyItem(0x69)}
            ,{"DECIMAL",new KeyItem(0x6E)}
            ,{"PLUS",new KeyItem(0xBB)}
            ,{"MINUS",new KeyItem(0xBD)}

            ,{"OEM2",new KeyItem(0xBF)} // /?
			,{"OEM1",new KeyItem(0xBA)} //;:
			,{"OEM4",new KeyItem(0xDB)} //[{
			,{"OEM6",new KeyItem(0xDD)} //]}
			,{"OEM5",new KeyItem(0xDC)} //|\
			,{"OEM7",new KeyItem(0xDE)} //'"

			,{"COMMA",new KeyItem(0xBC)}
            ,{"PERIOD",new KeyItem(0xBE)}
            ,{"SELECT",new KeyItem(0x29)}
            ,{"PRINT",new KeyItem(0x2A)}
            ,{"EXECUTE",new KeyItem(0x2B)}
            ,{"SNAPSHOT",new KeyItem(0x2C)}
            ,{"INSERT",new KeyItem(0x2D)}
            ,{"SLEEP",new KeyItem(0x5F)}
            ,{"ADD",new KeyItem(0x6B)}
            ,{"SUBTRACT",new KeyItem(0x6D)}

            ,{"SPACE",new KeyItem(0x20)},{"ENTER",new KeyItem(0x0D)},{"CAPSLOCK",new KeyItem(0x14)}
            ,{"DEL",new KeyItem(0x2E)},{"BACKSPACE",new KeyItem(0x08)},{"TAB",new KeyItem(0x09)}

            ,{"LEFT",new KeyItem(0x25)},{"UP",new KeyItem(0x26)},{"RIGHT",new KeyItem(0x27)},{"DOWN",new KeyItem(0x28)}

            ,{"F1",new KeyItem(0x70)},{"F2",new KeyItem(0x71)},{"F3",new KeyItem(0x72)},{"F4",new KeyItem(0x73)},{"F5",new KeyItem(0x74)}
            ,{"F6",new KeyItem(0x75)},{"F7",new KeyItem(0x76)},{"F8",new KeyItem(0x77)},{"F9",new KeyItem(0x78)},{"F10",new KeyItem(0x79)}
            ,{"F11",new KeyItem(0x7A)},{"F12",new KeyItem(0x7B)}

            ,{"0",new KeyItem(0x30)},{"1",new KeyItem(0x31)},{"2",new KeyItem(0x32)},{"3",new KeyItem(0x33)},{"4",new KeyItem(0x34)}
            ,{"5",new KeyItem(0x35)},{"6",new KeyItem(0x36)},{"7",new KeyItem(0x37)},{"8",new KeyItem(0x38)},{"9",new KeyItem(0x39)}

            ,{"a",new KeyItem(0x41)},{"b",new KeyItem(0x42)},{"c",new KeyItem(0x43)},{"d",new KeyItem(0x44)},{"e",new KeyItem(0x45)}
            ,{"f",new KeyItem(0x46)},{"g",new KeyItem(0x47)},{"h",new KeyItem(0x48)},{"i",new KeyItem(0x49)},{"j",new KeyItem(0x4A)}
            ,{"k",new KeyItem(0x4B)},{"l",new KeyItem(0x4C)},{"m",new KeyItem(0x4D)},{"n",new KeyItem(0x4E)},{"o",new KeyItem(0x4F)}
            ,{"p",new KeyItem(0x50)},{"q",new KeyItem(0x51)},{"r",new KeyItem(0x52)},{"s",new KeyItem(0x53)},{"t",new KeyItem(0x54)}
            ,{"u",new KeyItem(0x55)},{"v",new KeyItem(0x56)},{"w",new KeyItem(0x57)},{"x",new KeyItem(0x58)},{"y",new KeyItem(0x59)}
            ,{"z",new KeyItem(0x5A)}

            ,{"{!}",new KeyItem("{!}")},{"{@}",new KeyItem("{@}")},{"{#}",new KeyItem("{#}")},{"{$}",new KeyItem("{$}")},{"{%}",new KeyItem("{%}")}
            ,{"{^}",new KeyItem("{^}")},{"{&}",new KeyItem("{&}")},{"{*}",new KeyItem("{*}")},{"{(}",new KeyItem("{(}")},{"{)}",new KeyItem("{)}")}
            ,{"{<}",new KeyItem("{<}")},{"{>}",new KeyItem("{>}")},{"{~}",new KeyItem("{~}")},{"{_}",new KeyItem("{_}")},{"{+}",new KeyItem("{+}")}

            ,{"TILDE",new KeyItem(0xC0)}

            ,{"MUTE",new KeyItem(0xAD)},{"VOLUP",new KeyItem(0xAF)},{"VOLDOWN",new KeyItem(0xAE)}
            ,{"NEXTRACK",new KeyItem(0xB0)},{"PREVRACK",new KeyItem(0xB1)},{"MEDIASTOP",new KeyItem(0xB2)},{"MEDIAPAUSE",new KeyItem(0xB3)}
        };
        #endregion
        #region Constants
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        private const int KE_KEYUP = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
        #endregion

        #region DLL Imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion

        public static void PressKey(int key, bool? flag = null)
        {
            uint scanCode = MapVirtualKey((uint)key, 0);
            if (!flag.HasValue)
            {
                vKeyboard.keybd_event((byte)key, (byte)scanCode, 0u, (UIntPtr)0uL);
                vKeyboard.keybd_event((byte)key, (byte)scanCode, 2u, (UIntPtr)0uL);
            }
            else
            {
                if (flag.Value)
                {
                    vKeyboard.keybd_event((byte)key, (byte)scanCode, 2u, (UIntPtr)0uL);
                }
                else
                {
                    vKeyboard.keybd_event((byte)key, (byte)scanCode, 0u, (UIntPtr)0uL);
                }
            }
        }

        public static void ProcessCommand(KeyboardCommand kbCmd, bool? flag = null)
        {
            if (kbCmd.KBKeys.Length > 0 && KeyDict.ContainsKey(kbCmd.KBKeys[0]))
            {
                KeyItem keyItem = vKeyboard.KeyDict[kbCmd.KBKeys[0]];
                vKeyboard.PressKey(keyItem.code, flag);
            }
        }
    }

    public abstract class KeyLoopHandler
    {
        private static KeyboardCommand mKBCommand;
        private static Timer mTimer = null;
        private static bool mIsTimerOn = false;

        public static void EndKeypress() { StopTimer(); }//func
        public static void BeginKeypress(KeyboardCommand cmd)
        {
            if (mIsTimerOn) return;
            vKeyboard.ProcessCommand(cmd);

            mKBCommand = cmd;
            StartTimer();
        }//for

        private static void StopTimer() { mTimer.Stop(); mIsTimerOn = false; }
        private static void StartTimer()
        {
            if (mTimer == null)
            {
                mTimer = new Timer();
                mTimer.Interval = 200;
                mTimer.Elapsed += new ElapsedEventHandler(onTick);
            }
            else if (mIsTimerOn) return;

            mTimer.Start();
            mIsTimerOn = true;
        }//func

        private static void onTick(object sender, ElapsedEventArgs e) { vKeyboard.ProcessCommand(mKBCommand); }
    }//cls
}//ns
