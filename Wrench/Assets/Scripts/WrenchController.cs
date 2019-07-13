using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchController : MonoBehaviour
{
    private PlayerController owningPlayer;

    public enum WrenchState { Throw, ThrowEnd, ReturnStart, Return, WithPlayer };
    public WrenchState wrenchState { get; private set; } = WrenchState.WithPlayer;

    [SerializeField]
    float throwDistance = 150.0f,
    maxThrowSpeed = 10.0f,
    maxReturnSpeed = 50.0f,
    returnAcceleration = 0.1f,
    throwEndWait = 0.1f;
    
    Vector3 throwEndDirection;
    Quaternion throwEndRotation;
    float throwEndSpeed;

    float returnSpeed;

    public Vector2 throwPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        owningPlayer = GetComponentInParent<PlayerController>();   
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() {
        if (wrenchState == WrenchState.Throw) {
            transform.position += GetDirectionTowardsLocation(throwPosition) * maxThrowSpeed;
            if(Vector2.Distance(transform.position, throwPosition) < 30.0f) {
                wrenchState = WrenchState.ThrowEnd;
                throwEndSpeed = maxThrowSpeed;
                throwEndDirection = GetDirectionTowardsLocation(throwPosition);
                throwEndRotation = transform.rotation;
            }
        }

        if(wrenchState == WrenchState.ThrowEnd) {
            throwEndSpeed = Mathf.Lerp(throwEndSpeed, 0.0f, throwEndWait);
            transform.position += throwEndDirection * throwEndSpeed;
            if (throwEndSpeed < 0.05f) {
                returnSpeed = 0.0f;
                wrenchState = WrenchState.Return;
            }
        }

        if (wrenchState == WrenchState.Return) {
            returnSpeed = Mathf.Lerp(returnSpeed, maxReturnSpeed, returnAcceleration);
            transform.position += GetDirectionTowardsLocation(owningPlayer.transform.position) * returnSpeed;
            if (Vector2.Distance(transform.position, owningPlayer.transform.position) < 25.0f) {
                wrenchState = WrenchState.WithPlayer;
                transform.parent = owningPlayer.transform;
                transform.localPosition = Vector2.zero;
            }
        }
    }

    public Vector3 GetDirectionTowardsLocation(Vector3 Location) {
        return (Location - transform.position).normalized;
    }

    public void Throw() {
        if (wrenchState == WrenchState.WithPlayer) {
            throwPosition = new Vector2(owningPlayer.transform.position.x + throwDistance, owningPlayer.transform.position.y);
            transform.parent = null;
            wrenchState = WrenchState.Throw;
        }
    }
}
