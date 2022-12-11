using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Constants
    private const string VERTICAL_AXIS = "Vertical";
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const float LEFT_BOUND = -0.01f;
    private const float RIGHT_BOUND = 1.02f;
    private const float TOP_BOUND = 0.5f;
    private const float BOTTOM_BOUND = 0.05f;
    #endregion

    #region Variables
    [SerializeField]
    private float _speed = 3.5f;

    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
    }

    // Update is called once per frame
    void Update()
    {
        moveCharacter();
    }
    #endregion

    #region Methods

    //Move the character based on input within the viewport
    private void moveCharacter()
    {

        Vector3 verticalDirection = Vector3.up * Input.GetAxis(VERTICAL_AXIS) * _speed * Time.deltaTime;
        Vector3 horizontalDirection = Vector3.right * Input.GetAxis(HORIZONTAL_AXIS) * _speed * Time.deltaTime;

        Vector3 nextVerticalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);
        Vector3 nextHorizontalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);

        //If the character's new position is within the top and bottom bounds, then move it
        if (nextVerticalViewportPosition.y < TOP_BOUND && nextVerticalViewportPosition.y > BOTTOM_BOUND)
            transform.Translate(verticalDirection);

        //If the character's new position is outside the left or right bounds, teleport the character
        if (nextHorizontalViewportPosition.x > RIGHT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0f, nextHorizontalViewportPosition.y, nextHorizontalViewportPosition.z));
        }
        else if (nextHorizontalViewportPosition.x < LEFT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1f, nextHorizontalViewportPosition.y, nextHorizontalViewportPosition.z));
        }

        transform.Translate(horizontalDirection);

    }
    #endregion
}
