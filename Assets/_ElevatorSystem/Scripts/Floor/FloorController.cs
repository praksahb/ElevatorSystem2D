using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
    // Manages a single floor's hall call buttons (Up/Down) and floor label.
    // Wires button clicks to the ElevatorsManager and listens for status changes
    // to highlight/unhighlight the buttons when a lift is assigned or arrives.
    public class FloorController : MonoBehaviour
    {
        [Header("References for the Floor prefab")]
        [SerializeField] private TextMeshProUGUI _floorValueText;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;

        [Header("Using static floor generation for now while prototyping")]
        [SerializeField] private int _floorValue;

        private string _floorText;

        private Image _upBtnImage, _downBtnImage;

        private void Awake()
        {
            _upBtnImage = _upButton.GetComponent<Image>();
            _downBtnImage = _downButton.GetComponent<Image>();
        }

        private void OnEnable()
        {
            ElevatorsManager.OnFloorRequestStatusChanged += HandleFloorStatusChanged;
        }

        private void OnDisable()
        {
            ElevatorsManager.OnFloorRequestStatusChanged -= HandleFloorStatusChanged;
        }

        public void InitializeFloorContainer(int totalFloors)
        {
            InitFloor(totalFloors);
        }

        private void InitFloor(int totalFloors)
        {
            bool isGround = _floorValue == 0;
            bool isTopFloor = _floorValue == totalFloors - 1;

            _downButton.gameObject.SetActive(!isGround);
            _upButton.gameObject.SetActive(!isTopFloor);

            _floorText = (isGround) ? "G" : _floorValue.ToString();
            _floorValueText.SetText(_floorText);

            SubscribeToButtons(isGround, isTopFloor);
        }

        private void SubscribeToButtons(bool isGround, bool isTopFloor)
        {
            if (!isTopFloor)
            {
                _upButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Floor {_floorText} requested UP");
                    ElevatorsManager.Instance.RequestLift(_floorValue, Direction.Up);

                    _upBtnImage.color = Color.yellow;
                });
            }
            if (!isGround)
            {
                _downButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Floor {_floorText} requested DOWN");
                    ElevatorsManager.Instance.RequestLift(_floorValue, Direction.Down);

                    _downBtnImage.color = Color.yellow;
                });
            }
        }

        private void HandleFloorStatusChanged(int floor, Direction dir)
        {
            if (floor != _floorValue) return;

            Color targetColor = Color.white;

            if (dir == Direction.Up)
            {
                _upBtnImage.color = targetColor;
            }
            else if (dir == Direction.Down)
            {
                _downBtnImage.color = targetColor;
            }
            else if (dir == Direction.None)
            {
                // If None, we clear ALL buttons (this usually happens when a lift goes Idle)
                _upBtnImage.color = Color.white;
                _downBtnImage.color = Color.white;
            }
        }
    }
}