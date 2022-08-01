using System;
using UnityEngine;

namespace FirstPerson
{

	public class CameraState
	{

		private float origMinDistance=1;
		private float origMaxDistance=150000;
		private float origStartDistance = 20;
		private float origMinPitch = -1.6f;
		private float origMaxPitch = 1.6f;
		private float origPivotTranslateSharpness=.5f;
		private float orgMinHeight=3f;
		private float origMinHeightAtMaxDist=1000f;
		private float origMinHeightAtMinDist=.3f;
		private float origNearClipPlane = 0.5f;
		private float origFarClipPlane;
		private Transform origParent = null;

		public void saveState(FlightCamera flightCam) {
			origMinPitch = flightCam.minPitch;
			origMaxPitch = flightCam.maxPitch;
			origPivotTranslateSharpness = flightCam.pivotTranslateSharpness;
			orgMinHeight = flightCam.minHeight;
			origMinHeightAtMaxDist = flightCam.minHeightAtMaxDist;
			origMinHeightAtMinDist = flightCam.minHeightAtMinDist;
			origMinDistance = flightCam.minDistance;
			origMaxDistance = flightCam.maxDistance;
			origStartDistance = flightCam.startDistance;
			origNearClipPlane = flightCam.mainCamera.nearClipPlane;
			origFarClipPlane = flightCam.mainCamera.farClipPlane;
			origParent = flightCam.transform.parent;
		}

		public void recallState(FlightCamera flightCam)
		{
			flightCam.minPitch = origMinPitch;
			flightCam.maxPitch = origMaxPitch;
			flightCam.pivotTranslateSharpness = origPivotTranslateSharpness;
			flightCam.minHeight = orgMinHeight;
			flightCam.minHeightAtMaxDist = origMinHeightAtMaxDist;
			flightCam.minHeightAtMinDist = origMinHeightAtMinDist;
			flightCam.minDistance = origMinDistance;
			flightCam.maxDistance = origMaxDistance;
			flightCam.startDistance = origStartDistance;
			flightCam.SetDistanceImmediate(origStartDistance);
			flightCam.mainCamera.nearClipPlane = origNearClipPlane;
			flightCam.mainCamera.farClipPlane = origFarClipPlane;
			flightCam.transform.parent = origParent;
		}

	}
}

