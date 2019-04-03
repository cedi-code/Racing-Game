using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}


    public void OnStartClick()
    {
        SceneManager.LoadScene("Scene 2");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
