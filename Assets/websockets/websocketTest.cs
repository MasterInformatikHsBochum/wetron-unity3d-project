//this script represents the communication between view and game
//all necessary requests and responses are added as a placeholder

using UnityEngine;
using System.Collections;
using System;

using SimpleJSON;

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

public class websocketTest: MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		int gameId;

		//var s = "{\"g\":0,\"e\":2,\"p\":0,\"v\":{\"d\":1.323,\"x\":0,\"y\":0}}\n";
		//Debug.Log(s);

		//var N = JSON.Parse(s);
		//Debug.Log(N["v"]["d"].Value);
		//Debug.Log(N["v"]["d"].AsFloat);

		//establish connection our websocket server is reachable under: 193.175.85.50:80
		WebSocket w = new WebSocket(new Uri("ws://193.175.85.50:80"));
		yield return StartCoroutine(w.Connect());

		//send initial message
		w.SendString("Hi there, this is Unity3D.");

		//send request to join a game (information about the game id is needed from sessions menu
		gameId = 1;
		w.SendString("{\"event\":2, \"v\":{\"game\":" + gameId + "}}");

		//wait for response and react accordingly
		while (true)
		{
			string reply = w.RecvString();

			//response is not empty
			if (reply != null)
			{
				Debug.Log ("Received: "+reply);

				//received list of games?
				if (reply.Contains ("games")) {
					//display list of games in sessions menu

				} else if (reply.Contains ("player-id")) {
					//show player id
				
				} else if (reply.Contains ("countdown-ms")) {
					//countdown until game starts in ms

				} else if (reply.Contains ("d")) {
					//position of the players vehicle (x,y)

				} else if (reply.Contains ("win")) {
					//game is over and player has lost or won
				}
			}
			//an error occurred
			if (w.error != null)
			{
				Debug.LogError ("Error: "+w.error);
				break;
			}
			yield return 0;
		}
		w.Close();
	}
}
