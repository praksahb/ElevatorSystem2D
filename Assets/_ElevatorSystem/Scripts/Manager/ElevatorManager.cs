using UnityEngine;
using ElevatorSystem.Utils;
using System.Collections.Generic;
using System;

namespace ElevatorSystem
{
    public class ElevatorManager : GenericSingleton<ElevatorManager>
    {
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
        }


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

        public void ClearFloorRequest(int floor)
        {
            // Only remove and fire the event IF it actually exists
            if (_activeFloorRequests.Remove(floor))
            {
                OnFloorRequestStatusChanged?.Invoke(floor, Direction.Up, false);
            }
        }

        #region Config functions

        public float GetFloorPosition(int floor)
        {
            return _config.GetYPosition(floor);
        }

        public float GetDelayTimer()
        {
            return _config.DoorOpenDelay;
        }

        public float GetMoveSpeed() => _config.MovementSpeed;

        #endregion

    }
}
