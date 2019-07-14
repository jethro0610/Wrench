using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchController : MonoBehaviour
{
    private PlayerController owningPlayer;
    new Rigidbody2D rigidbody;

    public enum WrenchState { Throw, ThrowEnd, Return, WithPlayer, Attached, GrabAttach };
    public WrenchState wrenchState { get; private set; } = WrenchState.WithPlayer;

    [SerializeField]
    public Vector2 holdOffset,
    attachOffset;

    [SerializeField]
    float maxThrowSpeed = 10.0f,
    maxReturnSpeed = 50.0f,
    returnAcceleration = 0.1f,
    throwEndWait = 0.1f,
    screwSpinSpeed = 10.0f;

    Vector2 throwEndDirection;
    float throwEndSpeed;

    float returnSpeed;

    public Vector2 throwPosition { get; private set; }

    public GameObject attachedObject { get; private set; }

    Quaternion attachRotation;

    Transform parentSocket;
    Vector3 localPosition;
    Quaternion localRotation;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        owningPlayer = GetComponentInParent<PlayerController>();

        parentSocket = transform.parent;
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (wrenchState == WrenchState.WithPlayer) {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        if (wrenchState == WrenchState.GrabAttach) {
            if (attachedObject.tag == "Screw") {
                transform.position = attachedObject.transform.position - (attachOffset.x * transform.right) - (attachOffset.y * transform.up);
                float newRotation = transform.rotation.eulerAngles.z + screwSpinSpeed * owningPlayer.directionMultiplier * Time.deltaTime;
                Transform transformAroundPoint = GetTransformAroundPoint2D(attachedObject.transform.position, newRotation);
                rigidbody.position = transformAroundPoint.position;
                rigidbody.rotation = transformAroundPoint.rotation.eulerAngles.z;

                attachedObject.GetComponent<Rigidbody2D>().rotation += screwSpinSpeed * owningPlayer.directionMultiplier * Time.deltaTime;
                attachedObject.GetComponent<ScrewDetector>().isGrabbed = true;
            }
        }

        if (wrenchState == WrenchState.Attached) {
            transform.position = attachedObject.transform.position - (attachOffset.x * transform.right) - (attachOffset.y * transform.up);
            Transform transformAroundPoint = GetTransformAroundPoint2D(attachedObject.transform.position, transform.rotation.eulerAngles.z);
            rigidbody.position = transformAroundPoint.position;
            rigidbody.rotation = transformAroundPoint.rotation.eulerAngles.z;

            if (owningPlayer.isMagnetingToWrench) {
                if (Vector2.Distance(rigidbody.position + (transform.right * holdOffset), owningPlayer.transform.position) > 15.0f) {
                    if (attachedObject.tag == "Screw") {
                        transform.position = attachedObject.transform.position - (attachOffset.x * transform.right) - (attachOffset.y * transform.up);
                        float originalRotation = rigidbody.rotation;
                        float rotationTowardsPlayer = GetLookAwayRotation2D(owningPlayer.transform.position).eulerAngles.z;
                        float lerpedRotation = Mathf.LerpAngle(transform.rotation.eulerAngles.z, rotationTowardsPlayer, 4.0f * Time.deltaTime);
                        transformAroundPoint = GetTransformAroundPoint2D(attachedObject.transform.position, lerpedRotation);
                        rigidbody.position = transformAroundPoint.position;
                        rigidbody.rotation = transformAroundPoint.rotation.eulerAngles.z;

                        float rotationDifference = lerpedRotation - originalRotation;
                        attachedObject.GetComponent<Rigidbody2D>().rotation += rotationDifference;
                    }
                }
                else {
                    wrenchState = WrenchState.GrabAttach;
                }
            }
        }
    }

    void FixedUpdate() {
        if (wrenchState == WrenchState.Throw) {
            rigidbody.rotation -= -100.0f;

            rigidbody.position += GetDirectionTowardsLocation(throwPosition) * maxThrowSpeed;
            attachRotation = GetLookRotation2D(throwPosition);
            if(Vector2.Distance(rigidbody.position, throwPosition) < 30.0f) {
                EndThrow();
            }
        }

        if(wrenchState == WrenchState.ThrowEnd) {
            throwEndSpeed = Mathf.Lerp(throwEndSpeed, 0.0f, throwEndWait);

            rigidbody.rotation = Quaternion.Lerp(transform.rotation, GetLookAwayRotation2D(owningPlayer.transform.position), 1.0f - (throwEndSpeed / maxThrowSpeed)).eulerAngles.z;

            rigidbody.position += throwEndDirection * throwEndSpeed;
            attachRotation = GetLookRotation2D(throwPosition);
            if (throwEndSpeed < 0.25f) {
                StartReturn();
            }
        }

        if (wrenchState == WrenchState.Return) {
            returnSpeed = Mathf.Lerp(returnSpeed, maxReturnSpeed, returnAcceleration);

            rigidbody.rotation = Quaternion.Lerp(transform.rotation, GetLookAwayRotation2D(owningPlayer.transform.position), 0.5f).eulerAngles.z;

            rigidbody.position += GetDirectionTowardsLocation(owningPlayer.transform.position) * returnSpeed;
            attachRotation = GetLookAwayRotation2D(throwPosition);
            if (Vector2.Distance(rigidbody.position, owningPlayer.transform.position) < returnSpeed/2.0f) {
                ParentToPlayer();
            }
        }
    }

    public Vector2 GetDirectionTowardsLocation(Vector2 Location) {
        return (Location - (Vector2)transform.position).normalized;
    }

    public void Throw(Vector2 newThrowPosition) {
        if (wrenchState == WrenchState.WithPlayer) {
            transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            throwPosition = newThrowPosition;
            transform.parent = null;
            wrenchState = WrenchState.Throw;
            owningPlayer.animator.SetBool("Throw", true);
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
        if (wrenchState == WrenchState.Attached) {
            StartReturn();
        }
    }

    public void AttachTo(GameObject attachObject) {
        attachedObject = attachObject;
        transform.parent = null;
        transform.rotation = attachRotation;
        transform.position = attachedObject.transform.position - (attachOffset.x * transform.right) - (attachOffset.y * transform.up);
        wrenchState = WrenchState.Attached;
    }

    public void ParentToPlayer() {
        owningPlayer.transform.parent = null;
        wrenchState = WrenchState.WithPlayer;
        transform.parent = parentSocket;
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
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
        if(wrenchState != WrenchState.WithPlayer) {
            if(otherCollider.gameObject.tag == "Screw" || otherCollider.gameObject.tag == "Attachable")
                AttachTo(otherCollider.gameObject);
        }
    }
}
