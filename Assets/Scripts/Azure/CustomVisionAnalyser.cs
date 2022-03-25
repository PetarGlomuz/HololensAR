using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class CustomVisionAnalyser : MonoBehaviour
{
    /// <summary>
    /// Unique instance of this class
    /// </summary>
    public static CustomVisionAnalyser Instance;

    /// <summary>
    /// Insert your prediction key here
    /// </summary>
    private string predictionKey = "11b7fddd4a2e4d178ebd15dc6fe4c0b4";//"- Insert your key here -";
                                    
    /// <summary>
    /// Insert your prediction endpoint here
    /// </summary>
    private string predictionEndpoint = "https://australiaeast.api.cognitive.microsoft.com/customvision/v3.0/Prediction/24ba26a7-a166-4754-a2b0-4fdd7e86a2d1/detect/iterations/Iteration1/image";
    /// <summary>
    /// Bite array of the image to submit for analysis
    /// </summary>
    [HideInInspector] public byte[] imageBytes;

    /// <summary>
    /// Initializes this class
    /// </summary>
    private void Awake()
    {
        // Allows this instance to behave like a singleton
        Instance = this;
    }

    /// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    public IEnumerator AnalyseLastImageCaptured(Texture2D targetTexture/*string imagePath*/)
    {
        Debug.Log("Analyzing...");
        //LogManager.Instance.ShowLogStr("222222");

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            // Gets a byte array out of the saved image
            /*byte[] bytes = targetTexture.EncodeToPNG();//GetImageAsByteArray(imagePath);
            string localURL = Application.persistentDataPath + "/screenshot.png";
            File.WriteAllBytes(localURL, bytes);

            byte[] bytes1 = targetTexture.EncodeToJPG();//GetImageAsByteArray(imagePath);
            string localURL1 = Application.persistentDataPath + "/screenshot_jpg.jpg";
            File.WriteAllBytes(localURL1, bytes1);*/

            Texture2D newScreenshot = ScaleTexture(targetTexture, 1024, 576);
            imageBytes = newScreenshot.EncodeToJPG();

            /*string filename = Application.persistentDataPath + "/screenshot1.png";
            File.WriteAllBytes(filename, imageBytes);*/
            //Debug.Log("File Path : " + localURL);

            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            // The upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            // The download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            Debug.Log("Uploading...");
            // Send the request
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;

            Debug.Log("response: " + jsonResponse);
            //LogManager.Instance.ShowLogStr("response: " + jsonResponse, true);
            // The response will be in JSON format, therefore it needs to be deserialized
            AnalysisRootObject analysisRootObject = new AnalysisRootObject();
            try
            {
                //analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);
                SceneOrganiser.Instance.FinaliseLabel(jsonResponse);
            }
            catch(Exception ex)
            {
                LogManager.Instance.ShowLogStr("json parse exception :" + ex.Message, true);
            }
        }
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
        float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
                              incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    /// <summary>
    /// Returns the contents of the specified image file as a byte array.
    /// </summary>
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

        BinaryReader binaryReader = new BinaryReader(fileStream);

        return binaryReader.ReadBytes((int)fileStream.Length);
    }
}
