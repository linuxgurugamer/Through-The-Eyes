using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;




// http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
// search for "Mod integration into Stock Settings

public class TTE : GameParameters.CustomParameterNode
{
    public override string Title { get { return "General Settings"; } }
    public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
    public override string Section { get { return "Through The Eyes"; } }
    public override string DisplaySection { get { return "Through The Eyes"; } }
    public override int SectionOrder { get { return 1; } }
    public override bool HasPresets { get { return false; } }


    [GameParameters.CustomParameterUI("Force EVA")]
    public bool forceEVA = false;

    [GameParameters.CustomParameterUI("Show Sight Angle")]
    public bool showSightAngle = true;

    [GameParameters.CustomParameterUI("Force IVA Before Launch")]
    public bool forceIVABeforeLaunch = false;

    [GameParameters.CustomParameterUI("Force IVA")]
    public bool forceIVA = false;

    [GameParameters.CustomParameterUI("Disable Map View")]
    public bool disableMapView = false;



    public override void SetDifficultyPreset(GameParameters.Preset preset)
    {

    }

    public override bool Enabled(MemberInfo member, GameParameters parameters)
    {
        return true;
    }

    public override bool Interactible(MemberInfo member, GameParameters parameters)
    {
        return true;
    }

    public override IList ValidValues(MemberInfo member)
    {
        return null;
    }
}

