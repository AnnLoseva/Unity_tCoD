using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ÑameraMovement : MonoBehaviour
{
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    

    public void flipCamera(int direction)
    {
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(offsetX * direction, offsetY, -10f), speed);
    }
}
