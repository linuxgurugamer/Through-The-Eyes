//#define DEBUG_TURN_DATA

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace FirstPerson
{
	public class FPStateWalkRun
	{
		const float RUN_TURN_RATE = 0.7f;
		const float STAND_TURN_RATE = 0.1f;	

		FirstPersonEVA imgr;
		Vector3 walk_start_fwd;
		//Vector3 target_fwd;
		float current_turn = 0f;
		//int directionkeypresscount = 0;

		public FPStateWalkRun (FirstPersonEVA pmgr)
		{
			imgr = pmgr;
			imgr.fpCameraManager.OnEnterFirstPerson += new FirstPersonCameraManager.delEvtEVA(evt_OnEnterFirstPerson);
			imgr.OnFixedUpdate += new EventHandler(evt_OnFixedUpdate);
		}

		void evt_OnEnterFirstPerson(KerbalEVA eva)
		{
			//Hook it!
			ReflectedMembers.Initialize ();

			//*********************
			//***Walk/Run States***
			if (!(eva.st_walk_acd is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_walk_acd, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_walk_fps is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_walk_fps, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_run_acd is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_run_acd, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_run_fps is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_run_fps, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_bound_gr_acd is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_bound_gr_acd, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_bound_gr_fps is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_bound_gr_fps, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}
			if (!(eva.st_bound_fl is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_bound_fl, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}

			//*********************
			//***Turn to Heading***
			if (!(eva.st_heading_acquire is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_heading_acquire, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.PreOnFixedUpdate += ApplyModifiedDeltaHeading;
				st.Override_OnFixedUpdate = true;
			}

			//*******************
			//***Idle Grounded***
			if (!(eva.st_idle_gr is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_idle_gr, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetJetpackManualControls;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ApplyNoForwardTarget;
				st.PreOnFixedUpdate += GroundedJetpackCheck_PreFixedUpdate;

				st.Override_OnFixedUpdate = true;
			}

			//*************
			//***Jumping***
			if (!(eva.st_jump is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_jump, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnUpdate += Replacement_UpdateMovement;
				st.PreOnUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;

				st.Override_OnUpdate = true;
				st.Override_OnFixedUpdate = true;
			}

			//**************
			//***Swimming***
			if (!(eva.st_swim_fwd is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_swim_fwd, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetControlOrientation;
				st.PreOnFixedUpdate += ApplyKerbalForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.Override_OnFixedUpdate = true;
			}

			if (!(eva.st_swim_idle is HookedKerbalFSMState)) {
				HookedKerbalFSMState st = new HookedKerbalFSMState (eva.st_swim_idle, IsThisEVAIVA);
				st.Hook (eva);
				st.PreOnEnter += ResetJetpackManualControls;
				st.PreOnFixedUpdate += ForceArcadeMode;
				st.PreOnFixedUpdate += ResetTurningBasisVector;
				st.PreOnFixedUpdate += ApplyNoForwardTarget;
				st.PreOnFixedUpdate += Replacement_CorrectGroundedRotation;
				st.PreOnFixedUpdate += Replacement_UpdateMovement;
				st.PreOnFixedUpdate += Replacement_UpdateHeading;
				st.PreOnFixedUpdate += Replacement_UpdateRagdollVelocities;
				st.Override_OnFixedUpdate = true;
			}

			walk_start_fwd = eva.transform.forward;
			//directionkeypresscount = 0;
			//h_integral = 0f;
			//h_previouserror = 0f;
		}

		bool IsThisEVAIVA(KerbalEVA eva)
		{
			return imgr.fpCameraManager.isFirstPerson && FlightGlobals.ActiveVessel != null && imgr.fpCameraManager.currentfpeva == eva;
		}

		[Conditional("DEBUG_TURN_DATA")]
		void DEBUG_DeltaHdg(KerbalEVA eva, string nn)
		{
			float dotproduct = Vector3.Dot (eva.transform.forward, (Vector3)ReflectedMembers.eva_tgtFwd.GetValue (eva));
			float dheading = Mathf.Acos (Vector3.Dot (eva.transform.forward, (Vector3)ReflectedMembers.eva_tgtFwd.GetValue (eva))) * 57.29578f;
			float num = Mathf.Sign ((Quaternion.Inverse (eva.transform.rotation) * (Vector3)ReflectedMembers.eva_tgtFwd.GetValue (eva)).x);
			KSPLog.print (nn + "LastDH: " + ReflectedMembers.eva_lastDeltaHdg.GetValue (eva).ToString ()
				+ ", DH: " + ReflectedMembers.eva_deltaHdg.GetValue (eva).ToString ()
				+ ", TgtF: " + ReflectedMembers.eva_tgtFwd.GetValue(eva).ToString()
				+ ", TrnsF: " + eva.transform.forward.ToString()
				+ ", dot: " + dotproduct.ToString()
				+ ", dh: " + dheading.ToString()
				+ ", num: " + num.ToString()
				+ ", rpos: " + ReflectedMembers.eva_tgtRpos.GetValue(eva).ToString()
				//+ ", myTgtF: " + target_fwd.ToString()
			);
		}

		//**********************************
		//********Events for OnEnter********
		//**********************************

		void ResetTurningBasisVector(KerbalEVA eva) { ResetTurningBasisVector(eva, null); }
		void ResetTurningBasisVector(KerbalEVA eva, KFSMState s)
		{
			walk_start_fwd = eva.transform.forward;
			current_turn = 0f;
			//target_fwd = walk_start_fwd;
			//directionkeypresscount = 0;
			//h_integral = 0f;
			//h_previouserror = 0f;
			ReflectedMembers.eva_integral.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_prev_error.SetValue (eva, Vector3.zero);
		}

		void ResetJetpackManualControls(KerbalEVA eva, KFSMState s)
		{
			//When jetpacking, the kerbal may have been in manual axis control.
			//We are now grounded, so that needs to be disabled or we will sidestep-walk
			//rather than turning.
			ReflectedMembers.eva_manualAxisControl.SetValue (eva, false);
			ReflectedMembers.eva_cmdRot.SetValue (eva, Vector3.zero);
		}


		//**************************************
		//********Events for FixedUpdate********
		//**************************************

		void ForceArcadeMode(KerbalEVA eva)
		{
			ForceArcadeMode (eva, null);
		}
		void ForceArcadeMode(KerbalEVA eva, KFSMState s)
		{
			DEBUG_DeltaHdg (eva, "ForceArcadeMode ");

			if (GameSettings.EVA_back.GetKey(false))
				eva.CharacterFrameModeToggle = true;	//Force fps mode walking
			else
				eva.CharacterFrameModeToggle = false;	//Force arcade mode walking
		}

		void ResetControlOrientation(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "ResetControlOrientation ");

			//Re-run the movement input calculations from fixedupdate
			eva.fUp = FlightCamera.fetch.getReferenceFrame() * Vector3.up;
			//eva.fUp = (eva.transform.position - FlightGlobals.currentMainBody.position).normalized;
			eva.fFwd = Vector3.ProjectOnPlane(walk_start_fwd, eva.fUp).normalized;
			eva.fRgt = Vector3.Cross (eva.fUp, eva.fFwd);

			ReflectedMembers.eva_deltaHdg.SetValue (eva, 0f);
			ReflectedMembers.eva_cmdRot.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_packLinear.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_fuelFlowRate.SetValue (eva, 0f);
			ReflectedMembers.eva_tgtRpos.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_ladderTgtRPos.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_packTgtRPos.SetValue (eva, Vector3.zero);
			ReflectedMembers.eva_packRRot.SetValue (eva, Vector3.zero);

			bool rem = FlightInputHandler.SPACENAV_USE_AS_FLIGHT_CONTROL;
			FlightInputHandler.SPACENAV_USE_AS_FLIGHT_CONTROL = false;
			ReflectedMembers.eva_m_HandleMovementInput(eva);
			FlightInputHandler.SPACENAV_USE_AS_FLIGHT_CONTROL = rem;
		}

		void ApplyKerbalForwardTarget(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "ApplyKerbalForwardTarget ");

			ReflectedMembers.eva_tgtUp.SetValue (eva, eva.fUp);

			Vector3 target_fwd = Quaternion.AngleAxis (current_turn, eva.fUp) * walk_start_fwd;
			ReflectedMembers.eva_tgtFwd.SetValue (eva, target_fwd);
		}

		void ApplyNoForwardTarget(KerbalEVA eva)
		{
			//NOTE NOTE NOTE NOTE
			//When you start moving, the first movement happens in the
			//frame before the state switches to walking. But we wouldn't yet be setting
			//the target direction. So we need to null out movement of that frame in idle.
			ReflectedMembers.eva_tgtFwd.SetValue (eva, eva.transform.forward);
		}

		void Replacement_CorrectGroundedRotation(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "Replacement_CorrectGroundedRotation ");

			ReflectedMembers.eva_m_correctGroundedRotation(eva);
		}

		void Replacement_UpdateMovement(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "Replacement_UpdateMovement ");

			ReflectedMembers.eva_m_UpdateMovement(eva);
		}
			
		KerbalEVA cachedrbeva = null;
		Rigidbody cachedrb = null;
		void Replacement_UpdateHeading(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "Replacement_UpdateHeading ");

			if (eva.vessel.packed)
				return;
			float newDH = (float)ReflectedMembers.eva_deltaHdg.GetValue (eva);
			float D_DOT = float.PositiveInfinity;
			if ((Vector3)ReflectedMembers.eva_tgtRpos.GetValue (eva) != Vector3.zero) {
				newDH = Vector3.Dot (eva.transform.forward, (Vector3)ReflectedMembers.eva_tgtFwd.GetValue (eva));
				D_DOT = newDH;
				if (newDH >= 1f) {
					newDH = 1f;
				} else if (newDH <= -1f) {
					newDH = -1f;
				}
				newDH = Mathf.Acos (newDH) * 57.29578f; //180/pi
			}
			float sign = Mathf.Sign ((Quaternion.Inverse (eva.transform.rotation) * (Vector3)ReflectedMembers.eva_tgtFwd.GetValue (eva)).x);
			//KSPLog.print ("DOT: " + D_DOT.ToString () + ", SIGN: " + sign.ToString () + " EVAfwd: " + eva.transform.forward.ToString() + ", tgtFwd: " + ReflectedMembers.eva_tgtFwd.GetValue (eva).ToString() + ", ffwd: " + Vector3.ProjectOnPlane(walk_start_fwd, eva.fUp).normalized.ToString());
			newDH *= sign;
			if (eva != cachedrbeva) {
				cachedrb = eva.GetComponent<Rigidbody> ();
				cachedrbeva = eva;
			}

			if (Mathf.Abs (newDH) < (eva.turnRate * 2.0f))
				cachedrb.angularVelocity = newDH * 0.5f * eva.fUp;
			else
				cachedrb.angularVelocity = eva.turnRate * sign * eva.fUp;

			//KSPLog.print ("DH: " + newDH.ToString() + ", DOT: " + D_DOT.ToString () + ", SIGN: " + sign.ToString () + " EVAfwd: " + eva.transform.forward.ToString() + ", tgtFwd: " + ReflectedMembers.eva_tgtFwd.GetValue (eva).ToString() + ", ffwd: " + Vector3.ProjectOnPlane(walk_start_fwd, eva.fUp).normalized.ToString());
			ReflectedMembers.eva_deltaHdg.SetValue (eva, newDH);
		}

		void Replacement_UpdateRagdollVelocities(KerbalEVA eva)
		{
			DEBUG_DeltaHdg (eva, "Replacement_UpdateRagdollVelocities ");
			ReflectedMembers.eva_m_updateRagdollVelocities(eva);
		}

		void ApplyModifiedDeltaHeading(KerbalEVA eva)
		{
			//If walking and forward/back off, transition to heading acquire state.
			//Note that in stock we cannot get to that state from idle. This starts the
			//walking animation when turning only.
			if (
				(eva.fsm.CurrentState == eva.st_walk_acd
				|| eva.fsm.CurrentState == eva.st_walk_fps
				|| eva.fsm.CurrentState == eva.st_run_acd
				|| eva.fsm.CurrentState == eva.st_run_fps
				|| eva.fsm.CurrentState == eva.st_bound_gr_acd
				|| eva.fsm.CurrentState == eva.st_bound_gr_fps
				|| eva.fsm.CurrentState == eva.st_bound_fl)
				&& (GameSettings.EVA_left.GetKey () || GameSettings.EVA_right.GetKey ())
				&& !(GameSettings.EVA_forward.GetKey () || GameSettings.EVA_back.GetKey ()))
			{
				//Note that the direction of this is used to tilt the kerbal into the turn.
				if (GameSettings.EVA_left.GetKey ())
					ReflectedMembers.eva_deltaHdg.SetValue (eva, -61f * STAND_TURN_RATE);
				else
					ReflectedMembers.eva_deltaHdg.SetValue (eva, 61f * STAND_TURN_RATE);
			}
			//Likewise if we are turning only and forward/back is pressed, go back to walk state.
			else if (
				(eva.fsm.CurrentState == eva.st_heading_acquire)
				&& (GameSettings.EVA_forward.GetKey() || GameSettings.EVA_back.GetKey())
			)
				ReflectedMembers.eva_deltaHdg.SetValue (eva, 0f);
		}

		//********Idle State FixedUpdate********

		void GroundedJetpackCheck_PreFixedUpdate(KerbalEVA eva)
		{
			//If the jetpack is on, use jetpack controls while grounded.
			if (eva.JetpackDeployed)
				FirstPersonEVA.instance.fpStateFloating.evtHook_PreOnFixedUpdate (eva);

			DEBUG_DeltaHdg (eva, "Grounded Replacement_CorrectGroundedRotation ");
			Replacement_CorrectGroundedRotation (eva);
			DEBUG_DeltaHdg (eva, "Grounded Replacement_UpdateMovement ");
			Replacement_UpdateMovement (eva);
			DEBUG_DeltaHdg (eva, "Grounded Replacement_UpdateHeading ");
			Replacement_UpdateHeading (eva);
			DEBUG_DeltaHdg (eva, "Grounded eva_m_UpdatePackLinear ");
			ReflectedMembers.eva_m_UpdatePackLinear(eva);
			DEBUG_DeltaHdg (eva, "Grounded Replacement_UpdateRagdollVelocities ");
			Replacement_UpdateRagdollVelocities (eva);
			DEBUG_DeltaHdg (eva, "Grounded Done ");

			//If the jetpack is on, use jetpack controls while grounded.
			if (eva.JetpackDeployed)
				FirstPersonEVA.instance.fpStateFloating.evtHook_PostOnFixedUpdate (eva);
		}


		//***********************************
		//********Global Update Logic********
		//***********************************

		void evt_OnFixedUpdate(object sender, EventArgs none)
		{
			if (!imgr.fpCameraManager.isFirstPerson)
				return;

			//Note that turnrate is in radians/sec, thus * Mathf.Rad2Deg (=57.29578f (180/pi))
			if (GameSettings.EVA_left.GetKey (false)) {
				current_turn -= (imgr.fpCameraManager.currentfpeva.turnRate * 0.99f * RUN_TURN_RATE) * Time.fixedDeltaTime * Mathf.Rad2Deg;
				if (current_turn < 0f)
					current_turn += 360f;
			}
			else if (GameSettings.EVA_right.GetKey (false)) {
				current_turn += (imgr.fpCameraManager.currentfpeva.turnRate * 0.99f * RUN_TURN_RATE) * Time.fixedDeltaTime * Mathf.Rad2Deg;
				if (current_turn >= 360f)
					current_turn -= 360f;
			}

		}




	}
}

