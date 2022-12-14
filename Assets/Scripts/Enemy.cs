using UnityEngine;


public class Enemy : MonoBehaviour
{
    #region Constants
    private const string PLAYER_TAG = "Player";
    private const string LASER_TAG = "Laser";
    private const string ENEMY_TAG = "Enemy";
    #endregion

    #region Variables
    public readonly static float LEFT_BOUND = 0.05f;
    public readonly static float RIGHT_BOUND = 0.95f;
    public readonly static float TOP_BOUND = 1.05f;
    public readonly static float BOTTOM_BOUND = -0.05f;
    [SerializeField]
    private float _speed = 4.0f;
    private Rigidbody2D _rigidbody;
    #endregion

    #region UnityMethods



    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

    }


    void FixedUpdate()
    {
        move();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            if (other.CompareTag(PLAYER_TAG))
                other.GetComponent<Player>().TakeDamage();


            if (other.CompareTag(LASER_TAG))
                Destroy(other.gameObject);


            if (!other.CompareTag(ENEMY_TAG))
                Destroy(gameObject);
        }
    }

    #endregion

    #region Methods

    //Move the character to the bottom and if it is out of the viewport, teleport it to the top
    //at a random x location
    private void move()
    {
        Vector3 currentViewportPosition = Camera.main.WorldToViewportPoint(_rigidbody.position);
        if (currentViewportPosition.y < BOTTOM_BOUND)
        {

            _rigidbody.position = Camera.main.ViewportToWorldPoint(
                new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND),
                TOP_BOUND));

        }
        else
        {
            _rigidbody.MovePosition(_rigidbody.position + (Vector2.down * _speed * Time.deltaTime));
        }

    }
    #endregion
}
