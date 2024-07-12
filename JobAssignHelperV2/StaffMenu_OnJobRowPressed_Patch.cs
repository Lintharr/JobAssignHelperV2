using Harmony12;
using System;
using System.Reflection;
using TH20;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(StaffMenu), "OnJobRowPressed")]
    internal static class StaffMenu_OnJobRowPressed_Patch
    {
        private static void Postfix(StaffMenu __instance, Staff staff)
        {
            if (!Main.enabled)
                return;

            try
            {
                MethodInfo method = __instance.GetType().GetMethod("CreateJobIcons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); //TODO: change reflection for instance call
                if (method != null)
                {
                    method.Invoke(__instance, new object[]
                    {
                            staff.Definition._type
                    });
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex.ToString() + ": " + ex.StackTrace.ToString());
            }
        }
    }
}