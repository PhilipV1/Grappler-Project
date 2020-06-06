using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : object
{
    public static GameState gameState;
    public enum GameState 
    {
        Playing,
        Paused
    }


}
