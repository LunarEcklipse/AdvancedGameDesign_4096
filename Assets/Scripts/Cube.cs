using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class Cube : MonoBehaviour
{
    public Material baseCubeMaterial;
    public Material highlightCubeMaterial;

    public bool isIndestructible = false;
    public bool isHighlighted = false;
    public bool isClickedOn = false;
    public bool hasPlayedAnimation = false;

    public int x;
    public int y;
    public int z;

    private MeshRenderer meshRenderer;
    
    public void HighlightCube()
    {
        isHighlighted = true;
        meshRenderer.material = highlightCubeMaterial;
    }

    public void UnhighlightCube()
    {
        isHighlighted = false;
        meshRenderer.material = baseCubeMaterial;
    }

    public IEnumerator SpawnCubeAnimation()
    {
        this.gameObject.transform.localScale = Vector3.zero;
        float scale = 0.0f;
        // Scale the cube up to 1 over 0.5 seconds
        while (scale < 1.0f)
        {
            scale += Time.deltaTime * 2;
            this.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        // Get the meshrenderer and collider components off the object. If they don't exist, create them.
        if (!TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (!TryGetComponent<Collider>(out _))
        {
            gameObject.AddComponent<BoxCollider>();
        }
        // Set the material of the meshrenderer to the baseCubeMaterial
        meshRenderer.material = baseCubeMaterial;
        // Get the parent object
        if (this.transform.parent.GetComponent<CubeMap>().hasAnimationPlayed == false)
        {
            StartCoroutine(SpawnCubeAnimation());
        }
        }
}
