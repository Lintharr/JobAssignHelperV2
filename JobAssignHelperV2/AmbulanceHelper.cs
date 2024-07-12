using System.Collections.Generic;
using System.Linq;
using TH20;

namespace JobAssignHelperV2
{
    public static class AmbulanceHelper
    {
        private static Dictionary<string, AmbulanceConfig.UniqueAmbulanceID> _tooltipToIdDict = new Dictionary<string, AmbulanceConfig.UniqueAmbulanceID>()
            {
                { "Compliant Colin", AmbulanceConfig.UniqueAmbulanceID.Colin },
                { "Pantomobile", AmbulanceConfig.UniqueAmbulanceID.Clown },
                { "Big Healer", AmbulanceConfig.UniqueAmbulanceID.Monster },
                { "Relicopter", AmbulanceConfig.UniqueAmbulanceID.Davinci },
                { "Airloovator", AmbulanceConfig.UniqueAmbulanceID.Toilet },
                { "Feather Balloon", AmbulanceConfig.UniqueAmbulanceID.Duck },
            };

        private static Dictionary<AmbulanceConfig.UniqueAmbulanceID, string> _idToTooltipDict = _tooltipToIdDict.ToDictionary(x => x.Value, x => x.Key);

        public static AmbulanceConfig.UniqueAmbulanceID TranslateJobTooltipToAmbulance(string ambulanceTooltipString)
        {
            AmbulanceConfig.UniqueAmbulanceID ambulance = _tooltipToIdDict.GetDictValue(ambulanceTooltipString, AmbulanceConfig.UniqueAmbulanceID.NUM_AMBULANCE_TYPES);
            return ambulance;
        }

        public static string TranslateJobTooltipToAmbulance(AmbulanceConfig.UniqueAmbulanceID ambulance)
        {
            string ambulanceTooltipString = _idToTooltipDict.TryGetValueOrDefault(ambulance);
            return ambulanceTooltipString;
        }

        public static AmbulanceConfig.Type GetAmbulanceType(this JobAmbulanceDescription ambulanceJobDescription) => ambulanceJobDescription?.ItemDefinition?.BaseAmbulanceConfig?.Instance?.AmbulanceType ?? AmbulanceConfig.Type.All;
        //public static AmbulanceConfig.Type GetAmbulanceType(this Staff staffCharacter, int qualificationsNumber) => staffCharacter.Qualifications[qualificationsNumber].Definition.Modifiers.Where(m => (m as QualificationAmbulanceSpeedBoost) != null).Select((QualificationAmbulanceSpeedBoost m) => m.Type);
    }
}