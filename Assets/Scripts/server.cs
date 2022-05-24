using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class serverClient{
	public int connectionId;
	public string playerName;
	public Vector3 position;
	public Vector3 rotation;
	public int level;
    public string raze;
}

public class server : MonoBehaviour {

	private const int MAX_CONNECTION = 100;

	private int port = 8888;

	private int hostId;
	//private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	private bool isStarted = false;

	private List<serverClient> clients = new List <serverClient>();

	private float lastMovementUpdate;
	private float movementUpdateRate = 0.05f;

	public void Start (){
		NetworkTransport.Init ();

		//Configure connection
		ConnectionConfig cc = new ConnectionConfig ();
		reliableChannel = cc.AddChannel (QosType.Reliable);
		unreliableChannel = cc.AddChannel (QosType.Unreliable);
		HostTopology topo = new HostTopology (cc, MAX_CONNECTION);

		hostId = NetworkTransport.AddHost (topo, port);

		isStarted = true;
	}

	void Update()
	{
		if (!isStarted)
			return;
		
		int recHostId; 
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
		switch (recData)
		{
		case NetworkEventType.ConnectEvent:    //2
			print ("Player " + connectionId + " has connected");
			OnConnection (connectionId);
			break;

		case NetworkEventType.DataEvent:       //3
			string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
			print ("Receiving from " + connectionId + ": " + msg);
			string[] splitData = msg.Split ('|');

			switch (splitData [0]) {
			case "NAMEIS":
				OnNameIs (connectionId, splitData[1], float.Parse (splitData[2].Split(',')[0].Split('.')[0]), float.Parse (splitData[3].Split(',')[0].Split('.')[0]), float.Parse (splitData[4].Split(',')[0].Split('.')[0]), splitData[5]);
				break;

			case "MYPOSITION":
				OnMyPosition (connectionId, float.Parse (splitData [1].Split(',')[0].Split('.')[0]), float.Parse (splitData [2].Split(',')[0].Split('.')[0]), float.Parse (splitData [3].Split(',')[0].Split('.')[0]), float.Parse (splitData [4].Split(',')[0].Split('.')[0]));
				break;

			case "DESTROYIS":
				UpdateLevel (connectionId, splitData [1]);
				Destroy (splitData [1]);
				break;

			case "ANIMATION":
				SendAnimation (connectionId, splitData [1], float.Parse(splitData[2]));
				break;

			case "MOVEMENT":
				SendMovement (connectionId, bool.Parse(splitData [1]));
				break;

			case "SPAWNIS":
				SendRespawn (connectionId, float.Parse (splitData [1].Split(',')[0].Split('.')[0]), float.Parse (splitData [2].Split(',')[0].Split('.')[0]), float.Parse (splitData [3].Split(',')[0].Split('.')[0]));
				break;

			default:
				Debug.Log ("Invalid message: " + msg);
				break;
			}
			break;

		case NetworkEventType.DisconnectEvent: //4
			//Debug.Log ("Player " + connectionId + " has disconnected");
			OnDisconnection (connectionId);
			break;
		}

		//Ask player for their position
		if (Time.time - lastMovementUpdate > movementUpdateRate) {
			lastMovementUpdate = Time.time;
			string m = "ASKPOSITION|";
			foreach (serverClient sc in clients) {
				m += sc.connectionId.ToString () + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString () + '%' + sc.position.z.ToString () + '%' + sc.rotation.y.ToString () + '|';
			}
			m = m.Trim ('|');

			Send (m, unreliableChannel, clients);
		}
	}

	private void SendRespawn (int cnnId, float spawnX, float spawnY, float spawnZ){

		Send ("RESPAWN|" + cnnId + '|' + spawnX + '|' + spawnY + '|' + spawnZ, unreliableChannel, clients);
	}

