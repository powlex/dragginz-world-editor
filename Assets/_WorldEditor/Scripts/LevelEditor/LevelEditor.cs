﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using AssetsShared;

using RTEditor;

namespace DragginzWorldEditor
{
	struct undoItem {
		public GameObject go;
		public Vector3 position;
		public Quaternion rotation;
	};

	struct undoAction {
		public AppState action;
		public GameObject go;
		public string name;
		public Vector3 position;
		public Transform parent;
		public Material material;
		public List<undoItem> items;
	};

	public class LevelEditor : MonoSingleton<LevelEditor>
	{
		public TextAsset levelListJson;

		public Camera editCam;
		public Camera itemCam;
		public Camera playRigCam;
		public RTEditorCam itemCamScript;

		//public GameObject goWorld;
		//public GameObject goProps;
		public GameObject goPlayer;
		public GameObject goPlayerCameraRig;
		public GameObject goPlayerEdit;

		public GameObject laserAim;
		public GameObject laserAimCenterCube;
		public Material laserAimMaterial;

		public GameObject propAim;

		//public List<GameObject> itemPrefabs;
		public Material materialEdge;
        public List<Material> materialsWalls;

		private Dictionary<int, LevelChunk> _levelChunks;
		private LevelChunk _curLevelChunk;

		//private World _World;
		private Popup _popup;

		private List<EditorTool> _aEditorTools;
		private EditorTool _curEditorTool;

		private List<Texture> _aTextures;
		private List<Material> _aMaterials;
		private Dictionary<string, Material> _aDictMaterials;

		private List<undoAction> _undoActions;

		private GameObject _goCurProp;
		private List<GameObject> _selectedObjects;

		private Camera _activeCam;
		private Plane[] _planes;
		private float _nextDistanceUpdate;

		private float _fRockSize;
        private int _cubesPerQuadrant;
        private float _fQuadrantSize;

        private int _numCubes;

		private bool _isInitialised;

		#region Getters

		public float fRockSize {
			get { return _fRockSize; }
		}

		public int cubesPerQuadrant {
			get { return _cubesPerQuadrant; }
		}

		public float fQuadrantSize {
			get { return _fQuadrantSize; }
		}

		public List<Texture> aTextures {
			get { return _aTextures; }
		}

		public List<Material> aMaterials {
			get { return _aMaterials; }
		}

		public Dictionary<string, Material> aDictMaterials {
			get { return _aDictMaterials; }
		}

		public GameObject goCurItem {
			get { return _goCurProp; }
		}

		public LevelChunk curLevelChunk {
			get { return _curLevelChunk; }
		}

		#endregion

		#region SystemMethods

		void Awake()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			_isInitialised = false;

			_levelChunks = new Dictionary<int, LevelChunk> ();
			_curLevelChunk = null;

			//gameObject.AddComponent<World> ();
			//_World = World.Instance;

			_aTextures = new List<Texture> ();
			_aMaterials = new List<Material> ();
			_aDictMaterials = new Dictionary<string, Material> ();
			int i, len = Globals.materials.Length;
			for (i = 0; i < len; ++i) {
				_aTextures.Add(Resources.Load<Texture> ("Textures/Cubes/" + Globals.materials [i]));
				_aMaterials.Add(Resources.Load<Material> ("Materials/Cubes/" + Globals.materials [i]));
				_aDictMaterials.Add(Globals.materials [i], _aMaterials[_aMaterials.Count-1]);
			}

			_undoActions = new List<undoAction> ();

			_goCurProp = null;
			_selectedObjects = new List<GameObject> ();

			_activeCam = editCam;
			_nextDistanceUpdate = 0;

			_fRockSize = 0.5f;
			_cubesPerQuadrant = 2;
			_fQuadrantSize = (float)_cubesPerQuadrant * _fRockSize;

			// Instantiate app controller singleton
			if (GameObject.Find (Globals.appContainerName) == null) {
				GameObject goAppController = new GameObject (Globals.appContainerName);
				DontDestroyOnLoad (goAppController);
				goAppController.AddComponent<AppController> ();
			}

