using FullInspector;
using Harmony12;
using System.Collections.Generic;
using System.Linq;
using TH20;

namespace JobAssignHelperV2
{
    public static class JobTypeMatcher
    {
        public static List<RoomDefinition.Type> GetMatchingJobRooms(this Staff staff, QualificationSlot newestQualificationSlot)
        {
            List<RoomDefinition.Type> list = new List<RoomDefinition.Type>();
            if (staff.Definition._type == StaffDefinition.Type.Janitor)
                return list;

            newestQualificationSlot.GetMatchingJobRooms(list, staff.Definition._type == StaffDefinition.Type.Nurse || staff.Definition._type == StaffDefinition.Type.Doctor);
            return list;
        }

        public static List<RoomDefinition.Type> GetMatchingJobRooms(this Staff staff)
        {
            List<RoomDefinition.Type> list = new List<RoomDefinition.Type>();
            if (staff.Definition._type == StaffDefinition.Type.Janitor)
                return list;

            foreach (QualificationSlot qualificationSlot in staff.Qualifications)
            {
                qualificationSlot.GetMatchingJobRooms(list, staff.Definition._type == StaffDefinition.Type.Nurse || staff.Definition._type == StaffDefinition.Type.Doctor);
            }
            return list;
        }

        public static List<JobMaintenance.JobDescription> GetMatchingJanitorJobTypes(this Staff staff, QualificationSlot qualificationSlot)
        {
            List<JobMaintenance.JobDescription> list = new List<JobMaintenance.JobDescription>();
            if (staff.Definition._type != StaffDefinition.Type.Janitor)
                return list;

            staff.GetMatchingJanitorJobTypes(qualificationSlot, list);
            return list;
        }

        public static List<JobMaintenance.JobDescription> GetMatchingJanitorJobTypes(this Staff staff)
        {
            List<JobMaintenance.JobDescription> list = new List<JobMaintenance.JobDescription>();
            if (staff.Definition._type != StaffDefinition.Type.Janitor)
                return list;

            foreach (QualificationSlot qualificationSlot in staff.Qualifications)
            {
                staff.GetMatchingJanitorJobTypes(qualificationSlot, list);
            }
            return list;
        }

        private static void GetMatchingJobRooms(this QualificationSlot qualificationSlot, List<RoomDefinition.Type> recommendedRoomTypes, bool isDoctorOrNurse) //meh, yolo
        {
            if (qualificationSlot.Definition.RequiredRoomUnlocked != null)
                recommendedRoomTypes.Add(qualificationSlot.Definition.RequiredRoomUnlocked.Instance._type);

            if (qualificationSlot.Definition.RequiredIllnessWithTreatmentRoom != null)
                recommendedRoomTypes.Add(qualificationSlot.Definition.RequiredIllnessWithTreatmentRoom.Instance._type);

            //Main.Logger.Log($"[RecommendationsRoom] RequiredRoomUnlocked: '{qualificationSlot.Definition.RequiredRoomUnlocked?.Instance?._type}',  RequiredIllnessWithTreatmentRoom: '{qualificationSlot.Definition.RequiredIllnessWithTreatmentRoom?.Instance?._type}', qualificationSlot.Definition.NameLoc: '{ qualificationSlot.Definition.NameLocalised.ToString()}', '{qualificationSlot.Definition.Modifiers/*.GetType()*/.GetAllBaseTypesFromCollection()/*.Select(x => x.FullName)*/.ListThis("qualificationSlot.Definition.ModifiersType")}'.");
            foreach (var modifier in qualificationSlot.Definition.Modifiers)
            {
                var validRoomTypes = new List<RoomDefinition.Type>();
                if (modifier is QualificationBaseModifier baseModifier)
                {
                    var roomDefinition = Traverse.Create(baseModifier).Field("_validRooms").GetValue<SharedInstance<RoomDefinition>[]>();
                    if (roomDefinition.Length > 0)
                    {
                        validRoomTypes.AddRange(roomDefinition.Select(x => x.Instance._type));
                        //Main.Logger.Log($"[RecommendationsRoom] roomDefinitionsFromBaseModifier: '{string.Join(" | ", roomDefinition.Select(x => x.name))}, roomDefinitionsFromBaseModifierInstance: '{string.Join(" | ", roomDefinition.Select(x => x.Instance.GetLocalisedName()))}', roomDefinitionsFromBaseModifierInstanceTypes: '{string.Join(" | ", validRoomTypes)}'.");
                    }
                }

                if (validRoomTypes.Count > 0)
                {
                    recommendedRoomTypes.AddRange(validRoomTypes);
                }
                else
                {
                    if (modifier is QualificationDiagnosisModifier)
                        recommendedRoomTypes.AddRange(Main.settings.DiagnosisRooms);
                    else if (modifier is QualificationTreatmentModifier)
                        recommendedRoomTypes.AddRange(Main.settings.TreatmentRooms);
                    else if (modifier is QualificationResearchModifier)
                        recommendedRoomTypes.Add(RoomDefinition.Type.Research);
                    else if (modifier is QualificationServiceModifier)
                        recommendedRoomTypes.AddRange(Main.settings.CustomerServiceRooms);
                    else if (isDoctorOrNurse && modifier is CharacterModifierAtrributeMultiplier)
                        recommendedRoomTypes.Add(RoomDefinition.Type.OperatingTheater);
                    else if (modifier is QualificationMarketModifier)
                        recommendedRoomTypes.Add(RoomDefinition.Type.Marketing);
                    else if (modifier is QualificationDurationModifier)
                        recommendedRoomTypes.Add(RoomDefinition.Type.TimeTunnel);
                    else if (modifier is QualificationAmbulanceSpeedBoost)
                        recommendedRoomTypes.Add(RoomDefinition.Type.AmbulanceBay);
                }
            }

            //Main.Logger.Log($"[RecommendationsRoom] recommendedRoomTypes after assigning: '{string.Join(" | ", recommendedRoomTypes)}'.");
        }

