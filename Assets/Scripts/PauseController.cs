//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PauseController : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    public void Pause()
//    {
//        GameState currentGameState = GameStateManager.Instance.CurrentGameState;
//        GameState newGameState = currentGameState == GameState.Gameplay
//            ? GameState.Paused
//            : GameState.Gameplay;

//        GameStateManager.Instance.SetState(newGameState);
//    }
//}
