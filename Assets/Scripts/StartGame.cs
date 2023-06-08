using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StartGame : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //void Awake()
    //{
    //    GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    //}

    //void Destroy()
    //{
    //    GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    //}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        SceneManager.LoadScene("Home Menu");
    }

    public void onClickDay()
    {
        SceneManager.LoadScene("GameDay");
    }

    public void onClickNoon()
    {
        SceneManager.LoadScene("Paris");
    }

    public void onClickNight()
    {
        SceneManager.LoadScene("GameNight");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Day")
        {
            SceneManager.LoadScene("GameDay");
        }

        if (other.tag == "Noon")
        {
            SceneManager.LoadScene("GameNoon");
        }

        if (other.tag == "Night")
        {
            SceneManager.LoadScene("GameNight");
        }
    }
        
    //private void OnGameStateChanged(GameState newGameState)
    //{
    //    enabled = newGameState == newGameState.Gameplay;
    //}
    
}
