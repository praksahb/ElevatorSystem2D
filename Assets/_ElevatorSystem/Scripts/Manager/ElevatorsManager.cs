using UnityEngine;
using ElevatorSystem.Utils;
using System.Collections.Generic;
using System;

namespace ElevatorSystem
{
    // Central manager / Dispatcher / Mediator for the elevator system.
    // Handles external Request Lift Calls by scoring all lifts and assigning the cheapest one.
    // Also provides shared config values (from ElevatorConfig SO) to all elevators.
    public class ElevatorsManager : GenericSingleton<ElevatorsManager>
    {
        // FloorControllers listen to this to highlight/unhighlight their call buttons
        public static event Action<int, Direction, bool> OnFloorRequestStatusChanged;


        [Header("Configuration")]
        [SerializeField] private ElevatorConfig _config;

        [SerializeField] private List<Elevator> _elevators;

        // This will hold active calls to prevent redundant lift assignment
        private HashSet<int> _activeFloorRequests = new();


        private void Start()
        {
            // initializing Floors data 
            FloorsManager.Instance.InitializeFloors(_config.TotalFloors);

            // initialize Ref. in Elevators
            foreach (var elevator in _elevators)
            {
                elevator.SetElevatorManager(this);
            }
        }

        public Elevator GetElevator(int index)
        {
            if (index < 0 || index >= _elevators.Count)
            {
                Debug.LogError("Check index, buttons maybe added incorrectly in ControlViewUIManager");
                return null;
            }
            return _elevators[index];
        }


        // called by FloorController when someone presses a button on the floor, 
        // loops through all lifts, scores them, and assigns the cheapest one.
        public void RequestLift(int floor, Direction direction)
        {
            // 1. If someone is already going there, ignore the click!
            if (_activeFloorRequests.Contains(floor)) return;

            Elevator bestLift = null;
            int lowestCost = int.MaxValue;

            foreach (var elevator in _elevators)
            {
                int cost = elevator.CalculateCost(floor, direction);

                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    bestLift = elevator;
                }
            }

            if (bestLift != null)
            {
                _activeFloorRequests.Add(floor);
                bestLift.AddNewStop(floor);
                OnFloorRequestStatusChanged?.Invoke(floor, direction, true);
            }
        }

        // called by Elevator when it arrives at a floor, clears the active request
        public void ClearFloorRequest(int floor)
        {
            // Only remove and fire the event IF it actually exists
            if (_activeFloorRequests.Remove(floor))
            {
                OnFloorRequestStatusChanged?.Invoke(floor, Direction.Up, false);
            }
        }

        #region Config Getters - all values come from the ElevatorConfig ScriptableObject

        public float GetFloorPosition(int floor) => _config.GetYPosition(floor);

        public float GetDelayTimer() => _config.DoorOpenDelay;

        public float GetMoveSpeed() => _config.MovementSpeed;

        public int GetDistanceWeight() => _config.DistanceWeight;
        public int GetBusyPenalty() => _config.BusyPenalty;
        public int GetWorkloadWeight() => _config.WorkloadWeight;
        public int GetConflictPenalty() => _config.ConflictPenalty;

        #endregion

    }
}
