using UnityEngine;

[CreateAssetMenu(fileName ="waypointPath.asset", menuName = "SpawnInformation/WaypointPath")]
public class WaypointPathInfo : ScriptableObject
{
    public Vector3[] waypoints;
}
