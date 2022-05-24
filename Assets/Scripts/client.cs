using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;

public class Player{
	
	public string playerName;
	public GameObject avatar;
	public GameObject camera;
	public int connectionId;
	public string tag;
	public Vector3 position;
	public float rotation;
	public int level;
	public int respawnFlag;
	public Vector3 spawnPlace;

	//If the player is a wizard
	public GameObject orb;
	public bool wantsOrb;
	public bool startedRequest;

	public float startTime;
	//public float distanceToDestination;
}

public class client : MonoBehaviour {

	private const int MAX_CONNECTION = 100;
	private string ip =  	//"127.0.0.1"; 
							"192.168.178.10";
							//"88.0.106.67";

	private int port = 8888;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	public int ourClientId;
	public int connectionId;
	//private float connectionTime;

	private bool isConnected = false;
	private bool isStarted = false;
	private byte error;

	public static string playerName;
    public string raze;

	public GameObject goblin;
    public GameObject ethereal;
	public GameObject cam;
	public GameObject nTag;
	public GameObject arrow;
    public GameObject HUD;
	public GameObject orb;

	public static Dictionary<int,Player> players = new Dictionary<int, Player>();

	public static string lastPlayerAdded;
	public static int yourLevel;

	private bool justSpawn;

	public void ConnectEthereal(){

        //Does the player have a name
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
		if (pName == "") {
			print ("You must enter a name");
			return;
		}

		playerName = pName;
        raze = "Ethereal";

		NetworkTransport.Init ();

		//Configure connection
		ConnectionConfig cc = new ConnectionConfig ();
		reliableChannel = cc.AddChannel (QosType.Reliable);
		unreliableChannel = cc.AddChannel (QosType.Unreliable);
		HostTopology topo = new HostTopology (cc, MAX_CONNECTION);
		hostId = NetworkTransport.AddHost (topo, 0);

		connectionId = NetworkTransport.Connect (hostId, ip, port, 0, out error);
		if (error != 0)
			return;

		//connectionTime = Time.time;

		isConnected = true;
	}

    public void ConnectMossy()
    {

        //Does the player have a name
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (pName == "")
        {
            print("You must enter a name");
            return;
        }

        playerName = pName;
        raze = "Mossy";

        NetworkTransport.Init();

        //Configure connection
        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);
        hostId = NetworkTransport.AddHost(topo, 0);

        connectionId = NetworkTransport.Connect(hostId, ip, port, 0, out error);
        if (error != 0)
            return;

        //connectionTime = Time.time;

