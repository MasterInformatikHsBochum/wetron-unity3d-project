using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour {

    private bool fadeout;
    private AudioSource bgm; 

    // Use this for initialization
    void Start () {
        bgm = GameObject.FindObjectOfType<AudioSource>();
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		if (fadeout && bgm.volume > 0)
        {
            bgm.volume -= bgm.volume * Time.deltaTime / 0.5f;
        }
        else if (bgm.volume < 1)
        {
            bgm.volume += 1 * Time.deltaTime / 0.5f;
        }
	}


    public bool Fadeout
    {
        get
        {
            return fadeout;
        }

        set
        {
            fadeout = value;
        }
    }
}
