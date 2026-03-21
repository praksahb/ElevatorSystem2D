using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
    public class FloorController : MonoBehaviour
    {
        [Header("References for the Floor prefab")]
        [SerializeField] private TextMeshProUGUI _floorValueText;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;

        [Header("Using static floor generation for now while prototyping")]
        [SerializeField] private int _floorValue;

        private string _floorText;

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
                    ElevatorManager.Instance.RequestLift(_floorValue, Direction.Up);
                });
            }
            if (!isGround)
            {
                _downButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Floor {_floorText} requested DOWN");
                    ElevatorManager.Instance.RequestLift(_floorValue, Direction.Down);
                });
            }
        }
    }
}