using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMainCamera : MonoBehaviour
{
    private Camera mainCam;
    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(mainCam.transform.position);
    }
}
