using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour {

    public Image back;
    public Image front;
    public float value = 0.0f;

    // Use this for initialization
    void Start () {
        SetValue();
	}
	
	// Update is called once per frame
	void Update () {}


    public void SetValue(float val = 0.0f)
    {
        value = val;
        //
        front.rectTransform.localScale = new Vector3(Mathf.Clamp(value, 0.0f, 1.0f), front.rectTransform.localScale.y, front.rectTransform.localScale.z);
    }
}
