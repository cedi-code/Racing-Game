using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarBehaviour : MonoBehaviour {


    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public RawImage speedPointer;
    public Camera MainCamera;
    public float defualtAngle = 30f;
    
    public float frontFriction, sideFriction;
    
    public GameObject centerOfMass;
    
    public float maxTorque = 500;
    public float maxSteerAngle = 45;
    public float maxSpeedKMH = 150;
    public float maxSpeedBackwardKMH = 30;

    public ParticleSystem smokeL;
    public ParticleSystem smokeR;

    public ParticleSystem dustFL, dustFR, dustRL, dustRR;

    // public GameObject water;




    private float _currentSpeedKMH;
    private Rigidbody _rigidBody;
    private bool _velocityIsForeward;
    private float defaultMaxTorque;
    private float defaultMaxSpeed;

    private ParticleSystem.EmissionModule _smokeLEmission;
    private ParticleSystem.EmissionModule _smokeREmission;
    private ParticleSystem.EmissionModule _dustFLEmission, _dustRREmission, _dustFREmission, _dustRLEmission;

    private gear[] gears = new gear[]
    {
        new gear( 1, 900, 12, 1400),
        new gear( 12, 900, 25, 2000),
        new gear( 25, 1350, 45, 2500),
        new gear( 45, 1950, 70, 3500),
        new gear( 70, 2500, 112, 4000),
        new gear(112, 3100, 180, 5000)
    };
    private bool _carIsOnDrySand;
    private string _groundTagFL;
    private int _groundTextureFL;
    private string _groundTagFR;
    private int _groundTextureFR;

    private bool start = false;

    // Use this for initialization
    void Start () {

        SetCenterOfMass();
        SetFriction(frontFriction,sideFriction);
        SetSmoke();

        defaultMaxTorque = maxTorque;
        defaultMaxSpeed = maxSpeedKMH;
    }

    private void Update()
    {
        // guiSpeed.text = Mathf.RoundToInt(_currentSpeedKMH).ToString() + "km/h";
        SetCamera();
    }

    void OnGUI()
    {

        float scale = 3.0f;
        int sh = Screen.height;
        int size = (int)(sh / scale); 
        int lenN = (int)(size * 0.7777f); // length of needle
        int offN = (int)(size / 8.2f);

        

        float degPerKMH = Mathf.Round(36 + (_currentSpeedKMH / maxSpeedKMH) * 288);
        GUIUtility.RotateAroundPivot(degPerKMH,
            new Vector2(lenN / 2 + offN, sh - size + lenN / 2 + offN));
        // Draw the speed pointer
        
        GUI.DrawTexture(new Rect(offN + lenN / 2, sh - size + offN + lenN / 2 , lenN / 10, lenN / 2),
        speedPointer.texture,
        ScaleMode.StretchToFill);
    }


    void FixedUpdate () {


        _currentSpeedKMH = _rigidBody.velocity.magnitude * 3.6f;

        _velocityIsForeward = Vector3.Angle(transform.forward, _rigidBody.velocity) < 50f;

        // isWater();
        if(start)
        {
            motor();
        }
        

        

        SetSteerAngle((maxSteerAngle - _currentSpeedKMH * Time.deltaTime * 10) * Input.GetAxis("Horizontal"));

        int gearNum = 0;
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum);

        SetParticleSystems(engineRPM);


        // Evaluate ground under front wheels
        WheelHit hitFL = GetGroundInfos(ref wheelFL, ref _groundTagFL, ref _groundTextureFL);
        WheelHit hitFR = GetGroundInfos(ref wheelFR, ref _groundTagFR, ref _groundTextureFR);
        _carIsOnDrySand = _groundTagFL.CompareTo("Terrain") == 0 && _groundTextureFL == 0;
        /*if (wheelFL.rpm > 0 && _currentSpeedKMH > maxSpeedKMH)
        {
            SetMotorTorque(0);
        }
        else if(wheelFL.rpm < 0 && _currentSpeedKMH > maxSpeedBackwardKMH)
        {
            SetMotorTorque(0);
        }else
        {
            SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        }*/


    }


   
    void motor()
    {
        // bremsen
        if (isBarking())
        {
            barking();
        }
        else if(_velocityIsForeward && _currentSpeedKMH > maxSpeedKMH)
        {
            SetMotorTorque(0);
        }
        else if(!_velocityIsForeward && _currentSpeedKMH > maxSpeedBackwardKMH)
        {
            SetMotorTorque(0);
        }
        else
        {
            SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        }
    }
    private void barking()
    {
        wheelFL.brakeTorque = 3000;
        wheelFR.brakeTorque = 3000;
        wheelRL.brakeTorque = 3000;
        wheelRR.brakeTorque = 3000;
        wheelFL.motorTorque = 0;
        wheelFR.motorTorque = 0;
    }

    private void handbremse()
    {
        wheelFL.brakeTorque =  Mathf.Infinity;
        wheelFR.brakeTorque =  Mathf.Infinity;
        wheelRL.brakeTorque =  Mathf.Infinity;
        wheelRR.brakeTorque =  Mathf.Infinity;
    }
    // falls schnell dann weniger starke kurven!
    private void SetSteerAngle(float angle)
    {

        wheelFL.steerAngle = angle;
        wheelFR.steerAngle = angle;
        
      
    }

    private void SetMotorTorque(float amount)
    {
        wheelFL.brakeTorque = 0;
        wheelFR.brakeTorque = 0;
        wheelRL.brakeTorque = 0;
        wheelRR.brakeTorque = 0;
        wheelFL.motorTorque = amount;
        wheelFR.motorTorque = amount;
    }

    /*private void isWater()
    {
        if(water.transform.position.y - 0.5f < transform.position.y && maxTorque == defaultMaxTorque)
        {
            return;
        }
        if(water.transform.position.y - 0.5f > transform.position.y && maxTorque != defaultMaxTorque)
        {
            return;
        }

        if (water.transform.position.y - 0.5f > transform.position.y)
        {
            maxTorque = maxTorque / 2;
            maxSpeedKMH = maxSpeedKMH / 2;
        }else
        {
            maxTorque = defaultMaxTorque;
            maxSpeedKMH = defaultMaxSpeed;
        }
    }*/

    private float kmh2rpm(float kmh, out int gearNum)
    {
        for (int i = 0; i < gears.Length; ++i)
        {
            if (gears[i].speedFits(kmh))
            {
                gearNum = i + 1;
                return gears[i].interpolate(kmh);
            }
        }
        gearNum = 1;
        return 800;
    }

    private void SetParticleSystems(float engineRPM)
    {
        float smokeRate = engineRPM / 10f;
        _smokeLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);
        _smokeREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);

        float dustRate = 0;
        if (_currentSpeedKMH > 20.0f && _carIsOnDrySand)
        {
            dustRate = _currentSpeedKMH;
            setDustEnabled(true);
        }
        else
        {
            setDustEnabled(false);
        }
            
        // Debug.Log(dustRate);
        _dustFLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate / 1000);
        _dustFREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate / 1000);
        _dustRLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate / 1000);
        _dustRREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate / 1000);
    }

    private void SetFriction(float forewardFriction, float sidewaysFriction)
    {
        WheelFrictionCurve f_fwWFC = wheelFL.forwardFriction;
        WheelFrictionCurve f_swWFC = wheelFL.sidewaysFriction;

        f_fwWFC.stiffness = forewardFriction;
        f_swWFC.stiffness = sidewaysFriction;

        wheelFL.forwardFriction = f_fwWFC;
        wheelFL.sidewaysFriction = f_swWFC;
        wheelFR.forwardFriction = f_fwWFC;
        wheelFR.sidewaysFriction = f_swWFC;

        wheelRL.forwardFriction = f_fwWFC;
        wheelRL.sidewaysFriction = f_swWFC;
        wheelRR.forwardFriction = f_fwWFC;
        wheelRR.sidewaysFriction = f_swWFC;
    }

    private void SetCenterOfMass()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.centerOfMass = new Vector3(centerOfMass.transform.localPosition.x,
                                              centerOfMass.transform.localPosition.y,
                                              centerOfMass.transform.localPosition.z);
    }


    private void SetSmoke()
    {
        _smokeLEmission = smokeL.emission;
        _smokeREmission = smokeR.emission;
        _smokeLEmission.enabled = true;
        _smokeREmission.enabled = true;

        _dustFLEmission = dustFL.emission;
        _dustFREmission = dustFR.emission;
        _dustRLEmission = dustRL.emission;
        _dustRREmission = dustRR.emission;
        setDustEnabled(false);
    }

    private WheelHit GetGroundInfos(ref WheelCollider wheelCol,
                                    ref string groundTag,
                                    ref int groundTextureIndex)
    {
        groundTag = "InTheAir";
        groundTextureIndex = -1;

        // Query ground by ray shoot on the front left wheel collider
        WheelHit wheelHit;
        wheelCol.GetGroundHit(out wheelHit);
        // If not in the air query collider
        if (wheelHit.collider)
        {
            groundTag = wheelHit.collider.tag;
            if (wheelHit.collider.CompareTag("Terrain"))
                groundTextureIndex = TerrainSurface.GetMainTexture(transform.position);
        }
        return wheelHit;
    }

    private void SetCamera()
    {
        if (MainCamera.fieldOfView < 100 && _currentSpeedKMH / 100 >= 1)
        {
            MainCamera.fieldOfView = defualtAngle * (_currentSpeedKMH / 100);
        }
        else if(_currentSpeedKMH < 100)
        {
            MainCamera.fieldOfView = defualtAngle;
        }
    }

    private bool isBarking()
    {
        return _currentSpeedKMH > 0.5f && (Input.GetAxis("Vertical") < 0 && _velocityIsForeward ||
                                            Input.GetAxis("Vertical") > 0 && !_velocityIsForeward);
    }

    private void setDustEnabled(bool on)
    {
        _dustFLEmission.enabled = on;
        _dustFREmission.enabled = on;
        _dustRLEmission.enabled = on;
        _dustRREmission.enabled = on;
    }

    public void setStart(bool on)
    {
        this.start = on;
    }
    public bool getStart()
    {
        return this.start;
    }


}
