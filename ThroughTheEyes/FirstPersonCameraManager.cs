using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

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
		private const float MAX_LATITUDE = 45.0F; // Don't allow further motion than these (degrees)
		private const float MAX_AZIMUTH = 60.0F;
		private FPGUI fpgui;

		private Vector3 eyeOffset = Vector3.forward * -0.1F; // move the eye slightly backwards so that we don't have as many near-plane issues
		private Vector3 headLocation = Vector3.up * .31f; // Where the centre of the head is

		public delegate void delEvtEVA(KerbalEVA eva);
		public event delEvtEVA OnEnterFirstPerson;
		public event delEvtEVA OnExitFirstPerson;

		private static FieldInfo InternalCamera_currentRot = typeof(InternalCamera).GetField("currentRot", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo InternalCamera_currentPitch = typeof(InternalCamera).GetField("currentPitch", BindingFlags.Instance | BindingFlags.NonPublic);

		private FirstPersonCameraManager(){	}
		
		public static FirstPersonCameraManager initialize(bool showSightAngle = true) {
			FirstPersonCameraManager instance = new FirstPersonCameraManager();
			instance.showSightAngle = showSightAngle;

			return instance;
		}

		public void CheckAndSetFirstPerson(Vessel pVessel)
		{
			var kerbalEVA = ThroughTheEyes.GetKerbalEVAFromVessel(pVessel);

			if (kerbalEVA != null)
			{
				if (!isFirstPerson
				    && !pVessel.packed //this prevents altering camera until EVA is unpacked or else various weird effects are possible
				   )
				{
					currentfpeva = kerbalEVA;
					SetFirstPersonCameraSettings (currentfpeva);

					//Enter first person
					FirstPersonEVA.instance.state.Reset (currentfpeva);

					if (OnEnterFirstPerson != null)
						OnEnterFirstPerson (currentfpeva);
				}

				KeyDisabler.instance.disableKey (KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.FirstPersonEVA);
			}
			else
			{
				if (isFirstPerson)
				{
					resetCamera(null);
				}

				KeyDisabler.instance.restoreKey (KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.FirstPersonEVA);
			}
		}

		Transform GetOrCreateEyeTransform(KerbalEVA eva)
		{
			Transform result = eva.transform.Find("eyeTransform");
			if (result != null) return result;

			result = new GameObject("eyeTransform").transform;
			result.localPosition = getFPCameraPosition(eva);
			result.localRotation = Quaternion.identity;
			result.SetParent(eva.transform, false);

			return result;
		}

		public void SetFirstPersonCameraSettings(KerbalEVA eva)
		{
			FlightCamera flightCam = FlightCamera.fetch;
			InternalCamera internalCam = InternalCamera.Instance;

			enableRenderers(eva.transform, false);

			Transform eyeTransform = GetOrCreateEyeTransform(eva);

			flightCam.EnableCamera();
			flightCam.DeactivateUpdate();
			flightCam.transform.parent = eyeTransform;

			internalCam.SetTransform(eyeTransform, true);
			internalCam.EnableCamera();
			internalCam.maxRot = MAX_LATITUDE;
			internalCam.maxPitch = MAX_AZIMUTH;
			internalCam.minPitch = -MAX_AZIMUTH;
			internalCam.minZoom = 1.0f;
			internalCam.enabled = false;

			isFirstPerson = true;
			if (showSightAngle) {
				fpgui = flightCam.gameObject.AddOrGetComponent<FPGUI>();
			}
			
			viewToNeutral();
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
					renderer.name.Contains("kerbalGirl_mesh") //Females
				   ) {
					renderer.enabled = enable;
				}
				else
				{
					const int LAYER_EVA = 17;
					const int LAYER_IVA = 20;
					renderer.gameObject.layer = enable ? LAYER_EVA : LAYER_IVA;
				}
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
			InternalCamera internalCam = InternalCamera.Instance;

			internalCam.DisableCamera();
			internalCam.maxRot = 60f;
			internalCam.maxPitch = 60f;
			internalCam.minPitch = -30f;
			internalCam.minZoom = 2.0f;
			internalCam.enabled = true;

			flightCam.ActivateUpdate();
			flightCam.transform.SetParent(flightCam.GetPivot());

			isFirstPerson = false;
			
			KerbalEVA previousEVA = ThroughTheEyes.GetKerbalEVAFromVessel(previousVessel);

			if (previousEVA != null)
			{
				enableRenderers(previousEVA.transform, true);
			}

			//Exit first person

			if (OnExitFirstPerson != null)
				OnExitFirstPerson (currentfpeva);
			currentfpeva = null;

			//Restore stuff that changed in the evacontroller
			if (previousEVA != null) {
				//Axis control settings
				ReflectedMembers.eva_manualAxisControl.SetValue (previousEVA, false);
				ReflectedMembers.eva_cmdRot.SetValue (previousEVA, Vector3.zero);

				//Pack power (from fine controls)
				previousEVA.rotPower = 1f;
				previousEVA.linPower = 0.3f;
			}

			KeyDisabler.instance.restoreAllKeys (KeyDisabler.eDisableLockSource.FirstPersonEVA);
		}

		public bool isCameraProperlyPositioned(FlightCamera flightCam) {
			//Not a particularly elegant way to determine if camera isn't crapped by some background stock logic or change view attempts:
			// return Vector3.Distance(flightCam.transform.localPosition, getFPCameraPosition(getFPCameraRotation(), currentfpeva)) < 0.001f;
			return true;
		}
		
		public void update() {
			if (isFirstPerson && fpgui != null) {
				fpgui.yawAngle = (float)InternalCamera_currentRot.GetValue(InternalCamera.Instance);
				fpgui.pitchAngle = -(float)InternalCamera_currentPitch.GetValue(InternalCamera.Instance);
			}

			// the InternalCamera's update function will set the FlightCamera's transform according to InternalSpace rotations
			// But we're actually using InternalCamera in world space, so run the update manually here and then fix the flightcamera transform
			if (isFirstPerson)
			{
				// horrible hack - if right-clicking, force the camera mouse look to true so that the right-click will be interpreted as *unlocking* the mouse
				if (Input.GetMouseButtonDown(1) && !CameraMouseLook.MouseLocked)
				{
					var mouseLocked = typeof(CameraMouseLook).GetField("mouseLocked", BindingFlags.Static | BindingFlags.NonPublic);
					mouseLocked.SetValue(null, true);

					// the flight camera's nearplane is so far away that the normal game logic won't hit the kerbal.  Try to open the PAW here.
					if (!EventSystem.current.IsPointerOverGameObject())
					{
						Ray internalRay = InternalCamera.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
						if (Physics.Raycast(internalRay, out var hoveredPartHitInfo, 2.0f, Part.layerMask.value))
						{
							Part hoveredPart = FlightGlobals.GetPartUpwardsCached(hoveredPartHitInfo.collider.gameObject);

							if (hoveredPart != null && hoveredPart.vessel == FlightGlobals.ActiveVessel)
							{
								UIPartActionController.Instance.SpawnPartActionWindow(hoveredPart);
							}
						}
					}
				}

				InternalCamera.Instance.Update();

				FlightCamera.fetch.transform.localPosition = Vector3.zero;
				FlightCamera.fetch.transform.localRotation = InternalCamera.Instance.transform.localRotation;
			}
		}
		
		public void viewToNeutral() {
			InternalCamera.Instance.ManualReset(true);
		}
		
		private Vector3 getFPCameraPosition(KerbalEVA eva) {
			Vector3 ret = headLocation + eyeOffset;
			if ((eva != null) && (eva.part != null)) {
				List<ProtoCrewMember> c = eva.part.protoModuleCrew;
				if (c != null && c.Count > 0) {
					if (c [0].gender == ProtoCrewMember.Gender.Female) {
						ret += new Vector3 (0, GameSettings.FEMALE_EYE_OFFSET_Y, 0);
						//Female
					}
				}
			}
			return ret;
		}
	}
}
