using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class Movement : NetworkBehaviour {
	private int speed = 20;
	public Text scoretxt;
	private float score;
	// Use this for initialization

	void Start () {
		score = 0;
		setText ();
	}

	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer)
			return;


		score = score +  Time.deltaTime;
		setText();

		transform.Translate (Vector3.forward * Time.deltaTime * speed);

		if(Input.GetKeyDown("a"))
		{
			transform.rotation = transform.rotation * Quaternion.Euler (1, -90, 1);
			//transform.Rotate (Vector3.up*Time.deltaTime * speed);
		}
		if (Input.GetKeyDown ("d")) {
			transform.rotation = transform.rotation * Quaternion.Euler (1, 90, 1);
		}

	}

	void setText(){
		scoretxt.text = "Score: " + score.ToString();
	}
}