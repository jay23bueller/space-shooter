using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region Methods
    public void StartGame()
    {
        SceneManager.LoadScene(1); //Game
    }
    #endregion
}
