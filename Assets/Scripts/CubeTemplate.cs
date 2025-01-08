using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public struct CubeStruct
{
    public int x;
    public int y;
    public int z;
    public Material material;
}
public class CubeTemplate : MonoBehaviour
{
    public List<GameObject> children = new();

    public List<CubeStruct> GetCubePositions()
    {
        List<CubeStruct> cubeStructs = new();
        foreach (GameObject child in children)
        {
            child.SetActive(true);
            CubeStruct cubeStruct = new()
            {
                x = Mathf.RoundToInt(child.transform.localPosition.x) + 8,
                y = Mathf.RoundToInt(child.transform.localPosition.y) + 8,
                z = Mathf.RoundToInt(child.transform.localPosition.z) + 8,
                material = child.GetComponent<Renderer>().material
            };
            cubeStructs.Add(cubeStruct);
            child.SetActive(false);
        }
        return cubeStructs;
    }
    private void Start()
    {
        // Get all children of the current object
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        Debug.Log("Children size: " + children.Count);
    }
}
