/**
 *	SnapWindow
 *	
 *	@brief Tool to help level designers build their levels by snapping objects to visible grid
 *	@author Bruno "BMO" Tamer <pg03bruno@vfs.com>
 *	@date 2015.04.10
 */

using System;

using UnityEngine;
using UnityEditor;

/// <summary>
/// Snap tool window
/// </summary>
public sealed class SnapWindow : EditorWindow
{

	#region Properties

	private static SnapSettings _SnapSettings;             // Currently selected snap settings
	private static float        _SnapTimer;                // Timer to update the tool (given an interval)
	private static SceneView    _LastActiveSceneView;      // Currently selected scene view
	private static Vector3      _LastActiveSceneViewPivot; // Last selected scene view's camera pivot point

	#endregion
	
	#region Getters and setters
	
	/// <summary>
	/// Is snapping enabled?
	/// </summary>
	private static bool Snap
	{
		get { return EditorPrefs.GetBool("SnapWindow.Snap"); }
		set { EditorPrefs.SetBool("SnapWindow.Snap", value); }
	}
	
	/// <summary>
	/// Key that has to be held to snap
	/// </summary>
	private static KeyCode SnapKey
	{
		get { return (KeyCode)EditorPrefs.GetInt("SnapWindow.SnapKey"); }
		set
		{
			// If the snap key is about to change, then resets the key press flag to false
			if (SnapKey != value)
				HoldingSnapKey = false;
			
			EditorPrefs.SetInt("SnapWindow.SnapKey", (int)value);
		}
	}

	/// <summary>
	/// Flag that indicates if the selected key for snapping is being held
	/// </summary>
	private static bool HoldingSnapKey { get; set; }

	/// <summary>
	/// Snapping settings
	/// </summary>
	private static SnapSettings SnapSettings
	{
		get
		{
			if (_SnapSettings == null)
				SnapSettings = null;

			return _SnapSettings;
		}
		set
		{
			// The settings can never be null because they define how the snapping is going to work
			// If the new given value is null, then...
			if (value == null)
			{
				// ... Check if there was a settings object assigned previously
				if (_SnapSettings != null)
					// If so, then don't do anything
					return;
				
				// Otherwise, it tries to find the first object on the scene that has a snap settings attached to it
				value = GameObject.FindObjectOfType<SnapSettings>();
				
				if (value == null)
				{
					// If there's none, it tries to find the first object on the scene called SnapSettings
					GameObject gameObject = GameObject.Find("SnapSettings");
					
					if (gameObject == null)
						// If there's none, then it creates one
						gameObject = new GameObject("SnapSettings");
					
					// Then attaches a snap settings component to it
					value = gameObject.AddComponent<SnapSettings>();
				}
			}
			
			// At this point, 'value' is certainly not null, so assigns it
			_SnapSettings = value;
		}
	}

	/// <summary>
	/// Interval between each update call
	/// It's only recommended to change this value if the computer is very slow
	/// Default value is 0
	/// </summary>
	private static float SnapInterval
	{
		get { return EditorPrefs.GetFloat("SnapWindow.SnapInterval"); }
		set
		{
			// The interval can't be below 0, since there's no negative time
			if (value < 0.0f)
				value = 0.0f;

			// If the value has changed
			if (SnapInterval != value)
			{
				// Saves it and resets the timer
				EditorPrefs.SetFloat("SnapWindow.SnapInterval", value);
				_SnapTimer = 0.0f;
			}
		}
	}
	
	#region Position
	