			if (GameObject.Find (Globals.netContainerName) == null) {
				GameObject goNetManager = new GameObject (Globals.netContainerName);
				DontDestroyOnLoad (goNetManager);
				goNetManager.AddComponent<NetManager> ();
			}
		}

		//
		public void initOfflineMode()
		{
			if (_isInitialised) {
				return;
			}

			PropsManager.Instance.init ();

			_curLevelChunk = LevelManager.Instance.createOfflineLevelChunk ();
			_curLevelChunk.createOfflineLevel ();
			_curLevelChunk.activate (true, true);

			_isInitialised = true;

			launch ();
		}

		//
		public void initOnlineMode()
		{
			if (_isInitialised) {
				return;
			}

			PropsManager.Instance.init ();

			_levelChunks = LevelManager.Instance.createLevelChunks();

			_isInitialised = true;
		}

		//
		public void createLevelChunkWithIndex(int levelId, int levelIndex)
		{
			_curLevelChunk = _levelChunks [levelId];
			LevelManager.Instance.loadLevelByIndex (levelIndex);
			_curLevelChunk.activate (false, true);
		}

		//
		public void launch()
		{
			MainMenu.Instance.init();

			_popup = MainMenu.Instance.popup;

			//goPlayer.transform.position = new Vector3(0, 0.6f, -0.75f);
			//goPlayerCameraRig.transform.position = goPlayer.transform.position;

			_aEditorTools = new List<EditorTool> (Globals.NUM_EDITOR_TOOLS);
			_aEditorTools.Add(new EditorToolLook());
			_aEditorTools.Add(new EditorToolDig());
			_aEditorTools.Add(new EditorToolPaint());
			_aEditorTools.Add(new EditorToolBuild());
			_aEditorTools.Add(new EditorToolItem());

			_curEditorTool = null;

			if (!AppController.Instance.editorIsInOfflineMode) {
				int firstLevelId = LevelManager.Instance.getLevelIdByIndex (0);
				teleportToLevelWithId (firstLevelId);
			}

			SceneManager.UnloadSceneAsync(BuildSettings.SplashScreenScene);
			setMode (AppState.Null, true);

			if (!_popup.isVisible ()) {
				showHelpPopup ();
			}
		}

		//
		public void teleportToLevelWithIndex(int index)
		{
			int levelId = LevelManager.Instance.getLevelIdByIndex (index);
			teleportToLevelWithId (levelId);
		}

		//
		public void teleportToLevelWithId(int levelId)
		{
			if (_curLevelChunk != null) {
				_curLevelChunk.activate (false);
			}

			_curLevelChunk = _levelChunks [levelId];
			_curLevelChunk.activate (true);

			FlyCam.Instance.setNewInitialPosition (_curLevelChunk.getStartPos(), _curLevelChunk.getStartRotation());
			resetCamToStartPos ();

			checkLevelChunkDistances ();
		}

        #endregion

        // ----------------------------------------------------------------------------------------

        #region PublicMethods

		private void showHelpPopup() {
			AppController.Instance.showPopup(
				PopupMode.Notification,
				"Controls",
				"Normal movement: AWSD\nUp and down: QE\nLook around: Right mouse button\nWireframe mode: Shift\nToggle tools: Mouse wheel\n\nPress H to jump back to starting position.",
				startUpPopupCallback
			);
		}

		public void startUpPopupCallback(int buttonId) {

			AppController.Instance.hidePopup ();
			setMode (AppState.Select);
		}

		//
		public void resetAll() {

			resetUndoActions ();

			if (_curEditorTool != null) {
				_curEditorTool.resetAll ();
			}

			//_World.resetAll ();
		}

		//
		public void createNewLevel()
		{
			if (!AppController.Instance.editorIsInOfflineMode) {
				return;
			}

			LevelData.Instance.lastLevelName = Globals.defaultLevelName;

			_curLevelChunk.reset ();
			_curLevelChunk.createOfflineLevel ();

			Vector3 savedPos = new Vector3 (18.35f, 18.90f, 17.25f);
			Vector3 savedRot = Vector2.zero;
			FlyCam.Instance.setNewInitialPosition (savedPos, savedRot);
			resetCamToStartPos ();

			setMode (AppState.Select, true);
		}

