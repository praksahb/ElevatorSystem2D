using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
    // Simple color-only component for internal floor buttons (G, 1, 2, 3).
    // Added at runtime by ControlViewManager via AddComponent.
    // Doesn't know anything about elevators - just knows its floor index and how to change color.
    public class FloorButtonUI : MonoBehaviour
    {
        public int FloorIndex { get; set; }

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void SetHighlight(bool isActive)
        {
            _image.color = isActive ? Color.yellow : Color.white;
        }
    }
}
