using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelBehaviour2 : MonoBehaviour {


    public WheelCollider wheelCol;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Quaternion quat;
        Vector3 position;

        wheelCol.GetWorldPose(out position, out quat);
        transform.position = position;
        transform.rotation = quat;
    }
}
