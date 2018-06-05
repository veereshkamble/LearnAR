using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

using Newtonsoft.Json;

public class DisplayManager : MonoBehaviour
{

    [SerializeField]
    private RawImage img = null;
    [SerializeField]
    private TextMesh t;
    [SerializeField]
    private AnimateCurrentManager am = null;

    Dictionary<string, float> components = new Dictionary<string, float>();

    private float lastCalled = 0;
    private float debounce = 0.5f;

    private string host = "http://192.168.137.16:5000";

    private string post_uri = "/circuit/image";
    private string get_image_uri = "/circuit/image/hololens";
    private string get_points_uri = "/circuit/evaluate";
    private string get_labels_img = "/circuit/image/labels";

    // Update is called once per frame
    void Update()
    {

    }

    /**
     * Sends the circuit image to the REST server via POST request
     * Expects to receive a JSON Object with data about the circuit
     * 
     * NOTE: This will not be used in its current state - cannot see where 
     * you're taking the picture (too difficult) Use mobile application instead
     **/
    IEnumerator SendCircuitImage(string rawImage)
    {

        StringBuilder str = new StringBuilder();
        Dictionary<string, string> jsonObj = new Dictionary<string, string>();
        //jsonObj.Add("image", rawImage.ToString());
        jsonObj.Add("image", rawImage);
        string jsonStr = JsonConvert.SerializeObject(jsonObj);

        str.Append(jsonStr);

        var req = new UnityWebRequest(host + post_uri, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonStr);
        req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if(!req.isNetworkError)
        {

            str.Append(req.responseCode);
            str.Append(req.downloadHandler.text);
            //t.text = req.downloadHandler.text;
            if (req.downloadHandler.text == "")
            {
                str.Append("it's empty");
            }
            t.text = str.ToString();
        }
        else
        {
            t.text = "there was an error!";
        }
    }

    /**
     *  GET request to REST server to retrieve the corner points to animate the current flow
     **/
    IEnumerator evaluateCircuitConnections()
    {
        StringBuilder str = new StringBuilder();
        using (UnityWebRequest www = UnityWebRequest.Get(host + get_points_uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                str.Append(www.error);
            }
            else
            {
                // Show results as text
                string result = www.downloadHandler.text;
                str.Append(result);

                Dictionary<string, string> res = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                float xmin = System.Convert.ToSingle(res["xmin"]);
                float xmax = System.Convert.ToSingle(res["xmax"]);
                float ymin = System.Convert.ToSingle(res["ymin"]);
                float ymax = System.Convert.ToSingle(res["ymax"]);

                AnimateFlow(xmin, xmax, ymin, ymax);
                
            }
        }
    }

