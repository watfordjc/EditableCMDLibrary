using System;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Helper methods for key presses
    /// </summary>
    public static class KeyUtils
    {
        /// <summary>
        /// A KeyPress is a parsed KEY_EVENT_RECORD that removes the need for boolean arithmetic.
        /// </summary>
        public class KeyPress
        {
            /// <summary>
            /// If a KeyDown event.
            /// </summary>
            public bool KeyDown { get; private set; }
            /// <summary>
            /// If CAPS Lock was on.
            /// </summary>
            public bool CapsLockOn { get; private set; }
            /// <summary>
            /// If NUM Lock was on.
            /// </summary>
            public bool NumLockOn { get; private set; }
            /// <summary>
            /// If SCROLL Lock was on.
            /// </summary>
            public bool ScrollLockOn { get; private set; }
            /// <summary>
            /// If ALT was pressed or held.
            /// </summary>
            public bool AltModifier { get; private set; }
            /// <summary>
            /// If CTRL was pressed or held.
            /// </summary>
            public bool CtrlModifier { get; private set; }
            /// <summary>
            /// If SHIFT was pressed or held.
            /// </summary>
            public bool ShiftModifier { get; private set; }
            /// <summary>
            /// If ALT GR, or Ctrl+Alt, was pressed or held.
            /// </summary>
            public bool AltGrModifier { get; private set; }
            /// <summary>
            /// If any combination of SHIFT/ALT/CTRL was pressed or held.
            /// </summary>
            public bool HasModifier { get { return AltModifier || CtrlModifier || ShiftModifier; } }
            /// <summary>
            /// If the key is an enhanced key.
            /// </summary>
            public bool EnhancedKey { get; private set; }
            /// <summary>
            /// The key pressed cast to <see cref="System.ConsoleKey"/> (if possible).
            /// </summary>
            public ConsoleKey ConsoleKey { get; private set; }

            /// <summary>
            /// Turns a <see cref="NativeMethods.KEY_EVENT_RECORD"/> into something more usable.
            /// </summary>
            /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
            public KeyPress(NativeMethods.KEY_EVENT_RECORD e)
            {
                KeyDown = e.bKeyDown;
                CapsLockOn = IsCapsLockOn(e);
                NumLockOn = IsNumLockOn(e);
                ScrollLockOn = IsScrollLockOn(e);
                AltModifier = IsAltKey(e);
                CtrlModifier = IsCtrlKey(e);
                ShiftModifier = IsShiftKey(e);
                AltGrModifier = IsAltGrKey(e);
                ConsoleKey = (ConsoleKey)e.wVirtualKeyCode;
                EnhancedKey = IsEnhancedKey(e);
                KeyDown = IsKeyReallyDown(this);
            }
        }

        /// <summary>
        /// <para>
        /// ConHost.exe intercepts certain key combinations resulting in only a KeyUp event firing.
        /// </para>
        /// <para>
        /// The source file that determines these keys at the time of writing: <seealso href="https://github.com/microsoft/terminal/blob/main/src/types/KeyEvent.cpp">https://github.com/microsoft/terminal/blob/main/src/types/KeyEvent.cpp</seealso>
        /// </para>
        /// </summary>
        /// <param name="keyPress">An almost fully parsed KeyPress - this should be last called during instantiation.</param>
        /// <returns>
        /// The existing value of KeyDown if (a) KeyDown is true, (b) there are no modifier keys, (c) modifiers include both Ctrl+Alt, (d) the key and modifiers do not match an affected combination.
        /// True if the key and modifiers do match an affected combination.
        /// </returns>
        private static bool IsKeyReallyDown(KeyPress keyPress)
        {
            // If we already know the key is pressed, or there are no modifier keys, the existing value is correct
            if (keyPress.KeyDown || !keyPress.HasModifier)
            {
                return keyPress.KeyDown;
            }

            if (keyPress.CtrlModifier && keyPress.AltModifier)
            {
                // Don't bother with these for now
                return keyPress.KeyDown;
            }
            else if (keyPress.CtrlModifier && !keyPress.ShiftModifier && !keyPress.AltModifier)
            {
                // Ctrl+Home, Ctrl+End, Ctrl+DownArrow, Ctrl+UpArrow: treat KeyUp event as KeyDown event
                return keyPress.ConsoleKey switch
                {
                    ConsoleKey.Home or ConsoleKey.End or ConsoleKey.DownArrow or ConsoleKey.UpArrow => true,
                    _ => keyPress.KeyDown,
                };
            }
            else if (keyPress.AltModifier && !keyPress.CtrlModifier && !keyPress.ShiftModifier)
            {
                // Alt+F7, Alt+F10: treat KeyUp event as KeyDown event
                return keyPress.ConsoleKey switch
                {
                    ConsoleKey.F7 or ConsoleKey.F10 => true,
                    _ => keyPress.KeyDown,
                };
            }
            else
            {
                return keyPress.KeyDown;
            }
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if CAPS Lock was on.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if CAPS Lock was on.</returns>
        private static bool IsCapsLockOn(NativeMethods.KEY_EVENT_RECORD e)
        {
            return (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.CAPSLOCK_ON) != 0;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if NUM Lock was on.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if NUM Lock was on.</returns>
        private static bool IsNumLockOn(NativeMethods.KEY_EVENT_RECORD e)
        {
            return (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.NUMLOCK_ON) != 0;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if SCROLL Lock was on.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if SCROLL Lock was on.</returns>
        private static bool IsScrollLockOn(NativeMethods.KEY_EVENT_RECORD e)
        {
            return (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.SCROLLLOCK_ON) != 0;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if ALT was held or pressed.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if ALT was held or pressed.</returns>
        private static bool IsAltKey(NativeMethods.KEY_EVENT_RECORD e)
        {
            bool isLeftAlt = (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.LEFT_ALT_PRESSED) != 0;
            bool isRightAlt = (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.RIGHT_ALT_PRESSED) != 0;
            return isLeftAlt || isRightAlt || e.wVirtualKeyCode == 0x12;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if CTRL was held or pressed.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if ALT CTRL held or pressed.</returns>
        private static bool IsCtrlKey(NativeMethods.KEY_EVENT_RECORD e)
        {
            bool isLeftControl = (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.LEFT_CTRL_PRESSED) != 0;
            bool isRightControl = (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.RIGHT_CTRL_PRESSED) != 0;
            return isLeftControl || isRightControl || e.wVirtualKeyCode == 0x11;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if SHIFT was held or pressed.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if SHIFT was held or pressed.</returns>
        private static bool IsShiftKey(NativeMethods.KEY_EVENT_RECORD e)
        {
            bool isShift = (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.SHIFT_PRESSED) != 0;
            return isShift || e.wVirtualKeyCode == 0x10 || e.wVirtualKeyCode == 0xA0 || e.wVirtualKeyCode == 0xA1;
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if ALT GR was held or pressed.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if ALT GR was held or pressed.</returns>
        private static bool IsAltGrKey(NativeMethods.KEY_EVENT_RECORD e)
        {
            return IsCtrlKey(e) && IsAltKey(e);
        }

        /// <summary>
        /// Do the boolean arithmetic on a KEY_EVENT_RECORD to determine if the pressed key is an enhanced key.
        /// </summary>
        /// <param name="e">The KEY_EVENT_RECORD to parse.</param>
        /// <returns>True if it is an enhanced key.</returns>
        private static bool IsEnhancedKey(NativeMethods.KEY_EVENT_RECORD e)
        {
            return (e.dwControlKeyState & NativeMethods.KEY_EVENT_RECORD.ENHANCED_KEY) != 0;
        }
    }

}
