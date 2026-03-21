using ElevatorSystem.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSystem
{
    public class FloorsManager : GenericSingleton<FloorsManager>
    {
        [SerializeField] private List<FloorController> _floors;

        public void InitializeFloors(int totalFloors)
        {
            foreach (var floor in _floors)
            {
                floor.InitializeFloorContainer(totalFloors);
            }
        }
    }
}