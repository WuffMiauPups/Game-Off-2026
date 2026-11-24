using UnityEngine;
using UnityEngine.UI;

namespace Voices.UI
{
    // Scales the UI by the same whole-number zoom as the Pixel Perfect Camera, so UI pixels and room pixels
    // always have the same size. 6 UI units = 1 game pixel (the UI is laid out for 1920x1080 = 320x180 x6).
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelPerfectCanvas : MonoBehaviour
    {
        public int gameWidth = 320;
        public int gameHeight = 180;
        public float unitsPerGamePixel = 6f;

        CanvasScaler scaler;

        void OnEnable() => Apply();
        void Update() => Apply();

        void Apply()
        {
            if (!scaler) scaler = GetComponent<CanvasScaler>();
            int zoom = Mathf.Max(1, Mathf.Min(Screen.width / gameWidth, Screen.height / gameHeight));
            float factor = zoom / unitsPerGamePixel;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            if (!Mathf.Approximately(scaler.scaleFactor, factor)) scaler.scaleFactor = factor;
        }
    }
}
