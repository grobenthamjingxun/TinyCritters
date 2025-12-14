using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ForceARCamera : MonoBehaviour
{
    void Awake()
    {
        var cam = GetComponent<Camera>();
        var bg = GetComponent<ARCameraBackground>();

        if (cam) cam.enabled = true;
        if (bg) bg.enabled = true;

        Debug.Log("[ForceARCamera] Camera + AR background forced ON");
    }
}
