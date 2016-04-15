using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Paragon.Runtime.Desktop;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WPF
{
    internal class WindowSnapManager : IDisposable
    {
        private const int WindowGapAdjustment = 2;
        private const int WindowSnapDistance = 15;
        private bool _actionStarted;
        private HwndSource _hwnd;

        public WindowSnapManager(IntPtr hwnd)
        {
            _hwnd = HwndSource.FromHwnd(hwnd);
            if (_hwnd == null)
            {
                throw new InvalidOperationException("Unable to obtain HwndSource");
            }

            // Add a WndProc hook in order to learn when the window is about to be moved or resized.
            // We then capture location info for all other open Paragon windows and use that to perform
            // window snapping operations.
            _hwnd.AddHook(WndProc);
        }

        public void Dispose()
        {
            if (_hwnd != null)
            {
                _hwnd.RemoveHook(WndProc);
                _hwnd.Dispose();
                _hwnd = null;
            }
        }

        [DebuggerStepThrough]
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            switch ((WM) msg)
            {
                case WM.MOVING:
                    if (!_actionStarted)
                    {
                        _actionStarted = true;

                        var otherWindows = ParagonDesktop.FindWindows(
                            opts =>
                            {
                                opts.WindowsToExclude.Add(_hwnd.Handle);
                                opts.WindowVisibility = WindowVisibility.Normal;
                            });

                        var windowLookup = otherWindows.ToDictionary(pwi => pwi.Hwnd, pwi => pwi.Bounds);
                        var moveHandler = new WindowMoveHandler(_hwnd, windowLookup);
                        moveHandler.PerformMove();
                        return new IntPtr(1);
                    }

                    return IntPtr.Zero;

                case WM.SIZING:
                    if (!_actionStarted)
                    {
                        var otherWindows = ParagonDesktop.FindWindows(
                            opts =>
                            {
                                opts.WindowsToExclude.Add(_hwnd.Handle);
                                opts.WindowVisibility = WindowVisibility.Normal;
                            });

                        var windowLookup = otherWindows.ToDictionary(pwi => pwi.Hwnd, pwi => pwi.Bounds);
                        var direction = Win32Api.GetResizeDirection(wParam);
                        var resizeHandler = new WindowResizeHandler(_hwnd, direction, windowLookup);
                        resizeHandler.PerformResize();
                    }

                    return new IntPtr(1);

                case WM.EXITSIZEMOVE:
                    bHandled = true;
                    _actionStarted = false;
                    return IntPtr.Zero;

                default:
                    return new IntPtr(1);
            }
        }

        private class WindowMoveHandler
        {
            private readonly HwndSource _movedWin;
            private readonly Dictionary<IntPtr, RECT> _otherWindows = new Dictionary<IntPtr, RECT>();
            private Point _mousePosAtStartOfMove;
            private RECT _originalWindowRect;

            public WindowMoveHandler(HwndSource movedWindow, Dictionary<IntPtr, RECT> otherWindows)
            {
                _movedWin = movedWindow;
                _otherWindows = otherWindows;
            }

            public void PerformMove()
            {
                // Add a hook in order to process move related windows messages.
                _movedWin.AddHook(WndProc);

                // Store the original window rect before the move takes place.
                _originalWindowRect = RECT.FromHandle(_movedWin.Handle);

                // Get the mouse position at the start of the move. This is used to help calculate
                // movement when snapping windows together.
                _mousePosAtStartOfMove = Win32Api.GetCursorPosition();
            }

            private void EndMove()
            {
                _movedWin.RemoveHook(WndProc);
            }

            private void Move(ref RECT movedWindowRect)
            {
                // Iterate all registered windows and attempt to snap them together.
                foreach (var rect in _otherWindows.Values)
                {
                    // Attempt to snap them together.  If a snap operation was performed, the
                    // return value here will be an enum representing the direction of the snap.
                    var offsetPoint = new Point(WindowSnapDistance + 1, WindowSnapDistance + 1);
                    SnapTogether(ref offsetPoint, movedWindowRect, rect, WindowSnapDistance);
                    movedWindowRect.Offset(offsetPoint.X, offsetPoint.Y);
                }
            }

            private void SnapTogether(ref Point offsetPoint, RECT movedWindowRect, RECT otherWindowRect, int snapGap)
            {
                const int gapAdjustment = WindowGapAdjustment;

                // Attempt to snap together horizontally if the windows are in line with
                // each other vertically and are also near each other.
                if (movedWindowRect.Bottom >= (otherWindowRect.Top - snapGap)
                    && movedWindowRect.Top <= (otherWindowRect.Bottom + snapGap))
                {
                    if ((Math.Abs(movedWindowRect.Left - otherWindowRect.Right) <= Math.Abs(offsetPoint.X)))
                    {
                        // Snap left to right.
                        offsetPoint.X = otherWindowRect.Right - movedWindowRect.Left + gapAdjustment;
                    }
                    if ((Math.Abs(movedWindowRect.Left + movedWindowRect.Width - otherWindowRect.Left)
                         <= Math.Abs(offsetPoint.X)))
                    {
                        // Snap right to left.
                        offsetPoint.X = otherWindowRect.Left - movedWindowRect.Width - movedWindowRect.Left - gapAdjustment;
                    }

                    if (Math.Abs(movedWindowRect.Left - otherWindowRect.Left) <= Math.Abs(offsetPoint.X))
                    {
                        // Snap left to left.
                        offsetPoint.X = otherWindowRect.Left - movedWindowRect.Left;
                    }
                    if (Math.Abs(movedWindowRect.Left + movedWindowRect.Width
                                 - otherWindowRect.Left - otherWindowRect.Width)
                        <= Math.Abs(offsetPoint.X))
                    {
                        // Snap right to right.
                        offsetPoint.X = otherWindowRect.Left + otherWindowRect.Width
                                        - movedWindowRect.Width - movedWindowRect.Left;
                    }
                }

                // Attempt to snap together vertically if the windows are in line with
                // each other horizontally and are also near each other.
                if (movedWindowRect.Right >= (otherWindowRect.Left - snapGap)
                    && movedWindowRect.Left <= (otherWindowRect.Right + snapGap))
                {
                    if (Math.Abs(movedWindowRect.Top - otherWindowRect.Bottom) <= Math.Abs(offsetPoint.Y))
                    {
                        // Snap top to bottom.
                        offsetPoint.Y = otherWindowRect.Bottom - movedWindowRect.Top + gapAdjustment;
                    }
                    if (Math.Abs(movedWindowRect.Top + movedWindowRect.Height - otherWindowRect.Top)
                        <= Math.Abs(offsetPoint.Y))
                    {
                        // Snap bottom to top.
                        offsetPoint.Y = otherWindowRect.Top - movedWindowRect.Height - movedWindowRect.Top - gapAdjustment;
                    }

                    // Try to snap top to top also.
                    if (Math.Abs(movedWindowRect.Top - otherWindowRect.Top) <= Math.Abs(offsetPoint.Y))
                    {
                        // Snap top to top.
                        offsetPoint.Y = otherWindowRect.Top - movedWindowRect.Top;
                    }
                    if (Math.Abs(movedWindowRect.Top + movedWindowRect.Height
                                 - otherWindowRect.Top - otherWindowRect.Height) <= Math.Abs(offsetPoint.Y))
                    {
                        // Snap bottom to bottom.
                        offsetPoint.Y = otherWindowRect.Top + otherWindowRect.Height
                                        - movedWindowRect.Height - movedWindowRect.Top;
                    }
                }

                // Reset the offset to zero if no move took place.
                if (Math.Abs(offsetPoint.Y - (snapGap + 1)) < 0.001)
                {
                    offsetPoint.Y = 0;
                }

                if (Math.Abs(offsetPoint.X - (snapGap + 1)) < 0.001)
                {
                    offsetPoint.X = 0;
                }
            }

            [DebuggerStepThrough]
            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch ((WM) msg)
                {
                        // The move has ended.
                    case WM.EXITSIZEMOVE:
                    {
                        handled = true;
                        EndMove();
                        return IntPtr.Zero;
                    }

                        // The window is being moved.
                    case WM.MOVING:
                    {
                        // The window is being moved. This is where the "magnetic snapping" together
                        // of windows takes place when one window is dragged near to another one.
                        var mousePos = Win32Api.GetCursorPosition();
                        var offsetX = mousePos.X - _mousePosAtStartOfMove.X;
                        var offsetY = mousePos.Y - _mousePosAtStartOfMove.Y;

                        // Get the rectangle for the movedWindow being moved.
                        var movedWindowRect = (RECT) Marshal.PtrToStructure(lParam, typeof (RECT));

                        // Adjust the location to take any manual snap adjustments that have occurred
                        // during the current move operation into account.  In other words, calculate
                        // where the window would have been if no snap adjustments had taken place so 
                        // that we can smoothly unsnap them.
                        var newPos = _originalWindowRect;
                        newPos.Offset(offsetX, offsetY);
                        movedWindowRect.Location = newPos.Location;

                        // Perform the move.
                        Move(ref movedWindowRect);

                        // Marshal the adjusted rect back to the message.
                        Marshal.StructureToPtr(movedWindowRect, lParam, true);
                        handled = true;
                        return new IntPtr(1);
                    }

                        // Capture key down message in order to be able to cancel the move when ESC is pressed.
                    case WM.KEYDOWN:
                    {
                        // Cancel the move if the escape key is pressed.
                        if ((int) wParam == (int) VK.ESCAPE)
                        {
                            EndMove();
                            var r = _originalWindowRect;
                            Win32Api.MoveWindow(hwnd, r.Left, r.Top, r.Width, r.Height, true);
                        }

                        handled = true;
                        return IntPtr.Zero;
                    }
                }

                return new IntPtr(1);
            }
        }

        private class WindowResizeHandler
        {
            private readonly ResizeDirection _direction;

            private readonly Dictionary<IntPtr, RECT> _otherWindows =
                new Dictionary<IntPtr, RECT>();

            private readonly HwndSource _resizedWindow;

            private RECT _originalWindowLocation;

            public WindowResizeHandler(HwndSource resizedWindow, ResizeDirection direction, Dictionary<IntPtr, RECT> otherWindows)
            {
                _resizedWindow = resizedWindow;
                _direction = direction;
                _otherWindows = otherWindows;
            }

            public void PerformResize()
            {
                _resizedWindow.AddHook(Hook);
                _originalWindowLocation = RECT.FromHandle(_resizedWindow.Handle);
            }

            private void EndResize()
            {
                _resizedWindow.RemoveHook(Hook);
            }

            private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch ((WM) msg)
                {
                    case WM.EXITSIZEMOVE:
                    {
                        handled = true;
                        EndResize();
                        break;
                    }

                    case WM.SIZING:
                    {
                        // Get the rectangle for the movedWindow being moved.
                        var movedWindowRect = (RECT) Marshal.PtrToStructure(lParam, typeof (RECT));

                        // Perform the resize.
                        if (Resize(ref movedWindowRect))
                        {
                            // Adjust the actual position.
                            Marshal.StructureToPtr(movedWindowRect, lParam, true);
                            handled = true;
                        }
                        break;
                    }

                    case WM.KEYDOWN:
                    {
                        if ((int) wParam == (int) VK.ESCAPE)
                        {
                            EndResize();
                            var r = _originalWindowLocation;
                            Win32Api.MoveWindow(hwnd, r.Left, r.Top, r.Width, r.Height, true);
                        }
                        break;
                    }
                }

                return IntPtr.Zero;
            }

            private bool Resize(ref RECT movedWindowRect)
            {
                // Iterate all registered windows and attempt to snap them together.
                foreach (var kvp in _otherWindows)
                {
                    if (SnapTogether(ref movedWindowRect, kvp.Value))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool SnapTogether(ref RECT movedWindowRect, RECT otherWindowRect)
            {
                var snapped = false;

                // Attempt to snap horizontal edges if the windows are in the same vertical plane.
                if (movedWindowRect.Right >= (otherWindowRect.Left - WindowSnapDistance)
                    && movedWindowRect.Left <= (otherWindowRect.Right + WindowSnapDistance))
                {
                    if ((_direction & ResizeDirection.Top) == ResizeDirection.Top)
                    {
                        if (Math.Abs(movedWindowRect.Top - otherWindowRect.Bottom)
                            <= WindowSnapDistance + 1)
                        {
                            // Snap top to bottom.
                            movedWindowRect.Top += otherWindowRect.Bottom - movedWindowRect.Top + WindowGapAdjustment;
                            snapped = true;
                        }
                        else if (Math.Abs(movedWindowRect.Top - otherWindowRect.Top) <= WindowSnapDistance + 1)
                        {
                            // Snap top to top
                            movedWindowRect.Top += otherWindowRect.Top - movedWindowRect.Top;
                            snapped = true;
                        }
                    }

                    if ((_direction & ResizeDirection.Top) == ResizeDirection.Bottom)
                    {
                        if (Math.Abs(movedWindowRect.Bottom - otherWindowRect.Top)
                            <= WindowSnapDistance + 1)
                        {
                            // Snap Bottom to top
                            movedWindowRect.Bottom += otherWindowRect.Top - movedWindowRect.Bottom + WindowGapAdjustment;
                            snapped = true;
                        }
                        else if (Math.Abs(movedWindowRect.Bottom - otherWindowRect.Bottom)
                                 <= WindowSnapDistance + 1)
                        {
                            // Snap bottom to bottom
                            movedWindowRect.Bottom += otherWindowRect.Bottom - movedWindowRect.Bottom;
                            snapped = true;
                        }
                    }
                }

                if (movedWindowRect.Bottom >= (otherWindowRect.Top - WindowSnapDistance) && movedWindowRect.Top
                    <= (otherWindowRect.Bottom + WindowSnapDistance))
                {
                    if ((_direction & ResizeDirection.Top) == ResizeDirection.Right)
                    {
                        if (Math.Abs(movedWindowRect.Right - otherWindowRect.Left)
                            <= WindowSnapDistance + 1)
                        {
                            // Snap right to left
                            movedWindowRect.Right += otherWindowRect.Left - movedWindowRect.Right + WindowGapAdjustment;
                            snapped = true;
                        }
                        else if (Math.Abs(movedWindowRect.Right - otherWindowRect.Right)
                                 <= WindowSnapDistance + 1)
                        {
                            // Snap right to right
                            movedWindowRect.Right += otherWindowRect.Right - movedWindowRect.Right;
                            snapped = true;
                        }
                    }

                    if ((_direction & ResizeDirection.Top) == ResizeDirection.Left)
                    {
                        if (Math.Abs(movedWindowRect.Left - otherWindowRect.Right)
                            <= WindowSnapDistance + 1)
                        {
                            // Snap left to right
                            movedWindowRect.Left += otherWindowRect.Right - movedWindowRect.Left + WindowGapAdjustment;
                            snapped = true;
                        }
                        else if (Math.Abs(movedWindowRect.Left - otherWindowRect.Left)
                                 <= WindowSnapDistance + 1)
                        {
                            // Snap left to left
                            movedWindowRect.Left += otherWindowRect.Left - movedWindowRect.Left;
                            snapped = true;
                        }
                    }
                }

                return snapped;
            }
        }
    }
}