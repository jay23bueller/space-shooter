
using UnityEngine;


[CreateAssetMenu(fileName = "CircularMovementInfo.asset", menuName = "MovementInfo/CircularMovementInfo")]
public class CircularMovementInfo : MovementInfo
{

    public float distanceFromTargetPosition;
    public float distanceFromSlant;
    public float leftSlantedDirection;
    public float rightSlantedDirection;
    public float circularRotationSpeed;
    public float radius;
}
