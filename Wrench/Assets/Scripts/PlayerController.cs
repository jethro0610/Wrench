using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    new BoxCollider2D collider;
    new Rigidbody2D rigidbody;

    [SerializeField]
    float maxGroundSpeed,
    friction,
    gravitySpeed,
    groundDistance;

    float groundAcceleration {
        get {
            return (maxGroundSpeed * friction) / (-friction + 1.0f);
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
        RaycastHit2D initialGroundRaycast = GetGroundRaycast();
        if (initialGroundRaycast && initialGroundRaycast.collider != collider) {
            //Stick to ground
            transform.position = new Vector2(transform.position.x, initialGroundRaycast.point.y + GetHalfHeight());
            //Set gravity to zero
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0.0f);
            //Move based on input
            rigidbody.velocity += new Vector2(groundAcceleration * Input.GetAxis("Horizontal"), 0.0f);
            //Apply friction
            rigidbody.velocity += new Vector2(-rigidbody.velocity.x * friction, 0.0f);
        }
        else {
            rigidbody.velocity += new Vector2(0.0f, -gravitySpeed);
        }
    }
    
    public float GetHalfHeight() {
        return (collider.size.y / 2.0f) * transform.lossyScale.y;
    }

    public Vector2 GetBottomOrigin() {
        return transform.position - new Vector3(0.0f, GetHalfHeight());
    }

    public RaycastHit2D GetGroundRaycast() {
        Debug.DrawRay(GetBottomOrigin(), -transform.up);
        return Physics2D.Raycast(GetBottomOrigin(), -transform.up, groundDistance);
    }
}
