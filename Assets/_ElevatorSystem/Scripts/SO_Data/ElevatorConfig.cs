using UnityEngine;

namespace ElevatorSystem
{
    [CreateAssetMenu(fileName = "NewElevatorConfig", menuName = "ElevatorSystem/Elevator Config")]
    public class ElevatorConfig : ScriptableObject
    {
        [Header("Building Data")]
        [Tooltip("The total number of floors in your simulation.")]
        public int TotalFloors = 4;
        
        [Tooltip("The physical drag-and-drop Y coordinates of your floors on the Canvas.")]
        public float[] FloorYPositions; // Fill in the Inspector later

        [Header("Movement Data")]
        [Tooltip("How fast the elevator translates between floors (in seconds).")]
        public float MovementSpeed = 2.0f;
        
        [Tooltip("How long the state machine should pause to simulate doors opening/closing.")]
        public float DoorOpenDelay = 2.0f;
        
        [Header("Dispatcher Penalties")]
        public int DistanceWeight = 2;   // Multiplier for distance
        public int BusyPenalty = 5;     // Flat penalty for being non-Idle
        public int WorkloadWeight = 2;  // Penalty per floor in queue
        public int ConflictPenalty = 20; // Penalty for wrong direction



        public float GetYPosition(int floorIndex)
        {
            if (floorIndex < 0 || floorIndex > FloorYPositions.Length - 1)
            {
                return float.NaN;
            }

            return FloorYPositions[floorIndex];
        }
    }
}
