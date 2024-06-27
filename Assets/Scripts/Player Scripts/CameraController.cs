// using System;
// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 70.0f;
    public float zoomSpeed = 2.0f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;
    public Transform cameraTransform;
    // Start is called before the first frame update
    private Vector3 offset;
    void Start()
    {
        offset = cameraTransform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        bool rotateRight = Input.GetKey(KeyCode.E);
        bool rotateLeft = Input.GetKey(KeyCode.Q);
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (rotateLeft && rotateRight){
            rotateRight = false;
            rotateLeft = false;
        }

        if (rotateLeft){
            cameraTransform.RotateAround(transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
        }
        if (rotateRight){
            cameraTransform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
        offset = cameraTransform.position - transform.position;

        float distance = offset.magnitude;
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minZoom, maxZoom);

        offset = offset.normalized * distance;
        cameraTransform.position = transform.position + offset;

        cameraTransform.LookAt(transform);
    }
}
