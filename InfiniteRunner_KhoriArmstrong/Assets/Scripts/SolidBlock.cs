using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidBlock : MonoBehaviour {

    public enum BlockTypes
    {
        Solid,
        Damage,
        Death,
    }
    public BlockTypes type = BlockTypes.Solid;


	// Use this for initialization
	void Start () {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            if (type == BlockTypes.Damage)
            {
                sr.color = new Color(1.0f, 0.6f, 0.6f);
            }
            else if (type == BlockTypes.Death)
            {
                sr.color = new Color(1.0f, 0.3f, 1.0f);
                
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
