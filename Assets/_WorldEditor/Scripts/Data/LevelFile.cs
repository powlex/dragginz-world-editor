﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SimpleJSON;

namespace DragginzWorldEditor
{
	[Serializable]
	public class LevelFile {

		[SerializeField]
		public int fileFormatVersion { get; set; }

		[SerializeField]
		public string levelName { get; set; }

		[SerializeField]
		public List<LevelQuadrant> levelQuadrants { get; set; }

		[SerializeField]
		public List<LevelProp> levelProps { get; set; }

		[SerializeField]
		public DataTypeVector3 playerPosition  { get; set; }

		[SerializeField]
		public DataTypeVector3 playerEuler  { get; set; }

		//
		// Parse JSON data
		//
		public void parseJson(string json)
		{
			int i, len;
			LevelQuadrant levelQuadrant;
			LevelProp levelProp;

			JSONNode data = JSON.Parse(json);

			fileFormatVersion = -1;
			if (data ["v"] != null) {
				fileFormatVersion = Int32.Parse (data ["v"]);
			}

			levelName = "";
			if (data ["n"] != null) {
				levelName = data ["n"];
			}

			levelQuadrants = new List<LevelQuadrant> ();
			if (data ["quads"] != null) {
				JSONArray quads = (JSONArray) data ["quads"];
				if (quads != null) {
					len = quads.Count;
					for (i = 0; i < len; ++i) {
						levelQuadrant = new LevelQuadrant ();
						levelQuadrant.parseJson (quads [i]);
						levelQuadrants.Add (levelQuadrant);
					}
				}
			}
				
			levelProps = new List<LevelProp> ();
			if (data ["props"] != null) {
				JSONArray props = (JSONArray) data ["props"];
				if (props != null) {
					len = props.Count;
					for (i = 0; i < len; ++i) {
						levelProp = new LevelProp ();
						levelProp.parseJson (props [i]);
						levelProps.Add (levelProp);
					}
				}
			}

			playerPosition = new DataTypeVector3 ();
			if (data ["p"] != null) {
				playerPosition.x = data ["p"]["x"];
				playerPosition.y = data ["p"]["y"];
				playerPosition.z = data ["p"]["z"];
			}

			playerEuler = new DataTypeVector3 ();
			if (data ["r"] != null) {
				playerEuler.x = data ["r"]["x"];
				playerEuler.y = data ["r"]["y"];
				playerEuler.z = data ["r"]["z"];
			}
		}

		//
		// Create JSON string
		//
		public string getJsonString()
		{
			int i, len;

			string s = "{";

			s += "\"v\":" + fileFormatVersion.ToString();
			s += ",\"n\":" + "\"" + levelName + "\"";

			s += ",\"quads\":[";
			len = levelQuadrants.Count;
			for (i = 0; i < len; ++i) {
				s += (i > 0 ? "," : "");
				s += levelQuadrants [i].getJsonString ();
			}
			s += "]";

			s += ",\"props\":[";
			len = levelProps.Count;
			for (i = 0; i < len; ++i) {
				s += (i > 0 ? "," : "");
				s += levelProps [i].getJsonString ();
			}
			s += "]";

			s += ",\"p\":" + playerPosition.getJsonString();
			s += ",\"r\":" + playerEuler.getJsonString();

			s += "}";

			return s;
		}
	}
}