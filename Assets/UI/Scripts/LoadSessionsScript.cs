using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSessionsScript : MonoBehaviour {
    public GameObject buttonPrefab;
    [SerializeField] Transform sessionsPanel;
    private int GAMES_COUNT = 1;

    // Use this for initialization
    void Start () {
        GameObject canvas = gameObject;
        GameManager gm = canvas.GetComponent<GameManager>();
      
		for(int i = 1; i <= GAMES_COUNT;i++)
        {
            GameObject sessionButton = Instantiate(buttonPrefab) as GameObject;
            sessionButton.GetComponentInChildren<Text>().text = "Gameserver " + i;
            int gameId = 1;
            sessionButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                gm.joinGame(gameId);

            });

            sessionButton.transform.parent = sessionsPanel;
            
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
