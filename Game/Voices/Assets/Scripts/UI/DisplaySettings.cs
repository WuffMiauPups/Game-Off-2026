using System.Collections.Generic;
using UnityEngine;

namespace Voices.UI
{
    // Display options from the options menu: fullscreen or window, window size, frame rate limit.
    // Saved on the player's PC and applied again at every start.
    public static class DisplaySettings
    {
        const int GameWidth = 320;
        const int GameHeight = 180;

        // 0 = VSync (follows the monitor), -1 = no limit, otherwise frames per second.
        public static readonly int[] FpsChoices = { 0, 30, 60, 120, 144, 240, -1 };

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt("display_fullscreen", 1) == 1;
            set { PlayerPrefs.SetInt("display_fullscreen", value ? 1 : 0); Apply(); }
        }

        // Window size as a whole multiple of the game's 320 x 180 pixels, so the pixel art stays even.
        public static int WindowZoom
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("display_zoom", DefaultZoom), MinZoom, MaxZoom);
            set { PlayerPrefs.SetInt("display_zoom", Mathf.Clamp(value, MinZoom, MaxZoom)); Apply(); }
        }

        public static int FpsLimit
        {
            get => PlayerPrefs.GetInt("display_fps", 0);
            set { PlayerPrefs.SetInt("display_fps", value); Apply(); }
        }

        public const int MinZoom = 2;

        static int MonitorWidth => Display.main != null ? Display.main.systemWidth : Screen.currentResolution.width;
        static int MonitorHeight => Display.main != null ? Display.main.systemHeight : Screen.currentResolution.height;

        // Largest window that still fits on the monitor.
        public static int MaxZoom => Mathf.Max(MinZoom, Mathf.Min(MonitorWidth / GameWidth, MonitorHeight / GameHeight));

        // One step smaller than the monitor, so the window frame and task bar have room.
        static int DefaultZoom => Mathf.Max(MinZoom, MaxZoom - 1);

        public static string WindowSizeText(int zoom) => (GameWidth * zoom) + " x " + (GameHeight * zoom);

        public static List<int> ZoomChoices()
        {
            var list = new List<int>();
            for (int z = MinZoom; z <= MaxZoom; z++) list.Add(z);
            return list;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyAtStart()
        {
            // Nothing saved yet: keep what the game starts with.
            if (PlayerPrefs.HasKey("display_fullscreen") || PlayerPrefs.HasKey("display_zoom")) ApplyScreen();
            ApplyFps();
        }

        public static void Apply()
        {
            ApplyScreen();
            ApplyFps();
        }

        static void ApplyScreen()
        {
#if !UNITY_EDITOR
            if (Fullscreen) Screen.SetResolution(MonitorWidth, MonitorHeight, FullScreenMode.FullScreenWindow);
            else Screen.SetResolution(GameWidth * WindowZoom, GameHeight * WindowZoom, FullScreenMode.Windowed);
#endif
        }

        static void ApplyFps()
        {
            int fps = FpsLimit;
            QualitySettings.vSyncCount = fps == 0 ? 1 : 0;
            Application.targetFrameRate = fps > 0 ? fps : -1;
        }
    }
}
