using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float lerpSpeed,
    maxWrenchDistance,
    zoomOutMultiplier,
    parallaxScale = 1.0f;

    [SerializeField]
    Rigidbody2D player,
    wrench;

    [SerializeField]
    Transform background;

    float startSize;
    float cameraStartX;
    // Start is called before the first frame update
    void Start()
    {
        startSize = GetComponent<Camera>().orthographicSize;
        cameraStartX = transform.position.x;
        background.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        Vector2 point = player.position;
        Vector2 differnce = wrench.position - player.position;
        differnce = new Vector2(differnce.x, differnce.y * 2.0f);
        float distance = differnce.magnitude;
        if(distance > 50.0f) {
            point = (player.position + wrench.position) / 2.0f;
        }
        float newSize = startSize + (Mathf.Max(0.0f, distance - 50.0f) * zoomOutMultiplier);
        GetComponent<Camera>().orthographicSize = Mathf.Lerp(GetComponent<Camera>().orthographicSize, newSize, lerpSpeed * Time.deltaTime);

        transform.position = Vector2.Lerp(transform.position, point, lerpSpeed * Time.deltaTime);
        transform.position += Vector3.forward * -10.0f;

        float xDif = cameraStartX - transform.position.x;
        background.transform.localPosition = new Vector3(xDif * parallaxScale, 10.0f, 200.0f);
    }
}
