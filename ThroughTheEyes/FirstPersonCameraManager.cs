﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPerson
{
	/// <summary>
	/// Description of FirstPersonCameraManager.
	/// </summary>
	public class FirstPersonCameraManager
	{
		public bool isFirstPerson = false;
		public KerbalEVA currentfpeva = null;
		
		private bool showSightAngle;
		private CameraState cameraState;
		private float yaw = 0f;
		private float pitch = 0f;
		private const float MAX_LATITUDE = 45.0F; // Don't allow further motion than these (degrees)
		private const float MAX_AZIMUTH = 60.0F;
		private FPGUI fpgui;
		
		private Vector3 eyeOffset = Vector3.zero;//Vector3.forward * 0.1F; //Eyes don't exist at a point when you move your head
		private Vector3 headLocation = Vector3.up * .31f; // Where the centre of the head is

		public delegate void delEvtEVA(KerbalEVA eva);
		public event delEvtEVA OnEnterFirstPerson;
		public event delEvtEVA OnExitFirstPerson;

		private FirstPersonCameraManager(){	}
		
		public static FirstPersonCameraManager initialize(bool showSightAngle = true) {
			FirstPersonCameraManager instance = new FirstPersonCameraManager();
			instance.cameraState = new CameraState();
			instance.showSightAngle = showSightAngle;

			return instance;
		}
		
		public void CheckAndSetFirstPerson(Vessel pVessel)
		{
			FlightCamera flightCam = FlightCamera.fetch;

			if (pVessel.isEVA)
			{
				if (!isFirstPerson
				    && !pVessel.packed //this prevents altering camera until EVA is unpacked or else various weird effects are possible
				   )
				{
					currentfpeva = pVessel.evaController;
					SetFirstPersonCameraSettings (pVessel.evaController);

					//Enter first person
					FirstPersonEVA.instance.state.Reset (pVessel.evaController);

					if (OnEnterFirstPerson != null)
						OnEnterFirstPerson (pVessel.evaController);
				}

				KeyDisabler.instance.disableKey (KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.FirstPersonEVA);
			}
			else
			{
				if (isFirstPerson)
				{
					resetCamera(null);
				}
				else
				{
					cameraState.saveState(flightCam);
				}

				KeyDisabler.instance.restoreKey (KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.FirstPersonEVA);
			}
		}

		public void SetFirstPersonCameraSettings(KerbalEVA eva)
		{
			FlightCamera flightCam = FlightCamera.fetch;

			flightCam.transform.parent = eva.transform;
			//flightCam.transform.parent = FlightGlobals.ActiveVessel.transform;

			//enableRenderers(pVessel.transform, false);
			enableRenderers(eva.transform, false);

			flightCam.mainCamera.nearClipPlane = 0.01f;

			isFirstPerson = true;
			if (showSightAngle) {
				fpgui = flightCam.gameObject.AddOrGetComponent<FPGUI>();
			}
			flightCam.SetTargetNone();
			flightCam.DeactivateUpdate();
			viewToNeutral();
			reorient();
		}

		void override_idle_fl_OnEnter(KFSMState st)
		{
			
		}
		
		public void saveCameraState(FlightCamera flightCam) {
			cameraState.saveState(flightCam);
		}
		
		private void enableRenderers(Transform transform, bool enable) {
			Component[] renderers = transform.GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < renderers.Length; i++) {
				Renderer renderer = (Renderer)renderers[i];
				if (renderer.name.Contains("headMesh") ||
				    renderer.name.Contains("eyeball") ||
				    renderer.name.Contains("upTeeth") ||
				    renderer.name.Contains("downTeeth") ||
				    renderer.name.Contains("pupil") ||
					renderer.name.Contains("ponytail") //Females
				   ) {
					renderer.enabled = enable;
				}
			}
		}

		private void EnableRenderingOnPrevious(Vessel vessel)
		{
			if (vessel != null && vessel.isEVA)
			{
				enableRenderers(FlightGlobals.ActiveVessel.transform, true);
			}
		}

		public void resetCamera(Vessel previousVessel) {
			ReflectedMembers.Initialize ();

			GameObject.Destroy(fpgui);

			if (!isFirstPerson) {
				return;
			}

			Vessel pVessel = FlightGlobals.ActiveVessel;
			FlightCamera flightCam = FlightCamera.fetch;

			cameraState.recallState(flightCam);

			if (FlightGlobals.ActiveVessel != null)
			{
				flightCam.SetTargetTransform(pVessel.transform);
			}
			flightCam.ActivateUpdate();

			isFirstPerson = false;
			
			EnableRenderingOnPrevious(previousVessel);

			//Exit first person

			if (OnExitFirstPerson != null)
				OnExitFirstPerson (currentfpeva);
			currentfpeva = null;

			//Restore stuff that changed in the evacontroller
			if (previousVessel != null && previousVessel.evaController != null) {
				//Axis control settings
				ReflectedMembers.eva_manualAxisControl.SetValue (previousVessel.evaController, false);
				ReflectedMembers.eva_cmdRot.SetValue (previousVessel.evaController, Vector3.zero);

				//Pack power (from fine controls)
				previousVessel.evaController.rotPower = 1f;
				previousVessel.evaController.linPower = 0.3f;
			}

			KeyDisabler.instance.restoreAllKeys (KeyDisabler.eDisableLockSource.FirstPersonEVA);
		}
		
		public bool isCameraProperlyPositioned(FlightCamera flightCam) {
			//Not a particularly elegant way to determine if camera isn't crapped by some background stock logic or change view attempts:
			return Vector3.Distance(flightCam.transform.localPosition, getFPCameraPosition(getFPCameraRotation(), currentfpeva)) < 0.001f;
		}
		
		public void updateGUI() {
			if (isFirstPerson && fpgui != null) {
				fpgui.yawAngle = -yaw;
				fpgui.pitchAngle = -pitch;
			}
		}
		
		public void viewToNeutral() {
			yaw = 0f;
			pitch = 0f;
		}
		
		public void addYaw(float amount) {
			yaw = Mathf.Clamp(yaw + amount, -MAX_AZIMUTH, MAX_AZIMUTH);
			/*if (Mathf.Abs(yaw) > MAX_AZIMUTH) {
				this.yaw = MAX_AZIMUTH * Mathf.Sign(this.yaw);
			} */
		}
		
		public void addPitch(float amount) {
			pitch = Mathf.Clamp(pitch + amount, -MAX_LATITUDE, MAX_LATITUDE);
			/*if (Mathf.Abs(pitch) > MAX_LATITUDE) {
				this.pitch = MAX_LATITUDE * Mathf.Sign(this.pitch);
			}*/
		}
		
		private Quaternion getFPCameraRotation() {
			return Quaternion.Euler(0.0F, yaw, 0.0F) * Quaternion.Euler(-pitch, 0.0F, 0.0F);
		}

		private Vector3 getFPCameraPosition(Quaternion rotation, KerbalEVA eva) {
			Vector3 ret = headLocation + rotation * eyeOffset;
			if ((eva != null) && (eva.part != null)) {
				List<ProtoCrewMember> c = eva.part.protoModuleCrew;
				if (c != null && c.Count > 0) {
					if (c [0].gender == ProtoCrewMember.Gender.Female) {
						ret += new Vector3 (GameSettings.FEMALE_EYE_OFFSET_X, GameSettings.FEMALE_EYE_OFFSET_Y, GameSettings.FEMALE_EYE_OFFSET_Z);
						//Female
					}
				}
			}
			return ret;
		}

		public void reorient() {
			Quaternion rotation = getFPCameraRotation();
			Vector3 cameraForward = rotation * Vector3.forward;
			Vector3 cameraUp = rotation * Vector3.up;
			Vector3 cameraPosition = getFPCameraPosition(rotation, currentfpeva);
			FlightCamera flightCam = FlightCamera.fetch;

			//KSPLog.print ("prereorient cam fwd: " + flightCam.transform.forward.ToString () + ", maincam fwd: " + flightCam.mainCamera.transform.forward.ToString ());


			flightCam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
			flightCam.transform.localPosition = cameraPosition;
			//flightCam.mainCamera.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
			//flightCam.mainCamera.transform.localPosition = cameraPosition;

			flightCam.transform.parent = currentfpeva.transform;
			//flightCam.mainCamera.transform.parent = FlightGlobals.ActiveVessel.evaController.transform;

			//KSPLog.print (string.Format ("REORIENT Forward: {0}, Up: {1}, Position: {2}", cameraForward, cameraUp, cameraPosition));
		}
		
	}
}