		//
		public void customUpdateCheckControls(float time, float timeDelta)
		{
			if (_popup.isVisible ())
			{
				if (Input.GetKeyDown (KeyCode.Escape)) {
					AppController.Instance.hidePopup ();
					if (AppController.Instance.appState == AppState.Null) {
						startUpPopupCallback (-1);
					}
				}
			}
			else {
				
				if (Input.GetKeyDown (KeyCode.LeftShift)) {
					FlyCam.Instance.drawWireframe = true;
					itemCamScript.drawWireframe = true;
				}
				else if (Input.GetKeyUp (KeyCode.LeftShift)) {
					FlyCam.Instance.drawWireframe = false;
					itemCamScript.drawWireframe = false;
				}

				if (Input.GetKeyDown (KeyCode.F1)) {
					showHelpPopup ();
				} else if (Input.GetKeyDown (KeyCode.Escape)) {
					if (AppController.Instance.appState != AppState.Select) {
						setMode (AppState.Select);
					}
				} else if (Input.GetKeyDown (KeyCode.P)) { // (Input.GetKeyDown (KeyCode.Tab) || Input.GetKeyDown (KeyCode.BackQuote)) {
					setMode (AppController.Instance.appState == AppState.Play ? AppState.Select : AppState.Play);
				}
				else if (Input.GetKeyDown(KeyCode.H)) {
					resetCamToStartPos ();
				}
				//else if (Input.GetKeyDown(KeyCode.Alpha0)) {
				//	setMode (AppState.Play);
				//}
				else if (Input.GetKeyDown(KeyCode.Alpha1)) {
					setMode (AppState.Select);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha2)) {
					setMode (AppState.Dig);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha3)) {
					setMode (AppState.Build);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha4)) {
					setMode (AppState.Paint);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha5)) {
					setMode(AppState.Props);
				}
			}

			if (_curEditorTool != null) {
				_curEditorTool.customUpdateControls (time, timeDelta);
			}
		}

		// LateUpdate call
		public void customUpdate(float time, float timeDelta)
		{
			if (_curEditorTool != null) {
				_curEditorTool.customUpdate (time, timeDelta);
			}

			if (time > _nextDistanceUpdate) {
				_nextDistanceUpdate = time + 1.0f;
				checkLevelChunkDistances ();
			}
		}

		//
		private void checkLevelChunkDistances()
		{
			if (AppController.Instance.appState != AppState.Null && AppController.Instance.appState != AppState.Splash) {

				Vector3 playerPoint = FlyCam.Instance.player.position;
				Vector3 closestPoint;
				float dist;
				foreach (KeyValuePair<int, LevelChunk> chunk in _levelChunks) {
					//// if (chunk.Value.levelId != _curLevelChunk.levelId) {
					//// dist = Vector3.Distance (FlyCam.Instance.player.position, chunk.Value.chunkPos);
					//// dist = chunk.Value.chunkBounds.SqrDistance (FlyCam.Instance.player.position);
					closestPoint = chunk.Value.chunkBounds.ClosestPoint(playerPoint);
					dist = Vector3.Distance (playerPoint, closestPoint);
					if (dist > 18.0f) {
						chunk.Value.activate (false);
					} else {
						_planes = GeometryUtility.CalculateFrustumPlanes(_activeCam);
						if (GeometryUtility.TestPlanesAABB (_planes, chunk.Value.chunkBounds)) {
							chunk.Value.activate (true);
						} else {
							chunk.Value.activate (false);
						}
					}
					//// }
				}
			}
		}

		//
		public void setMode(AppState mode, bool forceMode = false)
		{
			if (mode == AppController.Instance.appState && !forceMode) {
				return;
			}

			AppController.Instance.setAppState (mode);
			MainMenu.Instance.setModeButtons (mode);
			MainMenu.Instance.setMenuPanels (mode);

			if (_curEditorTool != null) {
				_curEditorTool.setSingleMaterial (laserAim, laserAimMaterial, false);
				_curEditorTool.resetMaterial ();
				_curEditorTool.resetAim ();
				_curEditorTool.resetProp ();
				_curEditorTool.deactivate ();
			}
			_curEditorTool = null;

			MainMenu.Instance.showTransformBox (false);
			MainMenu.Instance.showDigButtons (false);
			MainMenu.Instance.showMaterialBox (false);
			MainMenu.Instance.showItemsBox (false);

			laserAim.SetActive (false);
			propAim.SetActive (false);

			editCam.enabled = true;
			itemCam.enabled = false;
			_activeCam = editCam;

			EditorObjectSelection.Instance.ClearSelection(false);

			setSelectedObjects ();

			SceneGizmo.Instance.editorCameraTransform = EditorCamera.Instance.transform;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			if (goPlayer != null && goPlayerEdit != null) {
				goPlayer.SetActive ((mode == AppState.Play));
				goPlayerCameraRig.SetActive ((mode == AppState.Play));
				goPlayerEdit.SetActive (!goPlayer.activeSelf);
				_activeCam = playRigCam;
			}

			if (mode == AppState.Play)
			{
				resetCamToStartPos (false); // don't go back to starting pos
			}
			else if (mode == AppState.Select)
			{
				MainMenu.Instance.showTransformBox (true);
				_curEditorTool = _aEditorTools [Globals.EDITOR_TOOL_LOOK];
				itemCam.enabled = true;
				itemCam.transform.position = editCam.transform.position;
				_activeCam = itemCam;
				editCam.enabled = false;
			}
			else if (mode == AppState.Dig)
			{
				MainMenu.Instance.showDigButtons (true);
				laserAim.SetActive (true);
				_curEditorTool = _aEditorTools [Globals.EDITOR_TOOL_DIG];
			}
			else if (mode == AppState.Paint)
			{
				MainMenu.Instance.showMaterialBox (true);
				MainMenu.Instance.showDigButtons (true);
				laserAim.SetActive (true);
				_curEditorTool = _aEditorTools [Globals.EDITOR_TOOL_PAINT];
				_curEditorTool.setCurAimMaterial ();
			}
			else if (mode == AppState.Build)
			{
				MainMenu.Instance.showMaterialBox (true);
				MainMenu.Instance.showDigButtons (true);
				laserAim.SetActive (true);
				_curEditorTool = _aEditorTools [Globals.EDITOR_TOOL_BUILD];
				//_curEditorTool.setSingleMaterial (laserAim, _aMaterials[MainMenu.Instance.iSelectedMaterial], false);
				_curEditorTool.setCurAimMaterial ();
			}
			else if (mode == AppState.Props)
			{
				//MainMenu.Instance.showItemsBox (true);
				_curEditorTool = _aEditorTools [Globals.EDITOR_TOOL_ITEMS];
				laserAim.SetActive (true);
				propAim.SetActive (true);
				if (_goCurProp == null) {
					newPropSelected ();
				}
			}

			SceneGizmo.Instance.editorCameraTransform = (itemCam.enabled ? EditorCamera.Instance.transform : FlyCam.Instance.player);

			RuntimeEditorApplication.Instance.gameObject.SetActive(mode == AppState.Select);

			MainMenu.Instance.setDigSliders (mode);
			//MainMenu.Instance.resetDigSettings (new Vector3 (1, 1, 1));
			updateDigSettings (MainMenu.Instance.v3DigSettings);

			if (_curEditorTool != null) {
				_curEditorTool.activate ();
			}
		}

		//
		public void addUndoAction (AppState action, GameObject go)
		{
			//Debug.Log ("addUndoAction " + action);
			undoAction undo = new undoAction();
			undo.action = action;
			if (go != null) {
				undo.go = go;
				undo.name = go.name;
				undo.position = go.transform.localPosition;
				undo.parent = go.transform.parent;
				if (go.GetComponent<Renderer> () != null) {
					undo.material = go.GetComponent<Renderer> ().sharedMaterial;
				}
			}

			// items
			undo.items = new List<undoItem> ();
			foreach (Transform child in _curLevelChunk.trfmProps) {
				undoItem item = new undoItem ();
				item.go = child.gameObject;
				item.position = child.position;
				item.rotation = child.rotation;
				undo.items.Add (item);
			}

			// save undo step
			_undoActions.Add(undo);
			MainMenu.Instance.setUndoButton (true);
		}

		//
		public void resetUndoActions()
		{
			undoAction undo;
			undoItem item;

			int i, len = _undoActions.Count;
			for (i = 0; i < len; ++i) {
				undo = _undoActions [i];
				undo.go = null;
				undo.parent = null;
				undo.material = null;

				// items
				int j, len2 = undo.items.Count;
				for (j = 0; j < len2; ++j) {
					item = undo.items[j];
					item.go = null;
					undo.items[j] = item;
				}
				undo.items.Clear ();

				_undoActions [i] = undo;
			}

			_undoActions.Clear ();
			MainMenu.Instance.setUndoButton (false);
		}

		//
		public void undoLastActions()
		{
			EditorObjectSelection.Instance.ClearSelection(false);

			int effectedCubes = 0;

			Shader shader = Shader.Find (Globals.defaultShaderName);
			Renderer renderer;

			undoAction undo;
			undoItem item;

			int i, len = _undoActions.Count;
			//Debug.Log ("undoLastActions " + len);
			for (i = 0; i < len; ++i)
			{
				undo = _undoActions [i];

				// DIG
				if (undo.action == AppState.Dig) {
					if (undo.go != null) {
						undo.go.SetActive (true);
						effectedCubes++;
					}
					/*if (undo.parent != null) {
						_World.createRock (undo.position, undo.parent.gameObject, undo.name, undo.material);
						undo.material.shader = shader;
						effectedCubes++;
					}*/
				}
				// BUILD
				else if (undo.action == AppState.Build) {
					if (undo.go != null) {
						//Destroy (undo.go);
						undo.go.SetActive (false);
						effectedCubes--;
					}
				}
				// PAINT
				else if (undo.action == AppState.Paint) {
					if (undo.go != null) {
						undo.material.shader = shader;
						renderer = undo.go.GetComponent<Renderer> ();
						if (renderer != null) {
							renderer.sharedMaterial = undo.material;
						}
					}
				}
				// ITEM
				else if (undo.action == AppState.Props) {
					if (undo.go != null) {
						_curLevelChunk.removeWorldProp (undo.go);
						Destroy (undo.go);
					}
				}

				undo.go = null;
				undo.parent = null;
				undo.material = null;

				// items
				int j, len2 = undo.items.Count;
				for (j = 0; j < len2; ++j) {
					item = undo.items[j];
					if (item.go != null) {
						item.go.transform.position = item.position;
						item.go.transform.rotation = item.rotation;
					}
					item.go = null;
				}
				undo.items.Clear ();

				_undoActions [i] = undo;
			}

			_undoActions.Clear ();
			MainMenu.Instance.setUndoButton (false);

			if (effectedCubes != 0) {
				_curLevelChunk.numCubes += effectedCubes;
				MainMenu.Instance.setCubeCountText (_curLevelChunk.numCubes);
			}
		}

		//
		public void setSelectedObjects(List<GameObject> selectedObjects = null)
		{
			propDef prop;

			int i, len = _selectedObjects.Count;
			for (i = 0; i < len; ++i) {
				if (_selectedObjects [i] != null) {

					prop = _curLevelChunk.getPropDefForGameObject (_selectedObjects [i]);
					if (prop.id != -1)
					{
						if (_selectedObjects [i].GetComponent<Rigidbody> () != null) {
							_selectedObjects [i].GetComponent<Rigidbody> ().useGravity = prop.useGravity;
						}
						if (_selectedObjects [i].GetComponent<Collider> () != null) {
							_selectedObjects [i].GetComponent<Collider> ().enabled = prop.useCollider;
						}
					}
				}
			}

			_selectedObjects.Clear ();

			if (selectedObjects != null) {
				
				_selectedObjects = selectedObjects;

				len = _selectedObjects.Count;
				for (i = 0; i < len; ++i) {
					if (_selectedObjects [i] != null) {
						if (_selectedObjects [i].GetComponent<Rigidbody> () != null) {
							_selectedObjects [i].GetComponent<Rigidbody> ().useGravity = false;
						}
						if (_selectedObjects [i].GetComponent<Collider> () != null) {
							_selectedObjects [i].GetComponent<Collider> ().enabled = false;
						}
					}
				}
			}
		}

		//
		public void updateDigSettings(Vector3 v3DigSettings)
		{
			float fScale = _fRockSize;
			if (AppController.Instance.appState == AppState.Build)
			{
				laserAim.transform.localScale = v3DigSettings * fScale;
				laserAimCenterCube.SetActive (true);
				fScale -= 0.01f;
				laserAimCenterCube.transform.localScale = new Vector3(fScale / laserAim.transform.localScale.x, fScale / laserAim.transform.localScale.y, fScale / laserAim.transform.localScale.z);
			}
			else if (AppController.Instance.appState == AppState.Paint)
			{
				fScale *= 0.5f;
				laserAim.transform.localScale = new Vector3(v3DigSettings.x, v3DigSettings.y, 1) * fScale;
				laserAimCenterCube.SetActive (false);
			}
			else {
				fScale *= 0.75f;
				laserAim.transform.localScale = v3DigSettings * fScale;
				laserAimCenterCube.SetActive (false);
			}
		}

		//
		public void newMaterialSelected (int iSelectedMaterial)
		{
			if (AppController.Instance.appState == AppState.Paint) {
				_curEditorTool.resetMaterial();
			}
			//else if (AppController.Instance.appState == AppState.Build) {
			//	_curEditorTool.setSingleMaterial (laserAim, _aMaterials [MainMenu.Instance.iSelectedMaterial], false);
			//}
		}

		//
		public void newPropSelected ()
		{
			if (AppController.Instance.appState == AppState.Null) {
				return;
			}
				
			if (propAim != null) {
				if (_goCurProp != null) {
					Destroy (_goCurProp);
					_goCurProp = null;
				}
				propDef prop = PropsManager.Instance.getSelectedPropDef ();
				_goCurProp = PropsManager.Instance.createProp (prop, Vector3.zero, prop.name , propAim.transform, false, false);
				_goCurProp.transform.localPosition = Vector3.zero;
			}

			if (_curEditorTool != null) {
				_curEditorTool.resetProp ();
			}
		}

		//
		public void resetCamToStartPos(bool resetPos = true)
		{
			if (resetPos) {
				FlyCam.Instance.reset ();
			}

			if (AppController.Instance.appState == AppState.Play) {
				goPlayer.transform.position = FlyCam.Instance.player.position;
				Vector3 playerPos = Vector3.zero;
				playerPos.y = FlyCam.Instance.player.eulerAngles.y;
				goPlayer.transform.eulerAngles = playerPos;
				goPlayerCameraRig.transform.position = goPlayer.transform.position;
			}
		}

		//
		public List<GameObject> getOverlappingObjects(Vector3 v3Pos, Vector3 extents)
		{
			List<GameObject> listCollidingObjects = new List<GameObject>();

			int i, len;

			//Vector3 pos = MainMenu.Instance.v3DigSettings * (_fRockSize * .75f) * .5f;
			Collider[] hitColliders = Physics.OverlapBox (v3Pos, extents);

			len = hitColliders.Length;
			for (i = 0; i < len; ++i) {
				if (hitColliders [i].tag == "DigAndDestroy") {
					listCollidingObjects.Add (hitColliders [i].gameObject);
				}
			}

			return listCollidingObjects;
		}

		//
		public List<Vector3> getAdjacentCubes(Vector3 v3CubePos) {

			List<Vector3> adjacentCubes = new List<Vector3> ();

			int len = 1;
			for (int x = -len; x <= len; ++x) {
				for (int y = -len; y <= len; ++y) {
					for (int z = -len; z <= len; ++z) {
						adjacentCubes.Add (new Vector3(v3CubePos.x+(x*_fQuadrantSize), v3CubePos.y+(y*_fQuadrantSize), v3CubePos.z+(z*_fQuadrantSize)));
					}
				}
			}

			return adjacentCubes;
		}

		#endregion
	}
}