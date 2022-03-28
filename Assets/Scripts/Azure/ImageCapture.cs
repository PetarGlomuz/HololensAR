using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;


public class ImageCapture : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static ImageCapture Instance;

    /// <summary>
    /// Keep counts of the taps for image renaming
    /// </summary>
    private int captureCount = 0;

    /// <summary>
    /// Photo Capture object
    /// </summary>
    private UnityEngine.Windows.WebCam.PhotoCapture photoCaptureObject = null;

    Texture2D targetTexture = null;

    /// <summary>
    /// Allows gestures recognition in HoloLens
    /// </summary>
    /// removed by Petar 2022.03.21
    //private GestureRecognizer recognizer;

    /// <summary>
    /// Flagging if the capture loop is running
    /// </summary>
    internal bool captureIsActive;

    /// <summary>
    /// File path of current analysed photo
    /// </summary>
    internal string filePath = string.Empty;

    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Runs at initialization right after Awake method
    /// </summary>
    public void StartAction()
    {
        // Clean up the LocalState folder of this application from all photos stored
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Debug.LogFormat("Cannot delete file: ", file.Name);
            }
        }

        // Subscribing to the Microsoft HoloLens API gesture recognizer to track user gestures
        /*recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();*/

        //To do the Azure Detect
        StartDetectingAction();        
    }

    public void StartDetectingAction()
    {
        if (!Global.gIsDetectingStarted)
        {
            Global.gIsDetectingStarted = true;
            LogManager.Instance.ShowLogStr("Detecting..", true);
            Invoke("ExecuteImageCaptureAndAnalysis", 0);
        }
    }
    

    /// <summary>
    /// Begin process of image capturing and send to Azure Custom Vision Service.
    /// </summary>
    private void ExecuteImageCaptureAndAnalysis()
    {
        SceneOrganiser.Instance.PlaceAnalysisLabel();

        // Set the camera resolution to be the highest possible
        
        Resolution cameraResolution = UnityEngine.Windows.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending
            ((res) => res.width * res.height).Last();

        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        
        // Begin capture process, set the image format
        UnityEngine.Windows.WebCam.PhotoCapture.CreateAsync(true, delegate (UnityEngine.Windows.WebCam.PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;

            UnityEngine.Windows.WebCam.CameraParameters camParameters = new UnityEngine.Windows.WebCam.CameraParameters
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = targetTexture.width,
                cameraResolutionHeight = targetTexture.height,
                pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32
            };

            // Capture the image from the camera and save it in the App internal folder
            captureObject.StartPhotoModeAsync(camParameters, delegate (UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
            {
                string old_filename = string.Format(@"CapturedImage{0}.jpg", captureCount);
                if (File.Exists(old_filename))
                {
                    File.Delete(old_filename);
                }

                captureCount++;
                string filename = string.Format(@"CapturedImage{0}.jpg", captureCount);
                
                filePath = Path.Combine(Application.persistentDataPath, filename);

                //photoCaptureObject.TakePhotoAsync(filePath, UnityEngine.Windows.WebCam.PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        });
    }

    /// <summary>
    /// Register the full execution of the Photo Capture. 
    /// </summary>
    void OnCapturedPhotoToDisk(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        try
        {
            // Call StopPhotoMode once the image has successfully captured
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        catch (Exception e)
        {
            Debug.LogFormat("Exception capturing photo to disk: {0}", e.Message);
        }
    }

    void OnCapturedPhotoToMemory(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.Windows.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    /// <summary>
    /// The camera photo mode has stopped after the capture.
    /// Begin the image analysis process.
    /// </summary>
    void OnStoppedPhotoMode(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        Debug.LogFormat("Stopped Photo Mode");
        // Dispose from the object in memory and request the image analysis 
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        // Call the image analysis
        StartCoroutine(CustomVisionAnalyser.Instance.AnalyseLastImageCaptured(targetTexture));
    }

    /// <summary>
    /// Stops all capture pending actions
    /// </summary>
    internal void ResetImageCapture()
    {
        LogManager.Instance.ShowLogStr("Detecting..", true);
        Invoke("ExecuteImageCaptureAndAnalysis", 0);
    }

    internal void StopImageCapture()
    {
        // Set the cursor color to green
        //SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.green;

        // Stop the capture loop if active
        CancelInvoke();
        //Show the buttons for Private mode and Video play
        //GameObject.Find("Manager").GetComponent<MainUIManager>().LabelStart.SetActive(false);
        //GameObject.Find("Manager").GetComponent<MainUIManager>().ConnectWnd.SetActive(true);
        //GameObject.Find("Manager").GetComponent<MainUIManager>().OnJoinedSuccess();
    }
}
