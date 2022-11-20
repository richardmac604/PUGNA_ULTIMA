using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// all game states during a game match
public enum GameStates { Waiting, Starting, Running, GameOver };

/// <summary>
/// Static GameManager Class to handle Game States, Timers and Win/Lose Logic in a Game Match.
/// Author(s): Jason Cheung
/// Date: Oct 27 2022
/// Source(s):
///     Countdown - How to create a 2D Arcade Style Top Down Car Controller in Unity tutorial Part 13: https://youtu.be/-SR24s7AryI?t=1560
/// Remarks:
/// Change History: Nov 11 2022 - Jason Cheung
/// - Modified methods to be networked and use rpc calls
/// - 
/// - bugfix: shows on both host & client instances
/// </summary>
public class GameManager : NetworkBehaviour
{
    // Static instance of GameManager so other scripts can access it
    public static GameManager Manager = null;

    // other scene objects to reference
    protected NetworkFighterObserver _networkPlayerObserver;

    // Current Game State
    public GameStates GameState { get; private set; } = GameStates.Waiting;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        Manager = this;
        Debug.Log("GameManager instance awake: " + Manager);
    }

    // Start is called after Awake, and before Update
    public void Start()
    {
        CacheOtherObjects();
    }

    // Helper method to initialize OTHER game objects and their components
    private void CacheOtherObjects()
    {
        if (!_networkPlayerObserver) _networkPlayerObserver = NetworkFighterObserver.Observer;
    }

    // Set Game State to waiting
    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    public void RPC_SetGameStateWaiting()
    {
        if (GameState != GameStates.Waiting)
        {
            GameState = GameStates.Waiting;
            Debug.Log("GameManager state is: " + GameState.ToString());
        }
    }

    // Set Game State to countdown
    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    public void RPC_SetGameStateStarting()
    {
        if (GameState != GameStates.Starting)
        {
            GameState = GameStates.Starting;
            Debug.Log("GameManager state is: " + GameState.ToString());

            RPC_OnGameStateStarting();
            // TODO: (not in this method) disable player input until this countdown is finished
        }
    }

    // Set Game State to running
    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    public void RPC_SetGameStateRunning()
    {
        if (GameState != GameStates.Running)
        {
            GameState = GameStates.Running;
            Debug.Log("GameManager state is: " + GameState.ToString());

            RPC_OnGameStateRunning();
        }
    }

    // Set Game State to gameOver
    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    public void RPC_SetGameStateGameOver()
    {
        if (GameState != GameStates.GameOver)
        {
            GameState = GameStates.GameOver;
            Debug.Log("GameManager state is: " + GameState.ToString());

            RPC_OnGameStateGameOver();
        }
    }

    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    protected void RPC_OnGameStateStarting()
    {
        // start the starting countdown
        CountdownController.Instance.RPC_StartStartingCountdown();
    }

    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    protected void RPC_OnGameStateRunning()
    {
        // start the game timer
        GameTimerController.Instance.RPC_StartTimer();

        StartCoroutine(GameRunningCheck());
    }

    IEnumerator GameRunningCheck()
    {
        while (GameState == GameStates.Running)
        {
            // TODO:
            // perform win/lose checks (a player's stock reaches zero)

            // check if a player has left

            // if so, forcibly end the game

            // perform this co-routine check every .5 seconds
            yield return new WaitForSeconds(1f);
        }

        // stop this check since gamestate has changed
        StopCoroutine(GameRunningCheck());
    }

    [Rpc(sources: RpcSources.All, RpcTargets.All)]
    protected void RPC_OnGameStateGameOver()
    {
        // TODO:
        // - disable player input once game is over (not in this method?)
        // - trigger endGame state to end the game, gather win/lose results, send info to database, and move to the next screen.

        StartCoroutine(GameOverCheck());
    }

    IEnumerator GameOverCheck()
    {
        while (GameState == GameStates.GameOver)
        {
            yield return new WaitForSeconds(3.0f);

            Debug.Log("returning to Main Menu...");
            SceneManager.LoadScene("Main Menu");
        }

        // stop this check since gamestate has changed
        StopCoroutine(GameOverCheck());
    }

}
