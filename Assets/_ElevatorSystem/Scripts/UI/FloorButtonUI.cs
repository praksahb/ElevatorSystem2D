using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
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
