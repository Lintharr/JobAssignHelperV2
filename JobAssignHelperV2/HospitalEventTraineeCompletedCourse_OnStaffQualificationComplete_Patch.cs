using Harmony12;
using System;
using System.Linq;
using TH20;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(HospitalEventTraineeCompletedCourse.Config), "OnStaffQualificationComplete")]
    internal static class HospitalEventTraineeCompletedCourse_OnStaffQualificationComplete_Patch
    {
        private static void Postfix(HospitalEventTraineeCompletedCourse.Config __instance, Staff staff, QualificationDefinition qualification, Staff teacher)
        {
            if (!Main.enabled || !Main.settings.AssignJobsOnTrainingComplete)
                return;

            try
            {
                //if (staff.Definition._type != StaffDefinition.Type.Doctor && staff.Definition._type != StaffDefinition.Type.Nurse)
                //    return;

                //Main.Logger.Log($"[BEFORE] Name: {staff.CharacterName.GetCharacterName()}, {staff?.Qualifications?.Select(x => x?.Definition?.NameLocalised.Translation).ListThis("Qualifications", true)}, {staff.JobExclusions.Select(x => x.ToLocalisedString()).ListThis("JobExclusions", true)}.");
                JobAssignQualificationToggle jobAssignQualificationToggle = new JobAssignQualificationToggle();
                jobAssignQualificationToggle.StaffCharacter = staff;
                var jobs = RoomAlgorithms.GetAllJobs(staff.Level.Metagame, staff.Level.WorldState, staff.Definition._type);
                jobAssignQualificationToggle.AllJobsAvailableForThisStaffType = jobs;
                jobAssignQualificationToggle.ForceLeftShift = true;
                jobAssignQualificationToggle.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(new ffs()) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left });
                //Main.Logger.Log($"[AFTER] Name: {staff.CharacterName.GetCharacterName()}, {staff?.Qualifications?.Select(x => x?.Definition?.NameLocalised.Translation).ListThis("Qualifications", true)}, {staff.JobExclusions.Select(x => x.ToLocalisedString()).ListThis("JobExclusions", true)}.");

                //var recommended = staff.GetMatchingJobRooms();
                //var jobs = RoomAlgorithms.GetAllJobs(staff.Level.Metagame, staff.Level.WorldState, staff.Definition._type);
                //for (int i = jobs.Count - 1; i >= 0; i--)
                //{
                //    if (jobs[i] is JobRoomDescription job && recommended.Exists(x => x == job.Room._type))
                //    {
                //        jobs.RemoveAt(i);
                //    }
                //}

                //staff.JobExclusions.Clear();
                //staff.JobExclusions.AddRange(jobs);
            }
            catch (Exception e)
            {
                Main.Logger.Error(e.ToString());
            }
        }

        private class ffs : UnityEngine.EventSystems.EventSystem { }
    }
}