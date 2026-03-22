using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
    // Changes the elevator's color based on its state.
    // Cyan = Idle, Red = Moving, Green = Stopped at floor.
    // Subscribes to the Elevator's OnStateChanged event, so no Update() needed.
    // Can be upgraded if using actual Elevator animation like door opening and door closing...
    public class ElevatorVFX : MonoBehaviour
    {
        private Image _elevatorSprite;
        private Elevator _elevator;

        private void Awake()
        {
            _elevatorSprite = GetComponent<Image>();
            _elevator = GetComponent<Elevator>();
        }

        private void OnEnable()
        {
            _elevator.OnStateChanged += StateChangeVisualFX;
        }

        private void OnDisable()
        {
            _elevator.OnStateChanged -= StateChangeVisualFX;
        }

        private void StateChangeVisualFX(ElevatorState currentState)
        {
            _elevatorSprite.color = currentState switch
            {
                ElevatorState.Idle => Color.cyan,
                ElevatorState.MovingUp => Color.red,
                ElevatorState.MovingDown => Color.red,
                ElevatorState.StoppingAtFloor => Color.green,
                _ => Color.white,
            };
        }
    }
}
