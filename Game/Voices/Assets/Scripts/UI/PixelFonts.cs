using UnityEngine;

namespace Voices.UI
{
    // Keeps the pixel fonts sharp: their textures must not be smoothed when scaled.
    [ExecuteAlways]
    public class PixelFonts : MonoBehaviour
    {
        public Font[] fonts;

        void OnEnable()
        {
            Apply(null);
            Font.textureRebuilt += Apply;
        }

        void OnDisable() => Font.textureRebuilt -= Apply;

        void Apply(Font rebuilt)
        {
            if (fonts == null) return;
            foreach (var f in fonts)
                if (f && f.material && f.material.mainTexture) f.material.mainTexture.filterMode = FilterMode.Point;
        }
    }
}
