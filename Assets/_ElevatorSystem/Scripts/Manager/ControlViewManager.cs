using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ElevatorSystem
{
    public class ControlViewManager : MonoBehaviour
    {
        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI _currentFloorText;
        [SerializeField] private Image _arrowUp;
        [SerializeField] private Image _arrowDown;

        [Header("Internal Floor Buttons (Order: G, 1, 2, 3)")]
        [SerializeField] private Button[] _floorBtns;

        private FloorButtonUI[] _floorButtons;

        private Elevator _selectedElevator;

        private void Start()
        {
            _floorButtons = new FloorButtonUI[_floorBtns.Length];

            for (int i = 0; i < _floorBtns.Length; i++)
            {
                var floorBtnUI = _floorBtns[i].gameObject.AddComponent<FloorButtonUI>();
                floorBtnUI.FloorIndex = i;

                int floorIndex = i; // capture for closure
                _floorBtns[i].onClick.AddListener(() => AddInternalRequest(floorIndex));

                _floorButtons[i] = floorBtnUI;
            }
        }

        private void OnEnable()
        {
            ElevatorSelectorUI.OnElevatorSelected += HandleElevatorSelected;
        }

        private void OnDisable()
        {
            ElevatorSelectorUI.OnElevatorSelected -= HandleElevatorSelected;

            // Clean up: unsubscribe from the last elevator
            if (_selectedElevator != null)
                _selectedElevator.OnStateChanged -= HandleStateChanged;
        }

        private void HandleElevatorSelected(int index)
        {
            // 1. UNSUBSCRIBE from the old lift (prevent ghost events)
            if (_selectedElevator != null)
                _selectedElevator.OnStateChanged -= HandleStateChanged;

            // 2. SWAP to the new lift
            _selectedElevator = ElevatorsManager.Instance.GetElevator(index);

            // 3. SUBSCRIBE to the new lift
            if (_selectedElevator != null)
                _selectedElevator.OnStateChanged += HandleStateChanged;

            // 4. Immediately refresh everything for the new context
            RefreshDisplay();
            RefreshFloorButtons();
        }

        private void HandleStateChanged(ElevatorState newState)
        {
            // This fires ONLY when the selected elevator changes state
            RefreshDisplay();
            RefreshFloorButtons();
        }

        private void RefreshDisplay()
        {
            if (_selectedElevator == null) return;

            _currentFloorText.SetText($"Current Floor: {_selectedElevator.CurrentFloor}");

            // Arrows: colored when active, white when idle
            _arrowUp.color = (_selectedElevator.CurrentState == ElevatorState.MovingUp)
                ? Color.yellow : Color.white;
            _arrowDown.color = (_selectedElevator.CurrentState == ElevatorState.MovingDown)
                ? Color.yellow : Color.white;
        }

        private void RefreshFloorButtons()
        {
            if (_selectedElevator == null) return;

            foreach (var btn in _floorButtons)
            {
                btn.SetHighlight(_selectedElevator.HasStop(btn.FloorIndex));
            }
        }

        private void AddInternalRequest(int floorIndex)
        {
            if (_selectedElevator != null)
            {
                _selectedElevator.AddNewStop(floorIndex);
                RefreshFloorButtons(); // Immediately show the new stop
            }
        }
    }
}