        private static void GetMatchingJanitorJobTypes(this Staff staff, QualificationSlot qualificationSlot, List<JobMaintenance.JobDescription> recommendedJobs)
        {
            //Main.Logger.Log($"[RecommendationsJob] {qualificationSlot.Definition.Modifiers?.Select(x => x.ToString()).ListThis("qualificationSlot.Definition.Modifiers", false, " | ")}, charCanRepairVehicles: {staff.CanRepairVehicles}, {staff.Qualifications.Select(x => x.Definition.NameLocalised.ToString()).ListThis("charQualDefs")}, {staff.Qualifications.Select(x => x.Definition.NameLocalised.Term).ListThis("charQualDefsTerms")}, {staff.Qualifications.Select(x => x.Definition.NameLocalised.ToAnalyticsTermString()).ListThis("charQualDefsAnalyticTerms")}.");
            foreach (CharacterModifier characterModifier in qualificationSlot.Definition.Modifiers)
            {
                //Main.Logger.Log($"[QualificationMaintenanceModifier] charCanRepairVehicles: {staff.CanRepairVehicles}. Which charModifier we're at: '{characterModifier}' (Desc: '{characterModifier.Description()}').");
                if (characterModifier is QualificationMaintenanceModifier)
                {
                    if (qualificationSlot.Definition.Modifiers.Count(m => m is QualificationVehicleMaintenanceOnly) > 0) //for some reason vehicle maintenance also has normal maintenance modifier, and we don't want our vehicle guy to do stuff regular maintenance guys do
                        continue;
                    //Main.Logger.Log($"[QualificationMaintenanceModifier] Add maintenance jobs: '{string.Join(", ", Main.settings.MaintenanceJobs)}'.");
                    recommendedJobs.AddRange(Main.settings.MaintenanceJobs);
                }
                else if (characterModifier is QualificationVehicleMaintenanceOnly)
                {
                    //Main.Logger.Log($"[QualificationVehicleMaintenanceOnly] .");
                    recommendedJobs.Add(JobMaintenance.JobDescription.Vehicular);
                }
                else if (characterModifier is QualificationUpgradeItemModifier) //yeah, that's not gonna work for Mechanics I, for some reason Mechanics I qual has no modifiers, gotta do it the way with analytics string (at the bottom)
                {
                    //Main.Logger.Log($"[UpgradeJobs] Add upgrade jobs: '{string.Join(", ", Main.settings.UpgradeJobs)}'.");
                    recommendedJobs.AddRange(Main.settings.UpgradeJobs);
                }
                else //there is also CharacterModifierMovementSpeed, but there's no reason to handle that really
                {
                    LocalisedString nameLocalised = qualificationSlot.Definition.NameLocalised;
                    //Main.Logger.Log($"[else] {nameLocalised.ToString()}. Which charModifier we're at: '{characterModifier}' (Desc: '{characterModifier.Description()}').");
                    if (!nameLocalised.ToString().ToLowerInvariant().StartsWith("mechanics"))
                    {
                        if (characterModifier is CharacterModifierIgnoreStatusEffect) //this is how game named ghost capture job lol
                        {
                            //Main.Logger.Log($"[CharacterModifierIgnoreStatusEffect] Add ghost jobs: '{string.Join(", ", Main.settings.GhostJobs)}'.");
                            recommendedJobs.AddRange(Main.settings.GhostJobs);
                        }
                        else
                        {
                            //Main.Logger.Log($"[elseElse] Add misc jobs: '{string.Join(", ", Main.settings.MiscJobs)}'.");
                            recommendedJobs.AddRange(Main.settings.MiscJobs);
                        }
                    }
                }
            }
            if (qualificationSlot.Definition.Modifiers.Length == 0 && qualificationSlot.Definition.NameLocalised.ToAnalyticsTermString().StartsWith("Janitor_Mechanics")) //Mechanics I has no modifiers
                recommendedJobs.AddRange(Main.settings.UpgradeJobs);

            //Main.Logger.Log($"[RecommendationsJob] recommendedJobs after assigning: '{string.Join(" | ", recommendedJobs)}'.");
        }
    }
}