using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPerson
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class ThroughTheEyes : MonoBehaviour
    {

        //bool disableMapView;
        KeyCode reviewDataKey, recoverKey;
		static KeyCode EVAKey = ConfigUtil.EVAKey(GameSettings.CAMERA_MODE.primary.code);
        CameraManager.CameraMode flight = CameraManager.CameraMode.Flight;
        CameraManager.CameraMode IVA = CameraManager.CameraMode.IVA;
        CameraManager.CameraMode map = CameraManager.CameraMode.Map;

		KeyDisabler keyDisabler;

        void OnGUI()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel.isEVA)
            {
                Vessel pVessel = FlightGlobals.ActiveVessel;
				KerbalEVA keva = FlightGlobals.ActiveVessel.evaController;
                int fuelPercent = (int)(keva.Fuel / keva.FuelCapacity);
            }
        }

        public ThroughTheEyes()
        {
         	
        }

		void disableKeys() {
			keyDisabler.disableKey(KeyDisabler.eKeyCommand.CAMERA_MODE, KeyDisabler.eDisableLockSource.MainModule);
			if (CameraManager.Instance.currentCameraMode == IVA && !FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.GetCrewCount() < 2) {
				keyDisabler.disableKey(KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.MainModule);
			} else {
				keyDisabler.restoreKey(KeyDisabler.eKeyCommand.CAMERA_NEXT, KeyDisabler.eDisableLockSource.MainModule);
			}
			if (ConfigUtil.DisableMapView()) {
				keyDisabler.disableKey(KeyDisabler.eKeyCommand.MAP_VIEW, KeyDisabler.eDisableLockSource.MainModule);
			}
		}

        void CheckAndSetCamera(Vessel pVessel)
        {
			CameraManager camManage = CameraManager.Instance;

			if (!ConfigUtil.ForceIVABeforeLaunch() && externalMaintenainceAvailable(pVessel)) { return; }
            if (!pVessel.isEVA)
			{
                if (pVessel.GetCrewCount() > 0)
                {

                    if (ConfigUtil.DisableMapView())
                    {
                        if (camManage.currentCameraMode == flight || camManage.currentCameraMode == map)
                        {
                            if (MapView.MapIsEnabled) { MapView.ExitMapView(); }
                            camManage.SetCameraMode(IVA);
                        }
                    }
                    else
					{
						if (camManage.currentCameraMode == flight || camManage.currentCameraMode == CameraManager.CameraMode.External)
						{ camManage.SetCameraMode(IVA); }
                    }

                }
#if false
                else if (pVessel.GetCrewCount() < 1)
                {


                }
#endif
            }
        }
        
        private void onVesselChange(Vessel v) {
			if (v.isEVA) {
				CameraManager.Instance.SetCameraFlight(); //Important. Without this switching to EVA from IVA would lead to heavy errors on attempts to return to IVA of originating vessel
			}        	
        }

		//Flight -> IVA
		void OnCameraChange(CameraManager.CameraMode m){
			if (CameraManager.Instance == null)
				return;
			
			if (m == CameraManager.CameraMode.IVA) {
				Kerbal k = CameraManager.Instance.IVACameraActiveKerbal;
				if (k.InPart == null || string.IsNullOrEmpty(k.InPart.partInfo.title))
					ScreenMessages.PostScreenMessage ("IVA: " + k.crewMemberName, 5f, ScreenMessageStyle.UPPER_CENTER);
				else
					ScreenMessages.PostScreenMessage (string.Format("IVA: {0} ({1})", k.crewMemberName, k.InPart.partInfo.title), 5f, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		//IVA -> IVA
		void OnIVACameraKerbalChange(Kerbal k){
			if (CameraManager.Instance == null)
				return;
			
			//NOTE NOTE NOTE
			//As of KSP 1.2.2, OnIVACameraKerbalChange first sets the next kerbal, then
			//sends out this event with the NEXT next kerbal. Whoops. We have to look at who is IVA ourselves.
			k = CameraManager.Instance.IVACameraActiveKerbal;
			if (k != null && k.crewMemberName != null) {
				if (k.InPart == null || string.IsNullOrEmpty (k.InPart.partInfo.title))
					ScreenMessages.PostScreenMessage ("IVA: " + k.crewMemberName, 5f, ScreenMessageStyle.UPPER_CENTER);
				else
					ScreenMessages.PostScreenMessage (string.Format ("IVA: {0} ({1})", k.crewMemberName, k.InPart.partInfo.title), 5f, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		void onGameSceneLoadRequested(GameScenes scene)
		{
			if (keyDisabler != null)
			{
				keyDisabler.restoreAllKeys();
			}
		}

        void Start()
        {

			keyDisabler = KeyDisabler.instance;

            /*GameEvents.onLaunch.Add((v) =>
            {
                if (ConfigUtil.ForceIVA()) { CameraManager.Instance.SetCameraIVA(); disableKeys(); }
            });*/

            GameEvents.onVesselChange.Add(onVesselChange);
			GameEvents.OnCameraChange.Add(OnCameraChange);
			GameEvents.OnIVACameraKerbalChange.Add(OnIVACameraKerbalChange);
			GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);

			reviewDataKey = ConfigUtil.checkKeys();
			recoverKey = ConfigUtil.RecoverKey();
			keyDisabler.restoreAllKeys();
        }

        void Update()
        {
            Vessel pVessel = FlightGlobals.ActiveVessel;
            CameraManager flightCam = CameraManager.Instance;
           
            if (HighLogic.LoadedSceneIsFlight && pVessel != null && pVessel.isActiveVessel && pVessel.state != Vessel.State.DEAD)
            {
                if (ConfigUtil.ForceIVA())
                {
                    if (HighLogic.CurrentGame.Parameters.Flight.CanIVA)
                    {
                        CheckAndSetCamera(pVessel);
                    }
                    if (!externalMaintenainceAvailable(pVessel)) {
						disableKeys();
					} else {
						keyDisabler.restoreAllKeys(KeyDisabler.eDisableLockSource.MainModule);
					}
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(EVAKey)) {
						KeyControls.GoEVA();
						//KeyControls.rescueAfterHatchCheck();
					}
                    if (Input.GetKeyDown(reviewDataKey))
                    {
                        KeyControls.MyReviewData();
					}

					if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(recoverKey)) {
						KeyControls.recoverVessel(pVessel);
					}
                }
            }
        }

        private bool externalMaintenainceAvailable(Vessel vessel) {
        	return vessel.situation == Vessel.Situations.PRELAUNCH || vessel.LandedInKSC;
        }

        void Destroy()
        {
			if (keyDisabler != null) {
				keyDisabler.restoreAllKeys();
			}
        }

    }
}
