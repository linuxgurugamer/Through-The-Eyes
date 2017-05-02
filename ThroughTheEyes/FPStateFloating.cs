using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPerson
{
	public class FPStateFloating
	{
		FirstPersonEVA imgr;
		KSP.UI.Screens.Flight.ThrottleGauge throttlegauge_ = null;

		public FPStateFloating (FirstPersonEVA pmgr)
		{
			imgr = pmgr;
			imgr.fpCameraManager.OnEnterFirstPerson += new FirstPersonCameraManager.delEvtEVA(evt_OnEnterFirstPerson);
			imgr.fpCameraManager.OnExitFirstPerson += new FirstPersonCameraManager.delEvtEVA(evt_OnExitFirstPerson);
			imgr.OnFixedUpdate += new EventHandler(evt_OnFixedUpdate);
			imgr.OnLateUpdate += new EventHandler(evt_OnLateUpdate);
		}

		void evt_OnEnterFirstPerson(KerbalEVA eva)
		{
			//Hook it!
			if (!(eva.st_idle_fl is HookedKerbalFSMState)) {
				HookedKerbalFSMState newst = new HookedKerbalFSMState (eva.st_idle_fl, IsThisEVAIVA);
				newst.Hook (eva);
				newst.PreOnFixedUpdate += evtHook_PreOnFixedUpdate;
				newst.PostOnFixedUpdate += evtHook_PostOnFixedUpdate;
			}
		}

		bool IsThisEVAIVA(KerbalEVA eva)
		{
			return imgr.fpCameraManager.isFirstPerson && FlightGlobals.ActiveVessel != null && imgr.fpCameraManager.currentfpeva == eva;
		}

		void evt_OnExitFirstPerson(KerbalEVA eva)
		{

		}

		void evt_OnFixedUpdate(object sender, EventArgs none)
		{
			if (!imgr.fpCameraManager.isFirstPerson)
				return;
			
			//EVA pack throttle control
			if (GameSettings.THROTTLE_UP.GetKey ())
				imgr.state.eva_throttle += Time.fixedDeltaTime;
			else if (GameSettings.THROTTLE_DOWN.GetKey ())
				imgr.state.eva_throttle -= Time.fixedDeltaTime;
			else if (GameSettings.THROTTLE_CUTOFF.GetKey ())
				imgr.state.eva_throttle = 0f;
			else if (GameSettings.THROTTLE_FULL.GetKey ())
				imgr.state.eva_throttle = 1f;
			imgr.state.eva_throttle = Mathf.Clamp (imgr.state.eva_throttle, 0.05f, 1f);
		}

		void evt_OnLateUpdate(object sender, EventArgs none)
		{
			if (!imgr.fpCameraManager.isFirstPerson)
				return;

			//EVA pack throttle display
			//Do we have to worry about call order here??
			if (throttlegauge_ == null)
				throttlegauge_ = (KSP.UI.Screens.Flight.ThrottleGauge)MonoBehaviour.FindObjectOfType (typeof(KSP.UI.Screens.Flight.ThrottleGauge));
			throttlegauge_.gauge.SetValue (imgr.state.eva_throttle);
		}

		public void evtHook_PreOnFixedUpdate (KerbalEVA eva)
		{
			ReflectedMembers.Initialize ();

			if ((FlightGlobals.ActiveVessel.situation != Vessel.Situations.SPLASHED
			     //&& FlightGlobals.ActiveVessel.situation != Vessel.Situations.LANDED //Allow landed jetpack operation
			)
			     && FlightGlobals.ActiveVessel.evaController.JetpackDeployed) {

				//************Rotation************
				Quaternion manualRotation = Quaternion.identity;
				Vector3 commandedManualRotation = Vector3.zero;
				if (GameSettings.YAW_LEFT.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)(-(double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.up);
					commandedManualRotation -= eva.transform.up;
					//KSPLog.print ("YAW LEFT");
				} else if (GameSettings.YAW_RIGHT.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)((double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.up);
					commandedManualRotation += eva.transform.up;
					//KSPLog.print ("YAW RIGHT");
				}

				if (GameSettings.PITCH_UP.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)(-(double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.right);
					commandedManualRotation -= eva.transform.right;
					//KSPLog.print ("PITCH UP");
				} else if (GameSettings.PITCH_DOWN.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)((double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.right);
					commandedManualRotation += eva.transform.right;
					//KSPLog.print ("PITCH DOWN");
				}

				if (GameSettings.ROLL_RIGHT.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)(-(double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.forward);
					commandedManualRotation -= eva.transform.forward;
					//KSPLog.print ("ROLL RIGHT");
				} else if (GameSettings.ROLL_LEFT.GetKey (false)) {
					manualRotation = manualRotation * Quaternion.AngleAxis ((float)((double)FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.forward);
					commandedManualRotation += eva.transform.forward;
					//KSPLog.print ("ROLL LEFT");
				}

				//Testing
				Vector3 kp = new Vector3 (3.0f, 3.0f, 3.0f);
				Vector3 ki = new Vector3 (0.25f, 0.25f, 0.25f);
				Vector3 kd = new Vector3 (0.001f, 0.001f, 0.001f);

				//Reduce pid response in physical time acceleration.
				if (TimeWarp.WarpMode == TimeWarp.Modes.LOW) {
					float warprate = TimeWarp.fetch.physicsWarpRates [TimeWarp.fetch.current_rate_index];
					if (warprate > 1f) {
						kp = kp / warprate;
					}
				}

				ReflectedMembers.eva_manualAxisControl.SetValue (eva, true);
				if (manualRotation == Quaternion.identity) {
					//No rotation controls active. SAS active, maybe.

					//Set manual mode based on SAS mode.
					if (GameSettings.EVA_ROTATE_ON_MOVE && FlightGlobals.ActiveVessel.situation != Vessel.Situations.LANDED) {
						//Run PID.
						Vector3 angularvelocity = eva.part.Rigidbody.angularVelocity;
						Vector3 currenterror = -Helpers.ClampVectorComponents (angularvelocity, -0.5f, 0.5f);
						imgr.state.rotationpid_integral = Helpers.ClampVectorComponents (imgr.state.rotationpid_integral + currenterror * Time.fixedDeltaTime, -1f, 1f);
						Vector3 derivative = (currenterror - imgr.state.rotationpid_previouserror) / Time.fixedDeltaTime;
						Vector3 pidresult = Helpers.PairwiseMultiplyVectors (kp, currenterror)
							+ Helpers.PairwiseMultiplyVectors (ki, imgr.state.rotationpid_integral)
							+ Helpers.PairwiseMultiplyVectors (kd, derivative);

						//KSPLog.print ("currenterror: " + currenterror.ToString ());
						//KSPLog.print ("rotationpid_integral: " + imgr.state.rotationpid_integral.ToString ());
						//KSPLog.print ("derivative: " + derivative.ToString ());

						imgr.state.rotationpid_previouserror = currenterror;

						//Assign command
						ReflectedMembers.eva_cmdRot.SetValue (eva, pidresult);
						//KSPLog.print ("SAS on, no command, PID result: " + pidresult.ToString () + ", actual velocity: " + angularvelocity.ToString ());
					} else {
						//KSPLog.print ("SAS off, no command");

						//This is so when landed the PID is reset.
						imgr.state.rotationpid_integral = Vector3.zero;
						imgr.state.rotationpid_previouserror = Vector3.zero;

						//Idle and SAS off. Do nothing.
						ReflectedMembers.eva_cmdRot.SetValue (eva, Vector3.zero);
					}
				} else {
					//Rotation controls active.

					//Reset PID
					imgr.state.rotationpid_integral = Vector3.zero;
					imgr.state.rotationpid_previouserror = Vector3.zero;

					ReflectedMembers.eva_cmdRot.SetValue (eva, commandedManualRotation);

					//KSPLog.print ("Manual command");
				}

				//************Translation************
				Vector3 manualTranslation = Vector3.zero;
				if (GameSettings.TRANSLATE_LEFT.GetKey (false)) {
					manualTranslation += Vector3.left;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) (-(double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.up);
					//KSPLog.print ("TRANSLATE LEFT");
				} else if (GameSettings.TRANSLATE_RIGHT.GetKey (false)) {
					manualTranslation += Vector3.right;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) ((double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.up);
					//KSPLog.print ("TRANSLATE RIGHT");
				}

				if (GameSettings.TRANSLATE_UP.GetKey (false)) {
					manualTranslation += Vector3.up;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) (-(double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.right);
					//KSPLog.print ("TRANSLATE UP");
				} else if (GameSettings.TRANSLATE_DOWN.GetKey (false)) {
					manualTranslation += Vector3.down;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) ((double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.right);
					//KSPLog.print ("TRANSLATE DOWN");
				}

				if (GameSettings.TRANSLATE_FWD.GetKey (false)) {
					manualTranslation += Vector3.forward;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) (-(double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.forward);
					//KSPLog.print ("TRANSLATE RIGHT");
				} else if (GameSettings.TRANSLATE_BACK.GetKey (false)) {
					manualTranslation += Vector3.back;
					//manualRotation = manualRotation * Quaternion.AngleAxis((float) ((double) FlightGlobals.ActiveVessel.evaController.turnRate * Mathf.Rad2Deg) * Time.deltaTime, FlightGlobals.ActiveVessel.evaController.transform.forward);
					//KSPLog.print ("TRANSLATE LEFT");
				}

				manualTranslation.Normalize ();
				manualTranslation = FlightGlobals.ActiveVessel.transform.rotation * manualTranslation;

				//KSPLog.print ("Resetting rpos. Old value: " + ((Vector3)ReflectedMembers.eva_packTgtRPos.GetValue (FlightGlobals.ActiveVessel.evaController)).ToString ()
				//+ ", new value: " + manualTranslation.ToString ());
				ReflectedMembers.eva_packTgtRPos.SetValue (FlightGlobals.ActiveVessel.evaController, manualTranslation);

				//************Set power**************
				eva.rotPower = 1f * imgr.state.eva_throttle;
				eva.linPower = 0.3f * imgr.state.eva_throttle;

			}
		}

		public void evtHook_PostOnFixedUpdate (KerbalEVA eva)
		{
			ReflectedMembers.Initialize();

			//Recalculate fuel flow: proportional to power factor
			//Default power factors: rot 1, lin 0.3
			if (eva.JetpackDeployed) {
				float newflowrate = 0f;

				Vector3 cmdrot = (Vector3)ReflectedMembers.eva_cmdRot.GetValue (eva);
				Vector3 packlinear = (Vector3)ReflectedMembers.eva_packLinear.GetValue (eva);

				newflowrate += cmdrot.magnitude * Time.fixedDeltaTime * eva.rotPower / 1f;
				newflowrate += packlinear.magnitude * Time.fixedDeltaTime * eva.linPower / 0.3f;

				//KSPLog.print ("Flow rates: " + ((float)ReflectedMembers.eva_fuelFlowRate.GetValue(eva)).ToString() + " -> " + newflowrate.ToString());
				ReflectedMembers.eva_fuelFlowRate.SetValue (eva, newflowrate);
			}
		}



	}
}

