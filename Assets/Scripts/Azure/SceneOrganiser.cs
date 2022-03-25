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
        /*_content = LoadPNG("3.jpg");
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
        //lastLabelPlaced = Instantiate(AppearingObj.transform, cursor.transform.position, transform.rotation);
        lastLabelPlaced = Instantiate(AppearingObj.transform, new Vector3(0,0,0), transform.rotation);
        lastLabelPlaced.transform.localScale = new Vector3(0f, 0f, 0f);

        /*quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadRenderer = quad.GetComponent<Renderer>() as Renderer;

        Material m = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        quadRenderer.material = m;
        // Here you can set the transparency of the quad. Useful for debugging
        float transparency = 0f;
        quadRenderer.material.color = new Color(1, 1, 1, transparency);

        quad.transform.parent = transform;
        quad.transform.rotation = transform.rotation;
        // The quad is positioned slightly forward in font of the user
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 2.46f);

        // The quad scale as been set with the following value following experimentation,  
        // to allow the image on the quad to be as precisely imposed to the real world as possible
        quad.transform.localScale = new Vector3(4.616f, 3.458f, 1f);//new Vector3(5.2f, 3.9f, 1f);
        quad.transform.parent = null;*/

        lastLabelPlaced.gameObject.SetActive(false);
        /*try
        {
            lastLabelPlaced = Instantiate(label.transform, cursor.transform.position, transform.rotation);
            lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
            lastLabelPlacedText.text = "";
            lastLabelPlaced.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            // Create a GameObject to which the texture can be applied
            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadRenderer = quad.GetComponent<Renderer>() as Renderer;
            
            Material m = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            quadRenderer.material = m;
            // Here you can set the transparency of the quad. Useful for debugging
            float transparency = 0f;
            quadRenderer.material.color = new Color(1, 1, 1, transparency);
            
            // Set the position and scale of the quad depending on user position
            quad.transform.parent = transform;
            quad.transform.rotation = transform.rotation;
            // The quad is positioned slightly forward in font of the user
            quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
            
            // The quad scale as been set with the following value following experimentation,  
            // to allow the image on the quad to be as precisely imposed to the real world as possible
            quad.transform.localScale = new Vector3(3f, 1.65f, 1f);
            quad.transform.parent = null;
        }
        catch(Exception ex)
        {
            Debug.Log("PlaceAnalysisLabel exception : " + ex.Message);
            LogManager.Instance.ShowLogStr("PlaceAnalysisLabel exception : " + ex.Message);

        }*/
    }

    /// <summary>
    /// Set the Tags as Text of the last label created. 
    /// </summary>
    public void FinaliseLabel(string jsonStr/*AnalysisRootObject analysisObject*/)
    {
        if (Global.gDetectingSkipped)
        {
            ImageCapture.Instance.StopImageCapture();
        }

        //for test
        //PlaceAnalysisLabel();
        try
        {
            var jsonObj = JSON.Parse(jsonStr);
            if (jsonObj["predictions"] != null && jsonObj["predictions"].Count > 0)
            {       
                //lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();

                List<Prediction> originalPredictions = new List<Prediction>();
                //LogManager.Instance.ShowLogStr("Prediction Count : " + jsonObj["predictions"].Count, true);
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
                //Debug.Log("Best Prediction : " + bestPrediction.probability.ToString());
                //LogManager.Instance.ShowLogStr("33333333");
                //LogManager.Instance.ShowLogStr("Best Prediction : " + bestPrediction.probability.ToString(), true);
                if (bestPrediction.probability > probabilityThreshold)
                {
                    //quadRenderer = quad.GetComponent<Renderer>() as Renderer;
                    Bounds quadBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));//quadRenderer.bounds; //new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));//quadRenderer.bounds;

                    // Position the label as close as possible to the Bounding Box of the prediction 
                    // At this point it will not consider depth
                    lastLabelPlaced.transform.parent = null;//quad.transform;
                    lastLabelPlaced.transform.localPosition = CalculateBoundingBoxPosition(quadBounds, bestPrediction.boundingBox);
                    // Set the tag text
                    //lastLabelPlacedText.text = bestPrediction.tagName;

                    if (bestPrediction.tagName.Contains("black box"))
                    {
                        Debug.Log("----Mobile is detected----");
                    }
                    // Cast a ray from the user's head to the currently placed label, it should hit the object detected by the Service.
                    // At that point it will reposition the label where the ray HL sensor collides with the object,
                    // (using the HL spatial tracking)
                    

                    Vector3 headPosition = Camera.main.transform.position;
                    RaycastHit objHitInfo;
                    Vector3 objDirection = lastLabelPlaced.position;
                    if (Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
                    {
                        lastLabelPlaced.position = objHitInfo.point;
                    }

                    lastLabelPlaced.gameObject.SetActive(true);

                    double objWidth = bestPrediction.boundingBox.width;
                    double objHeight = bestPrediction.boundingBox.height;

                    //4.616f, 3.458f

                    lastLabelPlaced.transform.localScale = new Vector3(1, 1f, 1);
                    //lastLabelPlaced.transform.localScale = new Vector3(1, 4.616f / 3.458f, 1);
                    //lastLabelPlaced.Find("Cube").transform.localScale = new Vector3((float)(objWidth / 4), (float)(objHeight / 4), 0.2f);

                    /*if (objWidth > objHeight)
                        lastLabelPlaced.Find("Plane").transform.localScale = new Vector3((float)(objHeight / 2) / 30, 1, (float)(objHeight / 2) / 30);
                    else
                        lastLabelPlaced.Find("Plane").transform.localScale = new Vector3((float)(objWidth / 2) / 30, 1, (float)(objWidth / 2) / 30);*/

                    //StartCoroutine("HideCubeObj");
                    // Stop the analysis process

                    Debug.Log("Repositioning Label : " + bestPrediction.tagName + " , " + bestPrediction.probability);
                    ImageCapture.Instance.StopImageCapture();
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
