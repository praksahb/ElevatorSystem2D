using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSystem
{
    public class Elevator : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string LiftID; // keeping it directly here for now

        public ElevatorState CurrentState { get; set; } = ElevatorState.Idle;
        public int CurrentFloor { get; set; } = 0;

        public SortedSet<int> RequestQueue = new SortedSet<int>();

        private Coroutine _processRoutine;


        private RectTransform _rectTransform;
        private ElevatorManager _elevatorManager;
        private float _delayOnFloor;
        private object _doorOpenDelay;

        // The request queue logic will be built here

        private void Awake()
        {
            // Initialization

            _rectTransform = GetComponent<RectTransform>();
        }

        public int CalculateCost(int requestedFloor, Direction callDirection)
        {
            // 1. BASE COST: Raw Distance
            int cost = Mathf.Abs(CurrentFloor - requestedFloor);

            // 2. STATE PENALTY: Idle is best. If we are already moving, add a penalty.
            if (CurrentState != ElevatorState.Idle)
            {
                cost += 5;
            }

            // 3. WORKLOAD PENALTY: Don't give all the jobs to one lift.
            cost += (RequestQueue.Count * 2);

            // 4. CRITICAL CONFLICT PENALTIES (The deal-breakers)
            // If we are moving UP, but the requested floor is BELOW US, we cannot take the job mid-flight.
            if (CurrentState == ElevatorState.MovingUp && requestedFloor < CurrentFloor)
            {
                cost += 20;
            }
            // If we are moving DOWN, but the requested floor is ABOVE US.
            else if (CurrentState == ElevatorState.MovingDown && requestedFloor > CurrentFloor)
            {
                cost += 20;
            }

            // 5. DIRECTIONAL CONFLICT
            // If the passenger wants to go Down, but we are moving Up.
            if (CurrentState == ElevatorState.MovingUp && callDirection == Direction.Down)
            {
                cost += 20;
            }
            else if (CurrentState == ElevatorState.MovingDown && callDirection == Direction.Up)
            {
                cost += 20;
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
                int nextFloor = RequestQueue.Min;
                RequestQueue.Remove(nextFloor);


                // 2. Wait until the MoveToFloor Tween is finished.
                Tween moveTween = MoveToFloor(nextFloor);
                if (moveTween != null)
                {
                    yield return moveTween.WaitForCompletion();
                    CurrentFloor = nextFloor;
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

        private Tween MoveToFloor(int floorIndex)
        {
            if (_elevatorManager == null)
            {
                _elevatorManager = ElevatorManager.Instance;
            }

            float targetY = _elevatorManager.GetFloorPosition(floorIndex);

            if (float.IsNaN(targetY))
            {
                Debug.LogError($"ElevatorManager returned NaN for floor {floorIndex}! Aborting lift request.");
                return null;
            }
            // the timer of the tween also needs to be handled properly
            return _rectTransform.DOAnchorPosY(targetY, 2.0f).OnStart(() =>
           {
               // this needs to be handled as well for later on.
               CurrentState = ElevatorState.MovingUp;
           }).OnComplete(() =>
           {
               // yes this one seems correct, we only go to idle if the process queue is empty
               CurrentState = ElevatorState.StoppingAtFloor;
           });
        }

    }
}
