using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    new BoxCollider2D collider;
    new Rigidbody2D rigidbody;

    [SerializeField]
    float maxMoveSpeed = 150.0f,
    groundFriction = 0.75f,
    airFriction = 0.15f,
    gravitySpeed = 10;

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

    public WrenchController controlledWrench { get; private set; }

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
        
    }

    void FixedUpdate() {
        ContactPoint2D? groundContactPoint = GetGroundContactPoint();
        if (groundContactPoint != null && groundContactPoint.Value.collider != collider) {
            //Set gravity to zero
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
}
