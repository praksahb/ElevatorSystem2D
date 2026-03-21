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
