using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed;

    [SerializeField]
    string levelName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).localRotation *= Quaternion.Euler(Vector3.forward * Time.deltaTime * rotationSpeed);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Player") {
            print("Blueprint Get");
        }
    }
}
