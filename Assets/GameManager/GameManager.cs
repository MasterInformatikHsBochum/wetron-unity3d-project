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
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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
    private Boolean useKeyboard;
	private Boolean status;
    private Boolean win;

    public GameObject refreshButton;
    public GameObject createButton;
    public GameObject maxPlayerDropDown;
    public GameObject returnButton;
    public GameObject statusText;
    public GameObject sessionsMenuPanel;
    [SerializeField] Transform sessionsListPanel;
    public GameObject hudText;

    // player objects
    public GameObject playerModel;
    public GameObject enemiesModel;
    private Dictionary<int, GameObject> players= new Dictionary<int, GameObject>();

    public Image QRPanel;
    public GameObject controllerButton;

    public GameObject buttonPrefab;

    private int areaW = 100;
    private int areaH = 100;


    //our websocket server is reachable under: 193.175.85.50:80
    WebSocket w = new WebSocket(new Uri("wss://wetron.tk:443/websocket/"));
    // REST URL
    private string url = "https://www.wetron.tk/api/";

    // Use this for initialization
    IEnumerator Start () {		

		//var s = "{\"g\":0,\"e\":2,\"p\":0,\"v\":{\"d\":1.323,\"x\":0,\"y\":0}}\n";
		//Debug.Log(s);

		//var N = JSON.Parse(s);
		//Debug.Log(N["v"]["d"].Value);
		//Debug.Log(N["v"]["d"].AsFloat);

        StartCoroutine(loadGameList());

        QRPanel.enabled = false;
		//establish connection
		yield return StartCoroutine(w.Connect());
		Debug.Log ("Connection established.");


        // refreshButton
        refreshButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(loadGameList());
        });
        
        // createButton
        createButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            int playersChoosen = (maxPlayerDropDown.GetComponent<Dropdown>().value + 1) *2;
            StartCoroutine(createGame(playersChoosen));
        });

        // button for Controller
        controllerButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            connectController();
        });

        // return to Menu
        returnButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
    }

    private IEnumerator loadGameList()
    {
        UnityWebRequest www = UnityWebRequest.Get(url + "games/");
        yield return www.Send();

        if(www.isError)
        {
            Debug.Log("Error loading GameList:" + www.GetHashCode());
        } 
        else
        {
            foreach (Transform child in sessionsListPanel)
            {
                GameObject.Destroy(child.gameObject);
            }
            string data = www.downloadHandler.text;
            JSONNode gameList = JSONNode.Parse(data);

            for (int i = 0; i < gameList.Count; i++)
            {
                StartCoroutine(loadGameInfo(gameList[i].AsInt));
                
            }
        }
        www.Dispose();
        www = null;
    }

    private IEnumerator loadGameInfo(int gameId)
    {
        UnityWebRequest www = UnityWebRequest.Get(url + "games/" + gameId + "");
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log("Error loading GameInfo:" + www.error);
        }
        else
        {
            string data = www.downloadHandler.text;
            JSONNode gameInfo = JSONNode.Parse(data);
            try
            {
                  if (gameId == gameInfo["id"].AsInt)
                            {
                                int maxPlayers = gameInfo["maxPlayers"].AsInt;
                                int activePlayers = gameInfo["players"].Count;
                                addGameserver(gameId, maxPlayers, activePlayers);
                            }
            } catch (Exception e)
            {
                Debug.Log("Couldn't load Game");
            }
          
        }
        www.Dispose();
        www = null;
    }

    private IEnumerator createGame(int maxPlayers)
    {
        WWWForm form = new WWWForm();
        form.AddField("maxPlayers", maxPlayers);
        UnityWebRequest www = UnityWebRequest.Post(url + "games/",form);
       // www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler.contentType = "application/json";
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log("Error loading GameInfo:" + www.error);
        }
        else
        {
            Debug.Log("http: " + www.responseCode);
            string data = www.downloadHandler.text;
            JSONNode gameInfo = JSONNode.Parse(data);
            int gameId = gameInfo["id"].AsInt;
            if (gameId > 0)
            {
            joinGame(gameId);
            }
        }
        www.Dispose();
        www = null;
    }

    private void addGameserver(int gameId, int maxPlayers, int activePlayers)
    {
        GameObject sessionButton = Instantiate(buttonPrefab) as GameObject;
        sessionButton.GetComponentInChildren<Text>().text = "Game " + gameId + " (" + activePlayers + "/" + maxPlayers + ")";
        sessionButton.GetComponent<Button>().enabled = (maxPlayers != activePlayers);
        int addedGameID = gameId;
        sessionButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            joinGame(addedGameID);

        });

        sessionButton.transform.SetParent(sessionsListPanel);
    }

    private void Update()
    {
        if (useKeyboard)
        {
            parseKeyboardInputs();
        }
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
                        hudText.GetComponent<Text>().text = "Game: " + gameId + ", Player:" + playerId;
                        statusText.GetComponent<Text>().text = "Connect \n Controller";
                        QRPanel.enabled = true;
                        controllerButton.SetActive(true);
                        StartCoroutine(loadQrCode());                
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
                        areaW = receivedJSONNode["v"]["grid"]["w"].AsInt;
                        areaH = receivedJSONNode["v"]["grid"]["h"].AsInt;
                        setBounds(areaW,areaH);
                    }
                    break;
                case 4:
                    countdown = receivedJSONNode["v"]["countdown-ms"].AsInt / 1000;
                    QRPanel.enabled = false;
                    controllerButton.SetActive(false);
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
                    returnButton.SetActive(true);
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
                case 8:
                    useKeyboard = receivedJSONNode["v"]["success"].AsBool;
                    break;
                case 9:
                    
                    break;
                default:break;
            }
        }
    }

    private void setBounds(int areaW, int areaH)
    {
        // TODO
    }

    private void parseKeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            request = "{\"g\":" + gameId + ",\"p\":" + playerId + "\"t\":\"c\",\"e\":6,\"v\":{\"d\":360}}";
            w.SendString(request);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            request = "{\"g\":" + gameId + ",\"p\":" + playerId + "\"t\":\"c\",\"e\":6,\"v\":{\"d\":90}}";
            w.SendString(request);
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
    

	public void getListOfGames() {

	}

	public void collisionDetected(int gameId) {
		request = "{\"g\":" + gameId + ",\"e\":14,\"v\":{} }";
		w.SendString(request);
		Debug.Log ("Collision Detected request send:  " + request);
	}

	//send request to join a game (information about the game id is needed from sessions menu)
	public void joinGame(int gameId) {
		request = "{\"g\":" + gameId + ",\"t\":\"v\",\"e\":0,\"v\":{} }";
		w.SendString(request);
		Debug.Log ("Join Game request send:  " + request);
        this.gameId = gameId;
        
        /*    //test
            playerId = 1;
            connectController();
            playerId = 2;
            connectController(); 
        */
	}

    private void connectController()
    {
        request = "{\"g\":" + gameId + ",\"p\":"+playerId+",\"t\":\"c\",\"e\":0,\"v\":null}";
        w.SendString(request);
        Debug.Log("Join Controller request send:  " + request);
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
		

		string url = "https://chart.googleapis.com/chart?cht=qr&chs=500x500&chl={\"gameId\":" + gameId + ",\"playerId\":"+ playerId +"}";
        Debug.Log(url);
		WWW www = new WWW (url);
		yield return www;
        Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);

        www.LoadImageIntoTexture(texture);
        Rect rec = new Rect(0, 0, texture.width, texture.height);
        Sprite spriteToUse = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
        QRPanel.sprite = spriteToUse;
		www.Dispose ();
		www = null;
        QRPanel.enabled = true;
	}
}
