using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchController : MonoBehaviour
{
    private PlayerController owningPlayer;
    public bool isWithPlayer { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        owningPlayer = GetComponentInParent<PlayerController>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
