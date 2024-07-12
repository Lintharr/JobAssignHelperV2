using Harmony12;
using System;
using System.Collections.Generic;
using TH20;
using UnityEngine;
using UnityEngine.UI;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(StaffMenuJobAssignRow), "Setup")]
    internal static class StaffMenuJobAssignRow_Setup_Patch
    {
        private static bool Prefix(StaffMenuJobAssignRow __instance, Staff staff, List<JobDescription> jobs, StaffMenu staffMenu, GameObject ____jobTogglePrefab, QualificationIcons ___QualificationIcons)
        {
            if (!Main.enabled || staff == null)
                return true;

            try
            {
                //TODO: pull jobs from level or worldstatemanager (but, like, not for this method here or the one invoked later, the jobs passed here are perfect: already filtered to staff type)
                //Main.Logger.Log($"[StaffMenuJobAssignRow] jobsCount: '{jobs.Count}', jobs: '{string.Join(" | ", jobs)}', jobs: '{string.Join(" | ", jobs.Select(x => $"{x.GetJobAssignmentTooltipString()} with {x.RequiredQualificationString()}"))}'.");
                var icons = Traverse.Create(___QualificationIcons).Field("_qualificationImages").GetValue<Image[]>();
                for (int i = 0; i < icons.Length; i++)
                {
                    var button = icons[i].gameObject.GetOrAddComponent<Button>();
                    button.enabled = true;
                    var obj = icons[i].gameObject.GetOrAddComponent<JobAssignQualificationToggle>();
                    obj.StaffCharacter = staff;
                    obj.StaffMenu = staffMenu;
                    obj.StaffQualificationIndexClicked = i;
                    obj.AllJobsAvailableForThisStaffType = jobs;
                }
            }
            catch (Exception e)
            {
                Main.Logger.Error(e.ToString());
            }

            return true;
        }
    }
}