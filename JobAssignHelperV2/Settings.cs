using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TH20;
using UnityModManagerNet;

namespace JobAssignHelperV2
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool UnassignJobsOnHire = false;
        public bool AssignJobsOnTrainingComplete = false;

        public List<RoomDefinition.Type> TreatmentRooms = new List<RoomDefinition.Type>
        {
            RoomDefinition.Type.DNAAnalysis, RoomDefinition.Type.LightHeaded, 
            RoomDefinition.Type.ElectricShockClinic, RoomDefinition.Type.PandemicClinic, RoomDefinition.Type.ClinicCubism, RoomDefinition.Type.TurtleHeadClinic, 
            RoomDefinition.Type.EightBitClinic, RoomDefinition.Type.EightBallClinic, RoomDefinition.Type.AstroClinic, RoomDefinition.Type.TechClinic, 
            RoomDefinition.Type.ToySoldierClinic, RoomDefinition.Type.SnowballedClinic, RoomDefinition.Type.HivesClinic, RoomDefinition.Type.UnderTheWeatherClinic, 
            RoomDefinition.Type.Pharmacy, RoomDefinition.Type.InjectionRoom, RoomDefinition.Type.Chromatherapy, 
            RoomDefinition.Type.ClownClinic, RoomDefinition.Type.AnimalMagnetismClinic, RoomDefinition.Type.MummyClinic, 
            RoomDefinition.Type.FrankensteinClinic, RoomDefinition.Type.DogClinic, RoomDefinition.Type.RobotMonsterClinic, RoomDefinition.Type.BlankLooksClinic, 
            RoomDefinition.Type.ExplorerClinic, RoomDefinition.Type.CardboardClinic, RoomDefinition.Type.FrogClinic, RoomDefinition.Type.PinocchioClinic, 
            RoomDefinition.Type.ScarecrowClinic, RoomDefinition.Type.PlantWardClinic, RoomDefinition.Type.StuntmanClinic, RoomDefinition.Type.MudPersonClinic
        };

        public List<RoomDefinition.Type> DiagnosisRooms = new List<RoomDefinition.Type>
        {
            RoomDefinition.Type.GeneralDiagnosis, RoomDefinition.Type.Cardiography, RoomDefinition.Type.FluidAnalysis,
            RoomDefinition.Type.XRay, RoomDefinition.Type.MRIScanner,
        };

        public List<RoomDefinition.Type> CustomerServiceRooms = new List<RoomDefinition.Type>
        {
            //RoomDefinition.Type.Reception, RoomDefinition.Type.Cafe,
        };

        [XmlIgnore]
        public readonly RoomDefinition.Type[] BannedRooms = new RoomDefinition.Type[]
        {
            RoomDefinition.Type.Invalid,
            RoomDefinition.Type.Hospital,
            RoomDefinition.Type.HospitalUnbuilt,
            RoomDefinition.Type.StaffRoom,
            RoomDefinition.Type.Toilets,
            RoomDefinition.Type.Training,
            RoomDefinition.Type.ClinicVI10, //wtf is that?
            RoomDefinition.Type.NoDataRoom,
            RoomDefinition.Type.AmbulanceBay,
            RoomDefinition.Type.Reception,
            RoomDefinition.Type.Marketing,
            RoomDefinition.Type.TimeTunnel,
            RoomDefinition.Type.Cafe,
        };

        public List<JobMaintenance.JobDescription> MiscJobs = new List<JobMaintenance.JobDescription>
        {
            JobMaintenance.JobDescription.BrokenMachine,
            JobMaintenance.JobDescription.BlockedToilet,
            JobMaintenance.JobDescription.OutOfStock,
            JobMaintenance.JobDescription.WiltedPlant,
            JobMaintenance.JobDescription.Litter,
            JobMaintenance.JobDescription.MedicalWaste,
            JobMaintenance.JobDescription.Ghost,
        };

        public List<JobMaintenance.JobDescription> MaintenanceJobs = new List<JobMaintenance.JobDescription>
        {
            JobMaintenance.JobDescription.BrokenMachine,
            JobMaintenance.JobDescription.BlockedToilet,
            JobMaintenance.JobDescription.OutOfStock,
            JobMaintenance.JobDescription.WiltedPlant,
            JobMaintenance.JobDescription.Litter,
            JobMaintenance.JobDescription.MedicalWaste,
        };

        [XmlIgnore]
        public readonly JobMaintenance.JobDescription[] GhostJobs = new JobMaintenance.JobDescription[]
        {
            JobMaintenance.JobDescription.Ghost
        };

        [XmlIgnore]
        public readonly JobMaintenance.JobDescription[] UpgradeJobs = new JobMaintenance.JobDescription[]
        {
            JobMaintenance.JobDescription.Max
        };

        [XmlIgnore]
        public readonly JobMaintenance.JobDescription[] IgnoreJobs = new JobMaintenance.JobDescription[]
        {
            JobMaintenance.JobDescription.None, /*JobMaintenance.JobDescription.Ghost, JobMaintenance.JobDescription.Max*/
        };

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }


        [XmlIgnore]
        public static readonly RoomInfo[] RoomInfos = new RoomInfo[]
        {
            new RoomInfo(RoomDefinition.Type.GPOffice, "GP's Office", 1, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.XRay, "X-Ray", 2, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.MRIScanner, "MEGA Scan", 3, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.Psychiatry, "Psychiatry", 4, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.OperatingTheater, "Surgery", 5, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.DNAAnalysis, "DNA Lab", 6, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.LightHeaded, "De-Lux Clinic", 7, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.ElectricShockClinic, "Shock Clinic", 8, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.PandemicClinic, "Pans Lab", 9, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.ClinicCubism, "Recurvery Room", 10, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.TurtleHeadClinic, "Head Office", 11, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.EightBitClinic, "Resolution Lab", 12, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.EightBallClinic, "Correcting Pool", 13, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.AstroClinic, "Personification", 14, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.TechClinic, "Tech Support", 15, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.ToySoldierClinic, "War Room", 16, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.SnowballedClinic, "Powder Room", 17, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.HivesClinic, "Wax Works", 18, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.UnderTheWeatherClinic, "Cloud Computing", 19, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.GeneralDiagnosis, "General Diagnosis", 20, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.Cardiography, "Cardiology", 21, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.FluidAnalysis, "Fluid Analysis", 22, RoomInfo.RoomPurpose.Diagnosis, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.Pharmacy, "Pharmacy", 23, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.Ward, "Ward", 24, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.InjectionRoom, "Injection Room", 25, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.Chromatherapy, "Chromatherapy", 26, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.ClownClinic, "Clown Clinic", 27, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.AnimalMagnetismClinic, "Pest Control", 28, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.FractureWard, "Fracture Ward", 29, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.MummyClinic, "Cryptology", 30, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.FrankensteinClinic, "Reanimation", 31, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.DogClinic, "Doghouse", 32, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.RobotMonsterClinic, "Urban Mythology", 33, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.BlankLooksClinic, "Indentification", 34, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.ExplorerClinic, "Escape Room", 35, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.CardboardClinic, "Self-Assembly", 36, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.FrogClinic, "Toad Hall", 37, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.PinocchioClinic, "Woodwork", 38, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.ScarecrowClinic, "Farmacology", 39, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.PlantWardClinic, "Herb Garden", 40, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.StuntmanClinic, "Danger Zone", 41, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.MudPersonClinic, "Wash Pit", 42, RoomInfo.RoomPurpose.Treatment, RoomInfo.RoomPersonnel.Nurse),
            new RoomInfo(RoomDefinition.Type.Research, "Research", 43, RoomInfo.RoomPurpose.Other, RoomInfo.RoomPersonnel.Doctor),
            new RoomInfo(RoomDefinition.Type.Reception, "Reception (room)", 44, RoomInfo.RoomPurpose.Other, RoomInfo.RoomPersonnel.Assistant),
            new RoomInfo(RoomDefinition.Type.Marketing, "Marketing", 45, RoomInfo.RoomPurpose.Other, RoomInfo.RoomPersonnel.Assistant),
            new RoomInfo(RoomDefinition.Type.TimeTunnel, "Speed Dating", 46, RoomInfo.RoomPurpose.Other, RoomInfo.RoomPersonnel.Assistant),
            new RoomInfo(RoomDefinition.Type.Cafe, "Café", 47, RoomInfo.RoomPurpose.Other, RoomInfo.RoomPersonnel.Assistant),
        };

        [XmlIgnore]
        private Dictionary<RoomDefinition.Type, RoomInfo> _roomDict = null;

        [XmlIgnore]
        public Dictionary<RoomDefinition.Type, RoomInfo> RoomDict => _roomDict ?? (_roomDict = RoomInfos.ToDictionary(x => x.Type, x => x));

        public class RoomInfo
        {
            public RoomDefinition.Type Type { get; set; }
            public string Name { get; set; }
            public int SortingOrder { get; set; }
            public RoomPurpose Purpose { get; set; }
            public RoomPersonnel Personnel { get; set; }

            public RoomInfo() { SortingOrder = 100; }

            public RoomInfo(RoomDefinition.Type type, string name, int sortingOrder, RoomPurpose purpose, RoomPersonnel personnel)
            {
                Type = type;
                Name = name;
                SortingOrder = sortingOrder;
                Purpose = purpose;
                Personnel = personnel;
            }

            public enum RoomPurpose
            {
                Diagnosis,
                Treatment,
                Other
            }

            public enum RoomPersonnel
            {
                Doctor,
                Nurse,
                Assistant
            }
        }
    }
}