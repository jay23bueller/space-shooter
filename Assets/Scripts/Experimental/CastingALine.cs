using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastingALine : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private List<Vector3> _positions = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (_positions.Count == 0)
                _positions.Add(transform.position);
            else
                _positions.Add(transform.right * 2f + _positions[_positions.Count - 1]);

            _lineRenderer.positionCount = _positions.Count;

            for (int i = 0; i < _positions.Count; i++)
            {
                _lineRenderer.SetPosition(i, _positions[i]);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            _positions.Clear();
            _lineRenderer.positionCount = 0;
        }
    }
}
