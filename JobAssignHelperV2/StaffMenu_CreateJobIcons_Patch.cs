using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using TH20;
using TMPro;
using UnityEngine;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(StaffMenu), "Refresh")]
    internal static class StaffMenu_Refresh_Patch
    {
        public static StaffMenu.ViewModes ViewMode;

        private static void Postfix(StaffMenu.ViewModes ____viewMode)
        {
            if (!Main.enabled)
                return;

            ViewMode = ____viewMode;
        }
    }

    [HarmonyPatch(typeof(StaffMenu), "CreateJobIcons")]
    internal static class StaffMenu_CreateJobIcons_Patch
    {
        public static Level _level;
        public static WorldState _worldState;

        private static void Postfix(StaffMenu __instance, StaffDefinition.Type staffType, StaffMenu.StaffMenuSettings ____staffMenuSettings, WorldState ____worldState, List<JobDescription>[] ____jobs, CharacterManager ____characterManager, int ____jobAssignmentCurrentPage, Level ____level)
        {
            if (!Main.enabled)
                return;

            _level = ____level;
            _worldState = ____worldState;

            try
            {
                var availableJobsForThatStaffType = ____jobs[(int)staffType];
                StaffMenu.GetCurrentJobAssignmentIndiciesForPage(availableJobsForThatStaffType, ____jobAssignmentCurrentPage, out int startIndex, out int endIndex);
                var enumer = ____staffMenuSettings.JobsListContainer.GetEnumerator();
                for (int i = 0; i <= endIndex; i++)
                {
                    if (i >= availableJobsForThatStaffType.Count || i + startIndex > endIndex)
                        break;

                    enumer.MoveNext();
                    object obj = enumer.Current;
                    Transform transform = (Transform)obj;
                    JobDescription job = availableJobsForThatStaffType[i+startIndex];
                    TMP_Text component = UnityEngine.Object.Instantiate<GameObject>(____staffMenuSettings.TitleText.gameObject).GetComponent<TMP_Text>();
                    component.rectTransform.SetParent(transform);
                    int staffCount = ____characterManager.StaffMembers.Count((Staff s) => !s.HasResigned() && !s.HasBeenFired() && job.IsSuitable(s) && !s.JobExclusions.Contains(job));

                    if (job is JobRoomDescription)
                    {
                         AddTooltipInfoToRoomJobs(i, job, component, staffCount);
                    }
                    else if (job is JobItemDescription)
                    {
                        component.text = staffCount.ToString();
                        SetTextAsTooltip(i, job.GetJobAssignmentTooltipString() + "\n\nStaff Assigned: " + staffCount.ToString());
                    }
                    else if (job is JobMaintenanceDescription)
                    {
                        AddTooltipInfoToMaintenanceJobs(i, job, component, staffCount);
                    }
                    else if (job is JobUpgradeDescription)
                    {
                        AddTooltipInfoToUpgradeJobs(i, job, component, staffCount);
                    }
                    else if (job is JobAmbulanceDescription)
                    {
                        AddTooltipInfoToAmbulanceJobs(i, job, component, staffCount);
                    }
                    else
                    {
                        if (!(job is JobGhostDescription) && !(job is JobFireDescription))
                            continue;

                        component.text = staffCount.ToString();
                    }

                    component.enableAutoSizing = false;
                    component.fontSize = 18f;
                    component.enableWordWrapping = false;
                    component.overflowMode = 0;
                    component.alignment = TextAlignmentOptions.Midline;
                    component.color = Color.white;
                    component.outlineColor = Color.black;
                    component.outlineWidth = 1f;
                    component.rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    component.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    component.rectTransform.anchoredPosition = new Vector2(0f, -15f);
                    component.rectTransform.sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex.ToString() + ": " + ex.StackTrace.ToString());
            }

            void AddTooltipInfoToRoomJobs(int i, JobDescription job, TMP_Text component, int staffCount)
            {
                int roomCount = ____worldState.AllRooms.Count((Room x) => x.Definition == ((JobRoomDescription)job).Room);
                component.text = staffCount.ToString() + "/" + roomCount.ToString();
                SetTextAsTooltip(i, string.Concat(new string[]
                {
                            job.GetJobAssignmentTooltipString(),
                            "\n\nStaff Assigned: ", staffCount.ToString(),
                            "\nRooms Built: ", roomCount.ToString()
                }));
            }

            void AddTooltipInfoToMaintenanceJobs(int i, JobDescription job, TMP_Text component, int staffCount)
            {
                string text = staffCount.ToString();
                int itemCount = ____worldState.GetRoomItemsWithMaintenanceDescription(((JobMaintenanceDescription)job).Description)
                    .Where(x => x.Definition.Interactions.Count(inter => inter.Type == InteractionAttributeModifier.Type.Maintain) > 0)
                    .Count();
                if (job.ToString().ToUpperInvariant().Contains("VEHIC"))
                    itemCount += (____worldState.AllRooms.Where(x => x.Definition._type == RoomDefinition.Type.AmbulanceBay).Select(x => x.FloorPlan).SelectMany(x => x.Items).Where(x => x?.AmbulanceConfig?.Instance?.AmbulanceType == AmbulanceConfig.Type.Air)?.Count()).GetValueOrDefault();

                text = text + "/" + itemCount.ToString();
                component.text = text;
                SetTextAsTooltip(i, string.Concat(new string[]
                {
                            job.GetJobAssignmentTooltipString(),
                            "\n\nStaff Assigned: ", staffCount.ToString(),
                            "\nMaintenance Items: ", itemCount.ToString()
                }));
            }

            void AddTooltipInfoToUpgradeJobs(int i, JobDescription job, TMP_Text component, int staffCount)
            {
                int upgradeCount = 0;
                var machinesForUpgrade = new List<string>();
                foreach (Room room in ____worldState.AllRooms)
                {
                    foreach (RoomItem roomItem in room.FloorPlan.Items)
                    {
                        RoomItemUpgradeDefinition nextUpgrade = roomItem.Definition.GetNextUpgrade(roomItem.UpgradeLevel);
                        if (nextUpgrade != null && roomItem.Level.Metagame.HasUnlocked(nextUpgrade) && roomItem.GetComponent<RoomItemUpgradeComponent>() == null)
                        {
                            upgradeCount++;
                            machinesForUpgrade.Add(roomItem.Name);
                        }
                    }
                }

                component.text = staffCount.ToString() + "/" + upgradeCount;
                SetTextAsTooltip(i, string.Concat(new string[]
                {
                            job.GetJobAssignmentTooltipString(),
                            "\n\nStaff Assigned: ", staffCount.ToString(),
                            "\nMachines to upgrade: ", machinesForUpgrade.Any() ? string.Join(", ", machinesForUpgrade) : "none"
                }));
            }

            void AddTooltipInfoToAmbulanceJobs(int i, JobDescription job, TMP_Text component, int staffCount)
            {
                int roomCount;
                var ambulanceString = job.GetJobAssignmentTooltipString(); //I have no idea how to find that info through any other way that is not localised-string-based
                AmbulanceConfig.UniqueAmbulanceID ambulance;
                ambulance = AmbulanceHelper.TranslateJobTooltipToAmbulance(ambulanceString);
                var ambulanceTooltipType = "";
                var baysFloorPlansItems = ____worldState.AllRooms.Where(x => x.Definition._type == RoomDefinition.Type.AmbulanceBay).Select(x => x.FloorPlan).SelectMany(x => x.Items);
                if (ambulance != AmbulanceConfig.UniqueAmbulanceID.NUM_AMBULANCE_TYPES)
                {
                    roomCount = (baysFloorPlansItems?.Where(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance == ambulance)?.Count()).GetValueOrDefault();
                }
                else
                {
                    var ambulanceType = job.RequiredQualificationString().ToUpperInvariant() == "FLYING" ? AmbulanceConfig.Type.Air : AmbulanceConfig.Type.Road; //I have no idea how to find that info through any other way
                    roomCount = (baysFloorPlansItems?.Where(x => x?.AmbulanceConfig?.Instance?.AmbulanceType == ambulanceType)?.Count()).GetValueOrDefault();
                    ambulanceTooltipType = $"{ambulanceType} ";
                }

                component.text = staffCount.ToString() + "/" + roomCount.ToString();
                SetTextAsTooltip(i, string.Concat(new string[]
                {
                            job.GetJobAssignmentTooltipString(),
                            "\n\nStaff Assigned: ", staffCount.ToString(),
                            $"\n{ambulanceTooltipType}Ambulances Owned: ", roomCount.ToString()
                }));
            }

            void SetTextAsTooltip(int iconIndex, string tooltipText)
            {
                StaffJobIcon[] componentsInChildren = ____staffMenuSettings.JobsListContainer.gameObject.GetComponentsInChildren<StaffJobIcon>();
                if (componentsInChildren != null && iconIndex < componentsInChildren.Length)
                {
                    componentsInChildren[iconIndex].Tooltip.SetDataProvider(delegate (Tooltip tooltip)
                    {
                        tooltip.Text = tooltipText;
                    });
                }
            }
        }

        private static FloorPlan LogginMorePrecisely(CharacterManager ____characterManager, JobDescription job, List<Room> bays)
        {
            //Main.Logger.Log($"[AmbulancesJob] assignTooltip: '{job.GetJobAssignmentTooltipString()}', reqQual: '{job.RequiredQualificationString()}', local: '{job.ToLocalisedString()}', toString: '{job.ToString()}'.");

            //var bays = ____worldState.AllRooms.Where(x => x.Definition._type == RoomDefinition.Type.AmbulanceBay).ToList();
            //var bayFloorPlan = bays.FirstOrDefault().FloorPlan; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
            //                                                    //TODO: fix the above for many bays instead of a single one
            //var ambulanceType = job.RequiredQualificationString().ToUpperInvariant() == "FLYING" ? AmbulanceConfig.Type.Air : AmbulanceConfig.Type.Road;
            //var typeOwned = ambulanceType == AmbulanceConfig.Type.Air ? "Air" : "Land";
            //roomCount = (bayFloorPlan?.Items?.Where(x => x?.AmbulanceConfig?.Instance?.AmbulanceType == ambulanceType)?.Count()).GetValueOrDefault();

            //component.text = staffCount.ToString() + "/" + roomCount.ToString();
            //SetTextAsTooltip(i, string.Concat(new string[]
            //{
            //    job.GetJobAssignmentTooltipString(),
            //    "\n\nStaff Assigned: ", staffCount.ToString(),
            //    $"\n{typeOwned} Ambulances Owned: ", roomCount.ToString()
            //}));


            //var allAmbulances = ____worldState.AvailableRoomItems.Where(x => x.IsAnAmbulance);
            //var bays = ____worldState.AllRooms.Where(x => x.Definition._type == RoomDefinition.Type.AmbulanceBay).ToList();

            var bayFloorPlan = bays.FirstOrDefault().FloorPlan; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
            //var floorPlanPlacedItemsNum = bayFloorPlan?.GetNumPlacedItems();
            //var road = string.Join(" | ", bayFloorPlan?.Items?.Where(x => x?.AmbulanceConfig?.Instance?.AmbulanceType == AmbulanceConfig.Type.Road)?.Select(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance));
            //var air = string.Join(" | ", bayFloorPlan?.Items?.Where(x => x?.AmbulanceConfig?.Instance?.AmbulanceType == AmbulanceConfig.Type.Air)?.Select(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance));
            //var configName = string.Join(" | ", bayFloorPlan?.Items?.Select(x => x?.AmbulanceConfig?.name));
            var diagnosisBonuses = string.Join(" | ", bayFloorPlan?.Items?.Select(x => x?.AmbulanceConfig?.Instance?.DiagnosisBonus));
            //Main.Logger.Log($"[Ambulances] floorPlanPlacedItemsNum: '{floorPlanPlacedItemsNum}', road: '{road}', air: '{air}', configName: '{configName}', diagnosisBonuses: '{diagnosisBonuses}'.");
            var vehicles = string.Join(" | ", bayFloorPlan?.Items?.Select(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance));
            //Main.Logger.Log($"[Ambulances] vehicles: '{vehicles}', diagnosisBonuses: '{diagnosisBonuses}'.");

            try
            {
                var roadCount = bayFloorPlan.Items.Where(x => x.AmbulanceConfig?.Instance != null && x.AmbulanceConfig.Instance.AmbulanceType == AmbulanceConfig.Type.Road).Count();
                var airCount = bayFloorPlan.Items.Where(x => x.AmbulanceConfig?.Instance != null && x.AmbulanceConfig.Instance.AmbulanceType == AmbulanceConfig.Type.Air).Count();
            }
            catch (Exception ex)
            {
                var itemsNum = bayFloorPlan.Items.Count;
                var configNum = bayFloorPlan.Items.Where(x => x?.AmbulanceConfig != null).Count();
                var instanceNum = bayFloorPlan.Items.Where(x => x?.AmbulanceConfig?.Instance != null).Count();
                var itemsNames = string.Join(" | ", bayFloorPlan.Items?.Select(x => x?.Name));
                Main.Logger.Log($"[Ambulances] Counts done goofed up. ItemsNum: {itemsNum}, configNum: {configNum}, instanceNum: {instanceNum}, ItemsNames: '{itemsNames}'. EX: {ex}.");
            }

            var staffs = ____characterManager.StaffMembers.Where(s => !s.HasResigned() && !s.HasBeenFired() && job.IsSuitable(s) && !s.JobExclusions.Contains(job));
            //var charNames = string.Join(" | ", staffs?.Select(x => x?.CharacterName.GetCharacterName()));
            //var recomJobRoomTypes = string.Join(" | ", staffs?.SelectMany(x => x?.GetRecommendedJobRooms())); //TODO: Focus
            //var recomJobs = string.Join(" | ", staffs?.SelectMany(x => x?.GetRecommendedJobs()));
            //var names = string.Join(" | ", staffs?.Select(x => x?.Name));
            var namesWithTitles = string.Join(" | ", staffs?.Select(x => x?.NameWithTitle));
            var qualifications = string.Join(" | ", staffs?.SelectMany(x => x?.Qualifications.Select(y => y?.Definition?.ToString())));
            //var traits = string.Join(" | ", staffs?.Select(x => x?.Traits?.GetShortName(Character.Sex.Female)));
            //var traitsTooltips = string.Join(" | ", staffs?.Select(x => x.Traits?.GetTooltipText(Character.Sex.Female)));
            //Main.Logger.Log($"[AmbulancesStaff] charNames: '{charNames}', recomJobRoomTypes: '{recomJobRoomTypes}', recomJobs: '{recomJobs}'. names: '{names}', namesWithTitles: '{namesWithTitles}', qualifications: '{qualifications}', traits: '{traits}', traitsTooltips: '{traitsTooltips}'.");
            //Main.Logger.Log($"[AmbulancesStaff] namesWithTitles: '{namesWithTitles}', qualifications: '{qualifications}'.");
            return bayFloorPlan;
        }

        private static int LogginLogginLogginLogginLogginYeah(IEnumerable<IRoomItemDefinition> allAmbulances, List<Room> bays)
        {
            int roomCount;
            var a = bays.FirstOrDefault().Definition; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
                                                      //a.GetRequiredItems();
            var a1 = string.Join(" | ", a?.GetRequiredItems()?.Select(x => x?.GroupName));
            var a2 = string.Join(" | ", a?.GetRequiredItems()?.SelectMany(x => x?.Items?.Select(y => y?.name)));
            var a3 = string.Join(" | ", a?._singlePlaceItems);
            var b = bays.FirstOrDefault().FloorPlan; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
            var b1 = b?.GetNumPlacedItems();
            //b.Items;
            var b2 = string.Join(" | ", b?.Items?.Select(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance));
            //b.LandscapeItems;
            var b3 = b?.LandscapeItems?.FirstOrDefault()?.Name;
            var b4 = string.Join(" | ", b?.LandscapeItems?.Select(x => x?.AmbulanceConfig?.Instance?.UniqueAmbulance));
            var c = bays.FirstOrDefault().Jobs; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
            var c1 = string.Join(" | ", c?.Select(x => x?.AltStaffType()));
            var c2 = string.Join(" | ", c?.Select(x => x?.StaffType()));
            var d = bays.FirstOrDefault().StaffJobs; //you have the staff (you're in a for-each), you need the number of ambulances of that type (across all ambulance bays, but that's for later)
                                                     //d.FirstOrDefault()?.AlternativeDefinition.;
                                                     //d.FirstOrDefault()?.Definition;
            var d1 = string.Join(" | ", d?.SelectMany(x => x?.QualificationInstance?.AdditionalStaffTypes));
            var d2 = string.Join(" | ", d?.Select(x => x?.QualificationInstance?.GetTooltipText()));
            //____worldState.AllRooms.First().FloorPlan;
            roomCount = allAmbulances.Count();
            //Main.Logger.Log($"[Ambulances] definitionRequiredItemsGroupNames: '{a1}', definitionRequiredItemsItemsNames: '{a2}', definitionSinglePlaceItems: '{a3}'. floorPlanPlacedItemsNum: '{b1}', floorPlanItemsAmbuCfgAmbuTypes: '{b2}', floorPlanLandscapeItemsNames: '{b3}', floorPlanLandscapeItemsAmbuCfgAmbuTypes: '{b4}'.");
            //Main.Logger.Log($"[Ambulances] jobsAltStaffTypes: '{c1}', jobsStaffTypes: '{c2}'. staffJobsQualificationAdditStaffTypes: '{d1}', staffJobsQualificationTooltips: '{d2}'. RoomCount: {roomCount}.");
            var allNames = string.Join(" | ", allAmbulances.Select(x => x.GetName()));
            var te = allAmbulances.First().GetName();
            //var te2 = allAmbulances.First().GetLocalisedName();
            //var te3 = allAmbulances.First().ToLocalisedString();
            //var te4 = allAmbulances.First().ToString();
            var te6 = allAmbulances.First().ItemType;
            var te7 = allAmbulances.First().ItemSize;
            //Main.Logger.Log($"[Ambulances] AllNames: {allNames}, Name: '{te}', ItemType: '{te6}', ItemSize: '{te7}'. BaysCount: {bays?.Count ?? -1})");
            return roomCount;
        }
    }
}