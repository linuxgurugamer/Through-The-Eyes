using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPerson
{
	public class KeyDisabler
	{
		public enum eKeyCommand : int
		{
			CAMERA_MODE = 0,
			CAMERA_NEXT = 1,
			MAP_VIEW = 2,
		}

		public enum eDisableLockSource
		{
			MainModule,
			FirstPersonEVA,
		}

		Dictionary<eKeyCommand, KeyBinding> KeyEnumToClassTranslator;
		Dictionary<eKeyCommand, KeyCode[]> KeySaver;
		Dictionary<eKeyCommand, List<eDisableLockSource>> KeyLocks = new Dictionary<eKeyCommand, List<eDisableLockSource>>();

		private KeyDisabler()
		{
			KeyEnumToClassTranslator = new Dictionary<eKeyCommand, KeyBinding> ();
			KeyEnumToClassTranslator [eKeyCommand.CAMERA_MODE] = GameSettings.CAMERA_MODE;
			KeyEnumToClassTranslator [eKeyCommand.CAMERA_NEXT] = GameSettings.CAMERA_NEXT;
			KeyEnumToClassTranslator [eKeyCommand.MAP_VIEW] = GameSettings.MAP_VIEW_TOGGLE;

			KeySaver = new Dictionary<eKeyCommand, KeyCode[]> ();
			KeySaver[eKeyCommand.CAMERA_MODE] = new KeyCode[] { GameSettings.CAMERA_MODE.primary.code, GameSettings.CAMERA_MODE.secondary.code };
			KeySaver[eKeyCommand.CAMERA_NEXT] = new KeyCode[] { GameSettings.CAMERA_NEXT.primary.code, GameSettings.CAMERA_NEXT.secondary.code };
			KeySaver[eKeyCommand.MAP_VIEW] = new KeyCode[] { GameSettings.MAP_VIEW_TOGGLE.primary.code, GameSettings.MAP_VIEW_TOGGLE.secondary.code };

			/*
			KSPLog.print(string.Format("KeyDisabler: {0}, {1}, {2}, {3}, {4}, {5}", GameSettings.CAMERA_MODE.primary.code, GameSettings.CAMERA_MODE.secondary.code,
				GameSettings.CAMERA_NEXT.primary.code, GameSettings.CAMERA_NEXT.secondary.code,
				GameSettings.MAP_VIEW_TOGGLE.primary.code, GameSettings.MAP_VIEW_TOGGLE.secondary.code
				));
			*/
		}

		static KeyDisabler inst = null;
		public static KeyDisabler instance {
			get{
				if (inst == null)
					inst = new KeyDisabler ();
				return inst;
			}
		}

		public KeyCode[] GetSavedKeyCodes(eKeyCommand index)
		{
			return KeySaver [index];
		}

		public void disableKey(eKeyCommand index, eDisableLockSource source)
		{
			if (KeyLocks.ContainsKey (index) && KeyLocks[index].Count > 0) {
				//Already locked.
				if (!KeyLocks [index].Contains (source))
					KeyLocks [index].Add (source);
			} else {
				if (!KeyLocks.ContainsKey (index))
					KeyLocks [index] = new List<eDisableLockSource> ();
				KeyLocks [index].Add (source);
				KeyEnumToClassTranslator [index].primary = new KeyCodeExtended(KeyCode.None);
				KeyEnumToClassTranslator [index].secondary = new KeyCodeExtended(KeyCode.None);
			}
		}

		public void restoreKey(eKeyCommand index, eDisableLockSource source)
		{
			//Make sure we hold a lock.
			if (!KeyLocks.ContainsKey (index))
				return;
			if (!KeyLocks [index].Contains (source))
				return;

			KeyLocks [index].Remove (source);
			if (KeyLocks [index].Count == 0) {
				KeyEnumToClassTranslator [index].primary = new KeyCodeExtended(KeySaver [index] [0]);
				KeyEnumToClassTranslator [index].secondary = new KeyCodeExtended(KeySaver [index] [1]);
			}
		}

		public void restoreAllKeys()
		{
			foreach (KeyValuePair<eKeyCommand, List<eDisableLockSource>> kp in KeyLocks) {
				if (kp.Value.Count > 0) {
					KeyEnumToClassTranslator [kp.Key].primary = new KeyCodeExtended(KeySaver [kp.Key] [0]);
					KeyEnumToClassTranslator[kp.Key].secondary = new KeyCodeExtended(KeySaver[kp.Key][1]);
				}
				kp.Value.Clear ();
			}
		}

		public void restoreAllKeys(eDisableLockSource source)
		{
			foreach (KeyValuePair<eKeyCommand, List<eDisableLockSource>> kp in KeyLocks) {
				if (kp.Value.Count > 0 && kp.Value.Contains(source)) {
					kp.Value.Remove (source);
					if (kp.Value.Count == 0) {
						KeyEnumToClassTranslator [kp.Key].primary = new KeyCodeExtended(KeySaver [kp.Key] [0]);
						KeyEnumToClassTranslator [kp.Key].secondary = new KeyCodeExtended(KeySaver [kp.Key] [1]);
					}
				}
			}
		}



	}
}

