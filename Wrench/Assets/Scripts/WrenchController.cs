using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchController : MonoBehaviour
{
    private PlayerController owningPlayer;

    public enum WrenchState { Throw, ThrowEnd, Return, WithPlayer, OnScrew, ScrewPlayerRotation };
    public WrenchState wrenchState { get; private set; } = WrenchState.WithPlayer;

    [SerializeField]
    Vector3 holdOffset,
    screwOffset;

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

    public GameObject testScrew;
    GameObject attachedScrew;

    // Start is called before the first frame update
    void Start()
    {
        //if (testScrew != null)
            //AttachToScrew(testScrew);

        owningPlayer = GetComponentInParent<PlayerController>();   
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() {
        if (wrenchState == WrenchState.Throw) {
            transform.rotation *= Quaternion.Euler(Vector3.forward * -100.0f);

            transform.position += GetDirectionTowardsLocation(throwPosition) * maxThrowSpeed;

            if(Vector2.Distance(transform.position, throwPosition) < 30.0f) {
                wrenchState = WrenchState.ThrowEnd;
                throwEndSpeed = maxThrowSpeed;
                throwEndDirection = GetDirectionTowardsLocation(throwPosition);
            }
        }

        if(wrenchState == WrenchState.ThrowEnd) {
            throwEndSpeed = Mathf.Lerp(throwEndSpeed, 0.0f, throwEndWait);

            transform.rotation *= Quaternion.Euler(Vector3.forward * -100.0f * (throwEndSpeed/maxThrowSpeed));

            transform.position += throwEndDirection * throwEndSpeed;
            if (throwEndSpeed < 0.25f) {
                throwEndRotation = transform.rotation;
                returnSpeed = 0.0f;
                wrenchState = WrenchState.Return;
            }
        }

        if (wrenchState == WrenchState.Return) {
            returnSpeed = Mathf.Lerp(returnSpeed, maxReturnSpeed, returnAcceleration);

            transform.rotation = Quaternion.Lerp(transform.rotation, GetLookAwayRotation2D(owningPlayer.transform.position), 0.5f);

            transform.position += GetDirectionTowardsLocation(owningPlayer.transform.position) * returnSpeed;
            if (Vector2.Distance(transform.position, owningPlayer.transform.position) < 25.0f) {
                wrenchState = WrenchState.WithPlayer;
                transform.parent = owningPlayer.transform;
                transform.localPosition = Vector2.zero;
            }
        }

        if(wrenchState == WrenchState.OnScrew) {
            //transform.RotateAround(attachedScrew.transform.position, Vector3.forward, 10.0f);
            //transform.rotation = Quaternion.LookRotation()
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

    public void AttachToScrew(GameObject screw) {
        attachedScrew = screw;
        transform.parent = null;
        transform.position = attachedScrew.transform.position - (screwOffset.x * transform.right) - (screwOffset.y * transform.up);
        wrenchState = WrenchState.OnScrew;
    }

    public Quaternion GetLookRotation2D(Vector2 lookPosition) {
        Vector2 direction = GetDirectionTowardsLocation(lookPosition);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return newRotation;
    }

    public Quaternion GetLookAwayRotation2D(Vector2 lookPosition) {
        Vector2 direction = -GetDirectionTowardsLocation(lookPosition);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return newRotation;
    }


    void OnTriggerEnter2D(Collider2D otherCollider) {
        if(otherCollider.gameObject.tag == "Screw") {
            print("found screw");
            AttachToScrew(otherCollider.gameObject);
        }

    }
}
