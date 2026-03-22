using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSystem
{
    public class Elevator : MonoBehaviour
    {
        public event Action<ElevatorState> OnStateChanged;

        #region Public Properties
        public ElevatorState CurrentState
        {
            get => _currentState;

            set
            {
                _currentState = value;
                OnStateChanged?.Invoke(CurrentState);
            }
        }
        public int CurrentFloor => _currentFloor;

        #endregion

        #region Private Variables

        [Header("Identity")]
        [SerializeField] private string LiftID;

        private int _currentFloor = 0;
        //private float _delayOnFloor;
        private object _doorOpenDelay;

        private RectTransform _rectTransform;
        private ElevatorsManager _elevatorManager;

        private Coroutine _processRoutine;
        private SortedSet<int> RequestQueue = new SortedSet<int>();
        private ElevatorState _currentState = ElevatorState.Idle;

        #endregion

        private void Awake()
        {
            // Initialization
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CurrentState = ElevatorState.Idle;
        }

        public void SetElevatorManager(ElevatorsManager manager) => _elevatorManager = manager; 

        public int CalculateCost(int requestedFloor, Direction callDirection)
        {
            // 1. BASE COST: Raw Distance
            int cost = Mathf.Abs(_currentFloor - requestedFloor) * _elevatorManager.GetDistanceWeight();

            // 2. STATE PENALTY: Idle is best. If we are already moving, add a penalty.
            if (CurrentState != ElevatorState.Idle)
            {
                cost += _elevatorManager.GetBusyPenalty();
            }

            // 3. WORKLOAD PENALTY: Don't give all the jobs to one lift.
            cost += (RequestQueue.Count * _elevatorManager.GetWorkloadWeight());

            // 4. CRITICAL CONFLICT PENALTIES (The deal-breakers)
            // If we are moving UP, but the requested floor is BELOW US, we cannot take the job mid-flight.
            if (CurrentState == ElevatorState.MovingUp && requestedFloor < _currentFloor)
            {
                cost += _elevatorManager.GetConflictPenalty();
            }
            // If we are moving DOWN, but the requested floor is ABOVE US.
            else if (CurrentState == ElevatorState.MovingDown && requestedFloor > _currentFloor)
            {
                cost += _elevatorManager.GetConflictPenalty();
            }

            // 5. DIRECTIONAL CONFLICT
            // If the passenger wants to go Down, but we are moving Up.
            if (CurrentState == ElevatorState.MovingUp && callDirection == Direction.Down)
            {
                cost += _elevatorManager.GetConflictPenalty();
            }
            else if (CurrentState == ElevatorState.MovingDown && callDirection == Direction.Up)
            {
                cost += _elevatorManager.GetConflictPenalty();
            }

            return cost;
        }

        public void AddNewStop(int floorIndex)
        {
            // Add stop to the RequestQueue
            RequestQueue.Add(floorIndex);

            if (CurrentState == ElevatorState.Idle)
            {
                _processRoutine = StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            while (RequestQueue.Count > 0)
            {
                // 1. Get the next destination
                int nextFloor = GetNextFloorFromQueue();

                RequestQueue.Remove(nextFloor);

                // 2. Wait until the MoveToFloor Tween is finished.
                Tween moveTween = MoveToFloor(nextFloor);
                if (moveTween != null)
                {
                    yield return moveTween.WaitForCompletion();
                    _currentFloor = nextFloor;

                    _elevatorManager.ClearFloorRequest(_currentFloor);
                }

                // 3. Door open delay...
                if (_doorOpenDelay == null)
                {
                    float delayTime = _elevatorManager.GetDelayTimer();
                    _doorOpenDelay = new WaitForSeconds(delayTime);
                }
                yield return _doorOpenDelay;
            }

            // 4. The queue is completely empty. Go back to sleep.
            CurrentState = ElevatorState.Idle;
        }

        // SCAN Algorithm
        private int GetNextFloorFromQueue()
        {
            if (CurrentState == ElevatorState.MovingUp || CurrentState == ElevatorState.Idle)
            {
                // sorted lowest to highest
                foreach (int floor in RequestQueue)
                {
                    if (floor >= _currentFloor)
                    {
                        CurrentState = ElevatorState.MovingUp;
                        return floor;
                    }
                }

                // if nothing CurrentFloor is max
                CurrentState = ElevatorState.MovingDown;
                return RequestQueue.Max;
            }
            else
            {
                // Moving Down, sorting highest to lowest
                foreach (int floor in RequestQueue.Reverse())
                {
                    if (floor <= _currentFloor)
                    {
                        CurrentState = ElevatorState.MovingDown;
                        return floor;
                    }
                }

                // If we found nothing below
                CurrentState = ElevatorState.MovingUp;
                return RequestQueue.Min;
            }
        }

        private Tween MoveToFloor(int floorIndex)
        {
            float targetY = _elevatorManager.GetFloorPosition(floorIndex);

            if (float.IsNaN(targetY))
            {
                Debug.LogError($"ElevatorManager returned NaN for floor {floorIndex}! Aborting lift request.");
                return null;
            }

            float speed = _elevatorManager.GetMoveSpeed();

            return _rectTransform.DOAnchorPosY(targetY, speed).SetSpeedBased()
                .OnComplete(() =>
           {
               // yes this one seems correct, we only go to idle if the process queue is empty
               CurrentState = ElevatorState.StoppingAtFloor;
           });
        }

    }
}
