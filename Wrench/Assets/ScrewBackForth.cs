using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewBackForth : MonoBehaviour
{
    [SerializeField]
    ScrewDetector linkedScrew;
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    Vector2 deltaPosition;

    Vector2 startPosition;
    float moveTick;

    bool returning= false;
    // Start is called before the first frame update
    void Start() {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (linkedScrew.isGrabbed) {
            if (!returning) {
                moveTick += moveSpeed * Time.deltaTime;
            }
            else {
                moveTick -= moveSpeed * Time.deltaTime;
            }
        }
        if(moveTick > 1.0f) {
            returning = true;
        }
        if (moveTick < 0.0f) {
            returning = false;
        }

        transform.position = Vector2.Lerp(startPosition, startPosition + deltaPosition, moveTick);
    }
}
