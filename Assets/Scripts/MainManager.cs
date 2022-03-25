using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.GetComponent<ImageCapture>().StartAction();
        Camera.main.GetComponent<SceneOrganiser>().StartAction();
    }

    public void OnExitClick()
    {
        Debug.Log("---Quit clicked---");
        Application.Quit();
    }
}
