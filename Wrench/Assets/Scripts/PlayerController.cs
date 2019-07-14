﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    new BoxCollider2D collider;
    new Rigidbody2D rigidbody;

    public enum Direction { Right, Left };
    public Direction direction { get; private set; } = Direction.Right;

    [SerializeField]
    GameObject playerModel;

    [SerializeField]
    Transform visual;

    [SerializeField]
    float maxMoveSpeed = 150.0f,
    groundFriction = 0.3f,
    airFriction = 0.05f,
    gravitySpeed = 10,
    jumpStrength = 250.0f,
    jumpStopSpeed = 0.8f,
    maxTargetWidth = 50.0f,
    maxTargetDistance = 100.0f,
    throwDistance = 150.0f,
    maxMagnetToWrenchSpeed = 10.0f,
    magnetToWrenchAcceleration = 0.8f,
    screwReleaseSpeedX = 500.0f,
    screwReleaseSpeedY = 300.0f;

    bool isJumping;
    public GameObject wrenchTarget { get; private set; }

    float groundAcceleration {
        get {
            return (maxMoveSpeed * groundFriction) / (-groundFriction + 1.0f);
        }
    }

    float airAcceleration {
        get {
            return (maxMoveSpeed * airFriction) / (-airFriction + 1.0f);
        }
    }

    float jumpStopMult {
        get {
            return 1.0f - jumpStopSpeed;
        }
    }

    public float directionMultiplier {
        get {
            float directionMult = 1.0f;
            if (direction == PlayerController.Direction.Right) {
                directionMult = 1.0f;
            }
            else {
                directionMult = -1.0f;
            }
            return directionMult;
        }
    }

    public WrenchController controlledWrench { get; private set; }
    public bool isMagnetingToWrench { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        controlledWrench = GetComponentInChildren<WrenchController>();
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        if(direction == Direction.Right) {
            playerModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        else {
            playerModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        Vector2 stickPos = new Vector2(-Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
        print(stickPos);
        if (stickPos.magnitude < 0.1f) {
            stickPos = new Vector2(-directionMultiplier, 0.0f);
        }
        float angle = Mathf.Atan2(stickPos.y, stickPos.x) * Mathf.Rad2Deg;
        Vector2 capsuleBottomOrigin = stickPos.normalized * (maxTargetDistance / 2.0f);
        List<Collider2D> targetColliders = new List<Collider2D>(Physics2D.OverlapBoxAll((Vector2)transform.position - capsuleBottomOrigin, new Vector2(maxTargetDistance, maxTargetWidth), angle));
        visual.position = (Vector2)transform.position - capsuleBottomOrigin;
        visual.localScale = new Vector2(maxTargetDistance, maxTargetWidth);
        visual.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        float targetDistance = Mathf.Infinity;

        foreach(Collider2D collider in targetColliders) {
            if (collider.tag == "Screw" || collider.tag == "Attachable") {
                if (Vector2.Distance(transform.position, collider.transform.position) < targetDistance) {
                    targetDistance = Vector2.Distance(transform.position, collider.transform.position);
                    wrenchTarget = collider.gameObject;
                }
            }
        }

        if (wrenchTarget != null) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.ScrewPlayerRotation) {
                wrenchTarget = null;
            }
            if (wrenchTarget.transform.position.x > transform.position.x && direction == Direction.Left) {
                wrenchTarget = null;
            }
            if (wrenchTarget.transform.position.x < transform.position.x && direction == Direction.Right) {
                wrenchTarget = null;
            }
        }

        HandleInput();
    }

    void HandleInput() {
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }

        if (Input.GetButtonUp("Jump") && isJumping) {
            isJumping = false;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y * jumpStopMult);
        }

        if (Input.GetButtonDown("Fire1")) {
            Vector2 throwPosition;
            if(wrenchTarget != null) {
                throwPosition = wrenchTarget.transform.position;
                //throwPosition = (Vector2)transform.position + (Vector2.right * throwDistance * directionMultiplier) + (Vector2.up * throwDistance * (Input.GetAxis("Vertical") / 2.0f));
            }
            else {
                throwPosition = (Vector2)transform.position + (Vector2.right* throwDistance * directionMultiplier) + (Vector2.up * throwDistance * (Input.GetAxis("Vertical") / 2.0f));
            }
            
            controlledWrench.Throw(throwPosition);
        }

        if (Input.GetButtonUp("Fire1")) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.ScrewPlayerRotation) {
                rigidbody.velocity = (directionMultiplier * Vector2.up * transform.right.y * screwReleaseSpeedY) + (directionMultiplier * Vector2.right * transform.right.x * screwReleaseSpeedX);
                controlledWrench.ParentToPlayer();
            }
        }

        if (!Input.GetButton("Fire1")) {
            controlledWrench.ForceReturn();
        }

        if (Input.GetButton("Fire2")) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.OnScrew)
                isMagnetingToWrench = true;
        }
        else {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.OnScrew)
                isMagnetingToWrench = false;
        }

        if (Input.GetAxis("Horizontal") > 0.0f)
            direction = Direction.Right;

        if (Input.GetAxis("Horizontal") < 0.0f)
            direction = Direction.Left;
    }

    void FixedUpdate() {
        if (!isMagnetingToWrench && controlledWrench.wrenchState != WrenchController.WrenchState.ScrewPlayerRotation) {
            rigidbody.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), 0.25f).eulerAngles.z;

            ContactPoint2D? groundContactPoint = GetGroundContactPoint();
            if (groundContactPoint != null && groundContactPoint.Value.collider != collider) {
                //Set gravity to zero if falling
                if (rigidbody.velocity.y < 0.0f)
                    rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0.0f);
                //Move based on input
                rigidbody.velocity += new Vector2(groundAcceleration * Input.GetAxis("Horizontal"), 0.0f);
                //Apply friction
                rigidbody.velocity += new Vector2(-rigidbody.velocity.x * groundFriction, 0.0f);
            }
            else {
                //Apply gravity
                rigidbody.velocity += new Vector2(0.0f, -gravitySpeed);
                //Move based on input
                rigidbody.velocity += new Vector2(airAcceleration * Input.GetAxis("Horizontal"), 0.0f);
                //Apply friction
                rigidbody.velocity += new Vector2(-rigidbody.velocity.x * airFriction, 0.0f);
            }

            if (rigidbody.velocity.y <= 0.0f)
                isJumping = false;
        }

        if(isMagnetingToWrench) {
            if (controlledWrench.wrenchState != WrenchController.WrenchState.OnScrew)
                isMagnetingToWrench = false;

            Vector2 direction = GetDirectionTowardsLocation(controlledWrench.transform.position);
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, direction * maxMagnetToWrenchSpeed, magnetToWrenchAcceleration);
            if (controlledWrench.wrenchState == WrenchController.WrenchState.ScrewPlayerRotation) {
                rigidbody.velocity = Vector2.zero;
                transform.rotation = controlledWrench.transform.rotation * Quaternion.Euler(Vector3.forward * -90.0f);
                transform.parent = controlledWrench.transform;
                transform.localPosition = new Vector2(-5.0f, 0.0f);
            }
        }
    }
    
    public float GetHalfHeight() {
        return (collider.size.y / 2.0f) * transform.lossyScale.y;
    }

    public Vector2 GetBottomOrigin() {
        return transform.position - new Vector3(0.0f, GetHalfHeight());
    }

    public ContactPoint2D? GetGroundContactPoint() {
        bool foundGroundPoint = false;
        ContactPoint2D returnPoint = new ContactPoint2D();

        //Get contact points on collider
        List<ContactPoint2D> contacts  = new List<ContactPoint2D>();
        collider.GetContacts(contacts);

        foreach(ContactPoint2D contactPoint in contacts) {
            //Check if contact point is on bottom
            if (contactPoint.point.y < transform.position.y) {
                returnPoint = contactPoint;
                foundGroundPoint = true;
            }
        }

        if (foundGroundPoint) {
            return returnPoint;
        }
        else {
            return null;
        }
    }

    public void Jump() {
        if (GetGroundContactPoint().HasValue) {
            isJumping = true;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpStrength);
        }
    }

    public Vector3 GetDirectionTowardsLocation(Vector3 Location) {
        return (Location - transform.position).normalized;
    }
}