	/// <summary>
	/// Is snapping position enabled?
	/// </summary>
	private static bool SnapPosition
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapPosition"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapPosition", value); }
	}
	
	/// <summary>
	/// Is snapping position X enabled? (requires snapping position to be enabled)
	/// </summary>
	private static bool SnapPositionX
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapPositionX"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapPositionX", value); }
	}
	
	/// <summary>
	/// Is snapping position Y enabled? (requires snapping position to be enabled)
	/// </summary>
	private static bool SnapPositionY
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapPositionY"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapPositionY", value); }
	}
	
	/// <summary>
	/// Is snapping position Z enabled? (requires snapping position to be enabled)
	/// </summary>
	private static bool SnapPositionZ
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapPositionZ"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapPositionZ", value); }
	}
	
	#endregion
	
	#region Rotation
	
	/// <summary>
	/// Is snapping rotation enabled?
	/// </summary>
	private static bool SnapRotation
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapRotation"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapRotation", value); }
	}
	
	/// <summary>
	/// Is snapping rotation X enabled? (requires snapping rotation to be enabled)
	/// </summary>
	private static bool SnapRotationX
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapRotationX"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapRotationX", value); }
	}
	
	/// <summary>
	/// Is snapping rotation Y enabled? (requires snapping rotation to be enabled)
	/// </summary>
	private static bool SnapRotationY
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapRotationY"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapRotationY", value); }
	}
	
	/// <summary>
	/// Is snapping rotation Z enabled? (requires snapping rotation to be enabled)
	/// </summary>
	private static bool SnapRotationZ
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapRotationZ"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapRotationZ", value); }
	}
	
	#endregion
	
	#region Scale
	
	/// <summary>
	/// Is snapping scale enabled?
	/// </summary>
	private static bool SnapScale
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapScale"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapScale", value); }
	}
	
	/// <summary>
	/// Is snapping scale X enabled?
	/// </summary>
	private static bool SnapScaleX
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapScaleX"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapScaleX", value); }
	}
	
	/// <summary>
	/// Is snapping scale Y enabled?
	/// </summary>
	private static bool SnapScaleY
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapScaleY"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapScaleY", value); }
	}
	
	/// <summary>
	/// Is snapping scale Z enabled?
	/// </summary>
	private static bool SnapScaleZ
	{
		get { return EditorPrefs.GetBool("SnapWindow.SnapScaleZ"); }
		set { EditorPrefs.SetBool("SnapWindow.SnapScaleZ", value); }
	}
	
	#endregion
	
	/// <summary>
	/// Is Unreal Mode enabled? (requires at least four scene views)
	/// </summary>
	private static bool UnrealMode
	{
		get { return EditorPrefs.GetBool("SnapWindow.UnrealMode"); }
		set { EditorPrefs.SetBool("SnapWindow.UnrealMode", value); }
	}
	
	/// <summary>
	/// Can the perspective view move freely or align with the others?
	/// (requires Unreal Mode to be enabled)
	/// </summary>
	private static bool FreePerspectiveView
	{
		get { return EditorPrefs.GetBool("SnapWindow.FreePerspectiveView"); }
		set { EditorPrefs.SetBool("SnapWindow.FreePerspectiveView", value); }
	}
	
	/// <summary>
	/// Should all grids be hidden on the perspective view?
	/// (requires Unreal Mode to be enabled)
	/// </summary>
	private static bool HideGridsOnPerspectiveView
	{
		get { return EditorPrefs.GetBool("SnapWindow.HideGridsOnPerspectiveView"); }
		set { EditorPrefs.SetBool("SnapWindow.HideGridsOnPerspectiveView", value); }
	}
	
	#endregion

	/// <summary>
	/// Starts up the tool
	/// </summary>
	[MenuItem("Tools/Snap %&s")]
	public static void Initialize()
	{
		// If this is the first time using the snap tool
		if (!EditorPrefs.HasKey("SnapWindow"))
		{
			EditorPrefs.SetBool("SnapWindow", true);
			
			// Assigns defaults
			Snap                       = true;
			SnapKey                    = KeyCode.Space;
			SnapInterval               = 0.0f;
			SnapPosition               = true;
			SnapPositionX              = true;
			SnapPositionY              = true;
			SnapPositionZ              = true;
			SnapRotation               = true;
			SnapRotationX              = true;
			SnapRotationY              = true;
			SnapRotationZ              = true;
			SnapScale                  = true;
			SnapScaleX                 = true;
			SnapScaleY                 = true;
			SnapScaleZ                 = true;
			UnrealMode                 = true;
			FreePerspectiveView        = true;
			HideGridsOnPerspectiveView = true;
		}
		
		// Displays the snap tool window
		SnapWindow window = (SnapWindow)EditorWindow.GetWindow(typeof(SnapWindow));
		window.title = "Snap";
		window.Show();
	}
	
	/// <summary>
	/// Updates 10 times per second when the window is visible
	/// </summary>
	public void Update()
	{
		// The user will most likely not have the tool window on screen the
		// whole time, so this will get snapping running on the background.
		//
		// The reason why this is not on Initialize is because Unity removes
		// all event listeners from their internal event handlers when it's
		// doing something specific (i.e just started running the game). As
		// there's no way to check if a listener is on any of those event
		// handlers "lists", it removes ALL references to the methods below (there
		// can be multiple ones to the same method) and adds ONE reference (for
		// each method) back.
		//
		// Unfortunately, the "Update" method is only called when the window
		// is visible. If the user has this window docked and hiding somewhere
		// on their editor and they run the game, they have to get the window
		// to display for a brief moment, so these listeners can be added
		// "for the first time" after the game starts running. 
		//
		// Also, the input method goes to a different event handler, because
		// EditorApplication.update doesn't catch KeyDown and KeyUp events.

		SceneView.onSceneGUIDelegate -= UpdateInput;
		SceneView.onSceneGUIDelegate += UpdateInput;
		
		EditorApplication.update -= UpdateTool;
		EditorApplication.update += UpdateTool;
	}

	/// <summary>
	/// Updates the key press flag
	/// </summary>
	private void UpdateInput(SceneView sceneView)
	{
		Event currentEvent = Event.current;
		
		if (currentEvent != null)
		{
			switch (currentEvent.type)
			{
			case EventType.KeyDown:
				if (currentEvent.keyCode == SnapKey)
					HoldingSnapKey = true;
				break;
			case EventType.KeyUp:
				if (currentEvent.keyCode == SnapKey)
					HoldingSnapKey = false;
				break;
			}
		}
	}

	/// <summary>
	/// Updates the snapping and Unreal Mode
	/// </summary>
	private void UpdateTool()
	{
		// If the interval is 0, there's no reason why to update the timer
		if (SnapInterval == 0.0f)
		{
			// Updates snapping and Unreal Mode
			UpdateSnap();
			UpdateUnrealMode();
		}
		else
		{
			// Otherwise, updates the timer
			_SnapTimer += Time.deltaTime;

			// And if it reaches the interval duration
			if (_SnapTimer >= SnapInterval)
			{
				// Updates snapping and Unreal Mode
				UpdateSnap();
				UpdateUnrealMode();

				// Resets the timer
				_SnapTimer %= SnapInterval;
			}
		}
	}

	#region Main Functionalities

	/// <summary>
	/// Handles snapping
	/// </summary>
	private void UpdateSnap()
	{
		// Does nothing if it's not enabled or if the user isn't holding
		// the snapping key
		if (!Snap || !HoldingSnapKey)
			return;

		Vector3    inputPosition;
		Vector3    inputRotation;
		Vector3    inputScale;
		
		Vector3    outputPosition;
		Quaternion outputRotation;
		Vector3    outputScale;

		// Loops through all selected objects and snap their position, rotation and scale
		// according to the selected settings
		foreach (Transform transform in Selection.transforms)
		{
			if (SnapPosition)
			{
				inputPosition = transform.localPosition;
				
				outputPosition = new Vector3(
					SnapPositionX ? SnapSettings.PositionStep.x * Mathf.Round(inputPosition.x / SnapSettings.PositionStep.x) : inputPosition.x,
					SnapPositionY ? SnapSettings.PositionStep.y * Mathf.Round(inputPosition.y / SnapSettings.PositionStep.y) : inputPosition.y,
					SnapPositionZ ? SnapSettings.PositionStep.z * Mathf.Round(inputPosition.z / SnapSettings.PositionStep.z) : inputPosition.z
				);
				
				if (transform.localPosition != outputPosition)
					transform.localPosition = outputPosition;
			}
			
			if (SnapRotation)
			{
				inputRotation = transform.localEulerAngles;
				
				outputRotation = Quaternion.Euler(
					SnapRotationX ? SnapSettings.RotationStep.x * Mathf.Round(inputRotation.x / SnapSettings.RotationStep.x) : inputRotation.x,
					SnapRotationY ? SnapSettings.RotationStep.y * Mathf.Round(inputRotation.y / SnapSettings.RotationStep.y) : inputRotation.y,
					SnapRotationZ ? SnapSettings.RotationStep.z * Mathf.Round(inputRotation.z / SnapSettings.RotationStep.z) : inputRotation.z
				);
				
				if (transform.localRotation != outputRotation)
					transform.localRotation = outputRotation;
			}
			
			if (SnapScale)
			{
				inputScale = transform.localScale;
				
				outputScale = new Vector3(
					SnapScaleX ? SnapSettings.ScaleStep.x * Mathf.Round(inputScale.x / SnapSettings.ScaleStep.x) : inputScale.x,
					SnapScaleY ? SnapSettings.ScaleStep.y * Mathf.Round(inputScale.y / SnapSettings.ScaleStep.y) : inputScale.y,
					SnapScaleZ ? SnapSettings.ScaleStep.z * Mathf.Round(inputScale.z / SnapSettings.ScaleStep.z) : inputScale.z
				);
				
				if (transform.localScale != outputScale)
					transform.localScale = outputScale;
			}
		}
	}

	/// <summary>
	/// If there are at least 4 scene views on the editor, sets them up
	/// so they behave somewhat close to what it's like on Unreal
	/// </summary>
	private void UpdateUnrealMode()
	{
		// Does nothing if Unreal Mode is off or there are less than 4
		// scene views on the editor
		if (!UnrealMode || (SceneView.sceneViews.Count < 4))
			return;
		
		SceneView perspective = (SceneView)SceneView.sceneViews[0];
		SceneView front       = (SceneView)SceneView.sceneViews[1];
		SceneView right       = (SceneView)SceneView.sceneViews[2];
		SceneView top         = (SceneView)SceneView.sceneViews[3];

		SceneView active = SceneView.lastActiveSceneView;

		// Even though there are four elements on the scene views list,
		// sometimes they are null
		if (
			(perspective == null) ||
			(front       == null) ||
			(right       == null) ||
			(top         == null) ||
			(active      == null)
		)
			// If any one of them is null, then it can't move on
			return;

		// If the currently selected view is the same as the last time it has updated
		if (active == _LastActiveSceneView)
		{
			// Gets how much is has moved
			Vector3 translation = active.pivot - _LastActiveSceneViewPivot;
			Vector3 pivot;

			// If the perspective view sync with the others, then moves its pivot
			if (!FreePerspectiveView)
			{
				// The reason for storing pivot, if it's orthographic, render mode and rotation in variables
				// before assigning, rather than assigning it right away is is because Unity doesn't check if
				// any of those values is equals to what it was previously, forcing every view to redraw
				// themselves (what slows down performance)
				pivot = perspective.pivot + translation;

				if (perspective.pivot != pivot)
					perspective.pivot = pivot;
			}

			// If the perspective view sync with the others or if
			// it doesn't and the active one isn't the perspective one,
			// then moves every pivot
			if (!FreePerspectiveView || (active != perspective))
			{
				pivot = new Vector3(
					top.pivot.x + translation.x,
					SnapSettings.transform.localPosition.y + SnapSettings.GridAnchor.y * SnapSettings.GridSize.y,
					top.pivot.z + translation.z
				);
				
				if (top.pivot != pivot)
					top.pivot = pivot;
				
				pivot = new Vector3(
					SnapSettings.transform.localPosition.x + SnapSettings.GridAnchor.x * SnapSettings.GridSize.x,
					right.pivot.y + translation.y,
					right.pivot.z + translation.z
				);
				
				if (right.pivot != pivot)
					right.pivot = pivot;
				
				pivot = new Vector3(
					front.pivot.x + translation.x,
					front.pivot.y + translation.y,
					SnapSettings.transform.localPosition.z + SnapSettings.GridAnchor.z * SnapSettings.GridSize.z
				);
				
				if (front.pivot != pivot)
					front.pivot = pivot;
			}
		}
		else
			// If the currently selected has changed, saves it
			_LastActiveSceneView = active;

		// Saves the current pivot
		_LastActiveSceneViewPivot = active.pivot;

		// On Unreal, all views are orthographic, except for the perspective one
		if (perspective.orthographic)
			perspective.orthographic = false;

		if (!front.orthographic)
			front.orthographic = true;

		if (!right.orthographic)
			right.orthographic = true;

		if (!top.orthographic)
			top.orthographic = true;

		// And also all views are wireframe, execept for the perspective one
		if (perspective.renderMode != DrawCameraMode.Textured)
			perspective.renderMode = DrawCameraMode.Textured;

		if (front.renderMode != DrawCameraMode.Wireframe)
			front.renderMode = DrawCameraMode.Wireframe;

		if (right.renderMode != DrawCameraMode.Wireframe)
			right.renderMode = DrawCameraMode.Wireframe;

		if (top.renderMode != DrawCameraMode.Wireframe)
			top.renderMode = DrawCameraMode.Wireframe;

		// Rotates every camera accordingly
		Quaternion rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

		if (front.rotation != rotation)
			front.rotation = rotation;

		rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

		if (right.rotation != rotation)
			right.rotation = rotation;

		rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

		if (top.rotation != rotation)
			top.rotation = rotation;

		// Changes the scene views' title in order to help the user to understand
		// what angle the each camera is set to (at first glance)
		perspective.title = "Perspective";
		front.title       = "Front";
		right.title       = "Right";
		top.title         = "Top";
	}

	#endregion

	#region GUI

	/// <summary>
	/// Utility to offset GUI content to the right
	/// </summary>
	private void OffsetRight(Action content)
	{
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			{
				content();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Utility to draw a toggle group with its content offsetted to the right
	/// </summary>
	private bool ToggleGroupWithOffset(string label, bool boolean, Action content)
	{
		bool toggled = EditorGUILayout.BeginToggleGroup(label, boolean);
		{
			OffsetRight(content);
		}
		EditorGUILayout.EndToggleGroup();
		
		return toggled;
	}

	/// <summary>
	/// Draws the window content
	/// </summary>
	public void OnGUI()
	{
		Snap = ToggleGroupWithOffset("Snap", Snap, () => {
			SnapKey      = (KeyCode)EditorGUILayout.EnumPopup("Snap Key", SnapKey);
			SnapSettings = (SnapSettings)EditorGUILayout.ObjectField("Snap Settings", SnapSettings, typeof(SnapSettings), true);
			SnapInterval = EditorGUILayout.FloatField("Snap Interval", SnapInterval);
			
			EditorGUILayout.Separator();
			
			SnapPosition = ToggleGroupWithOffset("Snap Position", SnapPosition, () => {
				SnapPositionX = EditorGUILayout.ToggleLeft("Snap Position X", SnapPositionX);
				SnapPositionY = EditorGUILayout.ToggleLeft("Snap Position Y", SnapPositionY);
				SnapPositionZ = EditorGUILayout.ToggleLeft("Snap Position Z", SnapPositionZ);
			});
			
			EditorGUILayout.Separator();
			
			SnapRotation = ToggleGroupWithOffset("Snap Rotation", SnapRotation, () => {
				SnapRotationX = EditorGUILayout.ToggleLeft("Snap Rotation X", SnapRotationX);
				SnapRotationY = EditorGUILayout.ToggleLeft("Snap Rotation Y", SnapRotationY);
				SnapRotationZ = EditorGUILayout.ToggleLeft("Snap Rotation Z", SnapRotationZ);
			});
			
			EditorGUILayout.Separator();
			
			SnapScale = ToggleGroupWithOffset("Snap Scale", SnapScale, () => {
				SnapScaleX = EditorGUILayout.ToggleLeft("Snap Scale X", SnapScaleX);
				SnapScaleY = EditorGUILayout.ToggleLeft("Snap Scale Y", SnapScaleY);
				SnapScaleZ = EditorGUILayout.ToggleLeft("Snap Scale Z", SnapScaleZ);
			});
		});
		
		EditorGUILayout.Separator();

		// If there are less than 4 scene view on the editor
		if (SceneView.sceneViews.Count < 4)
			// Tells the user that there are more options to explore
			EditorGUILayout.HelpBox("To display Unreal Mode settings, you must have at least 4 scene views on your editor. If you don't know how to add or remove scene views or don't want to bother doing so, maybe you should click on the \"4 Split\" button below.\nWARNING: your current layout will NOT be saved.", MessageType.Warning);
		else
		{
			// If there are at least 4 scene views, display the extra options
			UnrealMode = ToggleGroupWithOffset("Unreal Mode", UnrealMode, () => {
				FreePerspectiveView = EditorGUILayout.ToggleLeft("Free Perspective View", FreePerspectiveView);
				HideGridsOnPerspectiveView = EditorGUILayout.ToggleLeft("Hide Grids On Perspective View", HideGridsOnPerspectiveView);
			});
		}

		// Buttons to help easily changing layout
		if (GUILayout.Button("4 Split"))
		{
			EditorApplication.ExecuteMenuItem("Window/Layouts/4 Split");
			EditorApplication.ExecuteMenuItem("Tools/Snap %&s");
		}
		else if (GUILayout.Button("Wide"))
		{
			EditorApplication.ExecuteMenuItem("Window/Layouts/Wide");
			EditorApplication.ExecuteMenuItem("Tools/Snap %&s");
		}
	}
	
	#endregion
	
	#region Gizmos

	/// <summary>
	/// Draws the grid
	/// </summary>
	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Active)]
	public static void DrawGrid(SnapSettings snapSettings, GizmoType gizmoType)
	{
		// If the grid is hidden, then don't do anyhting
		if (!snapSettings.ShowGrid)
			return;

		// Gets the current scene view, so it's possible to retrieve its camera rotation,
		// so the grid can rotate as well
		SceneView currentDrawingSceneView = SceneView.currentDrawingSceneView;

		// If Unreal Mode is activated and it's set not to display grids on the perspective view and this is it,
		// then don't do anything
		if (UnrealMode && HideGridsOnPerspectiveView && (currentDrawingSceneView.title == "Perspective"))
			return;

		// Gets the camera rotation
		Vector3 sceneViewEulerAngles = currentDrawingSceneView.rotation.eulerAngles;

		// Depending on the camera rotation, the planes have to rotate
		bool invertPlaneXY;
		bool invertPlaneXZ;
		bool invertPlaneYZ;
		
		if (sceneViewEulerAngles.x < 0.0f)
			sceneViewEulerAngles.x += 360.0f;
		
		invertPlaneXZ = sceneViewEulerAngles.x > 90.0f;
		
		if (sceneViewEulerAngles.y < 0.0f)
			sceneViewEulerAngles.y += 360.0f;
		
		if (sceneViewEulerAngles.y < 90.0f)
		{
			invertPlaneXY = true;
			invertPlaneYZ = true;
		}
		else if (sceneViewEulerAngles.y < 180.0f)
		{
			invertPlaneXY = false;
			invertPlaneYZ = true;
		}
		else if (sceneViewEulerAngles.y < 270.0f)
		{
			invertPlaneXY = false;
			invertPlaneYZ = false;
		}
		else
		{
			invertPlaneXY = true;
			invertPlaneYZ = false;
		}

		// Changes the drawing color to the grid's color
		Gizmos.color = snapSettings.GridColor;

		// Gets how many tiles it's going to draw for each dimension
		int gridWidth  = Mathf.Abs(Mathf.FloorToInt(snapSettings.GridSize.x / snapSettings.PositionStep.x));
		int gridHeight = Mathf.Abs(Mathf.FloorToInt(snapSettings.GridSize.y / snapSettings.PositionStep.y));
		int gridDepth  = Mathf.Abs(Mathf.FloorToInt(snapSettings.GridSize.z / snapSettings.PositionStep.z));

		// Calculates the grid offset according to its object position and anchoring
		Vector3 gridOffset = new Vector3(
			snapSettings.PositionStep.x * Mathf.Round((snapSettings.transform.localPosition.x - snapSettings.GridAnchor.x * snapSettings.GridSize.x) / snapSettings.PositionStep.x),
			snapSettings.PositionStep.y * Mathf.Round((snapSettings.transform.localPosition.y - snapSettings.GridAnchor.y * snapSettings.GridSize.y) / snapSettings.PositionStep.y),
			snapSettings.PositionStep.z * Mathf.Round((snapSettings.transform.localPosition.z - snapSettings.GridAnchor.z * snapSettings.GridSize.z) / snapSettings.PositionStep.z)
		);

		// Depending on the camera type and rotation, it's possible to avoid drawing planes that aren't visible
		bool drawPlaneXY;
		bool drawPlaneXZ;
		bool drawPlaneYZ;

		// Checks if the camera is orthographic
		// (On Unreal Mode, there are 3 orthographic cameras with a set rotation, that display only one plane)
		if (currentDrawingSceneView.orthographic)
		{
			bool isSceneViewEulerAnglesXZero = Mathf.Approximately(sceneViewEulerAngles.x, 0.0f);
			bool isSceneViewEulerAnglesYZero = Mathf.Approximately(sceneViewEulerAngles.y, 0.0f);
			bool isSceneViewEulerAnglesZZero = Mathf.Approximately(sceneViewEulerAngles.z, 0.0f);

			// Checks the camera angle in order to evaluate what planes are not showing
			if (
				(
					Mathf.Approximately(sceneViewEulerAngles.x,  90.0f) || // Bottom
					Mathf.Approximately(sceneViewEulerAngles.x, 270.0f)    // Top
				) &&
				isSceneViewEulerAnglesYZero &&
				isSceneViewEulerAnglesZZero
			)
			{
				drawPlaneXY = false;
				drawPlaneXZ = true;
				drawPlaneYZ = false;
			}
			else if (
				isSceneViewEulerAnglesXZero &&
				(
					isSceneViewEulerAnglesYZero ||                      // Front
					Mathf.Approximately(sceneViewEulerAngles.y, 180.0f) // Back
				) &&
				isSceneViewEulerAnglesZZero
			)
			{
				drawPlaneXY = true;
				drawPlaneXZ = false;
				drawPlaneYZ = false;
			}
			else if (
				isSceneViewEulerAnglesXZero && (
					Mathf.Approximately(sceneViewEulerAngles.y,  90.0f) || // Right
					Mathf.Approximately(sceneViewEulerAngles.y, 270.0f)    // Left
				) &&
				isSceneViewEulerAnglesZZero
			)
			{
				drawPlaneXY = false;
				drawPlaneXZ = false;
				drawPlaneYZ = true;
			}
			else
			{
				drawPlaneXY = true;
				drawPlaneXZ = true;
				drawPlaneYZ = true;
			}
		}
		// If the camera is perspective, it's not possible to tell what planes are visible,
		// so draws them all
		else
		{
			drawPlaneXY = true;
			drawPlaneXZ = true;
			drawPlaneYZ = true;
		}
		
		Vector3 lineFrom;
		Vector3 lineTo;
		int     lineIndex;

		// Draws every plane that is visible
		if (drawPlaneXY)
		{
			lineFrom.z = invertPlaneXY ? gridOffset.z + snapSettings.GridSize.z : gridOffset.z;
			lineTo.z   = lineFrom.z;
			
			lineFrom.y = gridOffset.y;
			lineTo.y   = gridOffset.y + snapSettings.GridSize.y;
			
			for (lineIndex = 0; lineIndex <= gridWidth; lineIndex++)
			{
				lineFrom.x = lineIndex * snapSettings.PositionStep.x + gridOffset.x;
				lineTo.x   = lineFrom.x;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
			
			lineFrom.x = gridOffset.x;
			lineTo.x   = gridOffset.x + snapSettings.GridSize.x;
			
			for (lineIndex = 0; lineIndex <= gridHeight; lineIndex++)
			{
				lineFrom.y = lineIndex * snapSettings.PositionStep.y + gridOffset.y;
				lineTo.y   = lineFrom.y;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
		}
		
		if (drawPlaneXZ)
		{
			lineFrom.y = invertPlaneXZ ? gridOffset.y + snapSettings.GridSize.y : gridOffset.y;
			lineTo.y   = lineFrom.y;
			
			lineFrom.z = gridOffset.z;
			lineTo.z   = gridOffset.z + snapSettings.GridSize.z;
			
			for (lineIndex = 0; lineIndex <= gridWidth; lineIndex++)
			{
				lineFrom.x = lineIndex * snapSettings.PositionStep.x + gridOffset.x;
				lineTo.x   = lineFrom.x;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
			
			
			lineFrom.x = gridOffset.x;
			lineTo.x   = gridOffset.x + snapSettings.GridSize.x;
			
			for (lineIndex = 0; lineIndex <= gridDepth; lineIndex++)
			{
				lineFrom.z = lineIndex * snapSettings.PositionStep.z + gridOffset.z;
				lineTo.z   = lineFrom.z;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
		}
		
		if (drawPlaneYZ)
		{
			lineFrom.x = invertPlaneYZ ? gridOffset.x + snapSettings.GridSize.x : gridOffset.x;
			lineTo.x   = lineFrom.x;
			
			lineFrom.y = gridOffset.y;
			lineTo.y   = gridOffset.y + snapSettings.GridSize.y;
			
			for (lineIndex = 0; lineIndex <= gridDepth; lineIndex++)
			{
				lineFrom.z = lineIndex * snapSettings.PositionStep.z + gridOffset.z;
				lineTo.z   = lineFrom.z;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
			
			lineFrom.z = gridOffset.z;
			lineTo.z   = gridOffset.z + snapSettings.GridSize.z;
			
			for (lineIndex = 0; lineIndex <= gridHeight; lineIndex++)
			{
				lineFrom.y = lineIndex * snapSettings.PositionStep.y + gridOffset.y;
				lineTo.y   = lineFrom.y;
				
				Gizmos.DrawLine(lineFrom, lineTo);
			}
		}
	}
	
	#endregion

	/// <summary>
	/// Called when the tool has been closed
	/// </summary>
	public void OnDestroy()
	{
		// Removes the background events
		SceneView.onSceneGUIDelegate -= UpdateInput;
		EditorApplication.update     -= UpdateTool;
	}
	
}