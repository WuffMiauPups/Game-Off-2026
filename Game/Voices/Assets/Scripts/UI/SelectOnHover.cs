using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Voices.UI
{
    // The mouse moves the selection, so exactly one button is lit: the one under the mouse or the one
    // reached with the keyboard. (A selected button does not show its hover colour on its own.)
    [RequireComponent(typeof(Selectable))]
    public class SelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            var selectable = GetComponent<Selectable>();
            if (selectable.IsInteractable()) selectable.Select();
        }
    }
}
