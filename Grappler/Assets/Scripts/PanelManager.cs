using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{

    public GameObject pauseUI;
    public GameManager.GameState currentState;
    public bool cursorLocked;

    public GameObject playerObject;
    public GameObject cameraObject;

    // Start is called before the first frame update
    void Start()
    { 
        Cursor.lockState = CursorLockMode.Locked;
        pauseUI.SetActive(false);
        GameManager.gameState = GameManager.GameState.Playing;
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            cursorLocked = true;
        }
        else
        {
            cursorLocked = false;
        }
        currentState = GameManager.gameState;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.gameState == GameManager.GameState.Playing)
            {
                pauseUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                GameManager.gameState = GameManager.GameState.Paused;
                Time.timeScale = 0.0f;
            }
            else if (GameManager.gameState == GameManager.GameState.Paused)
            {
                pauseUI.SetActive(false);
                GameManager.gameState = GameManager.GameState.Playing;
                Cursor.lockState = CursorLockMode.Locked;
                // Need to fix cursor lock after resuming the game from pause menu. The cursor will not go back to locked state

                Time.timeScale = 1.0f;
            }
        }
    }
}
