using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;




namespace FirstPerson
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class InitialRegister : MonoBehaviour
    {
        public static string kspRootDir;
        bool initted = false;
        void OnGUI()
        {
            if (!initted)
            {
                kspRootDir = KSPUtil.ApplicationRootPath;
                initted = true;
            }
        }
    }
}
