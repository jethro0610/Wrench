using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    new BoxCollider2D collider;
    new Rigidbody2D rigidbody;
    public Animator animator { get; private set; }

    public enum Direction { Right, Left };
    public Direction direction { get; private set; } = Direction.Right;

    [SerializeField]
    GameObject playerModel;

    [SerializeField]
    float maxMoveSpeed = 150.0f,
    groundFriction = 0.3f,
    airFriction = 0.05f,
    gravitySpeed = 10,
    jumpStrength = 250.0f,
    jumpStopSpeed = 0.8f,
    targetWidthMouse = 10.0f,
    targetWidthController = 50.0f,
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

    [SerializeField]
    Camera camera;

    bool useController;
    Vector2 aimVector;

    public WrenchController controlledWrench { get; private set; }
    public bool isMagnetingToWrench { get; private set; }

    bool toggleJumpAnim;
    bool toggleThrowAnim;

    // Start is called before the first frame update
    void Start()
    {
        controlledWrench = GetComponentInChildren<WrenchController>();
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        if(direction == Direction.Right) {
            playerModel.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        }
        else {
            playerModel.transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        }

        if (useController) {
            wrenchTarget = GetWrenchTargetController();
        }
        else {
            wrenchTarget = GetWrenchTargetMouse();
        }

        if (wrenchTarget != null) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.GrabAttach) {
                wrenchTarget = null;
            }
            else if(Vector2.Distance(transform.position, wrenchTarget.transform.position) > maxTargetDistance) {
                wrenchTarget = null;
            }
            else if (useController) {
                if(transform.position.x > wrenchTarget.transform.position.x && direction == Direction.Right) {
                    wrenchTarget = null;
                }
                else if (transform.position.x < wrenchTarget.transform.position.x && direction == Direction.Left) {
                    wrenchTarget = null;
                }
            }
        }

        HandleInput();

        animator.SetFloat("Move Speed", Mathf.Abs(Input.GetAxis("Horizontal")));
        animator.SetBool("On Ground", GetGroundContactPoint().HasValue);
        animator.SetBool("Grab", isMagnetingToWrench || controlledWrench.wrenchState == WrenchController.WrenchState.GrabAttach);
        animator.SetFloat("Gravity", rigidbody.velocity.y);
        animator.SetBool("Jump", isJumping);
        if (toggleJumpAnim == true) {
            //animator.SetBool("Jump", false);
            toggleJumpAnim = false;
        }

        if (toggleThrowAnim == true) {
            animator.SetBool("Throw", false);
            toggleThrowAnim = false;
        }

        //if (animator.GetBool("Jump") == true)
            //toggleJumpAnim = true;

        if (animator.GetBool("Throw") == true)
            toggleThrowAnim = true;
    }

    void HandleInput() {
        if(Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f) {
            useController = false;
        }

        if(Mathf.Abs(Input.GetAxis("Aim X")) > 0.1f) {
            useController = true;
        }

        if (useController) {
            aimVector = new Vector2(Input.GetAxis("Aim X"), -Input.GetAxis("Aim Y"));
            if(Mathf.Abs(aimVector.x) < 0.1f && Mathf.Abs(aimVector.y) < 0.1f) {
                aimVector.x = directionMultiplier;
            }
        }
        else {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            aimVector = (mousePosition - (Vector2)transform.position).normalized;
        }

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
            }
            else {
                throwPosition = (Vector2)transform.position + (aimVector * throwDistance);
            }
            
            controlledWrench.Throw(throwPosition);
        }

        if (Input.GetButtonUp("Fire1")) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.GrabAttach) {
                if(controlledWrench.attachedObject.tag == "Screw")
                    rigidbody.velocity = (directionMultiplier * Vector2.up * transform.right.y * screwReleaseSpeedY) + (directionMultiplier * Vector2.right * transform.right.x * screwReleaseSpeedX);

                controlledWrench.ParentToPlayer();
            }
        }

        if (!Input.GetButton("Fire1")) {
            controlledWrench.ForceReturn();
        }

        if (Input.GetButton("Fire2")) {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.Attached)
                isMagnetingToWrench = true;
        }
        else {
            if (controlledWrench.wrenchState == WrenchController.WrenchState.Attached)
                isMagnetingToWrench = false;
        }

        if (controlledWrench.wrenchState != WrenchController.WrenchState.GrabAttach) {
            if (Input.GetAxis("Horizontal") > 0.0f)
                direction = Direction.Right;

            if (Input.GetAxis("Horizontal") < 0.0f)
                direction = Direction.Left;
        }
        else {
            if (aimVector.x > 0.0f)
                direction = Direction.Right;

            if (aimVector.x < 0.0f)
                direction = Direction.Left;
        }
    }

    void FixedUpdate() {
        if (!isMagnetingToWrench && controlledWrench.wrenchState != WrenchController.WrenchState.GrabAttach) {
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
            if (controlledWrench.attachedObject.tag == "Screw") {
                rigidbody.rotation = Quaternion.Lerp(transform.rotation, GetLookRotation2D(controlledWrench.transform.position) * Quaternion.Euler(0.0f, 0.0f, -90.0f), magnetToWrenchAcceleration).eulerAngles.z;
            }
            else {
                rigidbody.rotation = Quaternion.Lerp(transform.rotation, controlledWrench.transform.rotation * Quaternion.Euler(0.0f, 0.0f, -90.0f), magnetToWrenchAcceleration).eulerAngles.z;
            }

            if (controlledWrench.wrenchState != WrenchController.WrenchState.Attached)
                isMagnetingToWrench = false;

            Vector2 direction = GetDirectionTowardsLocation(controlledWrench.transform.position);
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, direction * maxMagnetToWrenchSpeed, magnetToWrenchAcceleration);

            if (controlledWrench.wrenchState == WrenchController.WrenchState.GrabAttach) {
                rigidbody.velocity = Vector2.zero;
                transform.rotation = controlledWrench.transform.rotation * Quaternion.Euler(Vector3.forward * -90.0f);
                transform.parent = controlledWrench.transform;
                transform.localPosition = new Vector2(controlledWrench.holdOffset.x, 0.0f);
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
            //animator.SetBool("Jump", true);
            isJumping = true;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpStrength);
        }
    }

    public Vector3 GetDirectionTowardsLocation(Vector3 Location) {
        return (Location - transform.position).normalized;
    }

    public Quaternion GetLookRotation2D(Vector2 lookPosition) {
        Vector2 direction = GetDirectionTowardsLocation(lookPosition);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return newRotation;
    }

    GameObject GetWrenchTargetMouse() {
        GameObject returnTarget = null;
        Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(-aimVector.y, -aimVector.x) * Mathf.Rad2Deg;
        Vector2 capsuleBottomOrigin = -aimVector.normalized * (maxTargetDistance / 2.0f);
        List<Collider2D> targetColliders = new List<Collider2D>(Physics2D.OverlapBoxAll((Vector2)transform.position - capsuleBottomOrigin, new Vector2(maxTargetDistance, targetWidthMouse), angle));
        float targetDistance = Mathf.Infinity;

        foreach (Collider2D collider in targetColliders) {
            if (collider.tag == "Screw" || collider.tag == "Attachable") {
                if (Vector2.Distance(mousePosition, collider.transform.position) < targetDistance) {
                    targetDistance = Vector2.Distance(transform.position, collider.transform.position);
                    returnTarget = collider.gameObject;
                }
            }
        }
        return returnTarget;
    }

    GameObject GetWrenchTargetController() {
        GameObject returnTarget = wrenchTarget;
        float angle = Mathf.Atan2(-aimVector.normalized.y, -aimVector.normalized.x) * Mathf.Rad2Deg;
        Vector2 capsuleBottomOrigin = -aimVector.normalized * (maxTargetDistance / 2.0f);
        List<Collider2D> targetColliders = new List<Collider2D>(Physics2D.OverlapBoxAll((Vector2)transform.position - capsuleBottomOrigin, new Vector2(maxTargetDistance, targetWidthController), angle));
        float targetDistance = Mathf.Infinity;

        foreach (Collider2D collider in targetColliders) {
            if (collider.tag == "Screw" || collider.tag == "Attachable") {
                if (Vector2.Distance(transform.position, collider.transform.position) < targetDistance) {
                    targetDistance = Vector2.Distance(transform.position, collider.transform.position);
                    returnTarget = collider.gameObject;
                }
            }
        }
        return returnTarget;
    }
}
