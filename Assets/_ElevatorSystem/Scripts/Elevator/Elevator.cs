using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSystem
{
    // Each elevator manages its own request queue and movement.
    // Uses a SCAN algorithm to decide which floor to visit next based on direction.
    // Moves floor-by-floor so it can pick up new requests mid-trip.
    // Fires OnStateChanged so other scripts (VFX, ControlView) can react without polling.
    public class Elevator : MonoBehaviour
    {
        // other scripts subscribe to this to know when the lift changes state or floor
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
        private Direction _currentDirection = Direction.None;

        #endregion

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CurrentState = ElevatorState.Idle;
        }

        // used by ControlViewManager to check if a floor button should be highlighted
        public bool HasStop(int floorIndex) => RequestQueue.Contains(floorIndex);

        // called by ElevatorsManager.Start() to pass itself in, so we dont need to use Singleton lookups everywhere
        public void SetElevatorManager(ElevatorsManager manager) => _elevatorManager = manager;

        // Scores how "expensive" it would be for this lift to handle a request.
        // ElevatorsManager calls this on all lifts and picks the one with the lowest cost.
        public int CalculateCost(int requestedFloor, Direction callDirection)
        {
            // 0. fix on If we are ALREADY here and stopping, we are the best candidate!
            if (_currentFloor == requestedFloor && CurrentState == ElevatorState.StoppingAtFloor)
            {
                return 0;
            }

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

        // Adds a floor to the queue. If idle, kicks off the ProcessQueue coroutine.
        // If already moving, the new stop gets picked up automatically on the next loop.
        public void AddNewStop(int floorIndex)
        {
            // Add stop to the RequestQueue
            RequestQueue.Add(floorIndex);

            if (CurrentState == ElevatorState.Idle)
            {
                _processRoutine = StartCoroutine(ProcessQueue());
            }
        }

        // Main loop: keeps running while there are floors to visit.
        // Moves one floor at a time so we can intercept new stops mid-trip.
        private IEnumerator ProcessQueue()
        {
            while (RequestQueue.Count > 0)
            {
                // 1. Ask the SCAN algorithm where we're headed
                int targetFloor = GetNextFloorFromQueue();
                // 2. Move one floor at a time towards target
                int step = (targetFloor > _currentFloor) ? 1 : -1;

                while (_currentFloor != targetFloor)
                {
                    int nextFloor = _currentFloor + step;
                    Tween moveTween = MoveToFloor(nextFloor);

                    if (moveTween != null)
                        yield return moveTween.WaitForCompletion();

                    // Check: did we land on a floor someone requested? (Interception)
                    if (RequestQueue.Contains(_currentFloor))
                    {
                        // we stop, remove it and open doors
                        RequestQueue.Remove(_currentFloor);
                        _elevatorManager.ClearFloorRequest(_currentFloor);

                        CurrentState = ElevatorState.StoppingAtFloor;
                        yield return _doorOpenDelay ??= new WaitForSeconds(_elevatorManager.GetDelayTimer());

                        // After door closes, re-evaluate direction (queue may have changed)
                        if (RequestQueue.Count > 0)
                        {
                            targetFloor = GetNextFloorFromQueue();
                            step = (targetFloor > _currentFloor) ? 1 : -1;
                        }
                    }
                }

                // 4. Final arrival at target floor (or if we started here)
                if (RequestQueue.Contains(_currentFloor))
                {
                    RequestQueue.Remove(_currentFloor);
                    _elevatorManager.ClearFloorRequest(_currentFloor);

                    CurrentState = ElevatorState.StoppingAtFloor;
                    yield return _doorOpenDelay ??= new WaitForSeconds(_elevatorManager.GetDelayTimer());
                }
            }

            // 5. Queue is empty - go to sleep
            CurrentState = ElevatorState.Idle;
            _currentDirection = Direction.None;
        }

        // SCAN Algorithm: picks the next floor to visit.
        // If going up, picks the nearest floor above us. If going down, nearest below.
        // If nothing left in our direction, we reverse (turnaround).
        private int GetNextFloorFromQueue()
        {
            if (_currentDirection == Direction.Up || _currentDirection == Direction.None)
            {
                // sorted lowest to highest
                foreach (int floor in RequestQueue)
                {
                    if (floor >= _currentFloor)
                    {
                        CurrentState = ElevatorState.MovingUp;
                        _currentDirection = Direction.Up;
                        return floor;
                    }
                }

                // if nothing above us, turnaround
                _currentDirection = Direction.Down;
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
                        _currentDirection = Direction.Down;
                        return floor;
                    }
                }

                // If we found nothing below, turnaround
                _currentDirection = Direction.Up;
                CurrentState = ElevatorState.MovingUp;
                return RequestQueue.Min;
            }
        }

        // Tweens the lift to a single floor. Updates _currentFloor on arrival and
        // notifies the UI. Does NOT change the elevator state - that's handled by ProcessQueue.
        private Tween MoveToFloor(int floorIndex)
        {
            float targetY = _elevatorManager.GetFloorPosition(floorIndex);

            if (float.IsNaN(targetY))
            {
                Debug.LogError($"ElevatorManager returned NaN for floor {floorIndex}! Aborting lift request.");
                return null;
            }

            float speed = _elevatorManager.GetMoveSpeed();

            return _rectTransform.DOAnchorPosY(targetY, speed)
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .OnComplete(() =>
           {
               _currentFloor = floorIndex;
               // Notify UI that floor changed
               OnStateChanged?.Invoke(CurrentState); 
           });
        }
    }
}
