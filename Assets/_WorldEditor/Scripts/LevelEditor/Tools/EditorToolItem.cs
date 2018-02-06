﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DragginzWorldEditor
{
	public class EditorToolItem : EditorTool {

		public EditorToolItem() : base(Globals.EDITOR_TOOL_ITEMS)
		{
			//
		}

		public override void customUpdate(float time, float timeDelta)
		{
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
				if (Input.GetKey (KeyCode.LeftShift)) {
					MainMenu.Instance.toggleItem (Input.GetAxis ("Mouse ScrollWheel"));
				}
			}

			doRayCast ();

			if (_goHit != null && _levelEditor.goCurItem != null)
			{
				//_trfmAimItem.forward = _hit.normal;
				//_trfmAimTool.forward = _hit.normal;

				//Vector3 v3Bounds = _levelEditor.goCurItem.GetComponent<Renderer> ().bounds.extents;
				//Vector3 v3Pos = _goHit.transform.position + _hit.normal;
				//v3Pos.x += (_hit.normal.x * v3Bounds.x);
				//v3Pos.y += (_hit.normal.y * v3Bounds.y);
				//v3Pos.z += (_hit.normal.z * v3Bounds.z);

				_trfmAimItem.position = _goHit.transform.position + _hit.normal;
				_trfmAimTool.position = _trfmAimItem.position;

				_trfmAimTool.localScale = _levelEditor.goCurItem.GetComponent<Renderer> ().bounds.extents * 2.0f;

				if (_mouseIsDown) {
					//placeIt (_trfmAimTool.position);
					_mouseIsDown = false;
				}
			}
			else {
				resetItem ();
			}
		}

		public void placeIt(Vector3 v3Pos)
		{
			/*int x = (int)(v3Pos.x < 0 ? Math.Round(v3Pos.x, MidpointRounding.AwayFromZero) : v3Pos.x);
			int y = (int)(v3Pos.y < 0 ? Math.Round(v3Pos.y, MidpointRounding.AwayFromZero) : v3Pos.y);
			int z = (int)(v3Pos.z < 0 ? Math.Round(v3Pos.z, MidpointRounding.AwayFromZero) : v3Pos.z);

			LevelEditor levelEditor = _levelEditor;

			// get quadrant

			Vector3 v3QuadrantPos = new Vector3 ((float)x / 1f, (float)y / 1f, (float)z / 1f);
			string sPos = v3QuadrantPos.x.ToString () + "_" + v3QuadrantPos.y.ToString () + "_" + v3QuadrantPos.z.ToString ();
			string sQuadrantName = Globals.containerGameObjectPrepend + sPos;
			Transform trfmQuadrant = levelEditor.goWorld.transform.Find (sQuadrantName);

			//Debug.Log ("quadrant: "+trfmQuadrant+" - "+trfmQuadrant.name);

			// get cild
			Vector3 v3LocalBlockPos = new Vector3 (
				Mathf.Abs(v3QuadrantPos.x-v3Pos.x),
				Mathf.Abs(v3QuadrantPos.y-v3Pos.y),
				Mathf.Abs(v3QuadrantPos.z-v3Pos.z)
			);

			string sName = "r";
			sName += "-" + ((int)(v3LocalBlockPos.x / levelEditor.fRockSize)).ToString ();
			sName += "-" + ((int)(v3LocalBlockPos.y / levelEditor.fRockSize)).ToString ();
			sName += "-" + ((int)(v3LocalBlockPos.z / levelEditor.fRockSize)).ToString ();

			Transform container = trfmQuadrant.Find ("container");
			Transform trfmChild = container.Find (sName);
			if (trfmChild != null) {
				Debug.LogError ("child "+sName+" exists!");
			} else {
				GameObject goNew = World.Instance.createRock (v3LocalBlockPos, container.gameObject, sName);
				setSingleMaterial (goNew, levelEditor.aMaterials[MainMenu.Instance.iSelectedMaterial], false);
				levelEditor.resetUndoActions ();
				levelEditor.addUndoAction (AppState.Build, goNew);
			}*/
		}
	}
}