using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeText : MonoBehaviour
{
    TimeTracker tracker;
    TextMesh textMesh;
    // Start is called before the first frame update
    void Start()
    {
        tracker = GameObject.FindWithTag("Time Tracker").GetComponent<TimeTracker>();
        textMesh = GetComponent<TextMesh>();

        int minutes = (int)Mathf.Floor(tracker.GetPlayTime() / 60);
        int seconds = Mathf.RoundToInt(tracker.GetPlayTime() % 60);

        string minutesString;
        string secondsString;

        minutesString = minutes.ToString();
        if (seconds < 10) {
            secondsString = "0" + seconds.ToString();
        }
        else {
            secondsString = seconds.ToString();
        }

        textMesh.text = "Your Time: " + minutesString + ":" + secondsString;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