	private void UpdateLevel(int cnnIdOr, string playerDestroyed){

		int cnnIdDst;
		cnnIdDst = clients.Find (x => x.playerName == playerDestroyed).connectionId;

		//Update killer's level
		if (cnnIdOr != cnnIdDst) {
			float y = clients.Find (x => x.connectionId == cnnIdDst).level - clients.Find (x => x.connectionId == cnnIdOr).level - 15f;
			clients.Find (x => x.connectionId == cnnIdOr).level += Mathf.RoundToInt (1 / (0.04f + Mathf.Pow (1.1f, -(y + 10f))) + 1f);
		}


		/*
		if (clients.Find (x => x.connectionId == cnnIdOr).level - 30 >= clients.Find (x => x.connectionId == cnnIdDst).level) 			//Kill a Blue
			clients.Find (x => x.connectionId == cnnIdOr).level += 1;
		else if (clients.Find (x => x.connectionId == cnnIdOr).level - 10 >= clients.Find (x => x.connectionId == cnnIdDst).level) 		//Kill a Green
			clients.Find (x => x.connectionId == cnnIdOr).level += 2;
		else if (clients.Find (x => x.connectionId == cnnIdOr).level + 40 <= clients.Find (x => x.connectionId == cnnIdDst).level)		//Kill a Black
			clients.Find (x => x.connectionId == cnnIdOr).level += 7;
		else if (clients.Find (x => x.connectionId == cnnIdOr).level + 30 <= clients.Find (x => x.connectionId == cnnIdDst).level)		//Kill a Purple
			clients.Find (x => x.connectionId == cnnIdOr).level += 6;
		else if (clients.Find (x => x.connectionId == cnnIdOr).level + 20 <= clients.Find (x => x.connectionId == cnnIdDst).level)		//Kill a Red
			clients.Find (x => x.connectionId == cnnIdOr).level += 5;
		else if (clients.Find (x => x.connectionId == cnnIdOr).level + 10 <= clients.Find (x => x.connectionId == cnnIdDst).level)		//Kill a Yellow
			clients.Find (x => x.connectionId == cnnIdOr).level += 4;
		else 																															//Kill a White
			clients.Find (x => x.connectionId == cnnIdOr).level += 3;
		*/

		//Max lvl 60
		if (clients.Find (x => x.connectionId == cnnIdOr).level >= 60)
			clients.Find (x => x.connectionId == cnnIdOr).level = 60;
			
		//Update Victim's level
		if (Mathf.RoundToInt (clients.Find (x => x.connectionId == cnnIdDst).level * 0.2F) != 0)
			clients.Find (x => x.connectionId == cnnIdDst).level -= Mathf.RoundToInt (clients.Find (x => x.connectionId == cnnIdDst).level * 0.2F);
		else
			clients.Find (x => x.connectionId == cnnIdDst).level -= 1;

		//Min lvl 1
		if (clients.Find (x => x.connectionId == cnnIdDst).level <= 1)
			clients.Find (x => x.connectionId == cnnIdDst).level = 1;

		Send("LVLUPDT|" + cnnIdOr + "|" + clients.Find (x => x.connectionId == cnnIdOr).level + "|" + cnnIdDst + "|" + clients.Find (x => x.connectionId == cnnIdDst).level, reliableChannel, clients);
	}

	private	void SendMovement (int cnnId, bool isOn){
		
		Send ("MOVEMENT|" + cnnId + "|" + isOn, reliableChannel, clients);
	}

	private void SendAnimation (int cnnId, string animation, double yRotation){

		Send ("ANIMATION|" +  cnnId + "|" + animation + "|" + yRotation, reliableChannel, clients);
	}

	private void Destroy(string player){

		Send("DESTROY|" + (clients.Find(x => x.playerName == player)).connectionId, reliableChannel, clients);
	}

	private void OnConnection(int cnnId){

		//Add him to a list
		serverClient c = new serverClient();
		c.connectionId = cnnId; 
		c.playerName = "TEMP";
		c.level = 1;
		c.raze = "TEMP";
		clients.Add (c);

		//When a player joins the server, tell him his ID
		//Request his name and send the name of all the other players
		string msg = "ASKNAME|" + cnnId + "|";
		foreach (serverClient sc in clients) {
			msg += sc.playerName + '%' + sc.connectionId  + '%' + sc.level + '%' + sc.position.x + '%' + sc.position.y + '%' + sc.position.z + '%' + sc.raze + '|';
		}

		msg = msg.Trim('|');

		//ASKNAME|3|DAVE%1|MICHAEL%2|TEMP%3
		Send(msg, reliableChannel, cnnId);
	}

	private void OnDisconnection(int cnnId){
		//Remove this player from aour client list
		clients.Remove(clients.Find(x => x.connectionId == cnnId));
		
		//Tell everyone that somebody else has disconnected
		Send("DC|" + cnnId, reliableChannel, clients);
	}

	private void OnNameIs(int cnnId, string playerName, float spawnX, float spawnY, float spawnZ, string raze){
		
		//Link the name to the connection Id
		clients.Find(x=>x.connectionId==cnnId).playerName = playerName;
		clients.Find(x=>x.connectionId==cnnId).position = new Vector3 (spawnX, spawnY, spawnZ);
        clients.Find(x => x.connectionId == cnnId).raze = raze;

        //Tell everybody that a new player has connected
        Send("CNN|" + playerName + '|' + cnnId + '|' + clients.Find(x=>x.connectionId==cnnId).level + '|' + spawnX + '|' + spawnY + '|' + spawnZ + '|' + raze, reliableChannel, clients);
	}
		
	private void OnMyPosition(int cnnId, float x, float y, float z, float ry){
		print (x);
		clients.Find(c=>c.connectionId == cnnId).position = new Vector3(x, y, z);
		clients.Find(c=>c.connectionId == cnnId).rotation = new Vector3(0, ry, 0);
	}
	
	private void Send (string message, int channelId, int cnnId){
		List<serverClient> c = new List<serverClient> ();
		c.Add (clients.Find (x => x.connectionId == cnnId));
		Send (message, channelId, c);
	}

	private void Send (string message, int channelId, List<serverClient> c){
		byte error;
		//Debug.Log ("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes (message);
		foreach (serverClient sc in c) {
			NetworkTransport.Send (hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
		}
	}
}