    /**
     * GET request to REST server to retrieve the labelled image and display on the canvas
     * 
     **/
    IEnumerator GetLabeledImage()
    {
        iTween.Pause(am.trailObj);
        StringBuilder str = new StringBuilder();
        Texture2D temp = new Texture2D(756, 425);
        using (UnityWebRequest www = UnityWebRequest.Get(host + get_labels_img))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string result = www.downloadHandler.text;

                Dictionary<string, string> res = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                byte[] image = Convert.FromBase64String(res["image"]);
                str.Append(res["image"]);
                temp.LoadImage(image);
                img.texture = temp;
            }
        }
    }

    /**
     * GET request to the REST server to retrieve the raw image 
     * Displays the image on the canvas (no bounding boxes or labels)
     **/ 
    IEnumerator GetImage()
    {
        iTween.Pause(am.trailObj);
        StringBuilder str = new StringBuilder();
        Texture2D temp = new Texture2D(756, 425);
        using (UnityWebRequest www = UnityWebRequest.Get(host + get_image_uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string result = www.downloadHandler.text;
      
                Dictionary<string, string> res = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                byte[] image = Convert.FromBase64String(res["image"]);
                str.Append(res["image"]);
                temp.LoadImage(image);
                img.texture = temp;
            }
        }
    }

    /**
     * Callback function for voice command to execute 
     * Starts coroutine to get image from REST server
     * 
     * COMMAND: get image
     **/
    public void GetCircuitImage()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        StartCoroutine(GetImage());
    }

    /**
     * Callback function for voice command to execute 
     * Starts coroutine to evaluate circuit, get points, and animate
     * 
     * COMMAND:show current
     **/ 
    public void EvaluateCircuit()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        StartCoroutine(evaluateCircuitConnections());
    }

    /**
    * Callback function for voice command to execute 
    * Starts coroutine to show lables on image
    * 
    * COMMAND:show labels
    **/
    public void ShowLabels()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        StartCoroutine(GetLabeledImage());

    }

    /**
     * Called by CaptureManager's CaptureCircuitAndSave. Once image is saved, DisplayCircuit is called 
     * to display the image on the canvas. Not used because HoloLens image capture shortcomings.
     * 
     * INDIRECT COMMAND: circuit capture
     **/
    public void DisplayCircuit(Texture2D targetTexture)
    {
        img.texture = targetTexture;

        // not sure which to send yet ... raw bytes or base64 encoded string
        byte [] imgData = targetTexture.GetRawTextureData();
        string encodedImg = System.Convert.ToBase64String(targetTexture.EncodeToJPG());

        // send circuit image
        StartCoroutine(SendCircuitImage(encodedImg));
    }

    /**
     * Displays current values for components
     * Gets called every time voice commands to set component values is called
     **/ 
    private void DisplayValues()
    {
        StringBuilder str = new StringBuilder();
        foreach (KeyValuePair<string, float> entry in components)
        {
            str.Append(entry.Key).Append(" = ").Append(entry.Value).Append('\n');
        }

        t.text = str.ToString();
    }

    /**
     * Sets value of voltage to 5v.
     * COMMAND: set voltage 5
     **/
    public void SetVoltageFive()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        if (components.ContainsKey("V"))
        {
            components["V"] = 5.0f;
        }
        else
        {
            components.Add("V", 5.0f);
        }
        // update values
        DisplayValues();
    }

    /**
     * Sets value of R1 to 1k ohm
     * COMMAND: set r1 1 kilo ohm
     **/ 
    public void SetR1oneK()
    {
        if (components.ContainsKey("R1"))
        {
            components["R1"] = 1000.0f;
        }
        else
        {
            components.Add("R1", 1000.0f);
        }
        // update values
        DisplayValues();
    }

    /**
     * Sets value of R2 to 1k ohm
     * COMMAND: set r2 1 kilo ohm
     **/
    public void SetR2oneK()
    {
        if (components.ContainsKey("R2"))
        {
            components["R2"] = 1000.0f;
        }
        else
        {
            components.Add("R2", 1000.0f);
        }
        // update values
        DisplayValues();
    }

    /**
     * Sets value of R3 to 1k ohm
     * COMMAND: set r3 1 kilo ohm
     **/
    public void SetR3oneK()
    {
        if (components.ContainsKey("R3"))
        {
            components["R3"] = 1000.0f;
        }
        else
        {
            components.Add("R3", 1000.0f);
        }
        // update values
        DisplayValues();
    }

    /**
    * Sets value of Capacitor to 10 micro farad
    * COMMAND: set capacitor 10 micro farad
    **/
    public void SetCapacitor10mF()
    {
        if (components.ContainsKey("C"))
        {
            components["C"] = 10.0f;
        }
        else
        {
            components.Add("C", 10.0f);
        }
        // update values
        DisplayValues();
    }

    /**
   * Sets value of Inductor to 10 micro henry
   * COMMAND: set inductor 10 micro henry
   **/
    public void SetInductor10mH()
    {
        if (components.ContainsKey("I"))
        {
            components["I"] = 10.0f;
        }
        else
        {
            components.Add("I", 10.0f);
        }
        // update values
        DisplayValues();
    }

    /**
    * Adds 1 to the current value of voltage
    * COMMAND: add 1 to voltage
    **/
    public void AddOneToVoltage()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        if (components.ContainsKey("V"))
        {
            components["V"] += 1;           
        } else
        {
            t.text = "Voltage value not set".ToString();
        }
       
        // update values
         DisplayValues();
    }

    /**
    * Adds 1 to the current value of capacitor
    * COMMAND: add 1 micro farad
    **/
    public void AddOneMicroFarad()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        if (components.ContainsKey("C"))
        {
            components["C"] += 1;
        }
        else
        {
            t.text = "Capacitor value not set".ToString();
        }

        // update values
        DisplayValues();
    }

    /**
    * Adds 1 to the current value of capacitor
    * COMMAND: add 1 micro farad
    **/
    public void AddOneMicroHenry()
    {
        if ((Time.time - lastCalled) < debounce)
        {
            return;
        }
        lastCalled = Time.time;
        if (components.ContainsKey("I"))
        {
            components["I"] += 1;
        }
        else
        {
            t.text = "Inductor value not set".ToString();
        }

        // update values
        DisplayValues();
    }


    /**
     * AnimateFlow is called by evaluateCircuitConnections
     **/
    public void AnimateFlow(float xmin, float xmax, float ymin, float ymax)
    {
        am.ShowCurrentFlow(xmin, xmax, ymin, ymax);
    }
}