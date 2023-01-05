using UnityEngine;


public class MovementTesting : MonoBehaviour
{
    private MovementMode _currentMovementMode;
    private delegate Vector3 Move(int direction);
    private Move _currentMovement;
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float _rise;
    [SerializeField]
    private float _run;
    [SerializeField]
    private float _AtAnAngleHorizontalDelta = 2f;
    private float _currentAngle = 0f;
    // Start is called before the first frame update
    void Start()
    {
        _currentMovement = MoveHorizontally;
    }

    private Vector3 MoveHorizontally(int direction)
    {
        return (Vector3.right * direction * _speed * Time.deltaTime);
    }

    private Vector3 MoveVertically(int direction)
    {
        return Vector3.up * direction * _speed * Time.deltaTime;
    }

    private Vector3 MoveCircular(int direction)
    {
        _currentAngle = ((_currentAngle + 1f) * Time.deltaTime) % 360f;
        Debug.Log(_currentAngle);
        Vector3 displacement =  new Vector3(Mathf.Cos(_currentAngle * Mathf.Deg2Rad),Mathf.Sin(_currentAngle * Mathf.Deg2Rad))* Time.deltaTime;

        return (displacement + transform.position) - transform.position;
    }

    private Vector3 MoveAtAnAngle(int direction)
    {
        //mx+b = y
        float x = (_AtAnAngleHorizontalDelta * direction * Time.deltaTime);
        
        Vector3 delta = new Vector3(x, (_rise / _run) * x);
        return delta;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            _currentMovement = MoveHorizontally;
        if (Input.GetKeyDown(KeyCode.S))
            _currentMovement = MoveVertically;
        if (Input.GetKeyDown(KeyCode.D))
        {
            _currentMovement = MoveAtAnAngle;
        }
        if (Input.GetKeyDown(KeyCode.W))
            _currentMovement = MoveCircular;
            
        
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.Translate(_currentMovement(1));
    }
}
