using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace FirstPerson
{
	public class EVABoundFix
	{
		public static void Hook(KerbalEVA eva)
		{
			// Validate the KFSM state before constructing EVABoundFix. The
			// constructor dereferences eva.On_bound_fall, eva.On_bound_land
			// and eva.st_bound_fl directly; if any is still null (some custom
			// EVA suits, partially-loaded vessels, or particular mod combos)
			// the resulting NullReferenceException is logged at frame rate
			// because FirstPersonEVA.Update() retries Hook every frame.
			if (eva == null)
			{
				LogSkipOnce("eva is null");
				return;
			}
			if (eva.vessel == null)
			{
				LogSkipOnce("eva.vessel is null");
				return;
			}
			if (eva.On_bound_fall == null)
			{
				LogSkipOnce("eva.On_bound_fall is null");
				return;
			}
			if (eva.On_bound_land == null)
			{
				LogSkipOnce("eva.On_bound_land is null");
				return;
			}
			if (eva.st_bound_fl == null)
			{
				LogSkipOnce("eva.st_bound_fl is null");
				return;
			}

			//The reference is held by the replacement delegate.
			EVABoundFix temp = new EVABoundFix(eva);
		}

		// Dedup skip log messages so a persistently-broken EVA state does
		// not spam Player.log at frame rate.
		static readonly HashSet<string> _seenSkipReasons = new HashSet<string>();
		static void LogSkipOnce(string reason)
		{
			if (_seenSkipReasons.Add(reason))
				UnityEngine.Debug.Log("[ThroughTheEyes] EVABoundFix.Hook skipped: " + reason);
		}

		KerbalEVA myeva;
		KFSMEvent evt_packtoggle_bound;
		KFSMEvent evt_long_bound_breakout;

		EVABoundFix(KerbalEVA eva)
		{
			myeva = eva;

			ReflectedMembers.Initialize();

			//*********Increase bound timeout*********
			myeva.On_bound_fall.OnCheckCondition = Replacement_On_bound_fall_OnCheckCondition;

			//*********Fix for "cannot land" on steep surfaces*********
			myeva.On_bound_land.OnCheckCondition = Replacement_On_bound_land_OnCheckCondition;

			//See if we are hooked yet.
			bool found_packtoggle_bound = false;
			for (int i = 0; i < myeva.st_bound_fl.StateEvents.Count; ++i)
			{
				if (myeva.st_bound_fl.StateEvents [i].name == "Pack Toggle (Bound)")
				{
					found_packtoggle_bound = true;
					break;
				}
			}

			if (!found_packtoggle_bound)
			{
				//KSPLog.print ("Bound Pack Toggle not found, adding.");

				//*********Bound floating RCS toggle*********
				evt_packtoggle_bound = new KFSMEvent ("Pack Toggle (Bound)");
				evt_packtoggle_bound.updateMode = KFSMUpdateMode.UPDATE;
				evt_packtoggle_bound.GoToStateOnEvent = myeva.st_idle_fl;
				evt_packtoggle_bound.OnCheckCondition = Check_Bound_Jetpack;
				evt_packtoggle_bound.OnEvent = Do_Toggle_Jetpack;
				myeva.fsm.AddEvent (evt_packtoggle_bound, myeva.st_bound_fl);

				//*********Ability to break out of long bound*********
				evt_long_bound_breakout = new KFSMEvent("Long Bound Breakout");
				evt_long_bound_breakout.updateMode = KFSMUpdateMode.UPDATE;
				evt_long_bound_breakout.GoToStateOnEvent = myeva.st_idle_fl;
				evt_long_bound_breakout.OnCheckCondition = Check_Bound_Breakout;
				myeva.fsm.AddEvent (evt_long_bound_breakout, myeva.st_bound_fl);
			}
		}

		//*********Ability to break out of long bound*********
		bool Check_Bound_Breakout(KFSMState currentstate)
		{
			//Don't break in the first few seconds with normal movement keys.
			if (myeva.fsm.TimeAtCurrentState > 4f)
			{
				if (GameSettings.EVA_left.GetKeyDown (false))
					return true;
				if (GameSettings.EVA_right.GetKeyDown (false))
					return true;
				if (GameSettings.EVA_forward.GetKeyDown (false))
					return true;
				if (GameSettings.EVA_back.GetKeyDown (false))
					return true;

				//These default to the same keys so leave them locked early on.
				if (myeva.JetpackDeployed)
				{
					if (GameSettings.EVA_Pack_left.GetKeyDown (false))
						return true;
					if (GameSettings.EVA_Pack_right.GetKeyDown (false))
						return true;
					if (GameSettings.EVA_Pack_forward.GetKeyDown (false))
						return true;
					if (GameSettings.EVA_Pack_back.GetKeyDown (false))
						return true;
				}
			}

			//The rest of the jetpack translation controls break us out anytime.
			if (myeva.JetpackDeployed)
			{
				if (GameSettings.EVA_Pack_up.GetKeyDown (false))
					return true;
				if (GameSettings.EVA_Pack_down.GetKeyDown (false))
					return true;

				//First person jetpack controls
				if (FirstPersonEVA.instance != null
				    && FirstPersonEVA.instance.fpCameraManager != null
				    && FirstPersonEVA.instance.fpCameraManager.isFirstPerson)
				{
					if (GameSettings.TRANSLATE_LEFT.GetKeyDown (false))
						return true;
					if (GameSettings.TRANSLATE_RIGHT.GetKeyDown (false))
						return true;
					if (GameSettings.TRANSLATE_UP.GetKeyDown (false))
						return true;
					if (GameSettings.TRANSLATE_DOWN.GetKeyDown (false))
						return true;
					if (GameSettings.TRANSLATE_FWD.GetKeyDown (false))
						return true;
					if (GameSettings.TRANSLATE_BACK.GetKeyDown (false))
						return true;
				}
			}

			return false;
		}

		//*********Bound floating RCS toggle*********
		bool Check_Bound_Jetpack(KFSMState currentstate)
		{
			if (!CanControl ())
				return false;
			if (!GameSettings.EVA_TogglePack.GetKeyDown (false))
				return false;
			return true;
		}

		void Do_Toggle_Jetpack()
		{
			ReflectedMembers.eva_m_ToggleJetpackBool (myeva, !myeva.JetpackDeployed);
		}

		bool CanControl()
		{
			if (myeva.vessel.packed)
				return false;
			if (myeva.vessel.state != Vessel.State.ACTIVE)
				return false;
			return true;
		}

		//*********Increase bound timeout*********
		bool Replacement_On_bound_fall_OnCheckCondition(KFSMState currentstate)
		{
			if (ReflectedMembers.eva_m_SurfaceOrSplashed (myeva))
				return false;

			double boundtimeout = (double)myeva.lastBoundStep;
			boundtimeout *= Mathf.Lerp (myeva.boundFallThreshold, myeva.boundFallThreshold * 5f,
				(myeva.minWalkingGee - (float)myeva.vessel.mainBody.GeeASL) / myeva.minWalkingGee);

			//KSPLog.print ("Bound fall? " + myeva.fsm.TimeAtCurrentState.ToString() + " / " + boundtimeout.ToString());
			return myeva.fsm.TimeAtCurrentState > boundtimeout;
		}

		//*********Fix for "cannot land" on steep surfaces*********
		bool Replacement_On_bound_land_OnCheckCondition(KFSMState currentstate)
		{
			if (myeva.vessel.Splashed)
				return true;
			if (myeva.fsm.TimeAtCurrentState < 0.1d)
				return false;

			double boundlandingheight = (double)(float)ReflectedMembers.eva_halfHeight.GetValue (myeva) + (double)myeva.boundThreshold;

			//The standard downward check
			Vector3d downdir = -FlightGlobals.getUpAxis (myeva.vessel.mainBody, myeva.vessel.vesselTransform.position);
			Vector3d hitnormal;
			double verticalheight = Math.Abs (ProbeSurface(downdir, out hitnormal, true));

			if (verticalheight < boundlandingheight)
			{
				//KSPLog.print ("Replacement_On_bound_land_OnCheckCondition: HIT 1");
				return true;
			}
			else
			{
				//Give them a little more time to take off on hills
				if (myeva.fsm.TimeAtCurrentState < 0.3d)
					return false;

				//Try again in the direction of the previous hit normal, but without updating our Vessel.
				verticalheight = Math.Abs (ProbeSurface(-hitnormal, out hitnormal, false));

				if (verticalheight < boundlandingheight)
				{
					//KSPLog.print ("Replacement_On_bound_land_OnCheckCondition: HIT 2");
					return true;
				}
			}

			//KSPLog.print ("Replacement_On_bound_land_OnCheckCondition: Miss");
			return false;
		}

		float ProbeSurface(Vector3d probedirection, out Vector3d hitnormal, bool updatevessel)
		{
			if (!myeva.vessel.loaded || myeva.vessel.packed) {
				hitnormal = probedirection;
				return myeva.vessel.heightFromSurface;
			}

			if (updatevessel)
				myeva.vessel.heightFromSurface = -1;
			
			float altitude = FlightGlobals.getAltitudeAtPos (myeva.vessel.vesselTransform.position, myeva.vessel.mainBody);
			if (altitude < 0f)
				altitude = 0f;
			RaycastHit hitdata;

			bool ishit = Physics.Raycast (myeva.vessel.vesselTransform.position, probedirection,
				             out hitdata, altitude + 100f, LayerUtil.DefaultEquivalent | 0x88000);

			if (updatevessel && ishit)
				myeva.vessel.heightFromSurface = hitdata.distance;

			hitnormal = hitdata.normal;
			return hitdata.distance;
		}



	}
}

