using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public bool isGameRunning = false;
    public bool isGamePaused = false;

    private GameInputActions gameInputActions;
    private InputAction startGame;
    private InputAction endGame;

    public UnityEvent OnGameStart;
    public UnityEvent OnGamePause;
    public UnityEvent OnGameResume;
    public UnityEvent OnGameEnd;
    public UnityEvent OnGameRestart;

    public GameObject TitleScreen;
    private void Awake()
    {
        gameInputActions = new GameInputActions();
    }

    private void OnEnable()
    {
        startGame = gameInputActions.Game.StartGame;
        endGame = gameInputActions.Game.EndGame;
        startGame.Enable();
        endGame.Enable();
        startGame.performed += StartGame;
        endGame.performed += EndGame;
    }


    void Start()
    {
        OnGameStart ??= new UnityEvent();
        OnGamePause ??= new UnityEvent();
        OnGameResume ??= new UnityEvent();
        OnGameEnd ??= new UnityEvent();
        OnGameRestart ??= new UnityEvent();
    }

    public void StartGame(InputAction.CallbackContext ctx)
    {
        if (isGameRunning)
        {
            return;
        }
        isGameRunning = true;
        isGamePaused = false;
        OnGameStart.Invoke();
        TitleScreen.SetActive(false);
    }

    public void EndGame(InputAction.CallbackContext ctx)
    {
        if (!isGameRunning)
        {
            // Close the game
            Application.Quit();
        }
        isGameRunning = false;
        isGamePaused = false;
        OnGameEnd.Invoke();
        TitleScreen.SetActive(true);
    }
}
