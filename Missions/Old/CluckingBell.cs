using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE.Missions
{
    public class CluckingBell : IMission
    {
        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }

        public string MissionName => "Chicken Mafia";
        public string MissionDescription => "Raid the Clucking Bell Factory.";

        private readonly Tuple<Vector3, float>[] _positions = new[]
        {
            new Tuple<Vector3, float>(new Vector3(-67.09161f, 6244.202f, 31.07941f), 31.95825f),
            new Tuple<Vector3, float>(new Vector3(-65.03455f, 6236.851f, 31.09081f), 206.6257f),
            new Tuple<Vector3, float>(new Vector3(-84.15538f, 6236.393f, 31.09118f), 277.0873f),
            new Tuple<Vector3, float>(new Vector3(-74.85006f, 6223.218f, 31.08985f), 25.72063f),
            new Tuple<Vector3, float>(new Vector3(-90.38643f, 6238.07f, 31.08986f), 256.335f),
            new Tuple<Vector3, float>(new Vector3(-87.53513f, 6229.471f, 31.08989f), 304.6022f),
            new Tuple<Vector3, float>(new Vector3(-99.40579f, 6208.824f, 31.02503f), 228.079f),
            new Tuple<Vector3, float>(new Vector3(-97.15708f, 6207.212f, 31.02508f), 47.57387f),
            new Tuple<Vector3, float>(new Vector3(-111.178f, 6198.421f, 31.02523f), 315.2809f),
            new Tuple<Vector3, float>(new Vector3(-104.6471f, 6212.195f, 31.02502f), 249.1403f),
            new Tuple<Vector3, float>(new Vector3(-110.3287f, 6184.22f, 31.02493f), 129.279f),
            new Tuple<Vector3, float>(new Vector3(-120.5256f, 6177.801f, 31.01669f), 231.0237f),
            new Tuple<Vector3, float>(new Vector3(-118.355f, 6175.367f, 31.01656f), 47.2483f),
            new Tuple<Vector3, float>(new Vector3(-125.8067f, 6172.056f, 31.02496f), 225.6526f),
            new Tuple<Vector3, float>(new Vector3(-131.1034f, 6166.07f, 31.02499f), 151.1873f),
            new Tuple<Vector3, float>(new Vector3(-129.1222f, 6161.926f, 31.02494f), 75.05102f),
            new Tuple<Vector3, float>(new Vector3(-145.2515f, 6161.538f, 31.20619f), 187.33f),
            new Tuple<Vector3, float>(new Vector3(-157.3186f, 6161.767f, 31.20634f), 91.33714f),
            new Tuple<Vector3, float>(new Vector3(-160.1631f, 6162.477f, 31.20635f), 259.2208f),
            new Tuple<Vector3, float>(new Vector3(-168.6152f, 6175.055f, 31.20638f), 319.0457f),
            new Tuple<Vector3, float>(new Vector3(-169.5518f, 6156.986f, 31.20639f), 230.3875f),
            new Tuple<Vector3, float>(new Vector3(-163.9035f, 6142.698f, 31.20639f), 149.2658f),
            new Tuple<Vector3, float>(new Vector3(-157.3064f, 6142.378f, 32.33513f), 63.64299f),
        };

        public Vector3[] EntryPoints => new[]
        {
            new Vector3(-74.42314f, 6271.079f, 31.3722f),
            new Vector3(-133.1731f, 6133.387f, 31.43232f),
            new Vector3(-182.2727f, 6166.434f, 31.35259f),
        };

        public string[] EntryNames => new[]
        {
            "Front",
            "Back",
            "Back"
        };

        public Vector3 CameraPosition => new Vector3(-95.21424f, 6225.51f, 230.3394f);

        public CluckingBell()
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            EntryComplete = false;
        }

        public void OnStartMission()
        {
            Function.Call(Hash.REMOVE_IPL, "CS1_02_cf_offmission");
            Function.Call(Hash.REQUEST_IPL, "CS1_02_cf_onmission1");
            Function.Call(Hash.REQUEST_IPL, "CS1_02_cf_onmission2");
            Function.Call(Hash.REQUEST_IPL, "CS1_02_cf_onmission3");
            Function.Call(Hash.REQUEST_IPL, "CS1_02_cf_onmission4");

            for (int i = 0; i < _positions.Length; i++)
            {
                var tmp = new Enemy(_positions[i].Item1, _positions[i].Item2);
                
                tmp.Character.Task.GuardCurrentPosition();
                BadGuys.Add(tmp);
                CleanupBag.Add(tmp.Character);
            }
        }

        public void OnEndMission()
        {
            foreach (var badGuy in CleanupBag)
            {
                badGuy.Delete();
            }
            CleanupBag.Clear();
            Function.Call(Hash.REQUEST_IPL, "CS1_02_cf_offmission");
            Function.Call(Hash.REMOVE_IPL, "CS1_02_cf_onmission1");
            Function.Call(Hash.REMOVE_IPL, "CS1_02_cf_onmission2");
            Function.Call(Hash.REMOVE_IPL, "CS1_02_cf_onmission3");
            Function.Call(Hash.REMOVE_IPL, "CS1_02_cf_onmission4");
        }

        public void MissionTick()
        {
        }
        

        public void StartEntry(int entryPoint, Ped[] team)
        {
            float heading = 0f;
            switch (entryPoint)
            {
                case 0: // Front
                    var entryPos = new Vector3(-74.42314f, 6271.079f, 31.3722f);
                    heading = 216.744f;
                    for (int index = 0; index < team.Length; index++)
                    {
                        team[index].Position = entryPos;
                        team[index].Heading = heading;
                    }
                    var enforcermodel = new Model(VehicleHash.Riot);
                    enforcermodel.Request(10);
                    var enforcer = World.CreateVehicle(enforcermodel, new Vector3(-79.03268f, 6277.942f, 31.11718f), 32.41939f);
                    enforcer.SirenActive = true;
                    enforcer.OpenDoor(VehicleDoor.BackLeftDoor, false, true);
                    enforcer.OpenDoor(VehicleDoor.BackRightDoor, false, true);
                    CleanupBag.Add(enforcer);

                    var polmodel2 = new Model(VehicleHash.Police2);
                    polmodel2.Request(10);
                    var polcar = World.CreateVehicle(polmodel2, new Vector3(-83.34383f, 6274.731f, 30.95424f), 324.9319f);
                    var polcar3 = World.CreateVehicle(polmodel2, new Vector3(-76.00045f, 6281.345f, 31.03065f), 270.7873f);
                    polcar.SirenActive = true;
                    polcar3.SirenActive = true;
                    CleanupBag.Add(polcar);
                    CleanupBag.Add(polcar3);

                    EntryComplete = true;
                    break;
                case 1: // Back
                    var entryPosBack = new Vector3(-133.1731f, 6133.387f, 31.43232f);
                    heading = 43.69439f;

                    Ped lastpedback = new Ped(0);
                    for (int index = 0; index < team.Length; index++)
                    {
                        team[index].Position = entryPosBack;
                        if (lastpedback.Exists()) team[index].Position -= lastpedback.ForwardVector * 2;
                        team[index].Heading = heading;
                        lastpedback = team[index];
                    }
                    var enforcermodelb = new Model(VehicleHash.Riot);
                    enforcermodelb.Request(10);
                    var enforcerb = World.CreateVehicle(enforcermodelb, new Vector3(-127.5568f, 6144.007f, 31.25361f), 335.7838f);
                    enforcerb.SirenActive = true;
                    enforcerb.OpenDoor(VehicleDoor.BackLeftDoor, false, true);
                    enforcerb.OpenDoor(VehicleDoor.BackRightDoor, false, true);
                    CleanupBag.Add(enforcerb);

                    var polmodel = new Model(VehicleHash.Police2);
                    polmodel.Request(10);
                    var polcar1 = World.CreateVehicle(polmodel, new Vector3(-143.3609f, 6126.71f, 31.22926f), 252.053f);
                    polcar1.SirenActive = true;
                    CleanupBag.Add(polcar1);

                    EntryComplete = true;
                    break;
                case 2: // Back2
                    var entryPos2 = new Vector3(-182.2727f, 6166.434f, 31.35259f);
                    heading = 221.0143f;
                    for (int index = 0; index < team.Length; index++)
                    {
                        team[index].Position = entryPos2;
                        team[index].Heading = heading;
                    }
                    var enforcermodel2 = new Model(VehicleHash.Riot);
                    enforcermodel2.Request(10);
                    var enforcer2 = World.CreateVehicle(enforcermodel2, new Vector3(-187.0582f, 6171.55f, 31.13244f), 43.85089f);
                    enforcer2.SirenActive = true;
                    enforcer2.OpenDoor(VehicleDoor.BackLeftDoor, false, true);
                    enforcer2.OpenDoor(VehicleDoor.BackRightDoor, false, true);
                    CleanupBag.Add(enforcer2);

                    EntryComplete = true;
                    break;
            }
            
        }

        public void DrawBriefingMap()
        {
            
        }
    }
}