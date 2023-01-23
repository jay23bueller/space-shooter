using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonstratingDotProduct : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dotProduct = Vector3.Dot(transform.up, (_targetTransform.position - transform.position).normalized);
        Debug.Log("Angle between the up direction and the direction to the target: " + Mathf.Acos(dotProduct) * Mathf.Rad2Deg);
    }
}
