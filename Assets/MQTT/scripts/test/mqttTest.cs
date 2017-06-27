using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;

public class mqttTest : MonoBehaviour {

    private MqttClient client;

    void onStart()
    {
        
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
	{ 
		Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message)  );
	} 

	void OnGUI(){

        if ( GUI.Button(new Rect(100,40,80,20), "Connect"))
        {
            Debug.Log("connecting...");

            //client = new MqttClient("test.mosquitto.org", 1883, false, null);
            client = new MqttClient(IPAddress.Parse("193.175.85.50"), 1883, false, null);
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            client.Subscribe(new string[] { "#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            Debug.Log("connected");
        }



        if ( GUI.Button (new Rect (100,100,80,20), "Publish"))
        {
			Debug.Log("sending...");

			client.Publish("test/unity", System.Text.Encoding.UTF8.GetBytes("Hallo! Hier ist Unity3D"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

			Debug.Log("sent");
		}

	}
	// Update is called once per frame
	void Update () {

	}
}
