using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public GameObject blackBoxObj;
    public GameObject bearObj;

    private void Awake()
    {
        blackBoxObj.SetActive(false);
        bearObj.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        Global.gStartPosObj = Instantiate(Camera.main.GetComponent<SceneOrganiser>().AppearingObj, new Vector3(0, 0, 0), Quaternion.identity);
        Global.gStartPosObj.transform.localScale = new Vector3(1f, 1f, 1f);
        Global.gStartPosObj.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.GetComponent<ImageCapture>().StartAction();
        Camera.main.GetComponent<SceneOrganiser>().StartAction();
    }

    public void OnExitClick()
    {
        Debug.Log("---Quit clicked---");
        Application.Quit();
    }

    public void OnObjDetected(string objName)
    {
        if(objName.Contains("bear"))
        {
            bearObj.transform.parent = Global.gStartPosObj.transform;
            bearObj.transform.localPosition = new Vector3(0f, 0, 0.8f);
            bearObj.SetActive(true);
        }
        else if(objName.Contains("black"))
        {
            blackBoxObj.transform.parent = Global.gStartPosObj.transform;
            blackBoxObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            blackBoxObj.SetActive(true);
        }
    }
}
