using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSystem
{
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
