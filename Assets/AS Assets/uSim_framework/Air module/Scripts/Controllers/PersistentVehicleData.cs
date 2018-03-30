using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class PersistentVehicleData : MonoBehaviour {



	// Use this for initialization
	void Start () {
		LoadData ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadData () {

		string fetchData = ReadData ();

		string[] dataSplit = fetchData.Split (',');

		float posX = float.Parse(dataSplit [1]);
		float posY = float.Parse(dataSplit [2]);
		float posZ = float.Parse(dataSplit [3]);

		float rotX = float.Parse(dataSplit [4]);
		float rotY = float.Parse(dataSplit [5]);
		float rotZ = float.Parse(dataSplit [6]);

		transform.position = new Vector3 (posX, posY, posZ);
		transform.rotation = Quaternion.Euler (rotX, rotY, rotZ);

	}


	public void SaveData () {
		
		string data = "";			

		data = gameObject.name + ","; //0
		data += transform.position.x + ","; //1
		data += transform.position.y + ","; //2
		data += transform.position.z + ","; //3
		data += transform.rotation.eulerAngles.x + ","; //4
		data += transform.rotation.eulerAngles.y + ","; //5
		data += transform.rotation.eulerAngles.z + ","; //6
		data += ",";
		data += ";";
					
		WriteDataToFile (data);

	}


	public void WriteDataToFile (string data) {

		string file = gameObject.name + ".txt";
		StreamWriter sr = null;

		if (File.Exists(Application.dataPath+ "/save/vehicles/" + file))
		{
			//			Debug.Log(Application.dataPath+ "/save/" + file + " already exists. Erasing old data...");
			File.Delete (Application.dataPath + "/save/vehicles/" + file);
		}
		sr = File.CreateText(Application.dataPath+ "/save/vehicles/" + file);

	

		sr.WriteLine (data);



		sr.Close ();

	}


	public string ReadData (){

		string data = "";

		StreamReader reader = new StreamReader(Application.dataPath+ "/save/vehicles/" + gameObject.name + ".txt", Encoding.Default);

		using (reader)
		{
			data = reader.ReadLine();					

			reader.Close();

			return data;
		}

	}

	void OnApplicationQuit () {

		SaveData ();

	}

}
