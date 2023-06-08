using System;
using System.Collections;
using System.Collections.Generic;
//using Mirror;
using UnityEngine;
using Cinemachine;
using HTC.UnityPlugin.Vive;

public class carController : MonoBehaviour
{
    
     /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
      
     //-------------------------------------------------------------------Variables implemented for VR------------------------------------------------------
     
     ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //Angle variables
    public float initialAngle;
    private float flagTurn = 0;

    private Vector3 numRot;
    private float correctAngle;
    private float angleVisual;

    //knowing the previous speed
    private float prevSpeed = 1;

    //Arduino
    private SerialController arduino; 

    private CarInputs carControls;
    private Rigidbody rb;

    //private CinemachineVirtualCamera cinemachineVirtualCamera;
    public float speed = 200f, turn = 100f, brake = 150f, friction = 70f, dragAmount = 4f, TurnAngle = 30f;
    
    public float maxRayLength = 0.8f, slerpTime = 0.2f;
    [HideInInspector]
    public bool grounded;

    public Transform groundCheak, fricAt, CentreOfMass;
    public Transform[] TireMeshes, TurnTires;

    public AnimationCurve frictionCurve, speedCurve, turnCurve, driftCurve, engineCurve;

    public AudioSource[] engineSounds;

    private float speedValue, fricValue, turnValue, curveVelocity, brakeInput;
    [HideInInspector]
    public Vector3 carVelocity;
    [HideInInspector]
    public RaycastHit hit;

    //public bool drftSndMachVel;
    public bool airDrag;
    public float SkidEnable = 20f;
    public float skidWidth = 0.12f;
    private float frictionAngle;

    //Variable for Pause
    private float flagPause = 0;

    [SerializeField] private GameObject pausePanel;

    //Variable for Reverse
    private float flagReverse = 0;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    //-------------------------------------------------------------------Awake Function--------------------------------------------------------------------

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    private void Awake()
    {
        carControls = new CarInputs();
        //cinemachineVirtualCamera = transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();

        rb = GetComponent<Rigidbody>();
        grounded = false;
        engineSounds[1].mute = true;
        rb.centerOfMass = CentreOfMass.localPosition;

        //Find the Arduino
        arduino = GameObject.Find("SerialController").GetComponent<SerialController>();
    }

    private void OnEnable()
    {
	    carControls.Enable();
    }

