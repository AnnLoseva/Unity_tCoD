using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class СameraMovement : MonoBehaviour
{
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;

    private bool right;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") > 0.1f)
        {
            right = true;
        }
        else if (Input.GetAxisRaw("Horizontal") < -0.1f)
        {
            right = false;
        }

        if (right ==true)
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(offsetX, offsetY, -10f), speed);
            //cam.transform.position = Vector3.
        }
        else
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(-offsetX, offsetY, -10f), speed);
        }

    }
}
