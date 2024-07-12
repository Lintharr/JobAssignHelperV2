using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TH20;
using UnityEngine;
using UnityModManagerNet;

namespace JobAssignHelperV2
{
    internal static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            try
            {
                _allRoomTypes = _allRoomTypes.OrderBy(x => settings.RoomDict.GetDictValue(x)?.SortingOrder);
            }
            catch (Exception ex)
            {
                Logger.Log("DaProblem:" + ex.ToString());
            }

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;

            return true;
        }

        private static IEnumerable<RoomDefinition.Type> _allRoomTypes = ((IEnumerable<RoomDefinition.Type>)Enum.GetValues(typeof(RoomDefinition.Type)));
        private static readonly RoomDefinition.Type _lastRoom = (RoomDefinition.Type)((IEnumerable<int>)_allRoomTypes).Max();

        private static IEnumerable<JobMaintenance.JobDescription> _allJobDescriptions = ((IEnumerable<JobMaintenance.JobDescription>)Enum.GetValues(typeof(JobMaintenance.JobDescription)));
        private static JobMaintenance.JobDescription _lastJobDescription = (JobMaintenance.JobDescription)((IEnumerable<int>)_allJobDescriptions).Max();

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Space(5);
            settings.UnassignJobsOnHire = GUILayout.Toggle(settings.UnassignJobsOnHire, " Unassign jobs when hiring.", GUILayout.Height(20));
            settings.AssignJobsOnTrainingComplete = GUILayout.Toggle(settings.AssignJobsOnTrainingComplete, " Assign jobs when training complete.", GUILayout.Height(20));
            GUILayout.Space(10);
            DrawRooms();
            GUILayout.Space(10);
            DrawJobs();
        }

        private static void DrawRooms()
        {
            GUILayout.Label("Jobs (Treatment/Diagnosis)", UnityModManager.UI.bold);
            GUILayout.BeginHorizontal();

            var columns = 3;
            var rows = Mathf.CeilToInt((_allRoomTypes.Count() - settings.BannedRooms.Length) / (float)columns);
            int i = 0;
            foreach (var room in _allRoomTypes/*.Except(settings.BannedRooms)*/)
            {
                if (settings.BannedRooms.Contains(room))
                {
                    if (room == _lastRoom)
                        GUILayout.EndVertical();
                    continue;
                }

                if (i == 0)
                    GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                var doSettingsContainCurrentRoom = settings.TreatmentRooms.Contains(room);
                var valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, "", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.TreatmentRooms.AddUnique(room);
                    else
                        settings.TreatmentRooms.Remove(room);
                }

                doSettingsContainCurrentRoom = settings.DiagnosisRooms.Contains(room);
                valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, $"   {settings.RoomDict.GetDictValue(room)?.Name ?? $"WTF {room}"}", GUILayout.ExpandWidth(false), GUILayout.Height(20));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.DiagnosisRooms.AddUnique(room);
                    else
                        settings.DiagnosisRooms.Remove(room);
                }

                //doSettingsContainCurrentRoom = settings.CustomerServiceRooms.Contains(room);
                //valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, $"   {settings.RoomDict.GetDictValue(room)?.Name ?? $"WTF {room}"}", GUILayout.ExpandWidth(false));
                //if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                //{
                //    if (valueOfThatRoomsToggle/* && !settings.CustomerServiceRooms.Contains(room)*/)
                //        settings.CustomerServiceRooms.AddUnique(room);
                //    else
                //        settings.CustomerServiceRooms.Remove(room);
                //}
                GUILayout.EndHorizontal();

                if (i == rows - 1)
                {
                    GUILayout.EndVertical();
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawRoomsVerbose()
        {
            GUILayout.Label("Jobs (Treatment/Diagnosis/CustomerService)", UnityModManager.UI.bold);
            GUILayout.BeginHorizontal();

            var columns = 3;
            var rows = Mathf.CeilToInt((_allRoomTypes.Count() - settings.BannedRooms.Length) / (float)columns);
            int i = 0;
            int startingVertical = 0, startHorizontal = 0, endingVertical = 0, endHorizontal = 0, why = 0;
            foreach (var room in _allRoomTypes)
            {
                if (settings.BannedRooms.Contains(room))
                {
                    //if (room == _lastRoom)
                    //{
                    //    why++;
                    //    endingVertical++;
                    //    GUILayout.EndVertical();

                    //}
                    //else
                    continue;
                }

                if (i == 0)
                {
                    startingVertical++;
                    GUILayout.BeginVertical();
                }

                startHorizontal++;
                GUILayout.BeginHorizontal();
                var doSettingsContainCurrentRoom = settings.TreatmentRooms.Contains(room);
                var valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, "", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.TreatmentRooms.Add(room);
                    else
                        settings.TreatmentRooms.Remove(room);
                }

                doSettingsContainCurrentRoom = settings.DiagnosisRooms.Contains(room);
                valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, "", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.DiagnosisRooms.Add(room);
                    else
                        settings.DiagnosisRooms.Remove(room);
                }

                doSettingsContainCurrentRoom = settings.CustomerServiceRooms.Contains(room);
                valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, $"   {settings.RoomDict.GetDictValue(room)?.Name ?? $"WTF {room}"}", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.CustomerServiceRooms.Add(room);
                    else
                        settings.CustomerServiceRooms.Remove(room);
                }
                GUILayout.EndHorizontal();
                endHorizontal++;

                if (i == rows - 1 || room == _lastRoom)
                {
                    GUILayout.EndVertical();
                    endingVertical++;
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            GUILayout.EndHorizontal();
            endHorizontal++;
            //GUILayout.EndHorizontal();
            //endHorizontal++;

            //Logger.Log($"{nameof(startingVertical)}: {startingVertical}; {nameof(startHorizontal)}: {startHorizontal}; {nameof(endingVertical)}: {endingVertical}; {nameof(endHorizontal)}: {endHorizontal}. Rows: {rows}, last room: {_lastRoom}, {nameof(why)}: {why}.");
        }

        //private static bool _firstRun = true;

        private static void DrawRoomsDebugWithCounter()
        {
            GUILayout.Label("Jobs (Treatment/Diagnosis/CustomerService)", UnityModManager.UI.bold);
            GUILayout.BeginHorizontal();

            var columns = 3;
            var rows = Mathf.CeilToInt((_allRoomTypes.Count() - settings.BannedRooms.Length) / (float)columns);
            int i = 0;
            int startingVertical = 0, startHorizontal = 0, endingVertical = 0, endHorizontal = 0, why = 0, counter = 0;
            //if (_firstRun) Logger.Log($"[Debug] Rows: {rows}, banned rooms: {settings.BannedRooms.Length}, total rooms: {_allRoomTypes.Count()}");
            foreach (var room in _allRoomTypes)
            {
                //if (_firstRun) Logger.Log($"[Debug] Counter: <{counter}>.");
                if (settings.BannedRooms.Contains(room))
                {
                    //if (_firstRun) Logger.Log($"[Debug] Banned room {room} at <{counter}>.");
                    if (room == _lastRoom)
                    {
                        why++;
                        endingVertical++;
                        GUILayout.EndVertical();
                        //if (_firstRun) Logger.Log($"[Debug] i:{i}, room:{room}, endVert at <{counter}>.");
                        continue;
                    }
                    else
                        continue;
                }

                if (i == 0)
                {
                    startingVertical++;
                    GUILayout.BeginVertical();
                    //if (_firstRun) Logger.Log($"[Debug] i==0, begun vertical at <{counter}>.");
                }

                startHorizontal++;
                GUILayout.BeginHorizontal();
                var doSettingsContainCurrentRoom = settings.TreatmentRooms.Contains(room);
                var valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, "", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.TreatmentRooms.Add(room);
                    else
                        settings.TreatmentRooms.Remove(room);
                }

                doSettingsContainCurrentRoom = settings.DiagnosisRooms.Contains(room);
                valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, "", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.DiagnosisRooms.Add(room);
                    else
                        settings.DiagnosisRooms.Remove(room);
                }

                doSettingsContainCurrentRoom = settings.CustomerServiceRooms.Contains(room);
                valueOfThatRoomsToggle = GUILayout.Toggle(doSettingsContainCurrentRoom, $"   {settings.RoomDict.GetDictValue(room)?.Name ?? $"WTF {room}"}", GUILayout.ExpandWidth(false));
                if (valueOfThatRoomsToggle != doSettingsContainCurrentRoom)
                {
                    if (valueOfThatRoomsToggle)
                        settings.CustomerServiceRooms.Add(room);
                    else
                        settings.CustomerServiceRooms.Remove(room);
                }
                GUILayout.EndHorizontal();
                endHorizontal++;

                if (i == rows - 1/* || room == _lastRoom*/)
                {
                    //if (_firstRun) Logger.Log($"[Debug] i:{i}, room:{room}, endVert at <{counter}>.");
                    GUILayout.EndVertical();
                    endingVertical++;
                    i = 0;
                }
                else
                {
                    //if (_firstRun) Logger.Log($"[Debug] {i} i++ {i + 1} at <{counter}>.");
                    i++;
                }

                counter++;
            }

            GUILayout.EndHorizontal();
            endHorizontal++;
            //GUILayout.EndHorizontal();
            //endHorizontal++;
            //if (_firstRun) Logger.Log($"[Debug] Ending first run at counter: <{counter}>.");

            //if (_firstRun)
            //    Logger.Log($"{nameof(startingVertical)}: {startingVertical}; {nameof(startHorizontal)}: {startHorizontal}; {nameof(endingVertical)}: {endingVertical}; {nameof(endHorizontal)}: {endHorizontal}. Rows: {rows}, last room: {_lastRoom}, {nameof(why)}: {why}.");
            //_firstRun = false;
        }

        private static void DrawJobs()
        {
            GUILayout.Label("Jobs for Janitor (Maintenance/Misc)", UnityModManager.UI.bold);

            //GUILayout.Label(new GUIContent("Jobs for Janitor (Maintenance/Misc)", "This is the tooltip"), UnityModManager.UI.bold);
            //GUI.Button(new Rect(10, 10, 100, 20), new GUIContent("Click me", "This is the button tooltip"));
            //GUI.Label(new Rect(0, 40, 100, 40), GUI.tooltip);

            //GUI.tooltip;

            //GUILayout.Box(Textures.Question, style ?? question, options);
            //if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            //{
            //    Instance.mTooltip = new GUIContent(str); (tooltip);
            //}

            ////BeginHorizontalTooltip(a);
            //GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
            ////EndHorizontalTooltip(a);
            //GUILayout.Box(Textures.Question, question);
            //if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            //{
            //    Instance.mTooltip = new GUIContent(a.Tooltip);
            //}

            foreach (var job in _allJobDescriptions)
            {
                if (settings.IgnoreJobs.Contains(job))
                    continue;

                GUILayout.BeginHorizontal();
                var value = settings.MaintenanceJobs.Exists(x => x == job);
                var result = GUILayout.Toggle(value, "", GUILayout.ExpandWidth(false));
                if (result != value)
                {
                    if (result)
                        settings.MaintenanceJobs.Add(job);
                    else
                        settings.MaintenanceJobs.Remove(job);
                }

                value = settings.MiscJobs.Exists(x => x == job);
                result = GUILayout.Toggle(value, $"   {job}", GUILayout.ExpandWidth(false));
                if (result != value)
                {
                    if (result)
                        settings.MiscJobs.Add(job);
                    else
                        settings.MiscJobs.Remove(job);
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.TreatmentRooms = settings.TreatmentRooms.Distinct().ToList();
            settings.DiagnosisRooms = settings.DiagnosisRooms.Distinct().ToList();
            settings.CustomerServiceRooms = settings.CustomerServiceRooms.Distinct().ToList();
            settings.MaintenanceJobs = settings.MaintenanceJobs.Distinct().ToList();
            settings.MiscJobs = settings.MiscJobs.Distinct().ToList();
            settings.Save(modEntry);
        }
    }
}