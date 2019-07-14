using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForth : MonoBehaviour
{
    [SerializeField]
    float waitTime,
    moveSpeed;

    [SerializeField]
    Vector2 deltaPosition;

    float startZ;
    Vector2 startPosition;

    float travelTick;
    float waitTick;

    bool canMove = false;
    bool returning = false;
    // Start is called before the first frame update
    void Start()
    {
        startZ = transform.position.z;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(waitTick >= waitTime) {
            canMove = true;
            waitTick = 0.0f;
        }
        else {
            waitTick += Time.deltaTime;
        }

        if (returning == false) {
            if (canMove) {
                travelTick += Time.deltaTime;
                if (travelTick >= 1.0f) {
                    canMove = false;
                    waitTick = 0.0f;
                    returning = true;
                }
            }
        }
        else if (returning == true) {
            if (canMove) {
                travelTick -= Time.deltaTime;
                if (travelTick <= 0.0f) {
                    canMove = false;
                    waitTick = 0.0f;
                    returning = false;
                }
            }
        }

        transform.position = Vector3.Lerp(startPosition, startPosition + deltaPosition, travelTick) + (Vector3.forward * startZ);
    }
}
