using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FirstPerson
{

    public static class ConfigUtil
    {
        static string configDir = "GameData/ThroughTheEyes/PluginData";
        static string configPath = configDir + "/options.cfg";
        static ConfigNode cfg;

        private static void SaveConfigFile()
        {
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
            cfg.Save(configPath);
        }

        private static void checkConfig()
        {
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
			cfg = ConfigNode.Load(configPath);
            if (cfg == null)
            {
                cfg = new ConfigNode();
                SaveConfigFile();
                Debug.Log("No config found. Writing one.");
            }
            
            if (!cfg.HasValue("toggleFirstPersonKey"))
            {
                cfg.AddValue("toggleFirstPersonKey", "default");
                SaveConfigFile();
                Debug.Log("No toggleFirstPersonKey value found. Reverting to camera mode key");
            }

            if (!cfg.HasValue("EVAKey"))
            {
                cfg.AddValue("EVAKey", "default");
                SaveConfigFile();
                Debug.Log("No EVAKey value found. Reverting to camera mode key");
            }
			if (!cfg.HasValue("recoverKey"))
			{
				cfg.AddValue("recoverKey", "R");
                SaveConfigFile();
                Debug.Log("No recoverKey value found. Making one");
			}
            if (!cfg.HasValue("reviewDataKey"))
            {
                cfg.AddValue("reviewDataKey", "Backslash");
                SaveConfigFile();
                Debug.Log("No reviewDataKey value found. Adding one");
            }
        }

		public static KeyCode RecoverKey()
		{
			checkConfig();

			KeyCode key;
			try
			{
				key = (KeyCode)Enum.Parse(typeof(KeyCode), cfg.GetValue("recoverKey"));
				return key;
			}
			catch
			{
				cfg.SetValue("recoverKey", "R");
                SaveConfigFile();

                Debug.Log("Make sure to use the list of keys to set the key! Reverting to R");
				return KeyCode.R;

			}
		}

        public static KeyCode EVAKey(KeyCode key)
        {
            checkConfig();


            if (cfg.GetValue("EVAKey") == "default")
            {
                return key;
            }
            else
            {
                try
                {
                    key = (KeyCode)Enum.Parse(typeof(KeyCode), cfg.GetValue("EVAKey"));
                    return key;
                }
                catch
                {
                    cfg.SetValue("EVAKey", "default");
                    SaveConfigFile();

                    Debug.Log("Set the key from the list of keys or use the string 'default'! Reverting to camera mode key");
                    return key;

                }
            }
        }

        public static KeyCode ToggleFirstPersonKey(KeyCode key)
        {
            checkConfig();


            if (cfg.GetValue("toggleFirstPersonKey") == "default")
            {
                return key;
            }
            else
            {
                try
                {
                    key = (KeyCode)Enum.Parse(typeof(KeyCode), cfg.GetValue("toggleFirstPersonKey"));
                    return key;
                }
                catch
                {
                    cfg.SetValue("toggleFirstPersonKey", "default");
                    SaveConfigFile();

                    Debug.Log("Set the key from the list of keys or use the string 'default'! Reverting to camera mode key");
                    return key;

                }
            }
        }

        public static bool ForceEVA()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<TTE>().forceEVA;
        }

        public static bool ForceIVA()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<TTE>().forceIVA;
        }

        public static bool ForceIVABeforeLaunch()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<TTE>().forceIVABeforeLaunch;
        }


        public static bool ShowSightAngle()
		{
            return HighLogic.CurrentGame.Parameters.CustomParams<TTE>().showSightAngle;
		}

        public static bool DisableMapView()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<TTE>().disableMapView;
        }

        public static KeyCode checkKeys()
        {
            checkConfig();

            KeyCode key;
            try
            {
                key = (KeyCode)Enum.Parse(typeof(KeyCode), cfg.GetValue("reviewDataKey"));
                return key;
            }
            catch
            {
                cfg.SetValue("reviewDataKey", "Backslash");
                SaveConfigFile();
                
                Debug.Log("Make sure to use the list of keys to set the key! Reverting to Backslash");
                return KeyCode.Backslash;
            }

        }
    }
}