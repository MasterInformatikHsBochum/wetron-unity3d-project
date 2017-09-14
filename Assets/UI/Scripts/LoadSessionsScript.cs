using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSessionsScript : MonoBehaviour {
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform sessionsPanel;

    // Use this for initialization
    void Start () {
		for(int i = 0; i < 4;i++)
        {
            GameObject sessionButton = (GameObject)Instantiate(buttonPrefab);
            sessionButton.GetComponentInChildren<Text>().text = "Yay Game";
            sessionButton.transform.parent = sessionsPanel;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
