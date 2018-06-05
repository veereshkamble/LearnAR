using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCurrentManager : MonoBehaviour
{
    [SerializeField]
    public GameObject trailObj;
    [SerializeField]
    GameObject planeObj;
    [SerializeField]
    List<Transform> PointTransforms;
    [SerializeField]
    [Range(0f, 1f)]
    float Period = 0.5f;
    [SerializeField] private GameObject point;

    // not used but keeping just in case...
    void SetLocalPoints(IEnumerable<Vector3> local_points)
    {
        int node_number = 1;
        foreach (var local_point in local_points)
        {
            var node = new GameObject("node" + node_number);
            var node_trasform = node.transform;
            node_trasform.SetParent(planeObj.transform);
            node_trasform.localPosition = local_point;
            node_number++;
        }
    }

    /**
     * Called by AnimateFlow to display current flow animations
     * This is triggered by the GET request evaluateCircuitConnections 
     * when show current command is used
     **/
    public void ShowCurrentFlow(float xmin, float xmax, float ymin, float ymax)
    {
        //calculate positions xmin & ymax should be converted
        float width_canv = 756f;
        float height_canv = 425f; 

        float x_orig = width_canv / 2;
        float y_orig = height_canv / 2;

        float xmin_conv = (x_orig - xmin) * -1.0f;
        float xmax_conv = (x_orig - xmax) * -1.0f;
        float ymin_conv = (y_orig - ymin);
        float ymax_conv = (y_orig - ymax);

        Vector3 topLeft = new Vector3(xmin_conv, ymin_conv, -20);
        Vector3 topMid = new Vector3(0, ymin_conv, -20);
        Vector3 topRight = new Vector3(xmax_conv, ymin_conv, -20);
        Vector3 rightMid = new Vector3(xmax_conv, 0, -20);
        Vector3 bottomRight = new Vector3(xmax_conv, ymax_conv, -20);
        Vector3 bottomMid = new Vector3(0, ymax_conv, -20);
        Vector3 bottomLeft = new Vector3(xmin_conv, ymax_conv, -20);
        Vector3 leftMid = new Vector3(xmin_conv, 0, -20);
        Vector3 back = new Vector3(xmin_conv, ymin_conv, -20);

        Vector3[] points = { topLeft, topMid, topRight, rightMid, bottomRight, bottomMid, bottomLeft, leftMid, back };

        iTween.MoveTo(trailObj, iTween.Hash("path", points,
            "time", 10, "looptype", "loop", "isLocal", true, "orientToPath", true, "easeType", iTween.EaseType.linear));
    }
}