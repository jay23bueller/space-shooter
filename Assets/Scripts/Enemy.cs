using UnityEngine;


public class Enemy : MonoBehaviour
{
    #region Constants
    private const float LEFT_BOUND = 0.05f;
    private const float RIGHT_BOUND = 0.95f;
    private const float TOP_BOUND = 1.0f;
    private const float BOTTOM_BOUND = -0.01f;
    private const string PLAYER_TAG = "Player";
    private const string LASER_TAG = "Laser";
    private const string ENEMY_TAG = "Enemy";
    #endregion

    #region Variables
    [SerializeField]
    private float _speed = 4.0f;
    #endregion

    #region UnityMethods
    // Update is called once per frame
    void Update()
    {
        move();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null) {
            if(other.CompareTag(PLAYER_TAG))
                other.GetComponent<Player>().TakeDamage();
            

            if(other.CompareTag(LASER_TAG))
                Destroy(other.gameObject);
            

            if(!other.CompareTag(ENEMY_TAG))
                Destroy(gameObject);
        }
    }

    #endregion

    #region Methods

    //Move the character to the bottom and if it is out of the viewport, teleport it to the top
    //at a random x location
    private void move()
    {
        Vector3 currentViewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (currentViewportPosition.y < BOTTOM_BOUND)
            transform.position = Camera.main.ViewportToWorldPoint(
                new Vector3(Random.Range(LEFT_BOUND, RIGHT_BOUND),
                TOP_BOUND,
                currentViewportPosition.z));
        else
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
    }
    #endregion
}
