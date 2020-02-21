using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateByBPM : MonoBehaviour
{

    public float RPM = 120f;
    float speed = 1;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, RPM * Time.deltaTime * speed);

    }
}
