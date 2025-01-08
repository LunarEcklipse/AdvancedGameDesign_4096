using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Collections;

public enum CubeMapping
{
    LANTERN,
    SHIP,
    NONE
}
public class CubeMap : MonoBehaviour
{
    public float rotationRampUpTime = 0.5f; // How long it takes for the rotation to reach full speed.
    public float rotationDegreesPerSecond = 60.0f;
    public int chunkSize = 16;
    public Material baseCubeMaterial;
    public Material highlightCubeMaterial;

    private Cube[,,] cubes;

    private GameInputActions gameInputActions;
    private InputAction rotateCubeX;
    private InputAction rotateCubeY;

    private bool isRotatingX = false;
    private bool isRotatingY = false;

    private float currentRotationSpeedX = 0.0f;
    private float currentRotationSpeedY = 0.0f;

    private bool transformationNeedsBake = false;

    private GameObject rotationParent;
    public GameObject effectCubes;
    public GameObject lanternPrefab;
    public GameObject boatPrefab;
    [System.NonSerialized]public bool hasAnimationPlayed = false;
    
    public GameManager gm;
    public AudioClip cubeClickSound;
    private List<CubeStruct> lanternCubes;
    public List<CubeStruct> boatCubes = new();
    public List<CubeStruct> coinCubes = new();
    private int brokenCubesToWin = 0;

    private bool isGameRunning = false;
    private bool isGamePaused = false;
    private CubeMapping lastMap = CubeMapping.NONE;

    [SerializeField]private ClickWatcher clickWatcher;

