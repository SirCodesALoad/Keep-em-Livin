using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class GameMechanicManager : MonoBehaviour
{

    //a bunch of ui variables to control game state.
    public Text GameTextBox;
    public Button resetGameButton;


    public WaveSpawner waveSpawner;
    public GameState gameState = GameState.BetweenWaves;

    public readonly int  maxNumOfWaves = 3;
    public int enemiesRemaining = 0;
    public float gracePeroidBetweenWaves = 10f;
    public float gracePeroidTimer = 10f;


    ControlActions actions;

    InputDevice device;
    InputControl control;

    void Start()
    {
        device = InputManager.ActiveDevice;
        control = device.GetControl(InputControlType.Action1);
        actions = new ControlActions();
        actions.restartGame.AddDefaultBinding(Key.T);

        KeepEmLivinEvents.enemyDeath.AddListener(EnemyDeath);
        KeepEmLivinEvents.playerDeath.AddListener(PlayerDeath);
        StartCoroutine("HideOnDelay");

    }



    // Update is called once per frame
    void Update()
    {
        switch (gameState)
        {
            case GameState.BetweenWaves:
                if (gracePeroidTimer <= 0)
                {
                    gracePeroidTimer = gracePeroidBetweenWaves;
                    waveSpawner.GenerateWave();
                    gameState = GameState.OnGoingWave;

                }
                else
                {
                    gracePeroidTimer -= Time.fixedDeltaTime;
                }
                break;
            case GameState.OnGoingWave:
                break;
            case GameState.GameOver:
                if (actions.restartGame.WasPressed)
                {
                    Restart();

                }
                break;
        }
    }

    public void PlayerDeath()
    {

        gameState = GameState.GameOver;
        GameTextBox.enabled = true;
        GameTextBox.text = "You Failed to Repel the Demons";


    }

    public void EnemyDeath()
    {
        enemiesRemaining--;
        if (enemiesRemaining <= 0)
        {
            OnEndOfWave();
        }
    }

    public void OnEndOfWave()
    {
        gameState = GameState.BetweenWaves;
        if (waveSpawner.currWave >= maxNumOfWaves)
        {
            //You win!
            gameState = GameState.GameOver;
            OnEndGame();
            return;
        }
        GameTextBox.enabled = true;
        GameTextBox.text = "Wave: " + waveSpawner.currWave + " Begins shortly.";
        StartCoroutine("HideOnDelay");
    }

    private IEnumerator HideOnDelay()
    {

        float progress = 0.0f;

        while (progress <= gracePeroidBetweenWaves / 2)
        {
            progress += Time.deltaTime;

            yield return null;
        }
        GameTextBox.enabled = false;

    }

    public void OnEndGame()
    {
        GameTextBox.enabled = true;
        GameTextBox.text = "You Successfuly Repelled The Demons";

    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

}

public enum GameState
{
    BetweenWaves,OnGoingWave, GameOver
}

public static class KeepEmLivinEvents
{
    public static UnityEvent enemyDeath = new UnityEvent();
    public static UnityEvent playerDeath = new UnityEvent();

}