using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {

    public Transform target; // The target we are following
    public float distance = 10.0f; // The distance in x-z plane to the target
    public float height = 2.0f; // the height of the camera above the target
    public float maxHeight = 10f;
    public float minHeight = 0.5f;
    public float cameraMoveSpeed = 0.1f;
    public float heightDamping = 2.0f; // How much we damp in height
    public float rotationDamping = 1.0f; // How much we damp in rotation

    // Use this for initialization
    private float rotator = 0f;
    private bool uglyWait = true;
    private float startHeight;
    private Vector3 startPos;



    private void Start()
    {
        startHeight = height;
        startPos = transform.position;
        // StartCoroutine("waiting");
    }

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetAxis("Jump") != 0)
        {
            height = startHeight;
            rotator = 0;
        }
        if (Input.GetMouseButton(1))
        {
            setHeight();
            setRotation();
        }
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }

    IEnumerator waiting()
    {
        uglyWait = false;
        yield return new WaitForSeconds(2);
        if (!uglyWait)
        {
            rotator = 0;
            height = startHeight;
        }
        StartCoroutine("waiting");

    }


    void LateUpdate () {

        if (!target) return;

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, 
                                               wantedRotationAngle, 
                                               rotationDamping * Time.deltaTime);

        currentHeight = Mathf.Lerp(currentHeight,
                                   wantedHeight,
                                   heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle + rotator, 0);

        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        transform.LookAt(target);


	}


    private void setRotation()
    {

        if (rotator > 2 || rotator < -2)
        {
            rotator = 0;
        }
        rotator += Input.GetAxis("Mouse X") * cameraMoveSpeed / 10;
        if(Input.GetAxis("Mouse X") != 0)
        {
            uglyWait = true;
        }
        
    }


    private void setHeight()
    {

        
        if(height > minHeight && height < maxHeight)
        {
            height -= Input.GetAxis("Mouse Y") * cameraMoveSpeed;
        }else if(height <= minHeight)
        {
            height = minHeight + 0.05f;
        }else if(height >= maxHeight)
        {
            height = maxHeight - 0.05f;
        }
        
    }
}