    private void ResetCubeArray(CubeMapping cubeMap)
    {
        Debug.Log(boatCubes.Count);
        Debug.Log(lanternCubes.Count);
        hasAnimationPlayed = false;
        // Create the array if it doesn't exist
        cubes ??= new Cube[chunkSize, chunkSize, chunkSize];
        int cubeCount = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    // Check if the cube exists
                    if (cubes[x, y, z] != null)
                    {
                        Destroy(cubes[x, y, z].gameObject);
                    }
                    // Create a new cube at this position
                    GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // Nest cubeObject under this object
                    cubeObject.name = "Cube_" + cubeCount++;
                    cubeObject.transform.parent = transform;
                    // Set the position of the cubeObject
                    cubeObject.transform.position = new Vector3(x - 8, y - 8, z - 8);
                    Cube cube = cubeObject.AddComponent<Cube>();
                    cube.baseCubeMaterial = baseCubeMaterial;
                    cube.highlightCubeMaterial = highlightCubeMaterial;
                    cube.x = x;
                    cube.y = y;
                    cube.z = z;
                    if (!IsCubeOnEdgeOfChunk(cube)) { DisableCube(cube); }
                    cubes[x, y, z] = cube;
                }
            }
        }
        if (cubeMap == CubeMapping.LANTERN)
        {
            Debug.Log("Building Lantern");
            brokenCubesToWin = cubeCount + 1 - lanternCubes.Count;
            foreach (CubeStruct cubeStruct in lanternCubes)
            {
                Cube cube = GetCubeAtPosition(cubeStruct.x, cubeStruct.y, cubeStruct.z);
                if (cube != null)
                {
                    Debug.Log(cube.gameObject.name + " is indestructible");
                    cube.isIndestructible = true;
                    cube.gameObject.GetComponent<MeshRenderer>().material = cubeStruct.material;
                    cube.baseCubeMaterial = cubeStruct.material;
                    cube.highlightCubeMaterial = cubeStruct.material;
                }
            }
        }
        else if (cubeMap == CubeMapping.SHIP)
        {
            Debug.Log("Building Ship");
            brokenCubesToWin = cubeCount + 1 - boatCubes.Count;
            foreach (CubeStruct cubeStruct in boatCubes)
            {
                Cube cube = GetCubeAtPosition(cubeStruct.x, cubeStruct.y, cubeStruct.z);
                Debug.Log("Cube: " + cube);
                if (cube != null)
                {
                    Debug.Log(cube.gameObject.name + " is indestructible");
                    cube.isIndestructible = true;
                    cube.gameObject.GetComponent<MeshRenderer>().material = cubeStruct.material;
                    cube.baseCubeMaterial = cubeStruct.material;
                    cube.highlightCubeMaterial = cubeStruct.material;
                }
            }
        }
    }
    public void ResetCubeArray()
    {
        ResetCubeArray(CubeMapping.NONE);
    }

    public bool IsCubeOnEdgeOfChunk(Cube cube)
    {
        return cube.x == 0 || cube.x == chunkSize - 1 || cube.y == 0 || cube.y == chunkSize - 1 || cube.z == 0 || cube.z == chunkSize - 1;
    }
    public bool IsCubeOnEdgeOfChunk(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        return IsCubeOnEdgeOfChunk(cube);
    }
    public bool IsCubeVisible(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        return IsCubeVisible(cube);

    }
    public bool IsCubeVisible(Cube cube)
    {
        if (cube.isClickedOn) { return false; }
        // Check if the cube is along an edge of the chunk
        if (IsCubeOnEdgeOfChunk(cube))
        {
            return true;
        }
        // Check if the cube is adjacent to a disabled cube
        if (!IsCubeEnabled(cube.x - 1, cube.y, cube.z) || !IsCubeEnabled(cube.x + 1, cube.y, cube.z) ||
            !IsCubeEnabled(cube.x, cube.y - 1, cube.z) || !IsCubeEnabled(cube.x, cube.y + 1, cube.z) ||
            !IsCubeEnabled(cube.x, cube.y, cube.z - 1) || !IsCubeEnabled(cube.x, cube.y, cube.z + 1))
        {
            return true;
        }
        return false;
    }
    public bool IsCubeEnabled(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        return IsCubeEnabled(cube);
    }
    public bool IsCubeEnabled(Cube cube)
    {
        return cube != null && cube.gameObject.activeSelf;
    }

    public void EnableCube(Cube cube)
    {
        cube.gameObject.SetActive(true);
    }
    public void EnableCube(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        EnableCube(cube);
    }
    public void DisableCube(Cube cube)
    {
        cube.gameObject.SetActive(false);
    }
    public void DisableCube(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        DisableCube(cube);
    }
    public List<Cube> GetNeighbors(Cube cube)
    {
        List<Cube> neighbors = new();
        Cube outCube = GetCubeAtPosition(cube.x - 1, cube.y, cube.z);
        if (outCube != null) { neighbors.Add(outCube); }
        outCube = GetCubeAtPosition(cube.x + 1, cube.y, cube.z);
        if (outCube != null) { neighbors.Add(outCube); }
        outCube = GetCubeAtPosition(cube.x, cube.y - 1, cube.z);
        if (outCube != null) { neighbors.Add(outCube); }
        outCube = GetCubeAtPosition(cube.x, cube.y + 1, cube.z);
        if (outCube != null) { neighbors.Add(outCube); }
        outCube = GetCubeAtPosition(cube.x, cube.y, cube.z - 1);
        if (outCube != null) { neighbors.Add(outCube); }
        outCube = GetCubeAtPosition(cube.x, cube.y, cube.z + 1);
        if (outCube != null) { neighbors.Add(outCube); }
        return neighbors;
    }
    public List<Cube> GetNeighbors(int x, int y, int z)
    {
        Cube cube = GetCubeAtPosition(x, y, z);
        return GetNeighbors(cube);
    }
    private Cube GetCubeAtPosition(int x, int y, int z)
    {
        if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
        {
            return null;
        }
        return cubes[x, y, z];
    }

    private void CreateEffectCubes(Cube targetCube)
    {
        float effectCubeSize = targetCube.transform.localScale.x / 4.0f;
        // We need to create 8 cubes that fill the space of the target cube
        targetCube.transform.GetPositionAndRotation(out Vector3 originalCubePosition, out Quaternion originalCubeRotation);
        // Create 8 cubes where the current cube is.
        for (int i = 0; i < 8; i++)
        {
            GameObject effectCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            effectCube.name = "EffectCube_" + i;
            effectCube.transform.parent = effectCubes.transform;
            effectCube.transform.localScale = new Vector3(effectCubeSize, effectCubeSize, effectCubeSize);
            effectCube.transform.SetPositionAndRotation(originalCubePosition, originalCubeRotation);
            effectCube.AddComponent<Rigidbody>();
            effectCube.GetComponent<MeshRenderer>().material = baseCubeMaterial;
            effectCube.layer = LayerMask.NameToLayer("EffectCubes");
            Vector3 randomDirection = Random.insideUnitSphere;
            effectCube.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(effectCube.transform.rotation.eulerAngles, randomDirection, Mathf.Deg2Rad * 45, 0.0f));
            effectCube.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(250.0f, 500.0f), originalCubePosition + randomDirection, 1.0f);
            effectCube.AddComponent<EffectCube>();
        }
        

    }
    private void HandleCubeClickOn(Cube clickedCube)
    {
        if (clickedCube.isIndestructible) { return; } // We don't want to destroy indestructible cubes
        clickedCube.isClickedOn = true;
        List<Cube> neighbors = GetNeighbors(clickedCube);
        foreach (Cube neighbor in neighbors)
        {
            
            if (IsCubeVisible(neighbor))
            {
                EnableCube(neighbor);
            }
        }
        CreateEffectCubes(clickedCube);
        DisableCube(clickedCube);
        // Play audio
        AudioSource.PlayClipAtPoint(cubeClickSound, clickedCube.transform.position);
        brokenCubesToWin -= 1;
        if (brokenCubesToWin <= 0)
        {
        }
    }

    private void BakeRotation() // TODO: Try making a separate object that the rotation transfers to when the player releases the input
    {
        // Unparent this object from the rotationParent
        transform.parent = null;
        // Reset the rotation of the rotationParent
        rotationParent.transform.localRotation = Quaternion.identity;
        // Set the local rotation of this object to the rotationParent's local rotation
        transform.parent = rotationParent.transform;
    }

    private void Awake()
    {
        gameInputActions = new GameInputActions();
        cubes = new Cube[chunkSize, chunkSize, chunkSize];
        rotationParent = new GameObject("RotationParent");
        // Set this object as the child of the rotationParent
        transform.parent = rotationParent.transform;
    }

    private void RotateCube(float rotateX, float rotateY)
    {
        // Smoothly interpolate the current rotation speed towards the target speed
        currentRotationSpeedX = Mathf.Lerp(currentRotationSpeedX, rotateX, Time.deltaTime / rotationRampUpTime);
        currentRotationSpeedY = Mathf.Lerp(currentRotationSpeedY, rotateY, Time.deltaTime / rotationRampUpTime);

        // Apply the rotation
        rotationParent.transform.Rotate(Vector3.up, -currentRotationSpeedX * rotationDegreesPerSecond * Time.deltaTime);
        rotationParent.transform.Rotate(Vector3.right, currentRotationSpeedY * rotationDegreesPerSecond * Time.deltaTime);
    }

    private void OnEnable()
    {
        rotateCubeX = gameInputActions.Game.RotateCubeX;
        rotateCubeX.Enable();
        rotateCubeY = gameInputActions.Game.RotateCubeY;
        rotateCubeY.Enable();
    }

    private void OnDisable()
    {
        rotateCubeX.Disable();
        rotateCubeY.Disable();
    }

    private void OnDestroy()
    {
        rotateCubeX.Dispose();
        rotateCubeY.Dispose();

        gameInputActions.Dispose();

        Destroy(rotationParent);
        
    }
    private void Start()
    {
        clickWatcher.cubeClicked.AddListener(HandleCubeClickOn);
        effectCubes = GameObject.Find("EffectCubes");
        effectCubes = effectCubes != null ? effectCubes : new GameObject("EffectCubes");
        lanternCubes = lanternPrefab.GetComponent<CubeTemplate>().GetCubePositions();
        boatCubes = boatPrefab.GetComponent<CubeTemplate>().GetCubePositions();

        // Subscribe to events
        gm.OnGameStart.AddListener(GameStart);
        gm.OnGamePause.AddListener(GamePause);
        gm.OnGameResume.AddListener(GameResume);
        gm.OnGameEnd.AddListener(GameEnd);
        gm.OnGameRestart.AddListener(GameRestart);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameRunning || isGamePaused)
        {
            return;
        }
        float rotateX = rotateCubeX.ReadValue<float>();
        float rotateY = rotateCubeY.ReadValue<float>();

        if (rotateX != 0.0f && isRotatingX == false)
        {
            isRotatingX = true;
            BakeRotation();
        }
        if (rotateY != 0.0f && isRotatingY == false)
        {
            isRotatingY = true;
            BakeRotation();
        }

        if (rotateX != 0.0f || rotateY != 0.0f)
        {
            transformationNeedsBake = true;
        }
        isRotatingX = rotateX != 0.0f;
        isRotatingY = rotateY != 0.0f;

        if (rotateX == 0.0f && Mathf.Abs(currentRotationSpeedX) < 0.05f)
        {
            currentRotationSpeedX = 0.0f;
        }
        if (rotateY == 0.0f && Mathf.Abs(currentRotationSpeedY) < 0.01f)
        {
            currentRotationSpeedY = 0.0f;
        }
        if (rotateX == 0.0f && rotateY == 0.0f && transformationNeedsBake)
        {
            if (currentRotationSpeedX == 0.0f && currentRotationSpeedY == 0.0f)
            {
                BakeRotation();
                transformationNeedsBake = false;
            }
        }
        RotateCube(rotateX, rotateY);
    }

    private IEnumerator WaitForGameStart()
    {
        yield return new WaitForSeconds(0.5f);
        isGameRunning = true;
        hasAnimationPlayed = true;
    }
    void GameStart()
    {
        lanternCubes = lanternPrefab.GetComponent<CubeTemplate>().GetCubePositions();
        boatCubes = boatPrefab.GetComponent<CubeTemplate>().GetCubePositions();
        StartCoroutine(WaitForGameStart());
        isGamePaused = false;
        // Pick a random number between 1 and 2
        // Randomly choose between CubeMapping.LANTERN and CubeMapping.SHIP
        if (lastMap == CubeMapping.LANTERN)
        {
            lastMap = CubeMapping.SHIP;
            ResetCubeArray(CubeMapping.SHIP);
        }
        else if (lastMap == CubeMapping.SHIP)
        {
            lastMap = CubeMapping.LANTERN;
            ResetCubeArray(CubeMapping.LANTERN);
        }
        else
        {
            int randomCubeMap = Random.Range(1, 3);
            if (randomCubeMap == 1)
            {
                lastMap = CubeMapping.LANTERN;
                ResetCubeArray(CubeMapping.LANTERN);
            }
            else
            {
                lastMap = CubeMapping.SHIP;
                ResetCubeArray(CubeMapping.SHIP);
            }
        }
    }
    void GamePause()
    {
        isGamePaused = true;
    }
    void GameResume()
    {
        isGamePaused = false;
    }
    void GameEnd()
    {
        isGameRunning = false;
        // Delete the cube array
        foreach (Cube cube in cubes)
        {
            Destroy(cube.gameObject);
        }
    }
    void GameRestart()
    {
        GameStart();
    }
}

