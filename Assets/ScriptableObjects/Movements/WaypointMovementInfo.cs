using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WaypointMovementInfo.asset", menuName = "MovementInfo/WaypointMovementInfo")]
public class WaypointMovementInfo : MovementInfo
{
    public float distanceFromNextPoint;
    public Vector3[] waypoints;
}
