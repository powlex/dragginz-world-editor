﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DragginzVoxelWorldEditor
{
	public class EditorToolProp : EditorTool {

		public EditorToolProp() : base((int)Globals.TOOL.PROPS)
		{
			//
		}

		public override void customUpdate(float time, float timeDelta)
		{
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
				if (time > _lastMouseWheelUpdate) {
					_lastMouseWheelUpdate = time + 0.2f;
					PropsManager.Instance.toggleSelectedProp (Input.GetAxis ("Mouse ScrollWheel"));
				}
			}

			doRayCast ();

			if (_goHit != null && _goCurProp != null)
			{
				_v3Pos = _hit.point;// _goHit.transform.position;

				if (_hit.normal.y > 0) {
					_goCurProp.transform.forward = Vector3.forward;
					_v3Pos.y += (_yOffset + 0.05f);
				}
				else if (_hit.normal.y < 0) {
					_goCurProp.transform.forward = Vector3.forward;
					_v3Pos.y -= (_yOffset + 0.05f);
				} else {
					_goCurProp.transform.forward = _hit.normal;
					_v3Pos += (_hit.normal * (_zOffset + 0.05f));
				}

				_goCurProp.transform.position = _v3Pos;

				if (_mouseIsDown) {
					placeIt (_v3Pos);
					_mouseIsDown = false;
				}

			}
			else {
				resetProp ();
			}
		}

		public void placeIt(Vector3 v3Pos)
		{
			string sName = _goCurProp.name + "_" + _levelEditor.curVoxelsLevelChunk.trfmProps.childCount;

			propDef prop = PropsManager.Instance.getSelectedPropDef ();

			GameObject goNew = PropsManager.Instance.createProp (prop, v3Pos, sName, _levelEditor.curVoxelsLevelChunk.trfmProps, prop.useCollider, prop.useGravity);
			goNew.transform.forward = _goCurProp.transform.forward;

			_levelEditor.curVoxelsLevelChunk.addWorldProp (prop.id, goNew);

			_levelEditor.resetUndoActions ();
			_levelEditor.addUndoAction (AppState.Props, goNew);
		}
	}
}