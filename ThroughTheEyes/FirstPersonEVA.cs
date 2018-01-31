using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPerson
{
	//TODO place the camera at correct position of a ragdolled kerbal (depend on helmet transform position?)
    
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class FirstPersonEVA : MonoBehaviour
	{
		public static FirstPersonEVA instance = null;

		public FirstPersonCameraManager fpCameraManager;
		public FPNavBall fpNavBall;
		public FPStateFloating fpStateFloating;
		public FPStateWalkRun fpStateWalkRun;

		internal event EventHandler OnUpdate;
		internal event EventHandler OnFixedUpdate;
		internal event EventHandler OnLateUpdate;
    	
		bool forceEVA;
		KeyCode toggleFirstPersonKey;
		KeyBinding resetivacamerabinding;
		Vessel lastHookedVessel = null;

		private const float mouseViewSensitivity = 3000f; //TODO take into account in-game mouse view sensitivity
		public EVAIVAState state = new EVAIVAState();

		private bool needCamReset = false;
		private bool stopTouchingCamera = false;

		public FirstPersonEVA() { } 
		
		private void onVesselDestroy(Vessel v) {
			if (FlightGlobals.fetch == null)
				return;
			
			if (v != null && v == FlightGlobals.ActiveVessel && v.isEVA) {
				fpCameraManager.resetCamera (v);
			}

			KeyDisabler.instance.restoreAllKeys (KeyDisabler.eDisableLockSource.FirstPersonEVA);
		}
		
		private void onVesselSwitching(Vessel from, Vessel to) {
			fpCameraManager.resetCamera((Vessel)from);
			lastHookedVessel = null;

			if (to != null && to.isEVA) {
				CameraManager.Instance.SetCameraFlight();
			}

			KeyDisabler.instance.restoreAllKeys (KeyDisabler.eDisableLockSource.FirstPersonEVA);
		}
			
		private void onMapExited() {
			//When exitting map view an attempt to set 1st person camera in the same update cycle is overridden with some stock camera handling
			//so we have to set flag to reset 1st person camera a bit later
			needCamReset = true; 
		}

		private void onSceneLoadRequested(GameScenes scene) {
			//This is needed to avoid fighting stock camera during "Revert to launch" as that causes NullRefences in Unity breaking the revert process
			stopTouchingCamera = true;
			if (fpCameraManager != null && fpCameraManager.isFirstPerson)
			{
				KSPLog.print ("TTE: Resetting because of scene change.");
				fpCameraManager.resetCamera (fpCameraManager.currentfpeva.vessel);
			}

			KeyDisabler.instance.restoreAllKeys (KeyDisabler.eDisableLockSource.FirstPersonEVA);
		}
		
		void Start()
		{
			instance = this;
			OnUpdate = null;
			OnFixedUpdate = null;
			OnLateUpdate = null;
			lastHookedVessel = null;

			forceEVA = ConfigUtil.ForceEVA();
			toggleFirstPersonKey = ConfigUtil.ToggleFirstPersonKey(KeyDisabler.instance.GetSavedKeyCodes(KeyDisabler.eKeyCommand.CAMERA_MODE)[0]);

			stopTouchingCamera = false;
			
			fpCameraManager = FirstPersonCameraManager.initialize(ConfigUtil.ShowSightAngle());
			fpNavBall = new FPNavBall (this);
			fpStateFloating = new FPStateFloating (this);
			fpStateWalkRun = new FPStateWalkRun (this);
 			
			//We unbind the main one, so this allows us to still read the key state.
			resetivacamerabinding = new KeyBinding ();
			KeyCode[] resetcameracodes = KeyDisabler.instance.GetSavedKeyCodes (KeyDisabler.eKeyCommand.CAMERA_NEXT);
			resetivacamerabinding.primary = new KeyCodeExtended(resetcameracodes [0]);
			resetivacamerabinding.secondary = new KeyCodeExtended(resetcameracodes [1]);

			GameEvents.onVesselDestroy.Add(onVesselDestroy);
			/*GameEvents.onCrewKilled.Add((v) => {
           		fpCameraManager.resetCamera(null);
			});*/

			GameEvents.onVesselSwitching.Add(onVesselSwitching);
			GameEvents.OnMapExited.Add(onMapExited);
			GameEvents.onGameSceneLoadRequested.Add(onSceneLoadRequested);
		}

		void Update()
		{
			Vessel pVessel = FlightGlobals.ActiveVessel;
			FlightCamera flightCam = FlightCamera.fetch;

			if (pVessel == null)
				return;
			if (pVessel.isEVA && !pVessel.packed && pVessel.loaded && pVessel.evaController != null) {
				if (pVessel != lastHookedVessel) {
					lastHookedVessel = pVessel;

					//KSPLog.print (string.Format("{0} switched to, hooking vessel for general hooks.", pVessel.GetName()));
					EVABoundFix.Hook (pVessel.evaController);
				}
			}

			if (FlightGlobals.ActiveVessel.isEVA && fpCameraManager.isFirstPerson && needCamReset) {
				fpCameraManager.isFirstPerson = false;
				fpCameraManager.CheckAndSetFirstPerson(pVessel);
			}
			needCamReset = false;

			if (HighLogic.LoadedSceneIsFlight && pVessel != null && pVessel.isActiveVessel && pVessel.state != Vessel.State.DEAD && !stopTouchingCamera) {
				if (forceEVA || fpCameraManager.isFirstPerson) {
					if (!fpCameraManager.isCameraProperlyPositioned(flightCam)) {
						fpCameraManager.isFirstPerson = false;
					}
					fpCameraManager.CheckAndSetFirstPerson(pVessel);
				} 
                if (!ThroughTheEyes.CheckControlLocks() && !forceEVA && pVessel.isEVA) {
					if (Input.GetKeyDown(toggleFirstPersonKey)) {
						if (!fpCameraManager.isFirstPerson) {
							fpCameraManager.saveCameraState(flightCam);
							fpCameraManager.CheckAndSetFirstPerson(pVessel);
						} else {
							fpCameraManager.resetCamera(pVessel);
						}
					}
				}

				fpCameraManager.updateGUI();

			}

			if (fpCameraManager.isFirstPerson && resetivacamerabinding.GetKeyDown ()) {
				fpCameraManager.viewToNeutral ();
				fpCameraManager.reorient ();
			}

			if (OnUpdate != null)
				OnUpdate (this, null);
		}

		void FixedUpdate()
		{
			if (fpCameraManager.isFirstPerson) {
				if (Input.GetMouseButton(1)) { // Right Mouse Button Down
					//Change the angles by the mouse movement
					fpCameraManager.addYaw(Input.GetAxis("Mouse X") / Screen.width * mouseViewSensitivity);
					fpCameraManager.addPitch(Input.GetAxis("Mouse Y") / Screen.height * mouseViewSensitivity);
					fpCameraManager.reorient();
					//state.kerballookrotation = FlightCamera.fetch.transform.rotation;
				} //button held down

				if (FlightGlobals.ActiveVessel.Landed && (GameSettings.EVA_back.GetKey() || GameSettings.EVA_forward.GetKey())) {
					//fpCameraManager.viewToNeutral();
					fpCameraManager.reorient();
					//FlightCamera.fetch.transform.rotation = state.kerballookrotation;
				}

				if (FlightGlobals.ActiveVessel.Landed && (GameSettings.EVA_back.GetKeyUp() || GameSettings.EVA_forward.GetKeyUp())) {
					//fpCameraManager.viewToNeutral();
					fpCameraManager.reorient();
				}
			}

			if (OnFixedUpdate != null)
				OnFixedUpdate (this, null);
		}

		void LateUpdate()
		{
			if (OnLateUpdate != null)
				OnLateUpdate (this, null);
		}



	}
    
}
