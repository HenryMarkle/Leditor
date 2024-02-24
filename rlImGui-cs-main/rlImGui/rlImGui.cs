/*******************************************************************************************
*
*   raylib-extras [ImGui] example - Simple Integration
*
*	This is a simple ImGui Integration
*	It is done using C++ but with C style code
*	It can be done in C as well if you use the C ImGui wrapper
*	https://github.com/cimgui/cimgui
*
*   Copyright (c) 2021 Jeffery Myers
*
********************************************************************************************/


using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

using Raylib_cs;
using ImGuiNET;

namespace rlImGui_cs
{
    public static class rlImGui
    {
        internal static IntPtr ImGuiContext = IntPtr.Zero;

        private static ImGuiMouseCursor CurrentMouseCursor = ImGuiMouseCursor.COUNT;
        private static Dictionary<ImGuiMouseCursor, Raylib_cs.MouseCursor> MouseCursorMap = new Dictionary<ImGuiMouseCursor, Raylib_cs.MouseCursor>();
        private static Texture2D FontTexture;

        static Dictionary<Raylib_cs.KeyboardKey, ImGuiKey> RaylibKeyMap = new Dictionary<Raylib_cs.KeyboardKey, ImGuiKey>();

        internal static bool LastFrameFocused = false;

        internal static bool LastControlPressed = false;
        internal static bool LastShiftPressed = false;
        internal static bool LastAltPressed = false;
        internal static bool LastSuperPressed = false;

