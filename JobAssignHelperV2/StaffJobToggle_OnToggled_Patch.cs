using Harmony12;
using System;
using System.Reflection;
using TH20;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(StaffJobToggle), "OnToggled")]
    internal static class StaffJobToggle_OnToggled_Patch
    {
        private static void Postfix(StaffJobToggle __instance, Staff ____staff)
        {
            if (!Main.enabled)
            {
                return;
            }
            try
            {
                StaffMenu staffMenu = ____staff.Level.HUD.FindMenu<StaffMenu>(true);
                if (staffMenu != null)
                {
                    MethodInfo method = staffMenu.GetType().GetMethod("CreateJobIcons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); //TODO: change reflection for instance call
                    if (method != null)
                    {
                        method.Invoke(staffMenu, new object[]
                        {
                                ____staff.Definition._type
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex.ToString() + ": " + ex.StackTrace.ToString());
            }
        }
    }
}