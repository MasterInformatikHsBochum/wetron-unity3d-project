using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using SimpleJSON;

public class GameManager: MonoBehaviour {

	//variables
	private String request;
	private int gameId = -1;
	private int playerId = -1;
	private int countdown = -1;
    private int factor = 5;
    private Boolean useKeyboard;
	private Boolean status;
    private Boolean win;
	private string maxPlayers = "";

    public GameObject refreshButton;
    public GameObject createButton;
    public GameObject maxPlayerInput;
    public GameObject returnButton;
    public GameObject statusText;
    public GameObject sessionsMenuPanel;
    [SerializeField] Transform sessionsListPanel;
    public GameObject hudText;

    // player objects
    public GameObject playerModel;
    public GameObject enemiesModel;
    private Dictionary<int, GameObject> players= new Dictionary<int, GameObject>();

    public GameObject ground;

    public Image QRPanel;
    public GameObject controllerButton;

    public GameObject buttonPrefab;

	//size of the map
    private int areaW = 100;
    private int areaH = 100;

	//websocket server
    WebSocket w = new WebSocket(new Uri("wss://wetron.tk:443/websocket/"));
    // REST URL
    private string url = "https://www.wetron.tk/api/";

    IEnumerator Start () {

        StartCoroutine(loadGameList());

        QRPanel.enabled = false;

		//establish websocket connection
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
            int playersChoosen = int.Parse(maxPlayers);
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

        maxPlayerInput.GetComponent<InputField>().onValueChanged.AddListener(delegate
        {
           maxPlayers = maxPlayerInput.GetComponentInChildren<InputField>().text;
           createButton.GetComponent<Button>().interactable = (maxPlayers != null && maxPlayers != "");
        });
        
    }

    private IEnumerator loadGameList()
    {
        refreshButton.GetComponent<Button>().interactable = false;
        UnityWebRequest www = UnityWebRequest.Get(url + "games/");
        yield return www.Send();

        if(www.isError)
        {
            Debug.Log("Error loading GameList:" + www.GetHashCode());
        } 
        else
        {
            if(www.responseCode == 200)
            {
                foreach (Transform child in sessionsListPanel)
                {
                    GameObject.Destroy(child.gameObject);
                }
                string data = www.downloadHandler.text;
                JSONNode gameList = JSONNode.Parse(data);

                for (int i = gameList.Count-1; i > -1; i--)
                {
                    yield return StartCoroutine(loadGameInfo(gameList[i].AsInt));                
                }
            }
            else
            {
                Debug.Log("No changes on GameList.");
            }
        }
        www.Dispose();
        www = null;
        refreshButton.GetComponent<Button>().interactable = true;
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
            if (www.responseCode == 200 || www.responseCode == 304)
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
                    Debug.Log("Couldn't add Game: " + gameId);
                }
            }
            else
            {
                Debug.Log("Couldn't find Game: " + gameId);
            }
        }
        www.Dispose();
        www = null;
    }

	// create a new game
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
                sessionsMenuPanel.SetActive(false);
                returnButton.SetActive(true);
                for (int i = 10; i > 0; i--)
                {
                    statusText.GetComponent<Text>().text = "Created Game : " + gameId + "\nConnecting in " + i;
                    yield return new WaitForSeconds(1); 
                }               
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
        sessionButton.GetComponent<Button>().interactable = (maxPlayers != activePlayers);
        int addedGameID = gameId;
        sessionButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            joinGame(addedGameID);

        });

        sessionButton.transform.SetParent(sessionsListPanel,false);

    }

	//MonoBehaviour function that is called every frame and reacts to incoming websocket messages
    private void Update()
    {
        
        // parseKeyboardInputs();

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
                //join game
                case 1:
                    status = receivedJSONNode["v"]["success"].AsBool;
                    if(status)
                    {
                        sessionsMenuPanel.SetActive(false);
                        hudText.GetComponent<Text>().text = "Game: " + gameId + ", Player:" + playerId;
                        statusText.GetComponent<Text>().text = "Connect \n Controller";
                        QRPanel.enabled = true;
                        //controllerButton.SetActive(true);
						returnButton.SetActive(true);
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
                        areaW = receivedJSONNode["v"]["grid"]["w"].AsInt * factor;
                        areaH = receivedJSONNode["v"]["grid"]["h"].AsInt * factor;
                        setBounds(areaW,areaH);
                    }
                    break;
				// game starts
				case 4:
					countdown = receivedJSONNode ["v"] ["countdown-ms"].AsInt / 1000;
					QRPanel.enabled = false;
					//controllerButton.SetActive (false);
					returnButton.SetActive (false);
                    if(countdown != 0)
                    {
                    statusText.GetComponent<Text>().text = countdown.ToString();
                    } else
                    {
                        statusText.GetComponent<Text>().text = "";
                    }
                    break;
				// game over
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
				// direction / position information
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
                    //useKeyboard = receivedJSONNode["v"]["success"].AsBool;
                    break;
                case 9:
                    
                    break;
                default:break;
            }
        }
    }

	//restrict boundaries of the grid
    private void setBounds(int areaW, int areaH)
    {
        ground.transform.localScale =new Vector3(areaW, 1, areaH);
        ground.transform.position = new Vector3(areaW / 2, -3, areaH / 2);
    }

    private void parseKeyboardInputs()
    {
        if (Input.GetKeyDown("a"))
        {
            request = "{\"g\":" + gameId + ",\"p\":" + playerId + "\"t\":\"c\",\"e\":6,\"v\":{\"d\":270}}";
			Debug.Log ("Turn left" + request);
            w.SendString(request);

        }
		else if (Input.GetKeyDown("d"))
        {
            request = "{\"g\":" + gameId + ",\"p\":" + playerId + "\"t\":\"c\",\"e\":6,\"v\":{\"d\":90}}";
			Debug.Log ("Turn right" + request);
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
            if(!movedPlayer.GetComponent<TrailRenderer>().enabled)
            {
                movedPlayer.GetComponent<TrailRenderer>().enabled = true;
            }
            movedPlayer.transform.position = new Vector3(x*factor, -1, z*factor);
            movedPlayer.transform.eulerAngles = new Vector3(1, direction, 1);

        }
    }

    void OnApplicationQuit()
    {
        w.Close();
        Debug.Log("Application ending after " + Time.time + " seconds");
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
	}

    private void connectController()
    {
        request = "{\"g\":" + gameId + ",\"p\":"+playerId+",\"t\":\"c\",\"e\":0,\"v\":null}";
        w.SendString(request);
        Debug.Log("Join Controller request send:  " + request);
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
