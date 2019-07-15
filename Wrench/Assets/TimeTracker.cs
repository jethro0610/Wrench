using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeTracker : MonoBehaviour
{
    float subtractTime;

    [SerializeField]
    GameObject menuText,
    quitText;
    // Start is called before the first frame update
    void Start()
    {
        print(GameObject.FindGameObjectsWithTag("Time Tracker").Length);
        if (GameObject.FindGameObjectsWithTag("Time Tracker").Length > 1) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            menuText.active = false;
            if(Application.platform != RuntimePlatform.WebGLPlayer)
                quitText.active = true;
        }
        else {
            menuText.active = true;
            quitText.active = false;
        }


        if (Input.GetButtonDown("Fire1")) {
            if(SceneManager.GetActiveScene().buildIndex == 0)
                SceneManager.LoadScene(1);
        }

        if (Input.GetButtonDown("Cancel")) {
            if (SceneManager.GetActiveScene().buildIndex == 0) {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                    Application.Quit();
            }
            else {
                SceneManager.LoadScene(0);
                ResetTimer();
            }
        }
    }

    public void ResetTimer() {
        subtractTime = Time.time;
    }

    public float GetPlayTime() {
        return Time.time - subtractTime;
    }
}
