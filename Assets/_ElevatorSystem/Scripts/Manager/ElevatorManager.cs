using UnityEngine;
using ElevatorSystem.Utils;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class ElevatorManager : GenericSingleton<ElevatorManager>
    {
        [Header("Configuration")]
        [SerializeField] private ElevatorConfig _config;

        [SerializeField] private List<Elevator> _elevators;
        
        // This will hold active calls to prevent redundant lift assignment
        private HashSet<int> _activeHallCalls = new HashSet<int>();


        private void Start()
        {
            // initializing Floors data 
            FloorsManager.Instance.InitializeFloors(_config.TotalFloors);
        }


        public void RequestLift(int floor, Direction direction)
        {
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
                bestLift.AddNewStop(floor);
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

        #endregion

    }
}
