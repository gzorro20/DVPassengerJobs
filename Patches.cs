﻿using DV.RenderTextureSystem.BookletRender;
using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PassengerJobsMod
{
    [HarmonyPatch(typeof(StationProceduralJobsController), "Awake")]
    class StationController_Start_Patch
    {
        private static HashSet<string> ManagedYards = new HashSet<string>() { "CSW", "MF", "FF", "HB", "GF" };

        static void Postfix( StationProceduralJobsController __instance )
        {
            string yardId = __instance.stationController.stationInfo.YardID;
            if( !ManagedYards.Contains(yardId) ) return;

            var gen = __instance.GetComponent<PassengerJobGenerator>();
            if( gen == null )
            {
                gen = __instance.gameObject.AddComponent<PassengerJobGenerator>();
                gen.Initialize();
            }
        }
    }

    [HarmonyPatch(typeof(BookletCreator), "GetJobLicenseTemplateData")]
    class BookletCreator_GetTemplate_Patch
    {
        static bool Prefix( JobLicenses jobLicense, ref LicenseTemplatePaperData __result )
        {
            if( jobLicense == PassLicenses.Passengers1 )
            {
                // override the BookletCreator method
                __result = PassengerLicenseUtil.GetPassengerLicenseTemplate();
                return false;
            }

            return true;
        }
    }
}
