using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TH20;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JobAssignHelperV2
{
    /// <summary>
    /// Controls clicking on the job qualification icon.
    /// left clicking on a qualification will toggle jobs so only those related to that qualification are assigned
    /// shift-LMB-click on any qualification toggles on all matching jobs for selected staff's qualifications
    /// ctrl-LMB-click on any qualification toggles _additionally_ jobs matching the selected qualification
    /// middle-click on any qualification toggles all jobs (same as if clicking on the row X/Y numbers, but for all jobs, not just the visible ones)
    /// </summary>
    public class JobAssignQualificationToggle : MonoBehaviour, IPointerClickHandler
    {
        //toggles are these rounded squares which you light up and down deciding whether staff is allowed to work there or not (and they each hold jobDescription data), the action of toggling them actually just adds or removes from staff's jobExclusion list

        public Staff StaffCharacter;
        public StaffMenu StaffMenu;
        public int StaffQualificationIndexClicked;
        public List<JobDescription> AllJobsAvailableForThisStaffType;
        public bool ForceLeftShift;

        public void OnPointerClick(PointerEventData e)
        {
            if (!Main.enabled || StaffCharacter == null || StaffCharacter.Qualifications.Count <= StaffQualificationIndexClicked || StaffCharacter.Definition.IsUniqueVehicularMechanic)
                return;
            try
            {
                if (e.button == PointerEventData.InputButton.Left)
                {
                    ToggleQualificationJobs();
                    RefreshStaffMenuJobRows();
                }
                else if (e.button == PointerEventData.InputButton.Middle)
                {
                    ToggleAllJobs();
                    RefreshStaffMenuJobRows();
                }
            }
            catch (Exception exception)
            {
                Main.Logger.Error(exception.ToString());
            }
        }

        private void ToggleQualificationJobs()
        {
            switch (StaffCharacter.Definition._type)
            {
                case StaffDefinition.Type.Doctor:
                case StaffDefinition.Type.Nurse:
                case StaffDefinition.Type.Assistant:
                    ToggleQualificationJobs(StaffCharacter.GetMatchingJobRooms, StaffCharacter.GetMatchingJobRooms, FilterJobsByRoomTypes);
                    break;

                case StaffDefinition.Type.Janitor:
                    ToggleQualificationJobs(StaffCharacter.GetMatchingJanitorJobTypes, StaffCharacter.GetMatchingJanitorJobTypes, FilterJobsByJanitorJobTypes);
                    StaffCharacter.JobExclusions.Remove(AllJobsAvailableForThisStaffType.First(x => x is JobFireDescription)); //this stays hard-coded like that, idc
                    break;

                default: break;
            }
        }

        private void ToggleQualificationJobs<T>(Func<List<T>> getMatchingJobTypesForAllQualifications, Func<QualificationSlot, List<T>> getMatchingJobTypesForSingleQualification, Func<JobDescription, List<T>, bool> filterJobsByTypes)
        {
            List<T> matchingJobTypes = (Input.GetKey(KeyCode.LeftShift) || ForceLeftShift) ? getMatchingJobTypesForAllQualifications() : getMatchingJobTypesForSingleQualification(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
            if (matchingJobTypes.Count == 0)
                return;

            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();
            List<JobDescription> matchingJobs = availableJobs.Where(x => filterJobsByTypes(x, matchingJobTypes)).ToList();

            AssignJobExclusionsBasedOnMatchingJobsAndInputKeys();

            void AssignJobExclusionsBasedOnMatchingJobsAndInputKeys()
            {
                if (!Input.GetKey(KeyCode.LeftControl)) //if ctrl isn't pressed, exclude (basically) all jobs, i.e. make character unable to work anywhere (and if ctrl is pressed, don't do that, because we want to add now some more jobs)
                    StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);

                List<JobDescription> currentStaffsJobsList = null;
                if (Input.GetKey(KeyCode.LeftControl)) //if ctrl is pressed, check if the qualification being pressed isn't the one which has already been assigned, and if so, remove these jobs from staff's jobs (yes, by adding them to exclusions)
                    currentStaffsJobsList = availableJobs.Except(StaffCharacter.JobExclusions).ToList();

                if (currentStaffsJobsList != null && matchingJobs.All(x => currentStaffsJobsList.Contains(x))) //if all matching jobs can already by performed by the character, remove them
                    StaffCharacter.JobExclusions.AddRangeUnique(matchingJobs);
                else
                    StaffCharacter.JobExclusions.RemoveAll(x => matchingJobs.Contains(x)); //add jobs where the staff is supposed to work now (yes, by removing them from exclusions, TPH logic ¯\_(ツ)_/¯)
            }
        }

        private bool FilterJobsByRoomTypes(JobDescription x, List<RoomDefinition.Type> matchingRoomTypes) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
        {
            return (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
                || (x is JobAmbulanceDescription ambulanceJob && GetAdequateCharacterModifiers().Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
                || (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && GetAdequateCharacterModifiers().Any(m => m is QualificationServiceModifier));

            //if (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
            //    return true;
            //else if (x is JobAmbulanceDescription ambulanceJob && GetAdequateCharacterModifiers().Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
            //    return false;
            //else if (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && GetAdequateCharacterModifiers().Any(m => m is QualificationServiceModifier))
            //    return true;

            //return false;
        }

        private IEnumerable<CharacterModifier> GetAdequateCharacterModifiers() => (Input.GetKey(KeyCode.LeftShift) || ForceLeftShift) ? StaffCharacter.Qualifications.SelectMany(y => y.Definition.Modifiers) : StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers;

        private bool FilterJobsByJanitorJobTypes(JobDescription x, List<JobMaintenance.JobDescription> matchingJanitorJobTypes) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
        {
            return (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobTypes.Contains(maintenanceJob.Description)
                || (x is JobGhostDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Ghost))
                || (x is JobUpgradeDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Max))
            );

            //if (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobs.Contains(maintenanceJob.Description))
            //    return true;
            //else if (x is JobGhostDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Ghost))
            //    return true;
            //else if (x is JobUpgradeDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Max))
            //    return true;

            //return false;
        }

        private void ToggleAllJobs()
        {
            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();
            if (StaffCharacter.JobExclusions.Count == 0)
                StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);
            else
                StaffCharacter.JobExclusions.Clear();
        }

        private void RefreshStaffMenuJobRows() //TODO: try changing that from reflection to instance calls from one of StaffMenu_ patches
        {
            if (StaffMenu != null)
            {
                var p = StaffMenu.GetType().GetField("_staffMenuRowProvider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(StaffMenu);
                if (p is StaffMenuRowProvider provider)
                    provider.RefreshRowJobs();

                MethodInfo method = StaffMenu.GetType().GetMethod("CreateJobIcons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                    method.Invoke(StaffMenu, new object[] { StaffCharacter.Definition._type });
            }
        }
    }


    //public class JobAssignQualificationToggle : MonoBehaviour, IPointerClickHandler
    //{
    //    //toggles are these rounded squares which you light up and down deciding whether staff is allowed to work there or not (and they each hold jobDescription data), the action of toggling them actually just adds or removes from staff's jobExclusion list
    //    public Staff StaffCharacter;
    //    public StaffMenu StaffMenu;
    //    public int StaffQualificationIndexClicked;
    //    public List<JobDescription> AllJobsAvailableForThisStaffType;

    //    public void OnPointerClick(PointerEventData e)
    //    {
    //        if (!Main.enabled || StaffCharacter == null || StaffCharacter.Qualifications.Count <= StaffQualificationIndexClicked || StaffCharacter.Definition.IsUniqueVehicularMechanic)
    //            return;
    //        try
    //        {
    //            var jobToggles = Traverse.Create(GetComponentInParent<StaffMenuJobAssignRow>()).Field("_jobToggles").GetValue<List<StaffJobToggle>>(); //TODO: change that call from reflection to reference from Patch instance? also, check if everything works if we get rid of this check
    //            if (jobToggles.Count == 0)
    //                return;

    //            var clickedQualification = StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.NameLocalised;
    //            var jobsWithRequiredQuals = jobToggles.Select(x => x.Job).Where(x => !string.IsNullOrWhiteSpace(x.RequiredQualificationString()));

    //            var a = StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Select(m => (m as QualificationVehicleMaintenanceOnly)?.ToString()).ToList();
    //            var a1 = StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Select(m => m.ToString()).ToList();
    //            var b = StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Count(m => m is QualificationVehicleMaintenanceOnly)/* > 0*/;
    //            Main.Logger.Log($"[FFS] '{a1.ListThis("qualModifiers")}', VehicleQuals: {b}, clickedQualification: '{clickedQualification}'.");

    //            //Main.Logger.Log($"[JobToggle] clickedQualification: '{clickedQualification}', jobsWithRequiredQualsThatThisCharCanApparentlyHave(BasedOnToggles): '{string.Join(" | ", jobsWithRequiredQuals)}'.");
    //            //Main.Logger.Log($"[JobToggle] jobTogglesCount: '{jobToggles.Count}', jobTogglesJobs: '{string.Join(" | ", jobToggles.Select(x => x.Job))}', StaffCharacter.JobExclusions: '{string.Join(" | ", StaffCharacter.JobExclusions.Select(x => $"{x.GetJobAssignmentTooltipString()} with {x.RequiredQualificationString()}"))}'.");
    //            if (e.button == PointerEventData.InputButton.Left)
    //            {
    //                //if (Input.GetKey(KeyCode.LeftControl))
    //                //{
    //                Main.Logger.Log("Left click control (not anymore) start.");
    //                //ToggleJobsBySingleQualification vs ToggleJobsByAllQualifications (or just: ToggleJobQualifications)
    //                ToggleQualificationJobs();
    //                //ToggleJobQualificationsByRemovingFromSuitableList();
    //                Main.Logger.Log("Left click control (not anymore) end.");
    //                //}
    //                //else if (Input.GetKey(KeyCode.LeftShift))
    //                //    ToggleAllSuitableQualifications(jobToggles);
    //                //else
    //                //    ToggleSpecificQualifications(jobToggles);

    //                RefreshStaffMenuJobRows();
    //            }
    //            else if (e.button == PointerEventData.InputButton.Middle)
    //            {
    //                //ToggleAllQualifications(jobToggles);
    //                ToggleAllJobs();
    //                //exclusionsCount == 0 => has all jobs, SO we need to add all excludes so that he/she will have none
    //                //exclusionsCount == maxCount => has no jobs, SO we need to clear all excludes so that he/she will have all jobs
    //                //exclusionsCount == 1 => has all but 1 job, SO we need to clear all excludes so that he/she will have all jobs
    //                //exclusionsCount == max-1 => has only 1 job, SO we need to clear all excludes so that he/she will have all jobs
    //                //Main.Logger.Log($"[JobToggle] Middle click follow-up. JobExclusions.Count: {StaffCharacter.JobExclusions.Count}.");
    //                RefreshStaffMenuJobRows();
    //            }
    //        }
    //        catch (Exception exception)
    //        {
    //            Main.Logger.Error(exception.ToString());
    //        }

    //        void ToggleQualificationJobs()
    //        {
    //            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();

    //            if (StaffCharacter.Definition._type != StaffDefinition.Type.Janitor)
    //            {
    //                List<RoomDefinition.Type> matchingRoomTypes = (Input.GetKey(KeyCode.LeftShift)) ? StaffCharacter.GetMatchingJobRooms() : StaffCharacter.GetMatchingJobRooms(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //                if (matchingRoomTypes.Count > 0)
    //                {
    //                    List<JobDescription> matchingJobs = availableJobs.Where(FilterJobsByRooms).ToList();

    //                    //foreach (var job in availableJobs)
    //                    //{
    //                    //    if (job is JobRoomDescription roomJob)
    //                    //    {
    //                    //        if (matchingRoomTypes.Contains(roomJob.Room._type))
    //                    //            matchingJobs.Add(roomJob); //allow working
    //                    //                                          //else
    //                    //                                          //    matchingJobs.Remove(roomJob); //disallow working
    //                    //    }
    //                    //    if (job is JobAmbulanceDescription ambulanceJob)
    //                    //    {
    //                    //        if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                    //            matchingJobs.Add(ambulanceJob);
    //                    //        //else
    //                    //        //    matchingJobs.Remove(ambulanceJob);
    //                    //    }
    //                    //    if (StaffCharacter.Definition._type == StaffDefinition.Type.Assistant && job is JobItemDescription itemJob)
    //                    //    {
    //                    //        if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Count((CharacterModifier m) => m is QualificationServiceModifier) > 0)
    //                    //            matchingJobs.Add(itemJob);
    //                    //        //else
    //                    //        //    matchingJobs.Remove(itemJob);
    //                    //    }
    //                    //}
    //                    bool FilterJobsByRooms(JobDescription x) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
    //                    {
    //                        return (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
    //                            || (x is JobAmbulanceDescription ambulanceJob && GetAdequateCharacterModifiers().Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                            || (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && GetAdequateCharacterModifiers().Any(m => m is QualificationServiceModifier));

    //                        //if (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
    //                        //    return true;
    //                        ////var cc = roomJob.StaffRequired.Definition.Components?.Select(y => y.ToString()).ListThis("components", true);
    //                        ////Main.Logger.Log($"am also here! {x} ___ {x.GetType().FullName}, {cc}.");
    //                        ////TODO: if reception room return false? return Main.settings.CustomerServiceRooms.contains?
    //                        //else if (x is JobAmbulanceDescription ambulanceJob && GetAdequateCharacterModifiers().Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                        //    return false;
    //                        //else if (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && GetAdequateCharacterModifiers().Any(m => m is QualificationServiceModifier))
    //                        //    return true;
    //                        ////should really only return true for reception and be done with it...
    //                        ////if (itemJob.ItemDefinition.Components.Any(y => y.ToString() == "TH20.RoomItemReceptionComponent")
    //                        ////return Main.settings.RoomDict.ContainsKey(itemJob.);
    //                        ////var a = itemJob.ItemDefinition.Components?.Select(y => y.ToString()).ListThis("components", true);
    //                        ////var b = itemJob.ItemDefinition.Filters?.Select(y => y.ToString()).ListThis("filters", true);
    //                        ////var c = itemJob.ItemDefinition.Interactions?.Select(y => $"{y.ToString()}_{y.DebugUniqueName()}_{y.Name}_{y.Type}").ListThis("interactions", true);
    //                        ////var d = itemJob.ItemDefinition.GetName();
    //                        ////var f = itemJob.ItemDefinition.ItemType;
    //                        ////var g = itemJob.ItemDefinition.ServiceDescription;
    //                        ////Main.Logger.Log($"am here! {x} ___ {x.GetType().FullName}. itemDefinitionName: '{d}', type: {f}, desc: {g}, {a}, {b}, {c}.");
    //                        //else
    //                        //    return false;
    //                    }

    //                    void AddJobsByRooms(JobDescription x) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
    //                    {
    //                        if (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
    //                            matchingJobs.Add(roomJob);
    //                        else if (x is JobAmbulanceDescription ambulanceJob && GetAdequateCharacterModifiers().Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                            matchingJobs.Add(ambulanceJob);
    //                        else if (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && GetAdequateCharacterModifiers().Any(m => m is QualificationServiceModifier))
    //                            matchingJobs.Add(itemJob);
    //                    }

    //                    IEnumerable<CharacterModifier> GetAdequateCharacterModifiers() => Input.GetKey(KeyCode.LeftShift) ? StaffCharacter.Qualifications.SelectMany(y => y.Definition.Modifiers) : StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers;

    //                    //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis("jobExclusions before")}, {matchingJobs.ListThis("matchingJobs")}.");
    //                    if (!Input.GetKey(KeyCode.LeftControl)) //if ctrl isn't pressed, exclude (basically) all jobs, i.e. make character unable to work anywhere (and if ctrl is pressed, don't do that, because we want to add now some more jobs)
    //                        StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);

    //                    List<JobDescription> currentStaffsJobsList = null;
    //                    if (Input.GetKey(KeyCode.LeftControl)) //if ctrl is pressed, check if the qualification being pressed isn't the one which has already been assigned, and if so, remove these jobs from staff's jobs (yes, by adding them to exclusions)
    //                    {
    //                        currentStaffsJobsList = availableJobs.Except(StaffCharacter.JobExclusions).ToList();
    //                        //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis("jobExclusions", true)}, {matchingJobs.ListThis("matchingJobs", true)}, {availableJobs.ListThis("availableJobs", true)}, {currentStaffsJobsList.ListThis("currentStaffsJobsList", true)}.");
    //                        //if (matchingJobs.All(x => currentStaffsJobsList.Contains(x)))
    //                        //{
    //                        //    Main.Logger.Log($"[LeftClickControl] IF passed.");
    //                        //    StaffCharacter.JobExclusions.AddRangeUnique(matchingJobs);
    //                        //}
    //                    }

    //                    if (currentStaffsJobsList != null && matchingJobs.All(x => currentStaffsJobsList.Contains(x)))
    //                        StaffCharacter.JobExclusions.AddRangeUnique(matchingJobs);
    //                    else
    //                        StaffCharacter.JobExclusions.RemoveAll(x => matchingJobs.Contains(x)); //add jobs where the staff is supposed to work now (yes, by removing them from exclusions, TPH logic ¯\_(ツ)_/¯)

    //                    //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis("jobExclusions after")}.");
    //                }
    //            }
    //            else
    //            {
    //                List<JobMaintenance.JobDescription> matchingJanitorJobTypes = (Input.GetKey(KeyCode.LeftShift)) ? StaffCharacter.GetMatchingJanitorJobTypes() : StaffCharacter.GetMatchingJanitorJobTypes(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //                if (matchingJanitorJobTypes.Count > 0)
    //                {
    //                    List<JobDescription> matchingJobs = availableJobs.Where(FilterJobsByRooms).ToList();

    //                    bool FilterJobsByRooms(JobDescription x) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
    //                    {
    //                        Main.Logger.Log($"am janitor here! {x} ___ {x.GetType().FullName}.");
    //                        return (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobTypes.Contains(maintenanceJob.Description)
    //                            || (x is JobGhostDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Ghost))
    //                            || (x is JobUpgradeDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Max)
    //                            /*|| (x is JobFireDescription)*/) //this stays hard-coded like that, idc (might need to be moved at the very end because of ctrl shenanigans)
    //                        );

    //                        //if (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobs.Contains(maintenanceJob.Description))
    //                        //    return true;
    //                        //else if (x is JobGhostDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Ghost))
    //                        //    return true;
    //                        //else if (x is JobUpgradeDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Max))
    //                        //    return true;
    //                        //else if (x is JobFireDescription) //this stays hard-coded like that, idc
    //                        //    return true;

    //                        //return false;
    //                    }
    //                    void AddJobsByRooms(JobDescription x)
    //                    {
    //                        if (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobTypes.Contains(maintenanceJob.Description))
    //                            matchingJobs.Add(maintenanceJob);
    //                        else if (x is JobGhostDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Ghost))
    //                            matchingJobs.Add(x);
    //                        else if (x is JobUpgradeDescription && matchingJanitorJobTypes.Contains(JobMaintenance.JobDescription.Max))
    //                            matchingJobs.Add(x);
    //                    }

    //                    if (!Input.GetKey(KeyCode.LeftControl))
    //                        StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);

    //                    List<JobDescription> currentStaffsJobsList = null;
    //                    if (Input.GetKey(KeyCode.LeftControl)) //if ctrl is pressed, check if the qualification being pressed isn't the one which has already been assigned, and if so, remove these jobs from staff's jobs (yes, by adding them to exclusions)
    //                        currentStaffsJobsList = availableJobs.Except(StaffCharacter.JobExclusions).ToList();

    //                    if (currentStaffsJobsList != null && matchingJobs.All(x => currentStaffsJobsList.Contains(x)))
    //                        StaffCharacter.JobExclusions.AddRangeUnique(matchingJobs/*.Except(availableJobs.Where(x => x is JobFireDescription)).ToList()*/);
    //                    else
    //                        StaffCharacter.JobExclusions.RemoveAll(x => matchingJobs.Contains(x));

    //                    StaffCharacter.JobExclusions.Remove(availableJobs.First(x => x is JobFireDescription)); //this stays hard-coded like that, idc
    //                }
    //            }

    //        }

    //        void Generic<T>(Func<List<T>> getMatchingJobTypesForAllQualifications, Func<QualificationSlot, List<T>> getMatchingJobTypesForSingleQualification, Func<JobDescription, bool> filterJobsByTypes)
    //        {
    //            List<T> matchingJobTypes = (Input.GetKey(KeyCode.LeftShift)) ? getMatchingJobTypesForAllQualifications() : getMatchingJobTypesForSingleQualification(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //            if (matchingJobTypes.Count > 0)
    //                return;

    //            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();
    //            List<JobDescription> matchingJobs = availableJobs.Where(filterJobsByTypes).ToList();

    //            AssignJobExclusionsBasedOnMatchingJobsAndInputKeys();

    //            void AssignJobExclusionsBasedOnMatchingJobsAndInputKeys()
    //            {
    //                if (!Input.GetKey(KeyCode.LeftControl)) //if ctrl isn't pressed, exclude (basically) all jobs, i.e. make character unable to work anywhere (and if ctrl is pressed, don't do that, because we want to add now some more jobs)
    //                    StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);

    //                List<JobDescription> currentStaffsJobsList = null;
    //                if (Input.GetKey(KeyCode.LeftControl)) //if ctrl is pressed, check if the qualification being pressed isn't the one which has already been assigned, and if so, remove these jobs from staff's jobs (yes, by adding them to exclusions)
    //                    currentStaffsJobsList = availableJobs.Except(StaffCharacter.JobExclusions).ToList();

    //                if (currentStaffsJobsList != null && matchingJobs.All(x => currentStaffsJobsList.Contains(x))) //if all matching jobs can already by performed by the character, remove them
    //                    StaffCharacter.JobExclusions.AddRangeUnique(matchingJobs);
    //                else
    //                    StaffCharacter.JobExclusions.RemoveAll(x => matchingJobs.Contains(x)); //add jobs where the staff is supposed to work now (yes, by removing them from exclusions, TPH logic ¯\_(ツ)_/¯)
    //            }
    //        }

    //        List<JobDescription> GetJanitorMatchingJobs(List<JobDescription> availableJobs)
    //        {
    //            List<JobMaintenance.JobDescription> matchingJanitorJobs = (Input.GetKey(KeyCode.LeftShift)) ? StaffCharacter.GetMatchingJanitorJobTypes() : StaffCharacter.GetMatchingJanitorJobTypes(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //            return availableJobs.Where(FilterJobsByRooms).ToList();
    //            bool FilterJobsByRooms(JobDescription x) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
    //            {
    //                Main.Logger.Log($"am janitor here! {x} ___ {x.GetType().FullName}.");
    //                return (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobs.Contains(maintenanceJob.Description)
    //                    || (x is JobGhostDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Ghost))
    //                    || (x is JobUpgradeDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Max)
    //                    /*|| (x is JobFireDescription)*/) //this stays hard-coded like that, idc (might need to be moved at the very end because of ctrl shaeningans
    //                );

    //                //if (x is JobMaintenanceDescription maintenanceJob && matchingJanitorJobs.Contains(maintenanceJob.Description))
    //                //    return true;
    //                //else if (x is JobGhostDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Ghost))
    //                //    return true;
    //                //else if (x is JobUpgradeDescription && matchingJanitorJobs.Contains(JobMaintenance.JobDescription.Max))
    //                //    return true;
    //                //else if (x is JobFireDescription) //this stays hard-coded like that, idc
    //                //    return true;

    //                //return false;
    //            }

    //            Generic(StaffCharacter.GetMatchingJobRooms, StaffCharacter.GetMatchingJobRooms, FilterJobsByRooms);
    //            Generic(StaffCharacter.GetMatchingJanitorJobTypes, StaffCharacter.GetMatchingJanitorJobTypes, FilterJobsByRooms);
    //        }

    //        void ToggleJobQualificationsByRemovingFromSuitableList()
    //        {
    //            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();
    //            //Main.Logger.Log($"[LeftClickControl] {availableJobs.Select(x => $"'{x.ToString()}'__({x.GetType().FullName})").ListThis("suitable jobs before filtering", true)}");
    //            AddAndRemoveRoomJobs();

    //            void AddAndRemoveRoomJobs()
    //            {
    //                List<RoomDefinition.Type> matchingRoomTypes = (Input.GetKey(KeyCode.LeftShift)) ? StaffCharacter.GetMatchingJobRooms() : StaffCharacter.GetMatchingJobRooms(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //                //Main.Logger.Log($"[LeftClickControl] {matchingRoomTypes.ListThis($"matchingRoomTypes ({matchingRoomTypes.Count})")}.");

    //                if (matchingRoomTypes.Count > 0)
    //                {
    //                    availableJobs.RemoveAll(FilterJobsByRooms);

    //                    bool FilterJobsByRooms(JobDescription x) //'return true' means: yes, this is a job you want this staff to do (if a job gets result 'false', it'll be removed from a list of jobs which will later be set as workable jobs)
    //                    {
    //                        if (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
    //                            return true;
    //                        //var cc = roomJob.StaffRequired.Definition.Components?.Select(y => y.ToString()).ListThis("components", true);
    //                        //Main.Logger.Log($"am also here! {x} ___ {x.GetType().FullName}, {cc}.");
    //                        //TODO: if reception room return false? return Main.settings.CustomerServiceRooms.contains?
    //                        else if (x is JobAmbulanceDescription ambulanceJob/* && StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType())*/)
    //                            if (Input.GetKey(KeyCode.LeftShift) && StaffCharacter.Qualifications.SelectMany(y => y.Definition.Modifiers).Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                                return true;
    //                            else if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                                return true;
    //                            else
    //                                return false;
    //                        else if (x is JobItemDescription itemJob && itemJob.ItemDefinition.ServiceDescription == JobService.JobDescription.ReceptionCheckIn && StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => m is QualificationServiceModifier))
    //                            return true;
    //                        //should really only return true for reception and be done with it...
    //                        //if (itemJob.ItemDefinition.Components.Any(y => y.ToString() == "TH20.RoomItemReceptionComponent")
    //                        //return Main.settings.RoomDict.ContainsKey(itemJob.);
    //                        //var a = itemJob.ItemDefinition.Components?.Select(y => y.ToString()).ListThis("components", true);
    //                        //var b = itemJob.ItemDefinition.Filters?.Select(y => y.ToString()).ListThis("filters", true);
    //                        //var c = itemJob.ItemDefinition.Interactions?.Select(y => $"{y.ToString()}_{y.DebugUniqueName()}_{y.Name}_{y.Type}").ListThis("interactions", true);
    //                        //var d = itemJob.ItemDefinition.GetName();
    //                        //var f = itemJob.ItemDefinition.ItemType;
    //                        //var g = itemJob.ItemDefinition.ServiceDescription;
    //                        //Main.Logger.Log($"am here! {x} ___ {x.GetType().FullName}. itemDefinitionName: '{d}', type: {f}, desc: {g}, {a}, {b}, {c}.");
    //                        else
    //                            return false;
    //                        //return (x is JobRoomDescription roomJob && matchingRoomTypes.Contains(roomJob.Room._type))
    //                        //|| (x is JobAmbulanceDescription ambulanceJob && StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJob.GetAmbulanceType()))
    //                        //|| (x is JobItemDescription itemJob && StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Count(m => m is QualificationServiceModifier) > 0);
    //                    }

    //                    //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis($"jobExclusions ({StaffCharacter.JobExclusions.Count}) before")}, {availableJobs.ListThis($"availableJobs ({availableJobs.Count})")}.");
    //                    if (Input.GetKey(KeyCode.LeftControl))
    //                    {
    //                        var jobsToExclude = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList().Except(availableJobs).ToList();
    //                        //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis($"jobsToExclude ({jobsToExclude.Count})")}.");
    //                        StaffCharacter.JobExclusions.RemoveAll(x => jobsToExclude.Contains(x));
    //                    }
    //                    else
    //                    {
    //                        StaffCharacter.JobExclusions.Clear();
    //                        StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);
    //                    }
    //                    //Main.Logger.Log($"[LeftClickControl] {StaffCharacter.JobExclusions.ListThis($"jobExclusions ({StaffCharacter.JobExclusions.Count}) after")}.");
    //                }
    //            }
    //        }

    //        void ToggleAllJobs()
    //        {
    //            var availableJobs = AllJobsAvailableForThisStaffType.Where(x => x.IsSuitable(StaffCharacter)).ToList();
    //            Main.Logger.Log($"[JobToggle] Middle click registered. {availableJobs.ListThis("availableJobs", true)}.");
    //            Main.Logger.Log($"[JobToggle] Middle click registered. {StaffCharacter.JobExclusions.ListThis("JobExclusions", true)}, {AllJobsAvailableForThisStaffType.ListThis("Jobs", true)}.");
    //            if (StaffCharacter.JobExclusions.Count == 0)
    //                StaffCharacter.JobExclusions.AddRangeUnique(availableJobs);
    //            else
    //                StaffCharacter.JobExclusions.Clear();
    //        }
    //    }

    //    private void ToggleSpecificQualifications(List<StaffJobToggle> jobToggles)
    //    {
    //        var matchingRoomTypes = StaffCharacter.GetMatchingJobRooms(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //        if (matchingRoomTypes.Count > 0)
    //        {
    //            foreach (var obj in jobToggles)
    //            {
    //                var toggle = obj.GetComponent<Toggle>();
    //                if (toggle.interactable && obj.Job is JobRoomDescription description)
    //                {
    //                    if (matchingRoomTypes.Contains(description.Room._type))
    //                        StaffCharacter.JobExclusions.Remove(description); //allow working
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(description); //disallow working
    //                }
    //                else if (toggle.interactable && obj.Job is JobAmbulanceDescription ambulanceJobDescription)
    //                {
    //                    if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJobDescription.GetAmbulanceType()))
    //                        StaffCharacter.JobExclusions.Remove(ambulanceJobDescription);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(ambulanceJobDescription);
    //                }
    //                else if (StaffCharacter.Definition._type == StaffDefinition.Type.Assistant && toggle.interactable && obj.Job is JobItemDescription)
    //                {
    //                    if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Count((CharacterModifier m) => m is QualificationServiceModifier) > 0)
    //                        StaffCharacter.JobExclusions.Remove(obj.Job);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(obj.Job);
    //                }
    //            }
    //        }

    //        List<JobMaintenance.JobDescription> matchingJobs = StaffCharacter.GetMatchingJanitorJobTypes(StaffCharacter.Qualifications[StaffQualificationIndexClicked]);
    //        if (matchingJobs.Count > 0)
    //        {
    //            foreach (var obj in jobToggles)
    //            {
    //                var toggle = obj.GetComponent<Toggle>();
    //                if (toggle.interactable && (obj.Job is JobMaintenanceDescription description))
    //                {
    //                    Main.Logger.Log($"[foreach through toggles] MaintenanceJobDesc: {description.Description}.");
    //                    if (matchingJobs.Contains(description.Description))
    //                        StaffCharacter.JobExclusions.Remove(description); //allow working
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(description); //disallow working
    //                }
    //                else if (toggle.interactable && obj.Job is JobGhostDescription)
    //                {
    //                    if (matchingJobs.Contains(JobMaintenance.JobDescription.Ghost))
    //                        StaffCharacter.JobExclusions.Remove(obj.Job);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(obj.Job);
    //                }
    //                else if (toggle.interactable && obj.Job is JobUpgradeDescription)
    //                {
    //                    if (matchingJobs.Contains(JobMaintenance.JobDescription.Max))
    //                        StaffCharacter.JobExclusions.Remove(obj.Job);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(obj.Job);
    //                }
    //            }
    //        }
    //    }

    //    public void ToggleAllSuitableQualifications(List<StaffJobToggle> jobToggles)
    //    {
    //        if (StaffCharacter.Qualifications.Count == 0)
    //            return;

    //        List<RoomDefinition.Type> matchingRoomTypes = StaffCharacter.GetMatchingJobRooms();
    //        if (matchingRoomTypes.Count > 0)
    //        {
    //            foreach (StaffJobToggle staffJobToggle in jobToggles)
    //            {
    //                Toggle toggle = staffJobToggle.GetComponent<Toggle>();
    //                if (toggle.interactable && staffJobToggle.Job is JobRoomDescription description)
    //                {
    //                    if (matchingRoomTypes.Contains(description.Room._type))
    //                        StaffCharacter.JobExclusions.Remove(staffJobToggle.Job); //allow working
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(staffJobToggle.Job); //disallow working
    //                }
    //                else if (toggle.interactable && staffJobToggle.Job is JobAmbulanceDescription ambulanceJobDescription)
    //                {
    //                    if (StaffCharacter.Qualifications[StaffQualificationIndexClicked].Definition.Modifiers.Any(m => (m as QualificationAmbulanceSpeedBoost)?.Type == ambulanceJobDescription.GetAmbulanceType()))
    //                        StaffCharacter.JobExclusions.Remove(ambulanceJobDescription);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(ambulanceJobDescription);
    //                }
    //                else if (StaffCharacter.Definition._type == StaffDefinition.Type.Assistant && toggle.interactable && staffJobToggle.Job is JobItemDescription)
    //                {
    //                    if (matchingRoomTypes.Contains(RoomDefinition.Type.Reception))
    //                        StaffCharacter.JobExclusions.Remove(staffJobToggle.Job);
    //                    else
    //                        StaffCharacter.JobExclusions.AddUnique(staffJobToggle.Job);
    //                }
    //            }
    //        }
    //        if (StaffCharacter.Definition._type == StaffDefinition.Type.Janitor)
    //        {
    //            List<JobMaintenance.JobDescription> matchingJobs = StaffCharacter.GetMatchingJanitorJobTypes();
    //            if (matchingJobs.Count > 0)
    //            {
    //                foreach (StaffJobToggle staffJobToggle in jobToggles)
    //                {
    //                    Toggle toggle = staffJobToggle.GetComponent<Toggle>();
    //                    if (toggle.interactable && (staffJobToggle.Job is JobMaintenanceDescription description))
    //                    {
    //                        if (matchingJobs.Contains(description.Description))
    //                            StaffCharacter.JobExclusions.Remove(staffJobToggle.Job);
    //                        else
    //                            StaffCharacter.JobExclusions.AddUnique(staffJobToggle.Job);
    //                    }
    //                    if (toggle.interactable && staffJobToggle.Job is JobGhostDescription)
    //                    {
    //                        if (matchingJobs.Contains(JobMaintenance.JobDescription.Ghost))
    //                            StaffCharacter.JobExclusions.Remove(staffJobToggle.Job);
    //                        else
    //                            StaffCharacter.JobExclusions.AddUnique(staffJobToggle.Job);
    //                    }
    //                    if (toggle.interactable && staffJobToggle.Job is JobUpgradeDescription)
    //                    {
    //                        if (matchingJobs.Contains(JobMaintenance.JobDescription.Max))
    //                            StaffCharacter.JobExclusions.Remove(staffJobToggle.Job);
    //                        else
    //                            StaffCharacter.JobExclusions.AddUnique(staffJobToggle.Job);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private void RefreshStaffMenuJobRows() //TODO: try changing that from reflection to instance calls from one of StaffMenu_ patches
    //    {
    //        if (StaffMenu != null)
    //        {
    //            var p = StaffMenu.GetType().GetField("_staffMenuRowProvider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(StaffMenu);
    //            if (p is StaffMenuRowProvider provider)
    //                provider.RefreshRowJobs();

    //            MethodInfo method = StaffMenu.GetType().GetMethod("CreateJobIcons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    //            if (method != null)
    //                method.Invoke(StaffMenu, new object[] { StaffCharacter.Definition._type });
    //        }
    //    }
    //}
}