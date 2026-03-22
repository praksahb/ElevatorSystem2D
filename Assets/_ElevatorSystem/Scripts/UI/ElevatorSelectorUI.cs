using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
    public class ElevatorSelectorUI : MonoBehaviour
    {
        public static event Action<int> OnElevatorSelected;

        [SerializeField] private int _elevatorIndex;

        private Image _image;
        private Button _button;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();

            _button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            OnElevatorSelected += HighlightSelectedButton;
        }

        private void OnDisable()
        {
            OnElevatorSelected -= HighlightSelectedButton;
        }

        private void OnClick()
        {
            OnElevatorSelected?.Invoke(_elevatorIndex);
        }

        private void HighlightSelectedButton(int index)
        {
            if (index == _elevatorIndex)
            {
                _image.color = Color.green;
            } else
            {
                _image.color = Color.white;
            }
        }
    }
}
