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
    throwEndWait = 0.1f,
    screwSpinSpeed = 10.0f;
    
    Vector3 throwEndDirection;
    float throwEndSpeed;

    float returnSpeed;

    public Vector2 throwPosition { get; private set; }

    public GameObject attachedScrew { get; private set; }

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
            transform.rotation *= Quaternion.Euler(Vector3.forward * -100.0f);

            transform.position += GetDirectionTowardsLocation(throwPosition) * maxThrowSpeed;

            if(Vector2.Distance(transform.position, throwPosition) < 30.0f) {
                EndThrow();
            }
        }

        if(wrenchState == WrenchState.ThrowEnd) {
            throwEndSpeed = Mathf.Lerp(throwEndSpeed, 0.0f, throwEndWait);

            transform.rotation = Quaternion.Lerp(transform.rotation, GetLookAwayRotation2D(owningPlayer.transform.position), 1.0f - (throwEndSpeed / maxThrowSpeed));

            transform.position += throwEndDirection * throwEndSpeed;
            if (throwEndSpeed < 0.25f) {
                StartReturn();
            }
        }

        if (wrenchState == WrenchState.Return) {
            returnSpeed = Mathf.Lerp(returnSpeed, maxReturnSpeed, returnAcceleration);

            transform.rotation = Quaternion.Lerp(transform.rotation, GetLookAwayRotation2D(owningPlayer.transform.position), 0.5f);

            transform.position += GetDirectionTowardsLocation(owningPlayer.transform.position) * returnSpeed;
            if (Vector2.Distance(transform.position, owningPlayer.transform.position) < returnSpeed/2.0f) {
                ParentToPlayer();
            }
        }

        if(wrenchState == WrenchState.OnScrew) {
            if (owningPlayer.isMagnetingToWrench) {
                if (Vector2.Distance(transform.position, owningPlayer.transform.position) > 5.0f) {
                    float rotationTowardsPlayer = GetLookAwayRotation2D(owningPlayer.transform.position).eulerAngles.z;
                    float lerpedRotation = Mathf.LerpAngle(transform.rotation.eulerAngles.z, rotationTowardsPlayer, 0.25f);
                    Transform transformAroundPoint = GetTransformAroundPoint2D(attachedScrew.transform.position, lerpedRotation);
                    transform.position = transformAroundPoint.position;
                    transform.rotation = transformAroundPoint.rotation;
                }
                else {
                    wrenchState = WrenchState.ScrewPlayerRotation;
                }
            }
        }

        if(wrenchState == WrenchState.ScrewPlayerRotation) {
            float newRotation = transform.rotation.eulerAngles.z + screwSpinSpeed;
            Transform transformAroundPoint = GetTransformAroundPoint2D(attachedScrew.transform.position, newRotation);
            transform.position = transformAroundPoint.position;
            transform.rotation = transformAroundPoint.rotation;
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

    public void EndThrow() {
        if(wrenchState == WrenchState.Throw) {
            wrenchState = WrenchState.ThrowEnd;
            throwEndSpeed = maxThrowSpeed;
            throwEndDirection = GetDirectionTowardsLocation(throwPosition);
        }
    }

    public void StartReturn() {
        returnSpeed = 0.0f;
        wrenchState = WrenchState.Return;
    }

    public void ForceReturn() {
        EndThrow();
        if (wrenchState == WrenchState.OnScrew || wrenchState == WrenchState.ScrewPlayerRotation) {
            StartReturn();
        }
    }

    public void AttachToScrew(GameObject screw) {
        attachedScrew = screw;
        transform.parent = null;
        transform.position = attachedScrew.transform.position - (screwOffset.x * transform.right) - (screwOffset.y * transform.up);
        wrenchState = WrenchState.OnScrew;
    }

    public void ParentToPlayer() {
        owningPlayer.transform.parent = null;
        wrenchState = WrenchState.WithPlayer;
        transform.parent = owningPlayer.transform;
        transform.localPosition = Vector2.zero;
        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    }

    public Transform GetTransformAroundPoint2D(Vector2 point, float rotation) {
        Transform originalTransform = transform;
        float rotationOffset = originalTransform.rotation.eulerAngles.z;
        transform.RotateAround(point, Vector3.forward, rotation - rotationOffset);
        Transform returnTransform = transform;
        transform.position = originalTransform.position;
        transform.rotation = originalTransform.rotation;
        return returnTransform;
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
        if(otherCollider.gameObject.tag == "Screw" && wrenchState != WrenchState.WithPlayer) {
            AttachToScrew(otherCollider.gameObject);
        }
    }
}
