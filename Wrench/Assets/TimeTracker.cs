using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    float subtractTime;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetTimer() {
        subtractTime = Time.time;
    }

    public float GetPlayTime() {
        return Time.time - subtractTime;
    }
}