        internal static bool rlImGuiIsControlDown() { return Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightControl) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl); }
        internal static bool rlImGuiIsShiftDown() { return Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightShift) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftShift); }
        internal static bool rlImGuiIsAltDown() { return Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightAlt) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftAlt); }
        internal static bool rlImGuiIsSuperDown() { return Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightSuper) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftSuper); }

        public delegate void SetupUserFontsCallback(ImGuiIOPtr imGuiIo);

        /// <summary>
        /// Callback for cases where the user wants to install additional fonts.
        /// </summary>
        public static SetupUserFontsCallback SetupUserFonts = null;

        /// <summary>
        /// Sets up ImGui, loads fonts and themes
        /// </summary>
        /// <param name="darkTheme">when true(default) the dark theme is used, when false the light theme is used</param>
        /// <param name="enableDocking">when true(not default) docking support will be enabled/param>
        public static void Setup(bool darkTheme = true, bool enableDocking = false)
        {
            MouseCursorMap = new Dictionary<ImGuiMouseCursor, Raylib_cs.MouseCursor>();
            MouseCursorMap = new Dictionary<ImGuiMouseCursor, Raylib_cs.MouseCursor>();

            LastFrameFocused = Raylib_cs.Raylib.IsWindowFocused();
            LastControlPressed = false;
            LastShiftPressed = false;
            LastAltPressed = false;
            LastSuperPressed = false;

            FontTexture.Id = 0;

            BeginInitImGui();

            if (darkTheme)
                ImGui.StyleColorsDark();
            else
                ImGui.StyleColorsLight();

            if (enableDocking)
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            EndInitImGui();
        }

        /// <summary>
        /// Custom initialization. Not needed if you call Setup. Only needed if you want to add custom setup code.
        /// must be followed by EndInitImGui
        /// </summary>
        public static void BeginInitImGui()
        {
            SetupKeymap();

            ImGuiContext = ImGui.CreateContext();
        }

        internal static void SetupKeymap()
        {
            if (RaylibKeyMap.Count > 0)
                return;

            // build up a map of raylib keys to ImGuiKeys
            RaylibKeyMap[Raylib_cs.KeyboardKey.Apostrophe] = ImGuiKey.Apostrophe;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Comma] = ImGuiKey.Comma;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Minus] = ImGuiKey.Minus;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Period] = ImGuiKey.Period;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Slash] = ImGuiKey.Slash;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Zero] = ImGuiKey._0;
            RaylibKeyMap[Raylib_cs.KeyboardKey.One] = ImGuiKey._1;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Two] = ImGuiKey._2;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Three] = ImGuiKey._3;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Four] = ImGuiKey._4;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Five] = ImGuiKey._5;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Six] = ImGuiKey._6;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Seven] = ImGuiKey._7;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Eight] = ImGuiKey._8;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Nine] = ImGuiKey._9;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Semicolon] = ImGuiKey.Semicolon;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Equal] = ImGuiKey.Equal;
            RaylibKeyMap[Raylib_cs.KeyboardKey.A] = ImGuiKey.A;
            RaylibKeyMap[Raylib_cs.KeyboardKey.B] = ImGuiKey.B;
            RaylibKeyMap[Raylib_cs.KeyboardKey.C] = ImGuiKey.C;
            RaylibKeyMap[Raylib_cs.KeyboardKey.D] = ImGuiKey.D;
            RaylibKeyMap[Raylib_cs.KeyboardKey.E] = ImGuiKey.E;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F] = ImGuiKey.F;
            RaylibKeyMap[Raylib_cs.KeyboardKey.G] = ImGuiKey.G;
            RaylibKeyMap[Raylib_cs.KeyboardKey.H] = ImGuiKey.H;
            RaylibKeyMap[Raylib_cs.KeyboardKey.I] = ImGuiKey.I;
            RaylibKeyMap[Raylib_cs.KeyboardKey.J] = ImGuiKey.J;
            RaylibKeyMap[Raylib_cs.KeyboardKey.K] = ImGuiKey.K;
            RaylibKeyMap[Raylib_cs.KeyboardKey.L] = ImGuiKey.L;
            RaylibKeyMap[Raylib_cs.KeyboardKey.M] = ImGuiKey.M;
            RaylibKeyMap[Raylib_cs.KeyboardKey.N] = ImGuiKey.N;
            RaylibKeyMap[Raylib_cs.KeyboardKey.O] = ImGuiKey.O;
            RaylibKeyMap[Raylib_cs.KeyboardKey.P] = ImGuiKey.P;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Q] = ImGuiKey.Q;
            RaylibKeyMap[Raylib_cs.KeyboardKey.R] = ImGuiKey.R;
            RaylibKeyMap[Raylib_cs.KeyboardKey.S] = ImGuiKey.S;
            RaylibKeyMap[Raylib_cs.KeyboardKey.T] = ImGuiKey.T;
            RaylibKeyMap[Raylib_cs.KeyboardKey.U] = ImGuiKey.U;
            RaylibKeyMap[Raylib_cs.KeyboardKey.V] = ImGuiKey.V;
            RaylibKeyMap[Raylib_cs.KeyboardKey.W] = ImGuiKey.W;
            RaylibKeyMap[Raylib_cs.KeyboardKey.X] = ImGuiKey.X;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Y] = ImGuiKey.Y;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Z] = ImGuiKey.Z;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Space] = ImGuiKey.Space;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Escape] = ImGuiKey.Escape;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Enter] = ImGuiKey.Enter;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Tab] = ImGuiKey.Tab;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Backspace] = ImGuiKey.Backspace;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Insert] = ImGuiKey.Insert;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Delete] = ImGuiKey.Delete;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Right] = ImGuiKey.RightArrow;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Left] = ImGuiKey.LeftArrow;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Down] = ImGuiKey.DownArrow;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Up] = ImGuiKey.UpArrow;
            RaylibKeyMap[Raylib_cs.KeyboardKey.PageUp] = ImGuiKey.PageUp;
            RaylibKeyMap[Raylib_cs.KeyboardKey.PageDown] = ImGuiKey.PageDown;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Home] = ImGuiKey.Home;
            RaylibKeyMap[Raylib_cs.KeyboardKey.End] = ImGuiKey.End;
            RaylibKeyMap[Raylib_cs.KeyboardKey.CapsLock] = ImGuiKey.CapsLock;
            RaylibKeyMap[Raylib_cs.KeyboardKey.ScrollLock] = ImGuiKey.ScrollLock;
            RaylibKeyMap[Raylib_cs.KeyboardKey.NumLock] = ImGuiKey.NumLock;
            RaylibKeyMap[Raylib_cs.KeyboardKey.PrintScreen] = ImGuiKey.PrintScreen;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Pause] = ImGuiKey.Pause;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F1] = ImGuiKey.F1;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F2] = ImGuiKey.F2;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F3] = ImGuiKey.F3;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F4] = ImGuiKey.F4;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F5] = ImGuiKey.F5;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F6] = ImGuiKey.F6;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F7] = ImGuiKey.F7;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F8] = ImGuiKey.F8;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F9] = ImGuiKey.F9;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F10] = ImGuiKey.F10;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F11] = ImGuiKey.F11;
            RaylibKeyMap[Raylib_cs.KeyboardKey.F12] = ImGuiKey.F12;
            RaylibKeyMap[Raylib_cs.KeyboardKey.LeftShift] = ImGuiKey.LeftShift;
            RaylibKeyMap[Raylib_cs.KeyboardKey.LeftControl] = ImGuiKey.LeftCtrl;
            RaylibKeyMap[Raylib_cs.KeyboardKey.LeftAlt] = ImGuiKey.LeftAlt;
            RaylibKeyMap[Raylib_cs.KeyboardKey.LeftSuper] = ImGuiKey.LeftSuper;
            RaylibKeyMap[Raylib_cs.KeyboardKey.RightShift] = ImGuiKey.RightShift;
            RaylibKeyMap[Raylib_cs.KeyboardKey.RightControl] = ImGuiKey.RightCtrl;
            RaylibKeyMap[Raylib_cs.KeyboardKey.RightAlt] = ImGuiKey.RightAlt;
            RaylibKeyMap[Raylib_cs.KeyboardKey.RightSuper] = ImGuiKey.RightSuper;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KeyboardMenu] = ImGuiKey.Menu;
            RaylibKeyMap[Raylib_cs.KeyboardKey.LeftBracket] = ImGuiKey.LeftBracket;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Backslash] = ImGuiKey.Backslash;
            RaylibKeyMap[Raylib_cs.KeyboardKey.RightBracket] = ImGuiKey.RightBracket;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Grave] = ImGuiKey.GraveAccent;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp0] = ImGuiKey.Keypad0;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp1] = ImGuiKey.Keypad1;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp2] = ImGuiKey.Keypad2;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp3] = ImGuiKey.Keypad3;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp4] = ImGuiKey.Keypad4;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp5] = ImGuiKey.Keypad5;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp6] = ImGuiKey.Keypad6;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp7] = ImGuiKey.Keypad7;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp8] = ImGuiKey.Keypad8;
            RaylibKeyMap[Raylib_cs.KeyboardKey.Kp9] = ImGuiKey.Keypad9;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpDecimal] = ImGuiKey.KeypadDecimal;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpDivide] = ImGuiKey.KeypadDivide;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpMultiply] = ImGuiKey.KeypadMultiply;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpSubtract] = ImGuiKey.KeypadSubtract;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpAdd] = ImGuiKey.KeypadAdd;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpEnter] = ImGuiKey.KeypadEnter;
            RaylibKeyMap[Raylib_cs.KeyboardKey.KpEqual] = ImGuiKey.KeypadEqual;
        }

        private static void SetupMouseCursors()
        {
            MouseCursorMap.Clear();
            MouseCursorMap[ImGuiMouseCursor.Arrow] = Raylib_cs.MouseCursor.Arrow;
            MouseCursorMap[ImGuiMouseCursor.TextInput] = Raylib_cs.MouseCursor.IBeam;
            MouseCursorMap[ImGuiMouseCursor.Hand] = Raylib_cs.MouseCursor.PointingHand;
            MouseCursorMap[ImGuiMouseCursor.ResizeAll] = Raylib_cs.MouseCursor.ResizeAll;
            MouseCursorMap[ImGuiMouseCursor.ResizeEW] = Raylib_cs.MouseCursor.ResizeEw;
            MouseCursorMap[ImGuiMouseCursor.ResizeNESW] = Raylib_cs.MouseCursor.ResizeNesw;
            MouseCursorMap[ImGuiMouseCursor.ResizeNS] = Raylib_cs.MouseCursor.ResizeNs;
            MouseCursorMap[ImGuiMouseCursor.ResizeNWSE] = Raylib_cs.MouseCursor.ResizeNwse;
            MouseCursorMap[ImGuiMouseCursor.NotAllowed] = Raylib_cs.MouseCursor.NotAllowed;
        }

        /// <summary>
        /// Forces the font texture atlas to be recomputed and re-cached
        /// </summary>
        public static unsafe void ReloadFonts()
        {
            ImGui.SetCurrentContext(ImGuiContext);
            ImGuiIOPtr io = ImGui.GetIO();

            int width, height, bytesPerPixel;
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out width, out height, out bytesPerPixel);

            Raylib_cs.Image image = new Raylib_cs.Image
            {
                Data = pixels,
                Width = width,
                Height = height,
                Mipmaps = 1,
                Format = Raylib_cs.PixelFormat.UncompressedR8G8B8A8,
            };

            if (FontTexture.Id > 0 && (FontTexture.Width > 0) &&
                (FontTexture.Height > 0) &&     // Validate texture size
                (FontTexture.Format > 0) &&     // Validate texture pixel format
                (FontTexture.Mipmaps > 0))
                Raylib_cs.Raylib.UnloadTexture(FontTexture);

            FontTexture = Raylib_cs.Raylib.LoadTextureFromImage(image);

            io.Fonts.SetTexID(new IntPtr(FontTexture.Id));
        }

        unsafe internal static sbyte* rImGuiGetClipText(IntPtr userData)
        {
            return Raylib_cs.Raylib.GetClipboardText();
        }

        unsafe internal static void rlImGuiSetClipText(IntPtr userData, sbyte* text)
        {
            Raylib_cs.Raylib.SetClipboardText(text);
        }

        private unsafe delegate sbyte* GetClipTextCallback(IntPtr userData);
        private unsafe delegate void SetClipTextCallback(IntPtr userData, sbyte* text);

        /// <summary>
        /// End Custom initialization. Not needed if you call Setup. Only needed if you want to add custom setup code.
        /// must be proceeded by BeginInitImGui
        /// </summary>
        public static void EndInitImGui()
        {
            SetupMouseCursors();

            ImGui.SetCurrentContext(ImGuiContext);

            var fonts = ImGui.GetIO().Fonts;
            ImGui.GetIO().Fonts.AddFontDefault();

            // remove this part if you don't want font awesome
            unsafe
            {
                ImFontConfig* icons_config = ImGuiNative.ImFontConfig_ImFontConfig();
                icons_config->MergeMode = 1;                      // merge the glyph ranges into the default font
                icons_config->PixelSnapH = 1;                     // don't try to render on partial pixels
                icons_config->FontDataOwnedByAtlas = 0;           // the font atlas does not own this font data

                icons_config->GlyphMaxAdvanceX = float.MaxValue;
                icons_config->RasterizerMultiply = 1.0f;
                icons_config->OversampleH = 2;
                icons_config->OversampleV = 1;

                ushort[] IconRanges = new ushort[3];
                IconRanges[0] = IconFonts.FontAwesome6.IconMin;
                IconRanges[1] = IconFonts.FontAwesome6.IconMax;
                IconRanges[2] = 0;

                fixed (ushort* range = &IconRanges[0])
                {
                    // this unmanaged memory must remain allocated for the entire run of rlImgui
                    IconFonts.FontAwesome6.IconFontRanges = Marshal.AllocHGlobal(6);
                    Buffer.MemoryCopy(range, IconFonts.FontAwesome6.IconFontRanges.ToPointer(), 6, 6);
                    icons_config->GlyphRanges = (ushort*)IconFonts.FontAwesome6.IconFontRanges.ToPointer();

                    byte[] fontDataBuffer = Convert.FromBase64String(IconFonts.FontAwesome6.IconFontData);

                    fixed (byte* buffer = fontDataBuffer)
                    {
                        var fontPtr = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(new IntPtr(buffer), fontDataBuffer.Length, 11, icons_config, IconFonts.FontAwesome6.IconFontRanges);
                    }
                }

                ImGuiNative.ImFontConfig_destroy(icons_config);
            }

            ImGuiIOPtr io = ImGui.GetIO();

            if (SetupUserFonts != null)
                SetupUserFonts(io);

            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad;

            io.MousePos.X = 0;
            io.MousePos.Y = 0;

            // copy/paste callbacks
            unsafe
            {
                GetClipTextCallback getClip = new GetClipTextCallback(rImGuiGetClipText);
                SetClipTextCallback setClip = new SetClipTextCallback(rlImGuiSetClipText);

                io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(setClip);
                io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(getClip);
            }

            io.ClipboardUserData = IntPtr.Zero;
            ReloadFonts();
        }

        private static void SetMouseEvent(ImGuiIOPtr io, Raylib_cs.MouseButton rayMouse, ImGuiMouseButton imGuiMouse)
        {
            if (Raylib_cs.Raylib.IsMouseButtonPressed(rayMouse))
                io.AddMouseButtonEvent((int)imGuiMouse, true);
            else if (Raylib_cs.Raylib.IsMouseButtonReleased(rayMouse))
                io.AddMouseButtonEvent((int)imGuiMouse, false);
        }

        private static void NewFrame(float dt = -1)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (Raylib_cs.Raylib.IsWindowFullscreen())
            {
                int monitor = Raylib_cs.Raylib.GetCurrentMonitor();
                io.DisplaySize = new Vector2(Raylib_cs.Raylib.GetMonitorWidth(monitor), Raylib_cs.Raylib.GetMonitorHeight(monitor));
            }
            else
            {
                io.DisplaySize = new Vector2(Raylib_cs.Raylib.GetScreenWidth(), Raylib_cs.Raylib.GetScreenHeight());
            }

            io.DisplayFramebufferScale = new Vector2(1, 1);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || Raylib_cs.Raylib.IsWindowState(Raylib_cs.ConfigFlags.HighDpiWindow))
                    io.DisplayFramebufferScale = Raylib_cs.Raylib.GetWindowScaleDPI();

            io.DeltaTime = dt >= 0 ? dt : Raylib_cs.Raylib.GetFrameTime();

            if (io.WantSetMousePos)
            {
                Raylib_cs.Raylib.SetMousePosition((int)io.MousePos.X, (int)io.MousePos.Y);
            }
            else
            {
                io.AddMousePosEvent(Raylib_cs.Raylib.GetMouseX(), Raylib_cs.Raylib.GetMouseY());
            }

            SetMouseEvent(io, Raylib_cs.MouseButton.Left, ImGuiMouseButton.Left);
            SetMouseEvent(io, Raylib_cs.MouseButton.Right, ImGuiMouseButton.Right);
            SetMouseEvent(io, Raylib_cs.MouseButton.Middle, ImGuiMouseButton.Middle);
            SetMouseEvent(io, Raylib_cs.MouseButton.Forward, ImGuiMouseButton.Middle + 1);
            SetMouseEvent(io, Raylib_cs.MouseButton.Back, ImGuiMouseButton.Middle + 2);

            var wheelMove = Raylib_cs.Raylib.GetMouseWheelMoveV();
            io.AddMouseWheelEvent(wheelMove.X, wheelMove.Y);

            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) == 0)
            {
                ImGuiMouseCursor imgui_cursor = ImGui.GetMouseCursor();
                if (imgui_cursor != CurrentMouseCursor || io.MouseDrawCursor)
                {
                    CurrentMouseCursor = imgui_cursor;
                    if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor.None)
                    {
                        Raylib_cs.Raylib.HideCursor();
                    }
                    else
                    {
                        Raylib_cs.Raylib.ShowCursor();

                        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) == 0)
                        {

                            if (!MouseCursorMap.ContainsKey(imgui_cursor))
                                Raylib_cs.Raylib.SetMouseCursor(Raylib_cs.MouseCursor.Default);
                            else
                                Raylib_cs.Raylib.SetMouseCursor(MouseCursorMap[imgui_cursor]);
                        }
                    }
                }
            }
        }

        private static void FrameEvents()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            bool focused = Raylib_cs.Raylib.IsWindowFocused();
            if (focused != LastFrameFocused)
                io.AddFocusEvent(focused);
            LastFrameFocused = focused;


            // handle the modifyer key events so that shortcuts work
            bool ctrlDown = rlImGuiIsControlDown();
            if (ctrlDown != LastControlPressed)
                io.AddKeyEvent(ImGuiKey.ModCtrl, ctrlDown);
            LastControlPressed = ctrlDown;

            bool shiftDown = rlImGuiIsShiftDown();
            if (shiftDown != LastShiftPressed)
                io.AddKeyEvent(ImGuiKey.ModShift, shiftDown);
            LastShiftPressed = shiftDown;

            bool altDown = rlImGuiIsAltDown();
            if (altDown != LastAltPressed)
                io.AddKeyEvent(ImGuiKey.ModAlt, altDown);
            LastAltPressed = altDown;

            bool superDown = rlImGuiIsSuperDown();
            if (superDown != LastSuperPressed)
                io.AddKeyEvent(ImGuiKey.ModSuper, superDown);
            LastSuperPressed = superDown;

            // get the pressed keys, they are in event order
            int keyId = Raylib_cs.Raylib.GetKeyPressed();
            while (keyId != 0)
            {
                Raylib_cs.KeyboardKey key = (Raylib_cs.KeyboardKey)keyId;
                if (RaylibKeyMap.ContainsKey(key))
                    io.AddKeyEvent(RaylibKeyMap[key], true);
                keyId = Raylib_cs.Raylib.GetKeyPressed();
            }

            // look for any keys that were down last frame and see if they were down and are released
            foreach (var keyItr in RaylibKeyMap)
	        {
                if (Raylib_cs.Raylib.IsKeyReleased(keyItr.Key))
                    io.AddKeyEvent(keyItr.Value, false);
            }

            // add the text input in order
            var pressed = Raylib_cs.Raylib.GetCharPressed();
            while (pressed != 0)
            {
                io.AddInputCharacter((uint)pressed);
                pressed = Raylib_cs.Raylib.GetCharPressed();
            }

            // gamepads
            if ((io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) != 0 && Raylib_cs.Raylib.IsGamepadAvailable(0))
            {
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftFaceUp, ImGuiKey.GamepadDpadUp);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftFaceRight, ImGuiKey.GamepadDpadRight);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftFaceDown, ImGuiKey.GamepadDpadDown);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftFaceLeft, ImGuiKey.GamepadDpadLeft);

                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightFaceUp, ImGuiKey.GamepadFaceUp);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightFaceRight, ImGuiKey.GamepadFaceLeft);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightFaceDown, ImGuiKey.GamepadFaceDown);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightFaceLeft, ImGuiKey.GamepadFaceRight);

                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftTrigger1, ImGuiKey.GamepadL1);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftTrigger2, ImGuiKey.GamepadL2);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightTrigger1, ImGuiKey.GamepadR1);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightTrigger2, ImGuiKey.GamepadR2);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.LeftThumb, ImGuiKey.GamepadL3);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.RightThumb, ImGuiKey.GamepadR3);

                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.MiddleLeft, ImGuiKey.GamepadStart);
                HandleGamepadButtonEvent(io, Raylib_cs.GamepadButton.MiddleRight, ImGuiKey.GamepadBack);

                // left stick
                HandleGamepadStickEvent(io, Raylib_cs.GamepadAxis.LeftX, ImGuiKey.GamepadLStickLeft, ImGuiKey.GamepadLStickRight);
                HandleGamepadStickEvent(io, Raylib_cs.GamepadAxis.LeftY, ImGuiKey.GamepadLStickUp, ImGuiKey.GamepadLStickDown);

                // right stick
                HandleGamepadStickEvent(io, Raylib_cs.GamepadAxis.RightX, ImGuiKey.GamepadRStickLeft, ImGuiKey.GamepadRStickRight);
                HandleGamepadStickEvent(io, Raylib_cs.GamepadAxis.RightY, ImGuiKey.GamepadRStickUp, ImGuiKey.GamepadRStickDown);
            }
        }


        private static void HandleGamepadButtonEvent(ImGuiIOPtr io, Raylib_cs.GamepadButton button, ImGuiKey key)
        {
            if (Raylib_cs.Raylib.IsGamepadButtonPressed(0, button))
                io.AddKeyEvent(key, true);
            else if (Raylib_cs.Raylib.IsGamepadButtonReleased(0, button))
                io.AddKeyEvent(key, false);
        }

        private static void HandleGamepadStickEvent(ImGuiIOPtr io, Raylib_cs.GamepadAxis axis, ImGuiKey negKey, ImGuiKey posKey)
        {
            const float deadZone = 0.20f;

            float axisValue = Raylib_cs.Raylib.GetGamepadAxisMovement(0, axis);

            io.AddKeyAnalogEvent(negKey, axisValue < -deadZone, axisValue < -deadZone ? -axisValue : 0);
            io.AddKeyAnalogEvent(posKey, axisValue > deadZone, axisValue > deadZone ? axisValue : 0);
        }

        /// <summary>
        /// Starts a new ImGui Frame
        /// </summary>
        /// <param name="dt">optional delta time, any value < 0 will use raylib GetFrameTime</param>
        public static void Begin(float dt = -1)
        {
            ImGui.SetCurrentContext(ImGuiContext);

            NewFrame(dt);
            FrameEvents();
            ImGui.NewFrame();
        }

        private static void EnableScissor(float x, float y, float width, float height)
        {
            Rlgl.EnableScissorTest();
            ImGuiIOPtr io = ImGui.GetIO();

            Vector2 scale = new Vector2(1.0f, 1.0f);
            if (Raylib_cs.Raylib.IsWindowState(Raylib_cs.ConfigFlags.HighDpiWindow) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                scale = io.DisplayFramebufferScale;

            Rlgl.Scissor(   (int)(x * scale.X),
                            (int)((io.DisplaySize.Y - (int)(y + height)) * scale.Y),
                            (int)(width * scale.X),
                            (int)(height * scale.Y));
        }

        private static void TriangleVert(ImDrawVertPtr idx_vert)
        {
            Vector4 color = ImGui.ColorConvertU32ToFloat4(idx_vert.col);

            Rlgl.Color4f(color.X, color.Y, color.Z, color.W);
            Rlgl.TexCoord2f(idx_vert.uv.X, idx_vert.uv.Y);
            Rlgl.Vertex2f(idx_vert.pos.X, idx_vert.pos.Y);
        }

        private static void RenderTriangles(uint count, uint indexStart, ImVector<ushort> indexBuffer, ImPtrVector<ImDrawVertPtr> vertBuffer, IntPtr texturePtr)
        {
            if (count < 3)
                return;

            uint textureId = 0;
            if (texturePtr != IntPtr.Zero)
                textureId = (uint)texturePtr.ToInt32();

            Rlgl.Begin(DrawMode.Triangles);
            Rlgl.SetTexture(textureId);

            for (int i = 0; i <= (count - 3); i += 3)
            {
                if (Rlgl.CheckRenderBatchLimit(3))
                {
                    Rlgl.Begin(DrawMode.Triangles);
                    Rlgl.SetTexture(textureId);
                }

                ushort indexA = indexBuffer[(int)indexStart + i];
                ushort indexB = indexBuffer[(int)indexStart + i + 1];
                ushort indexC = indexBuffer[(int)indexStart + i + 2];

                ImDrawVertPtr vertexA = vertBuffer[indexA];
                ImDrawVertPtr vertexB = vertBuffer[indexB];
                ImDrawVertPtr vertexC = vertBuffer[indexC];

                TriangleVert(vertexA);
                TriangleVert(vertexB);
                TriangleVert(vertexC);
            }
            Rlgl.End();
        }

        private delegate void Callback(ImDrawListPtr list, ImDrawCmdPtr cmd);

        private static void RenderData()
        {
            Rlgl.DrawRenderBatchActive();
            Rlgl.DisableBackfaceCulling();

            var data = ImGui.GetDrawData();

            for (int l = 0; l < data.CmdListsCount; l++)
            {
                ImDrawListPtr commandList = data.CmdLists[l];

                for (int cmdIndex = 0; cmdIndex < commandList.CmdBuffer.Size; cmdIndex++)
                {
                    var cmd = commandList.CmdBuffer[cmdIndex];

                    EnableScissor(cmd.ClipRect.X - data.DisplayPos.X, cmd.ClipRect.Y - data.DisplayPos.Y, cmd.ClipRect.Z - (cmd.ClipRect.X - data.DisplayPos.X), cmd.ClipRect.W - (cmd.ClipRect.Y - data.DisplayPos.Y));
                    if (cmd.UserCallback != IntPtr.Zero)
                    {
                        Callback cb = Marshal.GetDelegateForFunctionPointer<Callback>(cmd.UserCallback);
                        cb(commandList, cmd);
                        continue;
                    }

                    RenderTriangles(cmd.ElemCount, cmd.IdxOffset, commandList.IdxBuffer, commandList.VtxBuffer, cmd.TextureId);

                    Rlgl.DrawRenderBatchActive();
                }
            }
            Rlgl.SetTexture(0);
            Rlgl.DisableScissorTest();
            Rlgl.EnableBackfaceCulling();
        }

        /// <summary>
        /// Ends an ImGui frame and submits all ImGui drawing to raylib for processing.
        /// </summary>
        public static void End()
        {
            ImGui.SetCurrentContext(ImGuiContext);
            ImGui.Render();
            RenderData();
        }

        /// <summary>
        /// Cleanup ImGui and unload font atlas
        /// </summary>
        public static void Shutdown()
        {
            Raylib_cs.Raylib.UnloadTexture(FontTexture);
            ImGui.DestroyContext();

            // remove this if you don't want font awesome support
            {
                if (IconFonts.FontAwesome6.IconFontRanges != IntPtr.Zero)
                    Marshal.FreeHGlobal(IconFonts.FontAwesome6.IconFontRanges);

                IconFonts.FontAwesome6.IconFontRanges = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Draw a texture as an image in an ImGui Context
        /// Uses the current ImGui Cursor position and the full texture size.
        /// </summary>
        /// <param name="image">The raylib texture to draw</param>
        public static void Image(Texture2D image)
        {
            ImGui.Image(new IntPtr(image.Id), new Vector2(image.Width, image.Height));
        }

        /// <summary>
        /// Draw a texture as an image in an ImGui Context at a specific size
        /// Uses the current ImGui Cursor position and the specified width and height
        /// The image will be scaled up or down to fit as needed
        /// </summary>
        /// <param name="image">The raylib texture to draw</param>
        /// <param name="width">The width of the drawn image</param>
        /// <param name="height">The height of the drawn image</param>
        public static void ImageSize(Texture2D image, int width, int height)
        {
            ImGui.Image(new IntPtr(image.Id), new Vector2(width, height));
        }

        /// <summary>
        /// Draw a texture as an image in an ImGui Context at a specific size
        /// Uses the current ImGui Cursor position and the specified size
        /// The image will be scaled up or down to fit as needed
        /// </summary>
        /// <param name="image">The raylib texture to draw</param>
        /// <param name="size">The size of drawn image</param>
        public static void ImageSize(Texture2D image, Vector2 size)
        {
            ImGui.Image(new IntPtr(image.Id), size);
        }

        /// <summary>
        /// Draw a portion texture as an image in an ImGui Context at a defined size
        /// Uses the current ImGui Cursor position and the specified size
        /// The image will be scaled up or down to fit as needed
        /// </summary>
        /// <param name="image">The raylib texture to draw</param>
        /// <param name="destWidth">The width of the drawn image</param>
        /// <param name="destHeight">The height of the drawn image</param>
        /// <param name="sourceRect">The portion of the texture to draw as an image. Negative values for the width and height will flip the image</param>
        public static void ImageRect(Texture2D image, int destWidth, int destHeight, Raylib_cs.Rectangle sourceRect)
        {
            Vector2 uv0 = new Vector2();
            Vector2 uv1 = new Vector2();

            if (sourceRect.Width < 0)
            {
                uv0.X = -((float)sourceRect.X / image.Width);
                uv1.X = (uv0.X - (float)(Math.Abs(sourceRect.Width) / image.Width));
            }
            else
            {
                uv0.X = (float)sourceRect.X / image.Width;
                uv1.X = uv0.X + (float)(sourceRect.Width / image.Width);
            }

            if (sourceRect.Height < 0)
            {
                uv0.Y = -((float)sourceRect.Y / image.Height);
                uv1.Y = (uv0.Y - (float)(Math.Abs(sourceRect.Height) / image.Height));
            }
            else
            {
                uv0.Y = (float)sourceRect.Y / image.Height;
                uv1.Y = uv0.Y + (float)(sourceRect.Height / image.Height);
            }

            ImGui.Image(new IntPtr(image.Id), new Vector2(destWidth, destHeight), uv0, uv1);
        }

        /// <summary>
        /// Draws a render texture as an image an ImGui Context, automatically flipping the Y axis so it will show correctly on screen
        /// </summary>
        /// <param name="image">The render texture to draw</param>
        public static void ImageRenderTexture(RenderTexture2D image)
        {
            ImageRect(image.Texture, image.Texture.Width, image.Texture.Height, new Raylib_cs.Rectangle(0, 0, image.Texture.Width, -image.Texture.Height));
        }

        /// <summary>
        /// Draws a render texture as an image to the current ImGui Context, flipping the Y axis so it will show correctly on the screen
        /// The texture will be scaled to fit the content are available, centered if desired
        /// </summary>
        /// <param name="image">The render texture to draw</param>
        /// <param name="center">When true the texture will be centered in the content area. When false the image will be left and top justified</param>
        public static void ImageRenderTextureFit(RenderTexture2D image, bool center = true)
        {
            Vector2 area = ImGui.GetContentRegionAvail();

            float scale = area.X / image.Texture.Width;

            float y = image.Texture.Height * scale;
            if (y > area.Y)
            {
                scale = area.Y / image.Texture.Height;
            }

            int sizeX = (int)(image.Texture.Width * scale);
            int sizeY = (int)(image.Texture.Height * scale);

            if (center)
            {
                ImGui.SetCursorPosX(0);
                ImGui.SetCursorPosX(area.X / 2 - sizeX / 2);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (area.Y / 2 - sizeY / 2));
            }

            ImageRect(image.Texture, sizeX, sizeY, new Raylib_cs.Rectangle(0,0, (image.Texture.Width), -(image.Texture.Height) ));
        }

        /// <summary>
        /// Draws a texture as an image button in an ImGui context. Uses the current ImGui cursor position and the full size of the texture
        /// </summary>
        /// <param name="name">The display name and ImGui ID for the button</param>
        /// <param name="image">The texture to draw</param>
        /// <returns>True if the button was clicked</returns>
        public static bool ImageButton(System.String name, Texture2D image)
        {
            return ImageButtonSize(name, image, new Vector2(image.Width, image.Height));
        }

        /// <summary>
        /// Draws a texture as an image button in an ImGui context. Uses the current ImGui cursor position and the specified size.
        /// </summary>
        /// <param name="name">The display name and ImGui ID for the button</param>
        /// <param name="image">The texture to draw</param>
        /// <param name="size">The size of the button/param>
        /// <returns>True if the button was clicked</returns>
        public static bool ImageButtonSize(System.String name, Texture2D image, Vector2 size)
        {
            return ImGui.ImageButton(name, new IntPtr(image.Id), size);
        }

    }
}