        isConnected = true;
    }

    void Update()
	{
		if (!isConnected)
			return;

		int recHostId; 
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		byte error;

		//Receive data
		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
		if (recData == NetworkEventType.DataEvent){
			string msg = Encoding.Unicode.GetString (recBuffer, 0, dataSize);
			//Debug.Log ("Receiving : " + msg);
			string[] splitData = msg.Split ('|');

			switch (splitData [0]) {
			case "ASKNAME":
				OnAskName (splitData);
				break;

			case "CNN":
				SpawnPlayer (splitData [1], int.Parse (splitData [2]), int.Parse (splitData [3]), float.Parse (splitData[4]), float.Parse (splitData[5]), float.Parse (splitData[6]), splitData[7]);
				break;

			case "DC":
				PlayerDisconnected (int.Parse(splitData[1]));
				break;

			case "ASKPOSITION":
				OnAskPosition (splitData);
				break;

			case "DESTROY":
				Kill (int.Parse(splitData [1]));
				break;

			case "ANIMATION":
				PlayAnimation (int.Parse (splitData [1]), splitData [2], float.Parse(splitData[3]));
				break;

			case "MOVEMENT":
				PlayMovement (int.Parse (splitData [1]), bool.Parse(splitData [2]));
				break;

			case "LVLUPDT":
				UpdateLevels (int.Parse (splitData [1]), int.Parse (splitData [2]), int.Parse (splitData [3]), int.Parse (splitData [4]));
				break;

			case "RESPAWN":
				RespawnIs (int.Parse (splitData [1]), float.Parse (splitData[2]), float.Parse (splitData[3]), float.Parse (splitData[4])); 
				break;

			default:
				//Debug.Log ("Invalid message" + msg);
				break;
			}
		}

		//Interpolation
		foreach(KeyValuePair<int, Player> entry in players){
			if (entry.Value.connectionId != ourClientId && entry.Value.tag != "Dead") {
				float currentDuration = Time.time - entry.Value.startTime;

				/*
				float journeyFraction = 0f;
				if(entry.Value.distanceToDestination != 0)
					journeyFraction = currentDuration / entry.Value.distanceToDestination;
				*/

				//As we have 60 fps and a rate of 1/20 (onnce each 0.05 secs) positions send by the server, we have to fill the other 2 frames left interpolating 1/3 of the distance each frame. This is 0/60 * 20 = 0/3, 1/60 * 20 = 1/3, 2/60 * 20 = 2/3
				entry.Value.avatar.transform.position = Vector3.Lerp (entry.Value.avatar.transform.position, entry.Value.position, currentDuration * 20); //journeyFraction * 100);
				float angle = Mathf.LerpAngle (entry.Value.avatar.transform.eulerAngles.y, entry.Value.rotation, 0.15F);
				entry.Value.avatar.transform.eulerAngles = new Vector3 (0, angle, 0);
			}
		}

		//Wizard wants orb
		foreach(KeyValuePair<int, Player> entry in players){
			if (entry.Value.wantsOrb) {

				Vector3 target = entry.Value.avatar.transform.GetChild (0).GetChild (0).GetChild (0).GetChild (2).GetChild (1).GetChild (0).GetChild (0).GetChild (0).transform.position; // + new Vector3 (-0.3f, 0f, -0.7f);

				if (entry.Value.startedRequest) {
					entry.Value.orb.GetComponent<Rigidbody> ().velocity = Vector3.zero;
					entry.Value.orb.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
				}

				if (Vector3.Distance (entry.Value.orb.transform.position, target) > 3) {					
					entry.Value.orb.GetComponent<Rigidbody> ().AddForce ((target - entry.Value.orb.transform.position) * 350, ForceMode.Acceleration);
				} else {
					entry.Value.orb.transform.SetParent(entry.Value.avatar.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetChild(0));
					//Relatively to the finger (parent)
					entry.Value.orb.transform.localPosition = new Vector3 (-0.3f, 0f, -0.7f);
					entry.Value.wantsOrb = false;

					entry.Value.avatar.GetComponent<Animator> ().SetBool ("hasOrb", true);
				}
			}
		}
	}

	private void RespawnIs (int cnnId, float spawnX, float spawnY, float spawnZ){
		
		players [cnnId].spawnPlace = new Vector3 (spawnX, spawnY, spawnZ);
	}

	private void UpdateLevels(int cnnIdOr, int lvl1, int cnnIdDst, int lvl2){

		if (ourClientId == cnnIdOr) {
			players [ourClientId].level = lvl1;
			players [cnnIdDst].level = lvl2;
		} else if (ourClientId == cnnIdDst) {
			players [ourClientId].level = lvl2;
			players [cnnIdOr].level = lvl1;
		} else {
			players [cnnIdOr].level = lvl1;
			players [cnnIdDst].level = lvl2;
		}

		yourLevel = players[ourClientId].level;
		players [cnnIdOr].avatar.GetComponent<Animator> ().speed = 1F / (1F - players[cnnIdOr].level / 100F);
		players [cnnIdDst].avatar.GetComponent<Animator> ().speed = 1F / (1F - players[cnnIdDst].level / 100F);

		ColorUpdate ();
	}

	private void ColorUpdate (){
		
		foreach (KeyValuePair<int, Player> entry in players) {
			if (entry.Value.connectionId != ourClientId) {
				if (players [ourClientId].level - 30 >= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.blue;
				else if (players [ourClientId].level - 10 >= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.green;
				else if (players [ourClientId].level + 40 <= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.black;
				else if (players [ourClientId].level + 30 <= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.magenta;
				else if (players [ourClientId].level + 20 <= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.red;
				else if (players [ourClientId].level + 10 <= entry.Value.level)
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.yellow;
				else {
					GameObject.Find ("NameTag: " + entry.Value.playerName).GetComponent<TextMesh> ().color = Color.white;
				}
			}
		}
	}

	private void PlayMovement (int cnnId, bool isOn){

		players [cnnId].avatar.GetComponent<Animator> ().SetBool ("movement", isOn);
	}

	private void PlayAnimation (int cnnId, string animation, float yRotation){

		if (ourClientId != cnnId)
			players [cnnId].avatar.transform.eulerAngles = new Vector3 (0, yRotation, 0);

		//Play animation
		SendOurPosition();
		players [cnnId].avatar.GetComponent<Animator> ().Play (animation, -1, 0F);
		SendOurPosition();

		//If the animation is the goblin shot instantiate the arrows
		if (animation == "Shot") {
			//Spawn Arrow
			StartCoroutine(Arrow(cnnId));
		}

		//If the animation is the wizard's mouse
		if (animation == "Orb") {
			//Throw Orb
			StartCoroutine(OrbThrow(cnnId));
		}

		//If the animation is the wizard's mouse reactivation
		if (animation == "take_orb") {
			//Activate the acceleration of the ball towards the player in each update
			players [cnnId].wantsOrb = true;
			players [cnnId].startedRequest = true;
		}

		if (animation == "take_orbE") {
			//Play animation
			SendOurPosition();
			players [cnnId].avatar.GetComponent<Animator> ().Play ("take_orb", -1, 0F);
			SendOurPosition();

			//Activate the acceleration of the ball towards the player in each update
			players [cnnId].wantsOrb = true;
			players [cnnId].startedRequest = true;

			players[cnnId].avatar.transform.position = players[cnnId].orb.transform.position - new Vector3 (0, 7.5f, 0);
		}

		if (animation == "dodge") {
			//Teleports when some time has passed
			StartCoroutine (dodgeTimer (cnnId));
		}
	}

	IEnumerator Arrow (int cnnId) {
		
		yield return new WaitForSeconds(.95F - (0.95F * players [cnnId].level / 100F));  //Wait 0.95 seconds divided by the lvl
		GameObject go = Instantiate (arrow, players [cnnId].avatar.transform.position + new Vector3 (0, 5, 0), players [cnnId].avatar.transform.rotation) as GameObject;
		if (ourClientId != cnnId)
			go.tag = "NotMyWeapon";
		//go.transform.SetParent (players [cnnId].avatar.transform);
		go.GetComponent<Rigidbody> ().AddForce (players [cnnId].avatar.transform.forward * 5000);
	}

	IEnumerator OrbThrow (int cnnId) {

		yield return new WaitForSeconds(0.95F - (0.95F * players [cnnId].level / 100F));  //Wait 0.95 seconds divided by the lvl
		//GameObject go = players[cnnId].avatar.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;

		if (ourClientId != cnnId)
			players[cnnId].orb.tag = "NotMyWeapon";

		players[cnnId].orb.transform.parent.transform.DetachChildren ();
		players[cnnId].orb.GetComponent<Rigidbody> ().AddForce (players [cnnId].avatar.transform.forward * 4000);

		players [cnnId].avatar.GetComponent<Animator> ().SetBool ("hasOrb", false);
	}

	IEnumerator dodgeTimer (int cnnId) {
		yield return new WaitForSeconds(0.5f - (0.5F * players [cnnId].level / 100F));
		yield return new WaitForEndOfFrame();
		players[cnnId].avatar.GetComponent<CharacterController>().Move (players[cnnId].avatar.transform.forward * 20 + Vector3.up);
	}

	IEnumerator secuentialTimer (int cnnId) {
		yield return new WaitForSeconds(0.01f);
	}

	private void Kill(int cnnId){

		//Play Dead animation on every client
		players [cnnId].avatar.GetComponent<Animator> ().SetBool ("isDead", true);
		players [cnnId].avatar.tag = "Dead";

		//Timer to respawn
		StartCoroutine(deadTimer(cnnId));
	}

	IEnumerator deadTimer (int cnnId) {
		
		players [cnnId].avatar.GetComponent<Animator>().SetBool ("movement", false);

		//Create the spawn place if you're the one who died
		if (ourClientId == players [cnnId].connectionId) {
			RaycastHit hit;
			Ray ray = new Ray (new Vector3 (Random.Range (170, -12), 100, Random.Range (104, -96)), new Vector3 (0, -100, 0));

			Vector3 spawnPlace = Vector3.zero;
			if (Physics.Raycast (ray, out hit)) {        
				if (hit.collider != null) {
					// this is where the gameobject is actually put on the ground
					spawnPlace = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
				}
			}
			Send("SPAWNIS|" + spawnPlace.x + "|" + spawnPlace.y + "|" + spawnPlace.z, reliableChannel);
		}

		yield return new WaitForSeconds(2f);										// Wait respawn time

		players [cnnId].avatar.transform.position = players [cnnId].spawnPlace;
		players [cnnId].avatar.transform.eulerAngles = Vector3.zero;
		players [cnnId].position = players [cnnId].spawnPlace;
		players [cnnId].rotation = 0F;
		players [cnnId].respawnFlag = 5;

		players [cnnId].avatar.tag = players [cnnId].tag;
		players [cnnId].avatar.GetComponent<Animator> ().SetBool ("isDead", false);
	}

	private void OnAskName(string [] data){

		//Set this client's ID
		ourClientId = int.Parse (data [1]);

		//Create the spawn place
		RaycastHit hit;
		Ray ray = new Ray (new Vector3 (Random.Range (170, -12), 100, Random.Range (104, -96)), new Vector3 (0,-100,0));

		Vector3 spawnPlace = Vector3.zero;
		if (Physics.Raycast (ray, out hit)) {        
			if (hit.collider != null) {
				// This is where the gameobject is actually put on the ground
				spawnPlace = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
			}
		}

		//Send our name to the server
		Send("NAMEIS|" + playerName + "|" + spawnPlace.x + "|" + spawnPlace.y + "|" + spawnPlace.z + "|" + raze, reliableChannel);

		//Create all the other players
		for (int i = 2; i < data.Length; i++) {
			string[] d = data [i].Split ('%');
			if (d [0] != "TEMP") {
				SpawnPlayer (d [0], int.Parse (d [1]), int.Parse (d [2]), float.Parse (d [3]), float.Parse (d [4]), float.Parse (d [5]), d[6]);
				players[int.Parse(d[1])].level = int.Parse (d [2]);
			}
		}
	}

	private void OnAskPosition(string[] data){

		if (!isStarted)
			return;

		//Update everyone else
		for (int i = 1; i < data.Length; i++) {
			string[] d = data [i].Split ('%');

			if (players.ContainsKey (int.Parse (d [0]))) {
				//Save everyone's position and rotation but ours for interpolation
				if (ourClientId != int.Parse (d [0])) {
					//2 frames of not updating position of a dead player, so he doesn't interpolate
					if (players [int.Parse (d [0])].respawnFlag != 0) {
						players [int.Parse (d [0])].respawnFlag--;
					} else {
						Vector3 position = Vector3.zero;
						position.x = float.Parse (d [1]);
						position.y = float.Parse (d [2]);
						position.z = float.Parse (d [3]);
						players [int.Parse (d [0])].position = position;

						Vector3 rotation = Vector3.zero;
						rotation.y = float.Parse (d [4]);
						players [int.Parse (d [0])].rotation = rotation.y;

						//For linear interpolation
						players [int.Parse (d [0])].startTime = Time.time;
						//players [int.Parse (d [0])].distanceToDestination = Vector3.Distance (players [int.Parse (d [0])].avatar.transform.position, position);

						if (justSpawn) {
							players [int.Parse (d [0])].avatar.transform.position = position;
							players [int.Parse (d [0])].avatar.transform.eulerAngles = rotation;
							justSpawn = false;
						}
					}
				}
			}
		}

		//Send our own position and rotation if not dead
		SendOurPosition();
	}

	public void SendOurPosition (){
		
		//Send our own position and rotation if not dead
		if (players [ourClientId].avatar.tag != "Dead") {
			Vector3 myRotation = players [ourClientId].avatar.transform.eulerAngles;
			Vector3 myPosition = players [ourClientId].avatar.transform.position;
			string m = "MYPOSITION|" + myPosition.x.ToString () + '|' + myPosition.y.ToString () + '|' + myPosition.z.ToString () + '|' + myRotation.y.ToString ();
			Send (m, unreliableChannel);
		}
	}

	private void SpawnPlayer(string playerName, int cnnId, int level, float spawnX, float spawnY, float spawnZ, string raze){

		Player p = new Player ();

        //Initiate new player
        GameObject player = null;
		if (raze == "Mossy") {
			player = Instantiate (goblin) as GameObject;
			player.transform.position = new Vector3 (spawnX, spawnY, spawnZ);
		}

		if (raze == "Ethereal") {
			GameObject plasmaBall;
			player = Instantiate (ethereal) as GameObject;
			plasmaBall = Instantiate (orb) as GameObject;
			player.transform.position = new Vector3 (spawnX, spawnY, spawnZ);
			plasmaBall.transform.SetParent(player.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetChild(0));
			//Relatively to the finger (parent)
			plasmaBall.transform.localPosition = new Vector3 (-0.3f, 0f, -0.7f);
			p.orb = plasmaBall;
			p.wantsOrb = false;
			p.startedRequest = false;
		}

		player.name = playerName;
		lastPlayerAdded = playerName;

		p.avatar = player;
		p.playerName = playerName;
		p.connectionId = cnnId;
		p.tag = player.tag;
		p.respawnFlag = 0;
		p.level = level;
		p.position = new Vector3 (spawnX, spawnY, spawnZ);

		GameObject nameTag = Instantiate (nTag, player.transform.position + new Vector3 (0, 10, 0), player.transform.rotation) as GameObject;

		nameTag.GetComponent<TextMesh> ().text = playerName;

		//Atach nameTag script to nameTag and the owner of the gameTag
		nameTag.AddComponent<nameTag>().player = player;
		nameTag.name = "NameTag: " + playerName;

		// Is it our player?
		if (cnnId == ourClientId) {

			//Set the static variable of your level so the goblin script can access it
			yourLevel = level;

			//Our player's name will be white
			nameTag.GetComponent<TextMesh> ().color = Color.white;

			//Add movility just for him
			if (raze == "Mossy")
				player.AddComponent<goblin> ();
			if (raze == "Ethereal")
				player.AddComponent<wizard> ();

			//Give him arrows ...
			if (raze == "Mossy")
				player.GetComponent<goblin> ().arrow = arrow;
			//... Or give him orbs ...

			//Give him a sword script, so it doesn't request everyone's variables
			if (raze == "Mossy")
				player.transform.Find("Sword").gameObject.AddComponent<sword> ();

			//Then give him a camera
			GameObject go2 = Instantiate (cam) as GameObject;
			p.camera = go2;
			Vector3 offset = new Vector3 (0, 40, -15);
			go2.transform.position = (player.transform.position + offset);

			//Remove Canvas
			GameObject.Find ("Log In").SetActive (false);

            //Create HUD
            GameObject go3 = Instantiate (HUD) as GameObject;

			justSpawn = true;
			isStarted = true;
		}

		players.Add (cnnId, p);
		players[cnnId].avatar.GetComponent<Animator> ().speed = 1F / (1F - players[cnnId].level / 100F);

		if(players.ContainsKey(ourClientId))
			ColorUpdate ();
	}

	private void PlayerDisconnected (int cnnId){

		//Destroy orb
		if (players [cnnId].avatar.tag == "Ethereal")
			Destroy (players[cnnId].orb);
				
		Destroy (players [cnnId].avatar);
		Destroy (players [cnnId].camera);
		Destroy (GameObject.Find("NameTag: " + players[cnnId].playerName));
		players.Remove (cnnId);
	}

	public void Send (string message, int channelId){
		
		//Debug.Log ("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes (message);
		NetworkTransport.Send (hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
	}

	public static string getNameAvatar(){
		
		return lastPlayerAdded;
	}
}