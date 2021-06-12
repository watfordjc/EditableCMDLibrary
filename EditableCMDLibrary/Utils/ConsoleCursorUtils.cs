using System;
using System.Runtime.Versioning;
using uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Helper methods for current cursor position
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class ConsoleCursorUtils
    {
        /// <summary>
        /// Get the cursor's current position.
        /// </summary>
        /// <returns>A struct containing the X and Y coordinates of the cursor.</returns>
        public static NativeMethods.COORD GetCurrentCursorPosition()
        {
            return new NativeMethods.COORD()
            {
                X = (short)Console.CursorLeft,
                Y = (short)Console.CursorTop
            };
        }

        /// <summary>
        /// Determines if a position is inside the editable area.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="position">The <see cref="NativeMethods.COORD"/> of the position to test.</param>
        /// <returns>true if position is inside editable area, false if it is outside the editable area.</returns>
        [SupportedOSPlatform("windows")]
        public static bool CoordIsInsideEditableArea(ConsoleState state, NativeMethods.COORD position)
        {
            return position.Y < state.Input.EndPosition.Y;
        }

        /// <summary>
        /// Converts a display buffer COORD to a position in the current input command
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="cursorPosition">A COORD for a position on the screen</param>
        /// <returns>The 0-based index of the character</returns>
        public static int CursorPositionToCharPosition(ConsoleState state, NativeMethods.COORD cursorPosition)
        {
            // If these are equal, we're at the start of the input character array
            if (cursorPosition.Equals(state.Input.StartPosition))
            {
                return 0;
            }
            else
            {
                // An estimated position that assumes all characters are displayed as one character wide
                int estimatedPositionInCommand = ((cursorPosition.Y - state.Input.StartPosition.Y) * Console.BufferWidth) + cursorPosition.X - state.Input.StartPosition.X;
                // Try to find a tab character before the estimated position
                bool containsTab = state.Input.TabPositions.Count > 0 && state.Input.TabPositions[0] < estimatedPositionInCommand;
                // If there is a tab, calculate the position in the character array
                if (containsTab)
                {
                    // The position of the current character
                    int currentCharPosition = 0;
                    // The cumulative tabs-as-spaces additional length of the tab characters
                    int tabOffset = 0;
                    // The current tab
                    int currentTab = 0;
                    while (currentCharPosition + tabOffset < estimatedPositionInCommand)
                    {
                        // If currentTab is still within bounds of the array, process it
                        if (currentTab < state.Input.TabPositions.Count)
                        {
                            // The position of the tab character in the character array
                            int currentTabPosition = state.Input.TabPositions[currentTab];
                            // If the tab is after the position we want, we can skip it
                            if (currentTabPosition + tabOffset > estimatedPositionInCommand)
                            {
                                currentTab++;
                                continue;
                            }
                            // The current character is in the position after the current tab
                            currentCharPosition = currentTabPosition + 1;
                            // Add the additional space-equivalent length of the tab character to tabOffset
                            int additionalTabOffset = 7 - ((state.Input.StartPosition.X + currentTabPosition + tabOffset) % Console.BufferWidth % 8);
                            /*
                            if (consolePromptStart.X + currentTabPosition + tabOffset % Console.BufferWidth - 1 == 0)
                            {
                                additionalTabOffset = 0;
                            }
                            */
                            tabOffset += additionalTabOffset;
                            // Go to the next tab character
                            currentTab++;
                        }
                        // If currentTab is beyond the last index of the array, any remaining characters aren't tabs
                        else
                        {
                            int additionalChars = estimatedPositionInCommand - currentCharPosition - tabOffset;
                            //Debug.WriteLine(string.Format("Additional characters: {0}", additionalChars));
                            //currentCharPosition += additionalChars;
                            return currentCharPosition + additionalChars;

                            //virtualCharPosition += estimatedPositionInCommand - virtualCharPosition;
                        }
                    }
                    return currentCharPosition;
                }
                // If there isn't a tab, the estimate is probably correct
                return estimatedPositionInCommand;
            }
        }

        /// <summary>
        /// Gets the tab offset of a character in the current input.
        /// </summary>
        /// <param name="state"> The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="positionInCommand">The <see cref="NativeMethods.COORD"/> of the position to test.</param>
        /// <param name="singleTabCharacter">If true, returns the space-equivalent offset of a single tab character, <paramref name="positionInCommand"/>. If false, returns the cumulative offset of all tab characters up to and including <paramref name="positionInCommand"/></param>
        /// <returns>The number of spaces that would need adding to <paramref name="positionInCommand"/> to replace the tabs with spaces and <paramref name="positionInCommand"/> to be the same place on the display.</returns>
        public static int GetTabOffset(ConsoleState state, int positionInCommand, bool singleTabCharacter)
        {
            // tabOffset is the additional on-screen length of any tab characters contained in consoleCurrentCommand
            int tabOffset = 0;

            int currentCharPosition = 0;
            int currentTab = 0;
            while (currentCharPosition <= positionInCommand)
            {
                // The length of each tab character varies depending on where it is on screen, so each tab character needs calculating individually
                if (currentTab < state.Input.TabPositions.Count)
                {
                    int currentTabPosition = state.Input.TabPositions[currentTab];
                    // If the current tab character position is before the position of the character the cursor is on
                    if (currentTabPosition <= positionInCommand)
                    {
                        currentCharPosition = currentTabPosition + 1;
                        // The additional width of the current tab character
                        int tabAdditionalLength = 8 - ((state.Input.StartPosition.X + currentCharPosition + tabOffset) % Console.BufferWidth % 8);
                        // If console buffer is 120 characters wide, a tab on the last character of a line will be 1 character wide
                        /*
                        if (consolePromptStart.X + currentCharPosition + tabOffset % Console.BufferWidth - 1 == 0)
                        {
                            tabAdditionalLength = 1;
                        }
                        */
                        // Add the additional width of the current tab character to the running total 
                        tabOffset += tabAdditionalLength;
                        //Debug.WriteLine(string.Format("Additional length of tab {0} at position {1}: {2}", currentTab, currentTabPosition, tabAdditionalLength));
                        // If only the width of a single tab character is required
                        if (singleTabCharacter && positionInCommand == currentTabPosition)
                        {
                            return tabAdditionalLength + 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                    currentTab++;
                }
                else
                {
                    break;
                }
            }
            if (singleTabCharacter)
            {
                return 1;
            }
            else
            {
                return tabOffset;
            }
        }

    }
}
