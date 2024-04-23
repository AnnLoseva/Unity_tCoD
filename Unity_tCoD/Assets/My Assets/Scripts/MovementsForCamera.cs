using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MovementForCamera : MonoBehaviour
{
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    
    public void flipCamera(float direction)
    {
        cam.transform.localPosition = Vector3.Slerp(cam.transform.localPosition, new Vector3(offsetX * -direction, offsetY, -10f), speed * Time.deltaTime);
    }

    //private void Update()
    //{
    //    Vector3 newPosition = new Vector3(target.position.x, target.position.y, -10f);
    //    transform.position = Vector3.Slerp(transform.position, newPosition, speed * Time.deltaTime);
    //}
}
