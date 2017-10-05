using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wall_alpha : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        Material material = renderer.material;

        material.SetFloat("_Mode", 2f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
