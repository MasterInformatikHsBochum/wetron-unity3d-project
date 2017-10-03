using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recalcNormals : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.RecalculateNormals();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
