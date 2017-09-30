//this script represents the communication between view and game and should be launched on startup and run all the time
//all necessary requests and responses are added as a placeholder

//procedure:
//unity is launched with this script; a connection to the gateway is stablished
//the user enters the session menu and requests a list of active games (available sessions)
//the user selects a game and clicks join; the join game request is send
//after all players have joined the countdown reply is received. The countdown is displayed for the user.
//the game starts; the position of the players are received and the users position is send
//the backend checks for collisons and sends a win/loose reply which is displayed

using UnityEngine;
using System.Collections;
using System;

using SimpleJSON;
using System.Threading;
using UnityEngine.UI;
using System.Collections.Generic;

//protocol overview:
//{"g":0,"e":0,"p":0,"t":"c","v":{}}

//g: gameId (int)
//e: eventType (int)
//p: playerId (int)
//t: type("c" [controller], "v" [view])
//v: value (any)

//events
//index			name				source		destination		example		comment
//	0			connect				client		server			{"p":0}		
//	1			disconnect			client		server			{"p":0}		
//	2			startup				server		client			{"d":0.0,"x":0,"y":0}			
//	3			startup ack			client		server			{"p":0,"d":0.0,"x":0,"y":0}
//	4			game start			server		client			{"t":3000}
// 	5			game end			server		client			{"s":0}
//	6			change direction	client		server			{"d":0.0}
//	7			position			server		clients			{"p":0,"d":0.0,"x":0,"y":0}

public class GameManager: MonoBehaviour {


	//needed variables
	private String request;
	private int gameId = -1;
	private int[] games;
	private String position = null;
	private int playerId = -1;
	private int countdown = -1;
    private int factor = 5;
	private Boolean status;
    private Boolean win;
    public GameObject statusText;
    public GameObject sessionsMenuPanel;
    public GameObject playerModel;
    public GameObject enemiesModel;
    private Dictionary<int, GameObject> players= new Dictionary<int, GameObject>();

	//our websocket server is reachable under: 193.175.85.50:80
	WebSocket w = new WebSocket(new Uri("wss://wetron.tk:443/websocket/"));

    // Use this for initialization
    IEnumerator Start () {		

		//var s = "{\"g\":0,\"e\":2,\"p\":0,\"v\":{\"d\":1.323,\"x\":0,\"y\":0}}\n";
		//Debug.Log(s);

		//var N = JSON.Parse(s);
		//Debug.Log(N["v"]["d"].Value);
		//Debug.Log(N["v"]["d"].AsFloat);

		//establish connection
		yield return StartCoroutine(w.Connect());
		Debug.Log ("Connection established.");
	}

    private void Update()
    {
        String receive = w.RecvString();
        if (receive != null)
        {
            Debug.Log("Received Message: " + receive);
            JSONNode receivedJSONNode = JSONNode.Parse(receive);
            gameId = receivedJSONNode["g"].AsInt;
            playerId = receivedJSONNode["p"].AsInt;
            if (!players.ContainsKey(playerId))
            {
                players.Add(playerId, playerModel);
            }
            int eventtype = receivedJSONNode["e"].AsInt;
            switch(eventtype)
            {
                
                case 1:
                    status = receivedJSONNode["v"]["success"].AsBool;
                    if(status)
                    {
                        sessionsMenuPanel.SetActive(false);
                        statusText.GetComponent<Text>().text = "Waiting for Opponents";
                        Debug.Log("In Game");
                        JSONArray newPlayers = receivedJSONNode["v"]["o"].AsArray;
                        foreach (JSONNode newPlayer in newPlayers)
                        {
                            GameObject newPlayerModel = GameObject.Instantiate(enemiesModel);
                            int newPlayerId = newPlayer.AsInt;
                            if(!players.ContainsKey(newPlayerId))
                            {
                            players.Add(newPlayerId,newPlayerModel);
                            }
                        }
                        
                    }
                    break;
                case 4:
                    countdown = receivedJSONNode["v"]["countdown-ms"].AsInt / 1000;
                    if(countdown != 0)
                    {
                    statusText.GetComponent<Text>().text = countdown.ToString();
                    } else
                    {
                        statusText.GetComponent<Text>().text = "";
                    }
                    break;
                case 5:
                    win = receivedJSONNode["v"]["win"].AsBool;
                    if (win)
                    {
                        statusText.GetComponent<Text>().text = "You Win!";
                    }
                    else
                    {
                        statusText.GetComponent<Text>().text = "You Lose!";
                    }
                    break;
                case 7:
                     JSONArray playerList = receivedJSONNode["v"].AsArray;
                   foreach(JSONNode player in playerList)
                    {
                        int p = player["p"].AsInt;
                        int x = player["x"].AsInt;
                        int y = player["y"].AsInt;
                        int d = player["d"].AsInt;
                        movePlayer(p, x, y, d);
                    }
                    break;
                case 9:
                    
                    break;
                default:break;
            }
        }
    }

    private void movePlayer(int playerId, int x, int z, int direction)
    {
        if (players.ContainsKey(playerId))
        {
            GameObject movedPlayer = players[playerId];
            if(!movedPlayer.activeSelf)
            {
                movedPlayer.SetActive(true);
            }
            movedPlayer.transform.position = new Vector3(x*factor, -1.4f, z*factor);
            movedPlayer.transform.eulerAngles = new Vector3(1, direction, 1);
        }
    }

    void OnApplicationQuit()
    {
        w.Close();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    public void sendPosition() {
		request = "";
		w.SendString(request);
		Debug.Log ("Position send:  " + request);
	}

	public void getListOfGames() {

	}

	public void collisionDetected(int gameId) {
		request = "{\"g\":" + gameId + ", \"e\":14, \"v\":{} }";
		w.SendString(request);
		Debug.Log ("Collision Detected request send:  " + request);
	}

	//send request to join a game (information about the game id is needed from sessions menu)
	public void joinGame(int gameId) {
		request = "{\"g\":" + gameId + ",\"t\":\"v\", \"e\":0, \"v\":{} }";
		w.SendString(request);
		Debug.Log ("Join Game request send:  " + request);
	}

	//Getter/Setter
	public void setPosition(String message){
		this.position = message;
	}

	public String getPosition(){
		return this.position;
	}

	public void setPlayerId(String message){
		//this.playerId = message;
	}

	public int getPlayerId(){
		return this.playerId;
	}

	public void setGameId(String message){
		//this.gameId = message;
	}

	public int getGameId(){
		return this.gameId;
	}

	public void setCountdown(String message){
		//this.countdown = message;
	}

	public int getCountdown(){
		return this.countdown;
	}

	public void setStatus(String message){
		//this.status = message;
	}

	public Boolean getStatus(){
		return this.status;
	}

	public void setGames(String message){

	}

	//call with startcoroutine
	public IEnumerator loadQrCode(){
		//Texture2D texture = profileImage.canvasRenderer.GetMaterial().mainTexture as Texture2D;

		string url = "https://chart.googleapis.com/chart?cht=qr&chs=500x500&chl={%22gameId%22:" + gameId + " ,%22playerId%22:"+ playerId +"}";
		WWW www = new WWW (url);
		yield return www;


		//www.LoadImageIntoTexture(texture);
		www.Dispose ();
		www = null;
	}
}
