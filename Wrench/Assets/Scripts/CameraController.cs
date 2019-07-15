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

    float shakeTime;
    float shakeStrength;

    Vector3 shakeOffset;
    // Start is called before the first frame update
    void Start()
    {
        startSize = GetComponentInChildren<Camera>().orthographicSize;
        cameraStartX = transform.position.x;
        background.gameObject.SetActive(true);
    }

    private void Update() {
        if (shakeTime >= 0.0f) {
            shakeTime -= Time.deltaTime;
            shakeOffset.x = Random.Range(-shakeStrength, shakeStrength);
            shakeOffset.y = Random.Range(-shakeStrength, shakeStrength);
        }
        else {
            shakeOffset = Vector3.zero;
        }
        GetComponentInChildren<Camera>().transform.localPosition = shakeOffset;
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
        GetComponentInChildren<Camera>().orthographicSize = Mathf.Lerp(GetComponentInChildren<Camera>().orthographicSize, newSize, lerpSpeed * Time.deltaTime);

        transform.position = Vector2.Lerp(transform.position, point, lerpSpeed * Time.deltaTime);
        transform.position += Vector3.forward * -30.0f;

        float xDif = cameraStartX - transform.position.x;
        background.transform.localPosition = new Vector3(xDif * parallaxScale, 10.0f, 200.0f);
    }

    public void ShakeForSeconds(float time, float strength) {
        shakeTime = time;
        shakeStrength = strength;
    }
}
