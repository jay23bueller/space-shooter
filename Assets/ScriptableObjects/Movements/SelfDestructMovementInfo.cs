using System.Collections;
using UnityEngine;
using static SelfDestructMovement;

[CreateAssetMenu(fileName = "SelfDestructMovementInfo.asset", menuName = "MovementInfo/SelfDestructMovementInfo")]
public class SelfDestructMovementInfo : MovementInfo
{

    public float distanceBeforeCharging;
    public float chargingDelay;

    public float pitchDelay;
    public float pitchIncrement;

    public float chargingAccelerationRate;
    public float defaultChargeSpeed;
    public float maxChargeSpeed;
}
