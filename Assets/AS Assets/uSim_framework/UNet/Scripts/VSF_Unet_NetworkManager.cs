using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class VSF_Unet_NetworkManager : NetworkManager {

	public string playerName;

	public GameObject listPanel;
	public GameObject matchButtonPrefab;
	List<GameObject> spawnedButtons;
	MatchInfoSnapshot matchToJoin;
	string matchPassword;

	//AI
	public Slider aiSizeSlider;
	public Text aiSizeText;
	int aiSize;


	// Use this for initialization
	void Start () {
		spawnedButtons = new List<GameObject> ();
		aiSizeSlider.onValueChanged.AddListener(delegate {SetAiSize(); });
	}


	public void GetMatches () {

		NetworkManager.singleton.StartMatchMaker();
		NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);

	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
	{
		if (success)
			FillMatchesList (matches);
	}

	void FillMatchesList (List<MatchInfoSnapshot> matches) {

		ClearMatchesList ();


		for (int i = 0; i < matches.Count; i++) {
			MatchInfoSnapshot result = matches [i];
			GameObject newMatch = (GameObject)Instantiate (matchButtonPrefab, listPanel.transform);
			spawnedButtons.Add (newMatch);
			newMatch.transform.localPosition = new Vector3 (107f, -10f * (1+i) , 0f);
			Button button = newMatch.GetComponent<Button> ();
			string usePwd = "No";
			if (result.isPrivate)
				usePwd = "Yes";
			button.GetComponentInChildren<Text> ().text = result.name + " | " + result.currentSize.ToString () + "/" + result.maxSize.ToString () + " | " + " N/A " + " | " + usePwd;
			button.onClick.AddListener(delegate { JoinMatch(result); });
			print ("Found match: " + result.name);
		}

	}

	void ClearMatchesList (){

		if (spawnedButtons.Count == 0)
			return;
		
		foreach (GameObject button in spawnedButtons) {

			Destroy (button);

		}

	}

	public void CreateMatch (){
		
		NetworkManager.singleton.StartMatchMaker();
		string matchName = NetworkManager.singleton.matchName;
		uint matchSize = NetworkManager.singleton.matchSize;

		NetworkManager.singleton.matchMaker.CreateMatch (matchName, matchSize, true, "", "", "", 0, 0, OnMatchCreate);

	}

	void JoinMatch (MatchInfoSnapshot match){

		NetworkManager.singleton.matchMaker.JoinMatch (match.networkId, "", "", "", 0, 0, OnMatchJoined);

	}

	public void OnLevelWasLoaded (int scene){
		
		if (scene == 1) {
			
			StartCoroutine (SetPlayerData ());

		}
	}

	void OnMatchJoined (bool success, string extendedInfo, MatchInfo matchInfo){

		if (success)
		{
			Debug.Log("Match Joined");
			MatchInfo hostInfo = matchInfo;
			NetworkManager.singleton.StartClient(hostInfo);

		}
		else
		{
			Debug.Log("ERROR : Match Join Failure");
		}
	}

	public virtual void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if (success)
		{
			Debug.Log("Match Created");
			Utility.SetAccessTokenForNetwork (matchInfo.networkId, matchInfo.accessToken);
			MatchInfo hostInfo = matchInfo;
			NetworkServer.Listen(hostInfo, 9000);
			NetworkManager.singleton.StartHost(hostInfo);

		}
		else
		{
			Debug.Log("ERROR : Match Create Failure");
		}
	}


	public void OnConnected(NetworkMessage msg)
	{
		Debug.Log("Connected!");
	}

	public virtual void OnClientConnect(NetworkConnection conn)
	{
		ClientScene.Ready(conn);
		ClientScene.AddPlayer(0);
	}


	IEnumerator SetPlayerData (){

		do{
			yield return new WaitForFixedUpdate();
		}
		while(VSF_Unet_DemoMain.main.playerEntity == null);
		VSF_Unet_DemoMain.main.playerEntity.playerName = playerName;

	}
	
	public void SetMatchName (string name){

		matchName = name;

	}

	public void SetMatchSize (string size){

		int sizeToint = int.Parse (size);
		matchSize = (uint) sizeToint;

	}

	public void SetMatchPwd (string pwd){

		matchPassword = pwd;

	}

	public void SetAiSize (){

		aiSize = (int) aiSizeSlider.value;
		aiSizeText.text = aiSize.ToString ();
	}

	public void SetPlayerName (string callSign){

		playerName = callSign;

	}

}
