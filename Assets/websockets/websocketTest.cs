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

		//var s = "{\"g\":0,\"e\":2,\"p\":0,\"v\":{\"d\":1.323,\"x\":0,\"y\":0}}\n";
		//Debug.Log(s);

		//var N = JSON.Parse(s);
		//Debug.Log(N["v"]["d"].Value);
		//Debug.Log(N["v"]["d"].AsFloat);

		//establish connection
		WebSocket w = new WebSocket(new Uri("ws://127.0.0.1:8080"));
		yield return StartCoroutine(w.Connect());
		int i=0;

		//send initial message
		w.SendString("Hi there");

		//wait for replies and react accordingly
		while (true)
		{
			string reply = w.RecvString();

			if (reply != null)
			{
				Debug.Log ("Received: "+reply);
				w.SendString("Hi there"+i++);
			}
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
