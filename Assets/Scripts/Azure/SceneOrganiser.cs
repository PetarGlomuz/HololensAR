using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections;
using System.IO;

public class SceneOrganiser : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static SceneOrganiser Instance;

    /// <summary>
    /// The cursor object attached to the Main Camera
    /// </summary>
    internal GameObject cursor;

    /// <summary>
    /// The label used to display the analysis on the objects in the real world
    /// </summary>
    public GameObject AppearingObj;
    
    

    /// <summary>
    /// Reference to the last Label positioned
    /// </summary>
    internal Transform lastLabelPlaced;

    /// <summary>
    /// Reference to the last Label positioned
    /// </summary>
    internal TextMesh lastLabelPlacedText;

    /// <summary>
    /// Current threshold accepted for displaying the label
    /// Reduce this value to display the recognition more often
    /// </summary>
    public float probabilityThreshold;

    /// <summary>
    /// The quad object hosting the imposed image captured
    /// </summary>
    private GameObject quad;

    /// <summary>
    /// Renderer of the quad object
    /// </summary>
    internal Renderer quadRenderer;

    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        // Use this class instance as singleton
        Instance = this;

        // Add the ImageCapture class to this Gameobject
        gameObject.AddComponent<ImageCapture>();

        // Add the CustomVisionAnalyser class to this Gameobject
        gameObject.AddComponent<CustomVisionAnalyser>();

        // Add the CustomVisionObjects class to this Gameobject
        gameObject.AddComponent<CustomVisionObjects>();
    }
    Texture2D _content;

    public void StartAction()
    {
        //for test
        /*_content = LoadPNG("1.jpg");
        if(_content != null)
            StartCoroutine(CustomVisionAnalyser.Instance.AnalyseLastImageCaptured(_content));*/
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Instantiate a Label in the appropriate location relative to the Main Camera.
    /// </summary>
    public void PlaceAnalysisLabel()
    {
    }

    /// <summary>
    /// Set the Tags as Text of the last label created. 
    /// </summary>
    public void FinaliseLabel(string jsonStr)
    {
        if (Global.gDetectingSkipped)
        {
            ImageCapture.Instance.StopImageCapture();
        }

        try
        {
            var jsonObj = JSON.Parse(jsonStr);
            if (jsonObj["predictions"] != null && jsonObj["predictions"].Count > 0)
            {       
                List<Prediction> originalPredictions = new List<Prediction>();

                for (int i = 0; i < jsonObj["predictions"].Count; i++)
                {
                    Prediction newPro = new Prediction();
                    newPro.probability = Double.Parse(jsonObj["predictions"][i]["probability"].ToString());
                    newPro.tagId = jsonObj["predictions"][i]["tagId"].ToString();
                    newPro.tagName = jsonObj["predictions"][i]["tagName"].ToString();
                    BoundingBox newBound = new BoundingBox();
                    try
                    {
                        newBound.left = Double.Parse(jsonObj["predictions"][i]["boundingBox"]["left"].ToString());
                        newBound.top = Double.Parse(jsonObj["predictions"][i]["boundingBox"]["top"].ToString());
                        newBound.width = Double.Parse(jsonObj["predictions"][i]["boundingBox"]["width"].ToString());
                        newBound.height = Double.Parse(jsonObj["predictions"][i]["boundingBox"]["height"].ToString());
                    }
                    catch(Exception ex)
                    {

                    }

                    newPro.boundingBox = newBound;

                    originalPredictions.Add(newPro);
                }

                // Sort the predictions to locate the highest one
                List<Prediction> sortedPredictions = new List<Prediction>();
                sortedPredictions = originalPredictions.OrderBy(p => p.probability).ToList();
                Prediction bestPrediction = new Prediction();

                bestPrediction = sortedPredictions[sortedPredictions.Count - 1];
                
                if (bestPrediction.probability > probabilityThreshold)
                {
                    //quadRenderer = quad.GetComponent<Renderer>() as Renderer;
                    Bounds quadBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

                    if (bestPrediction.tagName.Contains("black box") && !Global.gBlackBoxDetected)
                    {
                        LogManager.Instance.ShowLogStr("Detect black box, prediction : " + bestPrediction.probability, false);
                        Global.gBlackBoxDetected = true;
                    }
                    else if(bestPrediction.tagName.Contains("bear") && !Global.gBearDetected)
                    {
                        LogManager.Instance.ShowLogStr("Detect bear, prediction : " + bestPrediction.probability, false);
                        Global.gBearDetected = true;
                    }

                    transform.GetComponent<MainManager>().OnObjDetected(bestPrediction.tagName);

                    if (Global.gBlackBoxDetected && Global.gBearDetected)
                    {
                        ImageCapture.Instance.StopImageCapture();
                    }
                    else
                    {
                        ImageCapture.Instance.ResetImageCapture();
                    }
                }
                else
                {
                    ImageCapture.Instance.ResetImageCapture();
                }
            }
            // Reset the color of the cursor
            //cursor.GetComponent<Renderer>().material.color = Color.green;
        }
        catch(Exception ex)
        {
            LogManager.Instance.ShowLogStr("FinaliseLabel exception : " + ex.Message, true);
        }
    }

    IEnumerator HideCubeObj()
    {
        yield return new WaitForSeconds(5f);

        Destroy(lastLabelPlaced.gameObject);
        //Destroy(quad.gameObject);
    }
    /// <summary>
    /// This method hosts a series of calculations to determine the position 
    /// of the Bounding Box on the quad created in the real world
    /// by using the Bounding Box received back alongside the Best Prediction
    /// </summary>
    public Vector3 CalculateBoundingBoxPosition(Bounds b, BoundingBox boundingBox)
    {
        Debug.Log($"BB: left {boundingBox.left}, top {boundingBox.top}, width {boundingBox.width}, height {boundingBox.height}");

        double centerFromLeft = boundingBox.left + (boundingBox.width / 2);
        double centerFromTop = boundingBox.top + (boundingBox.height / 2);
        Debug.Log($"BB CenterFromLeft {centerFromLeft}, CenterFromTop {centerFromTop}");

        double quadWidth = 1; //b.size.normalized.x;
        double quadHeight = 1; //b.size.normalized.y;
        Debug.Log($"Quad Width {quadWidth}, Quad Height {quadHeight}");

        double normalisedPos_X = (quadWidth * centerFromLeft) - (quadWidth / 2);
        //double normalisedPos_Y = (quadHeight * centerFromTop) - (quadHeight / 2);
        double normalisedPos_Y = (quadHeight / 2) - (quadHeight * centerFromTop);

        return new Vector3((float)normalisedPos_X, (float)normalisedPos_Y, 0);
    }

    // Start is called before the first frame update
}
