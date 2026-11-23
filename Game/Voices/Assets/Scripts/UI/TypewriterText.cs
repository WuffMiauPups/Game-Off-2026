using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voices.UI
{
    public class TypewriterText : MonoBehaviour
    {
        public Text target;
        public float charsPerSecond = 32f;

        public Action<char> onChar;

        string full;
        float shown;
        float speed;

        public bool IsDone => full == null || shown >= full.Length;

        // speed 0 = the default charsPerSecond
        public void Play(string text, float speed = 0f)
        {
            this.speed = speed > 0f ? speed : charsPerSecond;
            full = text ?? "";
            shown = 0f;
            target.text = "";
        }

        public void Complete()
        {
            if (full == null) return;
            shown = full.Length;
            target.text = full;
        }

        public void Stop()
        {
            full = null;
        }

        void Update()
        {
            if (IsDone) return;
            int before = (int)shown;
            shown = Mathf.Min(full.Length, shown + Time.deltaTime * speed);
            int after = (int)shown;
            for (int i = before; i < after; i++) onChar?.Invoke(full[i]);
            target.text = full.Substring(0, after);
        }
    }
}
