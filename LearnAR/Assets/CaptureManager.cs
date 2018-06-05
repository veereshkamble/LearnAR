using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

public class CaptureManager : MonoBehaviour
{
    //static public int imageCount = 0;

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    GameObject displayObject = null;
    [SerializeField]
    DisplayManager dsp = null;

    private float lastCalled = 0;
    private float debounce = 0.005f;

    /**
     * Captures a photo using the HoloLens and displays it
     * 
     * Currently not used/should not be used because it is too difficult to see
     * where the camera is pointing when taking pictures
     * 
     * COMMAND: circuit capture
     **/ 
    public void CaptureCircuitAndSave()
    {
        if((Time.time - lastCalled) < debounce)
        {
            return;
        }

        lastCalled = Time.time;
        //Resolution cameraResolution = PhotoCapture.SupportedResolutions;
        Debug.Log("CaptureCircuitAndSave");
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = 896;
            cameraParameters.cameraResolutionHeight = 504;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {

                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

            });
        });
    }

    /**
     * Callback for StartPhotoModeAsync in CaptureCircuitAndSave
     * Displays the image on the canvas and destroys the current photoCapture object
     **/
    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            targetTexture = new Texture2D(896, 504);

            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            dsp.DisplayCircuit(targetTexture);
        }

        // Deactivate our camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    /**
     * Callback for StopPhotoModeAsync
     * used for cleanup
     **/ 
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}