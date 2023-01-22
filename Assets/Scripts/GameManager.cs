using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables
    private bool _isGameOver;
    public static readonly float LEFT_BOUND = -10.63f;
    public static readonly float RIGHT_BOUND = 10.63f;
    public static readonly float ENVIRONMENT_TOP_BOUND = 8f;
    public static readonly float ENVIRONMENT_BOTTOM_BOUND = -6f;
    public static readonly float PLAYER_BOTTOM_BOUND = -4.84f;
    public static readonly float PLAYER_TOP_BOUND = 0f;
    public static readonly float SPAWN_LEFTRIGHT_OFFSET = .7f;
    #endregion
    #region UnityMethods

    // Update is called once per frame
    void Update()
    {
        RestartGame();
        QuitGame();
    }

    #endregion
    #region Methods
    private void RestartGame()
    {
        if(Input.GetKeyDown(KeyCode.R) && _isGameOver)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    public void GameOver()
    {
        _isGameOver = true;
    }
    #endregion
}
