using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bikeController : MonoBehaviour
{
    //Arduino
    private SerialController arduino;

    private CarInputs carControls;
    private Rigidbody rb;

    private void Awake()
    {
        carControls = new CarInputs();
        //cinemachineVirtualCamera = transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();

        rb = GetComponent<Rigidbody>();
        
        //rb.centerOfMass = CentreOfMass.localPosition;

        //Find the Arduino
        arduino = GameObject.Find("SerialController").GetComponent<SerialController>();
    }

    void FixedUpdate()
    {

        float speedBike = float.Parse(arduino.ReadSerialMessage());
    }

        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
