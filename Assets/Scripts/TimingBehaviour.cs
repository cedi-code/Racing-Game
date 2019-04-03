using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingBehaviour : MonoBehaviour {


    public Text countDown;
    public int countMax = 3;
    public GameObject buggy;

    private int _countDown;
    private float _pastTime = 0;
    private bool _isFinished = false;
    private bool _isStarted = false;

    // Use this for initialization
    void Start () {
        countDown.text = countMax.ToString();
        StartCoroutine(GameStart());
        
    }

    void Update()
    {
        if (buggy.GetComponent<CarBehaviour>().getStart())
        {
            if (_isStarted && !_isFinished)
                _pastTime += Time.deltaTime;
            
        }
        if(_isFinished)
        {
            countDown.text = _pastTime.ToString("0.0 sec");
            countDown.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        {
            if (!_isStarted)
                _isStarted = true;
            else _isFinished = true;
            Debug.Log("Finished!");
        }
    }

    IEnumerator GameStart()
    {
        for(_countDown = countMax; _countDown > 0; _countDown--)
        {
           countDown.text = _countDown.ToString();
           yield return new WaitForSeconds(1);
        }
        countDown.text = "GOO!";
        _isStarted = true;
        buggy.GetComponent<CarBehaviour>().setStart(true);
        yield return new WaitForSeconds(1);
        countDown.enabled = false;
    }
}
