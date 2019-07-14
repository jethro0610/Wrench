using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewMove : MonoBehaviour
{
    [SerializeField]
    ScrewDetector linkedScrew;
    [SerializeField]
    float moveSpeed,
    moveBackSpeed;
    [SerializeField]
    Vector2 deltaPosition;

    Vector2 startPosition;
    float moveTick;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (linkedScrew.isGrabbed) {
            moveTick += moveSpeed * Time.deltaTime;
        }
        else {
            moveTick -= moveBackSpeed * Time.deltaTime;
        }
        moveTick = Mathf.Clamp(moveTick, 0.0f, 1.0f);

        transform.position = Vector2.Lerp(startPosition, startPosition + deltaPosition, moveTick);
    }
}