    private void OnDisable()
    {
	    carControls.Disable();
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    //-------------------------------------------------------------------Update Functions-------------------------------------------------------------------
    //      Here on this part of the code, we are running a constant code which is updating certain variables that enable us to move the bike on the world

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void FixedUpdate()
    {

        carVelocity = transform.InverseTransformDirection(rb.velocity); //local velocity of car
        curveVelocity = Mathf.Abs(carVelocity.magnitude) / 100;
        Debug.Log("TURN: " + carControls.carAction.moveH.ReadValue<float>());
        //inputs
        float turnInput = turn * carControls.carAction.moveH.ReadValue<float>() * Time.fixedDeltaTime * 1000;

        //get speed from the arduino(real bike)
        float speedBike = float.Parse(arduino.ReadSerialMessage());

        Debug.Log("ARDUINO: " + speedBike);
        if (speedBike < 500 && speedBike > 0)
        {
            speedBike = speedBike / 6;
        }

        Debug.Log("SPEED: " + speedBike);

        //filter of arduino´s values
        if (speedBike < 20 && speedBike > 1)
        {
            if (speedBike == -2)
            {
                speedBike = 10;
            }
            prevSpeed = speedBike;
        }

        if (speedBike > 21)
        {
            speedBike = prevSpeed;
        }

        if (speedBike < 0)
        {
            speedBike = prevSpeed;
        }

        //speedBike = 8f;

        //Movement of Bike Variable
        float speedInput = (speed * speedBike * Time.fixedDeltaTime * 1000) + 37000; //speedBike

        Debug.Log("FINAL SPEED: "+ speedInput);
        brakeInput = brake * carControls.carAction.brake.ReadValue<float>() * Time.fixedDeltaTime * 1000;

        Debug.Log("INPUT: " +turnInput);
        //helping veriables
        speedValue = speedInput * speedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
        fricValue = friction * frictionCurve.Evaluate(carVelocity.magnitude / 100);
        turnValue = turnInput * turnCurve.Evaluate(carVelocity.magnitude / 100);
        Debug.Log(turnValue);


        //Angle of Tracker Variable
        var rotationTracker = VivePose.GetPoseEx(TrackerRole.Tracker1).rot;
        numRot = AngleFromQ2(rotationTracker);

        if(numRot.z > 180)
        {
            numRot.z -= 360;
        }
        

        correctAngle = numRot.z - initialAngle;
        if(correctAngle < 10 && correctAngle > -10)
        {
            flagTurn = 0;
        }
        else if(correctAngle < -10)
        {
            flagTurn = -1;
        }
        else
        {
            flagTurn = 1;
        }

        angleVisual = correctAngle ;
       
        correctAngle *= 900;
       
        turnValue = correctAngle;

        if(flagPause == 1)
        {
            speedBike = 0;
        }
       
        //Grounded check variables
        if (Physics.Raycast(groundCheak.position, -transform.up, out hit, maxRayLength))
        {
            accelarationLogic(speedBike);
            turningLogic();
            frictionLogic();
            brakeLogic();

            //for drift behaviour
            rb.angularDrag = dragAmount * driftCurve.Evaluate(Mathf.Abs(carVelocity.x) / 70);
            Debug.Log(rb.angularDrag);

            //draws green ground checking ray ....ingnore
            Debug.DrawLine(groundCheak.position, hit.point, Color.green);
            grounded = true;

	        rb.centerOfMass = Vector3.zero;
        }
        else
        {
            grounded = false;
            rb.drag = 0.1f;
            rb.centerOfMass = CentreOfMass.localPosition;
            if (!airDrag)
            {
                rb.angularDrag = 0.1f;
            }

        }
        
	    
    }
    // Update visuals of the Bike and Audios
    void Update()
	{
		
        tireVisuals();
        //ShakeCamera(1.2f, 10f);
		audioControl();
	    
    }

    public void ShakeCamera(float amplitude, float frequency)
    {
       // CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            //cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = curveVelocity * amplitude;
        //cinemachineBasicMultiChannelPerlin.m_FrequencyGain = curveVelocity * frequency;
    }

    public void audioControl()
    {
        //audios
        if (grounded)
        {
            if (Mathf.Abs(carVelocity.x) > SkidEnable - 0.1f)
            {
                engineSounds[1].mute = false;
            }
            else { engineSounds[1].mute = true; }
        }
        else
        {
            engineSounds[1].mute = true;
        }

        /*if (drftSndMachVel) 
        { 
            engineSounds[1].pitch = (0.7f * (Mathf.Abs(carVelocity.x) + 10f) / 40);
        }
        else { engineSounds[1].pitch = 1f; }*/

        engineSounds[1].volume = 1f;

        engineSounds[0].volume = 2 * engineCurve.Evaluate(curveVelocity);
        if (engineSounds.Length == 2)
        {
            return;
        }
        else { engineSounds[2].volume = 2 * engineCurve.Evaluate(curveVelocity); }

        

    }

    public void tireVisuals()
    {
        //Tire mesh rotate
        foreach (Transform mesh in TireMeshes)
        {
            mesh.transform.RotateAround(mesh.transform.position, mesh.transform.right, carVelocity.z / 3);
            mesh.transform.localPosition = Vector3.zero;
        }

        //TireTurn
        foreach (Transform FM in TurnTires)
        {
            FM.localRotation = Quaternion.Slerp(FM.localRotation, Quaternion.Euler(FM.localRotation.eulerAngles.x,
                               angleVisual , FM.localRotation.eulerAngles.z), slerpTime);
        }
    }



    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    //-------------------------------------------------------------------Physical Logics Functions--------------------------------------------------------
    //      Here on this part of the code, we are running functions that enable us to determine the movement of the bike, this functions are always being
    //                                                               called by the "Update Function".

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void accelarationLogic(float speedBike)
    {
        //speed control    carControls.carAction.moveV.ReadValue<float>()
        if (speedBike > 0.1f)
        {
            rb.AddForceAtPosition(transform.forward * speedValue, groundCheak.position);
        }
        if (flagReverse == 1)
        {
            if(flagPause == 0)
            {
                rb.AddForceAtPosition(transform.forward * -400000 , groundCheak.position);
            }
            
        }
    }

    public void turningLogic()
    {
        //turning
        if (carVelocity.z > 0.1f)
        {
            rb.AddTorque(transform.up * turnValue);
        }
        else if (carControls.carAction.moveV.ReadValue<float>() > 0.1f)
        {
            rb.AddTorque(transform.up * turnValue);
        }
        //para cuando va de reversa
        if (carVelocity.z < -0.1f && carControls.carAction.moveV.ReadValue<float>() < -0.1f)
        {
            rb.AddTorque(transform.up * -turnValue);
        }
    }

    public void frictionLogic()
    {
        //Friction
        if (carVelocity.magnitude > 1)
        {
            frictionAngle = (-Vector3.Angle(transform.up, Vector3.up)/90f) + 1 ;
            rb.AddForceAtPosition(transform.right * fricValue * frictionAngle * 100 * -carVelocity.normalized.x, fricAt.position);
        }
    }

    public void brakeLogic()
    {
        //brake
	    if (carVelocity.z > 1f)
        {
            rb.AddForceAtPosition(transform.forward * -brakeInput, groundCheak.position);
        }
	    if (carVelocity.z < -1f)
        {
            rb.AddForceAtPosition(transform.forward * brakeInput, groundCheak.position);
        }
	    if(carVelocity.magnitude < 1)
	    {
	    	rb.drag = 5f;
	    }
	    else
	    {
	    	rb.drag = 0.1f;
	    }
    }


    //Change quaternion to angle for tracker interpretation values
    public static Vector3 AngleFromQ2(Quaternion q1)
    {
        float sqw = q1.w * q1.w;
        float sqx = q1.x * q1.x;
        float sqy = q1.y * q1.y;
        float sqz = q1.z * q1.z;
        float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        float test = q1.x * q1.w - q1.y * q1.z;
        Vector3 v;

        if (test > 0.4995f * unit)
        { // singularity at north pole
            v.y = Convert.ToSingle(2.0 * Math.Atan2(q1.y, q1.x));
            v.x = Convert.ToSingle(Math.PI / 2.0);
            v.z = 0;
            return NormalizeAngles(RadianToDegree(v));
        }
        if (test < -0.4995f * unit)
        { // singularity at south pole
            v.y = Convert.ToSingle(-2.0 * Math.Atan2(q1.y, q1.x));
            v.x = Convert.ToSingle(-Math.PI / 2.0);
            v.z = 0;
            return NormalizeAngles(RadianToDegree(v));
        }
        Quaternion q = new Quaternion(q1.w, q1.z, q1.x, q1.y);
        v.y = (float)Math.Atan2(2f * q.x * q.w + 2.0 * q.y * q.z, 1 - 2.0 * (q.z * q.z + q.w * q.w));     // Yaw
        v.x = (float)Math.Asin(2f * (q.x * q.z - q.w * q.y));                             // Pitch
        v.z = (float)Math.Atan2(2f * q.x * q.y + 2.0 * q.z * q.w, 1 - 2.0 * (q.y * q.y + q.z * q.z));      // Roll
        return NormalizeAngles(RadianToDegree(v));
    }

    static Vector3 NormalizeAngles(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    static float NormalizeAngle(float angle)
    {
        while (angle > 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }

    static Vector3 RadianToDegree(Vector3 v)
    {
        v.x = Convert.ToSingle((180 / Math.PI) * (v.x));
        v.y = Convert.ToSingle((180 / Math.PI) * (v.y));
        v.z = Convert.ToSingle((180 / Math.PI) * (v.z));
        return v;
    }

    public void PauseController()
    {
        if (flagPause == 0)
        {
            flagPause = 1;
            pausePanel.SetActive(true);
        }
        else
        {
            flagPause = 0;
            pausePanel.SetActive(false);
        }
    }

    public void ReverseController()
    {
        if (flagReverse == 0)
        {
            flagReverse = 1;
        }
        else
        {
            flagReverse = 0;
        }
    }

}
