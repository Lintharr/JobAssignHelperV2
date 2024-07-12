using Harmony12;
using System;
using TH20;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(HospitalEventStaffHired.Config), "OnStaffHiredEvent")]
    internal static class HospitalEventStaffHired_OnStaffHiredEvent_Patch
    {
        private static void Postfix(HospitalEventStaffHired.Config __instance, Staff staff, JobApplicant applicant)
        {
            if (!Main.enabled || !Main.settings.UnassignJobsOnHire)
                return;

            try
            {
                if (staff.Definition._type != StaffDefinition.Type.Doctor && staff.Definition._type != StaffDefinition.Type.Nurse)
                    return;

                var jobs = RoomAlgorithms.GetAllJobs(staff.Level.Metagame, staff.Level.WorldState, staff.Definition._type);
                staff.JobExclusions.Clear();
                staff.JobExclusions.AddRange(jobs);
            }
            catch (Exception e)
            {
                Main.Logger.Error(e.ToString());
            }
        }
    }
}