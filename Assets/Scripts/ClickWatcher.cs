using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
public class ClickWatcher : MonoBehaviour
{
    public GameManager gm;
    private InputAction clickAction;
    private Cube currentCube;

    private bool isGameRunning = false;
    public bool handleCubeClicks = true;
    public UnityEvent<Cube> cubeClicked;

    private UnityEvent GameStart;
    private UnityEvent GameEnd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        clickAction.performed += OnClick;
        clickAction.Enable();
    }

    private void OnEnable()
    {
        clickAction.Enable();
    }
    private void OnDisable()
    {
        clickAction.Disable();
    }

    void Start()
    {
        cubeClicked ??= new UnityEvent<Cube>();
        GameStart ??= new UnityEvent();
        GameEnd ??= new UnityEvent();
        gm.OnGameStart.AddListener(OnGameStart);
        gm.OnGameEnd.AddListener(OnGameEnd);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameRunning) {
            if (currentCube != null)
            {
                currentCube.UnhighlightCube();
                currentCube = null;
            }
            return;
        }
        // Get the mouse position
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        // Get the first collider hit by a raycast from the mouse position out into the scene
        if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out RaycastHit hit))
        {
            // Get the Cube component off the object that was hit
            // If the object has a Cube component, toggle the isHighlighted property
            if (hit.collider.TryGetComponent<Cube>(out var cube))
            {
                if (currentCube != cube)
                {
                    if (currentCube != null)
                    {
                        currentCube.UnhighlightCube();
                    }
                    currentCube = cube;
                    currentCube.HighlightCube();
                }
            }
        }
        else
        {
            if (currentCube != null)
            {
                currentCube.UnhighlightCube();
                currentCube = null;
            }
            
        }
    }

    void OnClick(InputAction.CallbackContext context)
    {
        if (!isGameRunning) { return; }
        // Get the screen position the mouse clicked
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        // Get the first collider hit by a raycast from the mouse click position out into the scene
        if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out RaycastHit hit))
        {
            // Get the Cube component off the object that was hit
            // If the object has a Cube component, toggle the isHighlighted property
            if (hit.collider.TryGetComponent<Cube>(out var cube))
            {
                Debug.Log("Clicked on " + cube.gameObject.name);
                cubeClicked.Invoke(cube);
            }
        }
    }
    private IEnumerator WaitForGameStart()
    {
        yield return new WaitForSeconds(0.5f);
        isGameRunning = true;
    }

    private void OnGameEnd()
    {
        isGameRunning = false;
        if (currentCube != null)
        {
            currentCube.UnhighlightCube();
            currentCube = null;
        }
    }

    private void OnGameStart()
    {
        StartCoroutine(WaitForGameStart());
        if (currentCube != null)
        {
            currentCube.UnhighlightCube();
            currentCube = null;
        }
    }
}
