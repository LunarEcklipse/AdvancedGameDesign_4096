using System.Collections;
using UnityEngine;

public class EffectCube : MonoBehaviour
{
    private IEnumerator DestroyAfterTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        // Shrink to nothing over a second before destroying the object
        float timeScale = 1.0f;
        float objectScale = transform.localScale.x;
        while (timeScale > 0.0f)
        {
            timeScale -= Time.deltaTime;
            float scale = objectScale * timeScale;
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyAfterTimeout(5.0f));
    }
}
