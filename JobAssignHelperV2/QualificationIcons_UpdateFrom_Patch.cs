using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using TH20;

namespace JobAssignHelperV2
{
    [HarmonyPatch(typeof(QualificationIcons), "UpdateFrom")]
    internal static class QualificationIcons_UpdateFrom_Patch
    {
        private static TooltipSpawner[] _tooltipSpawners;

        private static void Postfix(QualificationIcons __instance, List<QualificationSlot> qualifications, int maxQualifications, List<Staff> allStaff, TooltipSpawner[] ____tooltipSpawner)
        {
            if (!Main.enabled || StaffMenu_Refresh_Patch.ViewMode != StaffMenu.ViewModes.ViewModeJobAssignment)
                return;

            try
            {
                _tooltipSpawners = ____tooltipSpawner;
                foreach (var tooltipSpawner in _tooltipSpawners)
                {
                    var dataProvider = Traverse.Create(tooltipSpawner).Field("_dataProvider").GetValue<Action<Tooltip>>();
                    tooltipSpawner.SetDataProvider(dataProvider + AddMouseClickDescriptions);
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Log($"[QualIcons] Exception occured: {ex}.");
            }

            void AddMouseClickDescriptions(Tooltip tooltip)
            {
                var qualTooltip = tooltip as TooltipQualification;
                qualTooltip.Info.text += "<color=#707070ff><size=11>" +
                    "\n\n<b>Press LMB</b> to toggle <i>on</i> only jobs matching this qualification." +
                    "\n\n<b>Press Shift+LMB</b> to toggle <i>on</i> all jobs matching every qualification this staff has." +
                    "\n\n<b>Press Ctrl+LMB</b> to toggle <i>on/off</i> all jobs matching this qualification, without overwriting existing jobs." +
                    "\n\n<b>Press MMB</b> to toggle <i>on/off</i> all jobs this staff can have." +
                    "</size></color>";
            }
        }

        private static void tests()
        {
            //tooltip.SetDataProvider(x => new Tooltip());

            //var p = StaffMenu.GetType().GetField("_staffMenuRowProvider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(StaffMenu);
            //if (p is StaffMenuRowProvider provider)
            //	provider.RefreshRowJobs();

            //MethodInfo method = StaffMenu.GetType().GetMethod("CreateJobIcons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            //if (method != null)
            //	method.Invoke(StaffMenu, new object[] { StaffCharacter.Definition._type });

            //var icons = Traverse.Create(___QualificationIcons).Field("_qualificationImages").GetValue<Image[]>();
            //var jobToggles = Traverse.Create(GetComponentInParent<StaffMenuJobAssignRow>()).Field("_jobToggles").GetValue<List<StaffJobToggle>>();
            //var tryout = _tooltipSpawners.Where(x => x?.enabled ?? false);
            //if (tryout.Any()) //so this is true for every qualification(Icon) a character has, so (qutie possibly) basically always true except for staff without any qual
            //	Main.Logger.Log($"[tryout] {tryout.Select(x => x?.TooltipText).ListThis("WIN")}.");

            //var test = GetField<TooltipQualification>(tooltipSpawner, "_tooltip");
            //var a = tooltipSpawner.enabled; //always true
            //var b = tooltipSpawner.gameObject != null; //always true
            //var c = tooltipSpawner.isActiveAndEnabled; //true only when staff has that qualification(Icon)
            //var d = tooltipSpawner.Prefab != null; //always true
            //var e = tooltipSpawner.TooltipText != null; //always true
            //var e = !string.IsNullOrWhiteSpace(tooltipSpawner.TooltipText); //always false
            //Main.Logger.Log($"[QualIcons] enabled: '{a}', gameObject: '{b}', isActiveAndEnabled: '{c}', Prefab: '{d}', TooltipText: '{e}'. test: {test?.Info?.text}.");
            //if (!string.IsNullOrWhiteSpace(test?.Info?.text))
            //               {
            //	test.Info.text += "\nPress LMB to toggle on all jobs matching this qualification.";
            //	Main.Logger.Log($"[QualIcons!] newTest: {test?.Info?.text}.");
            //}

            //MethodInfo method = tooltip.GetType().GetMethod("_dataProvider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            //var fieldDP = tooltipSpawner.GetType().GetField("_dataProvider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(tooltipSpawner);
            //var traverseDP = Traverse.Create(tooltipSpawner).Field("_dataProvider").GetValue<Action<Tooltip>>();

            //var fieldTQ = tooltipSpawner.GetType().GetField("_tooltip", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(tooltipSpawner) as TooltipQualification;
            //var traverseTQ = Traverse.Create(tooltipSpawner).Field("_tooltip").GetValue<TooltipQualification>();
            //Main.Logger.Log($"[QualIcons] fieldDP isnull: '{fieldDP == null}', fieldDP as T: '{fieldDP == null && fieldDP as Tooltip == null}', traverseDP isnull: '{traverseDP == null}'. " +
            //	$"fieldT isnull: '{fieldT == null}', traverseT isnull: '{traverseT == null}', fieldT text: '{fieldT?.Text}', traverseT text: '{traverseT?.Text}'. " +
            //	$"fieldTQ isnull: '{fieldTQ == null}', traverseTQ isnull: '{traverseTQ == null}', fieldTQ text: '{fieldTQ?.Text}', traverseTQ text: '{traverseTQ?.Text}'. " +
            //	$"fieldTQ Desctext: '{fieldTQ?.Description?.text}', traverseTQ Desctext: '{traverseTQ?.Description?.text}', fieldTQ Infotext: '{fieldTQ?.Info?.text}', traverseTQ Infotext: '{traverseTQ?.Info?.text}'.");

            //if (method != null)
            //	method.Invoke(tooltip, new object[] { delegate(Tooltip tooltipDelegate) { } });
            //Main.Logger.Log($"[QualIcons] TooltipLocText: '{tooltip.TooltipLocText}', TooltipTerm: '{tooltip.TooltipTerm}', TooltipText: '{tooltip.TooltipText}', gameObject: '{tooltip?.gameObject?.ToString()}'.");

            T GetField<T>(object objectToTraverse, string nameOfFieldToGet)
                where T : class
            {
                var fieldTQ = objectToTraverse.GetType().GetField(nameOfFieldToGet, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(objectToTraverse) as T;
                return fieldTQ;
            }
        }

        //private void SetActive(GameObject gameObject, bool active)
        //{
        //	if (_layoutSafe)
        //	{
        //		gameObject.transform.localScale = (active ? Vector3.one : Vector3.zero);
        //	}
        //	else
        //	{
        //		GameObjectUtils.SetActive(gameObject, active);
        //	}
        //}

        //public void UpdateFrom(List<QualificationSlot> qualifications, int maxQualifications, List<Staff> allStaff)
        //{
        //	for (int i = 0; i < 5; i++)
        //	{
        //		Image image = _qualificationSlots[i];
        //		Image image2 = (_qualificationBack != null) ? _qualificationBack[i] : null;
        //		Image image3 = _qualificationImages[i];
        //		TooltipSpawner tooltipSpawner = _tooltipSpawner[i];
        //		QualificationSlot qualification = (i < qualifications.Count) ? qualifications[i] : null;
        //		if (i >= maxQualifications)
        //		{
        //			image.color = _slotUnavailableColor;
        //			SetActive(image3.gameObject, active: false);
        //			if (image2 != null)
        //			{
        //				SetActive(image2.gameObject, active: false);
        //			}
        //			if (tooltipSpawner != null)
        //			{
        //				tooltipSpawner.enabled = false;
        //			}
        //		}
        //		else if (i >= qualifications.Count)
        //		{
        //			image.color = _slotEmptyColor;
        //			SetActive(image3.gameObject, active: false);
        //			if (image2 != null)
        //			{
        //				SetActive(image2.gameObject, active: false);
        //			}
        //			if (tooltipSpawner != null)
        //			{
        //				tooltipSpawner.enabled = true;
        //				tooltipSpawner.SetDataProvider(delegate (Tooltip tooltip)
        //				{
        //					TooltipQualification tooltipQualification2 = tooltip as TooltipQualification;
        //					tooltip.Text = ScriptLocalization.Tooltip.Qualification_ReadyForTraining_CS;
        //					if (tooltipQualification2 != null)
        //					{
        //						GameObjectUtils.SetActive(tooltipQualification2.Info.gameObject, isActive: false);
        //						GameObjectUtils.SetActive(tooltipQualification2.Description.gameObject, isActive: false);
        //						GameObjectUtils.SetActive(tooltipQualification2.ProgressBar.gameObject, isActive: false);
        //					}
        //				});
        //			}
        //		}
        //		else if (qualification != null)
        //		{
        //			image.color = _slotQualificationCompleteColor;
        //			if (image2 != null)
        //			{
        //				image2.color = _slotQualificationIncompleteColor;
        //				GameObjectUtils.SetImageSprite(image2, qualification.Definition.Icon);
        //				SetActive(image2.gameObject, active: true);
        //			}
        //			image3.fillAmount = qualification.FractionComplete;
        //			GameObjectUtils.SetImageSprite(image3, qualification.Definition.Icon);
        //			SetActive(image3.gameObject, active: true);
        //			if (tooltipSpawner != null)
        //			{
        //				tooltipSpawner.enabled = true;
        //				tooltipSpawner.SetDataProvider(delegate (Tooltip tooltip)
        //				{
        //					TooltipQualification tooltipQualification = tooltip as TooltipQualification;
        //					if (tooltipQualification != null)
        //					{
        //						int num = 0;
        //						foreach (Staff item in allStaff)
        //						{
        //							if (item.HasCompletedQualification(qualification.Definition))
        //							{
        //								num++;
        //							}
        //						}
        //						string qualification_StaffCount_CS = ScriptLocalization.Tooltip.Qualification_StaffCount_CS;
        //						qualification_StaffCount_CS = qualification_StaffCount_CS.Replace("{[COUNT]}", num.ToString());
        //						tooltipQualification.Text = qualification.Definition.NameLocalised.Translation;
        //						tooltipQualification.Description.text = qualification.Definition.GetTooltipText();
        //						tooltipQualification.Info.text = qualification_StaffCount_CS;
        //						GameObjectUtils.SetActive(tooltipQualification.ProgressBar.gameObject, !qualification.IsComplete());
        //						tooltipQualification.ProgressBar.Progress = qualification.FractionComplete;
        //					}
        //				});
        //			}
        //		}
        //	}
        //}
    }
}