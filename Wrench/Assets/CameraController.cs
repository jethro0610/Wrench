using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float lerpSpeed,
    maxWrenchDistance,
    zoomOutMultiplier;

    [SerializeField]
    Rigidbody2D player,
    wrench;

    float startSize;
    // Start is called before the first frame update
    void Start()
    {
        startSize = GetComponent<Camera>().orthographicSize;
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
        GetComponent<Camera>().orthographicSize = startSize + (Mathf.Max(0.0f, distance - 50.0f) * zoomOutMultiplier);

        transform.position = Vector2.Lerp(transform.position, point, lerpSpeed * Time.deltaTime);
        transform.position += Vector3.forward * -10.0f;
    }
}
