using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables
    private bool _isGameOver;
    #endregion
    #region UnityMethods

    // Update is called once per frame
    void Update()
    {
        RestartGame();
    }

    #endregion
    #region Methods
    private void RestartGame()
    {
        if(Input.GetKeyDown(KeyCode.R) && _isGameOver)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void GameOver()
    {
        _isGameOver = true;
    }
    #endregion
}
