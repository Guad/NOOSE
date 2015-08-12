using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE.Missions
{
    public class AircraftCarrier : IMission
    {
        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }

        public string MissionName => "Aircraft Carrier";
        public string MissionDescription => "Requires additional mods.";

        private readonly Tuple<Vector3, float>[] _positions = new[]
        {
            new Tuple<Vector3, float>(new Vector3(3075.76f, -4790.958f, 6.077335f), 296.7687f),
            new Tuple<Vector3, float>(new Vector3(3062.974f, -4797.502f, 6.072035f), 18.25719f),
            new Tuple<Vector3, float>(new Vector3(3062.373f, -4795.169f, 6.072035f), 199.162f),
            new Tuple<Vector3, float>(new Vector3(3075.799f, -4768.266f, 6.07725f), 255.1492f),
            new Tuple<Vector3, float>(new Vector3(3077.728f, -4767.808f, 6.07725f), 147.9376f),
            new Tuple<Vector3, float>(new Vector3(3069.083f, -4754.663f, 6.077251f), 252.276f),
            new Tuple<Vector3, float>(new Vector3(3094.657f, -4753.466f, 11.57459f), 166.7303f),
            new Tuple<Vector3, float>(new Vector3(3076.337f, -4736.371f, 6.077289f), 358.3229f),
            new Tuple<Vector3, float>(new Vector3(3065.644f, -4739.458f, 6.077291f), 147.4111f),
            new Tuple<Vector3, float>(new Vector3(3058.735f, -4748.673f, 10.74215f), 240.1439f),
            new Tuple<Vector3, float>(new Vector3(3056.915f, -4740.158f, 10.74208f), 347.4011f),
            new Tuple<Vector3, float>(new Vector3(3050.927f, -4718.804f, 10.74208f), 291.5142f),
            new Tuple<Vector3, float>(new Vector3(3043.368f, -4691.401f, 10.74208f), 254.3429f),
            new Tuple<Vector3, float>(new Vector3(3039.682f, -4685.943f, 6.077288f), 219.7414f),
            new Tuple<Vector3, float>(new Vector3(3040.878f, -4689.271f, 6.077288f), 351.0654f),
            new Tuple<Vector3, float>(new Vector3(3052.067f, -4697.13f, 6.077293f), 240.3306f),
            new Tuple<Vector3, float>(new Vector3(3073.331f, -4691.434f, 6.077292f), 71.66299f),
            new Tuple<Vector3, float>(new Vector3(3069.586f, -4700.419f, 6.077291f), 167.9892f),
            new Tuple<Vector3, float>(new Vector3(3052.45f, -4719.999f, 6.244971f), 232.6044f),
            new Tuple<Vector3, float>(new Vector3(3075.122f, -4724.314f, 6.077291f), 211.1678f),
            new Tuple<Vector3, float>(new Vector3(3045.38f, -4668.398f, 6.077312f), 149.3625f),
            new Tuple<Vector3, float>(new Vector3(3038.954f, -4655.213f, 6.077303f), 264.1053f),
            new Tuple<Vector3, float>(new Vector3(3028.127f, -4638.172f, 6.07731f), 329.9224f),
            new Tuple<Vector3, float>(new Vector3(3030.358f, -4634.693f, 6.07731f), 143.6715f),
            new Tuple<Vector3, float>(new Vector3(3042.884f, -4626.646f, 6.077309f), 196.5662f),
            new Tuple<Vector3, float>(new Vector3(3083.879f, -4704.698f, 10.72702f), 107.9183f),
            new Tuple<Vector3, float>(new Vector3(3096.668f, -4707.576f, 12.24404f), 54.48837f),
            new Tuple<Vector3, float>(new Vector3(3083.871f, -4709.903f, 15.24511f), 332.0058f),
            new Tuple<Vector3, float>(new Vector3(3090.932f, -4702.692f, 18.31511f), 110.8478f),
        };

        public Vector3[] EntryPoints => new[]
        {
            new Vector3(3098.805f, -4801.487f, 2.037071f),
            new Vector3(3049.975f, -4704.251f, 15.25679f),
            new Vector3(3079.936f, -4649.66f, 10.87023f),
        };

        public string[] EntryNames => new[]
        {
            "Boat",
            "Plane",
            "Helicopter"
        };

        public Vector3 CameraPosition => new Vector3(3052.136f, -4688.732f, 158.4777f);

        public AircraftCarrier()
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            EntryComplete = false;
        }

        public void OnStartMission()
        {
            Function.Call(Hash.REQUEST_IPL, "hei_carrier");
            Function.Call(Hash.REQUEST_IPL, "hei_carrier_DistantLights");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int1");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int2");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int3");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int4");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int5");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int6");
            Function.Call(Hash.REQUEST_IPL, "hei_carrier_LODLights");


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

            foreach (var entry in rappelUpdates)
            {
                entry?.Clean();
            }
            foreach (var rappelEntry in rappelStarted)
            {
                rappelEntry?.Clean();
            }
            planeEntry?.Clean();
            Function.Call(Hash.REMOVE_IPL, "hei_carrier");
            Function.Call(Hash.REMOVE_IPL, "hei_carrier_DistantLights");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int1");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int2");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int3");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int4");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int5");
            Function.Call(Hash.REMOVE_IPL, "hei_Carrier_int6");
            Function.Call(Hash.REMOVE_IPL, "hei_carrier_LODLights");
        }

        public void MissionTick()
        {
            planeEntry?.Tick();   
            for (int i = rappelUpdates.Count - 1; i >= 0; i--)
            {
                if (!rappelUpdates[i].EntryInProgress && !rappelStarted.Contains(rappelUpdates[i]))
                {
                    rappelUpdates[i].Start();
                    rappelStarted.Add(rappelUpdates[i]);
                }
                if (rappelUpdates[i].EntryInProgress)
                {
                    rappelUpdates[i].Tick();
                    break;
                }
                if (!rappelUpdates[i].EntryInProgress && rappelStarted.Contains(rappelUpdates[i]))
                {
                    rappelUpdates.RemoveAt(i);
                }
            }
            if (rappelStarted.Count == 2 && rappelStarted.All(f => !f.EntryInProgress) && !EntryComplete)
                EntryComplete = true;
        }

        private List<RappelEntry> rappelUpdates = new List<RappelEntry>();
        private List<RappelEntry> rappelStarted = new List<RappelEntry>();
        private PlaneEntry planeEntry;
        public void StartEntry(int entryPoint, Ped[] team)
        {
            float heading = 0f;
            switch (entryPoint)
            {
                case 0: // Water
                    var entryPos = new Vector3(3098.805f, -4801.487f, 2.037071f);
                    heading = 14.18145f;
                    Ped lastped = new Ped(0);
                    for (int index = 0; index < team.Length; index++)
                    {
                        team[index].Position = entryPos;
                        if (lastped.Exists()) team[index].Position -= lastped.ForwardVector*2;
                        team[index].Heading = heading;
                        lastped = team[index];
                    }
                    var enforcermodel = new Model(VehicleHash.Dinghy2);
                    enforcermodel.Request(10);
                    var enforcer = World.CreateVehicle(enforcermodel, new Vector3(3094.513f, -4802.502f, 0.1324978f), 15.62522f);
                    CleanupBag.Add(enforcer);
                    EntryComplete = true;
                    break;
                case 1: // Plane
                    planeEntry = new PlaneEntry(new Vector3(-1598.288f, -2992.517f, 14.87434f), 241.2218f, new Vector3(2541.847f, -3829.26f, 115.3642f),
                        new Vector3(3002.718f, -4602.471f, 15.26161f), new Vector3(3049.975f, -4704.251f, 15.25679f));
                    foreach (Ped ped in team)
                    {
                        planeEntry.AddPassenger(ped);
                    }
                    planeEntry.Start();
                    break;
                case 2: // Rappel
                    Vector3[] rappelpads = new[]
                    {
                        new Vector3(-1114.415f, -2884.573f, 14.04894f),
                        new Vector3(-1145.432f, -2864.025f, 14.05568f),
                    };
                    for (int i = 0; i < Convert.ToInt32(team.Length/2); i++)
                    {
                        var rappel = new RappelEntry(rappelpads[i], new Vector3(3079.936f, -4649.66f, 23.87023f));
                        var team2 = team.Reverse().ToList();
                        for (int d = 0 + (2*i); d < 2 + (2*i); d++)
                        {
                            Ped t = team2[d];
                            rappel.AddPassenger(t);
                        }
                        rappelUpdates.Add(rappel);
                    }
                    break;
            }
            
        }

        public void DrawBriefingMap()
        {
            Function.Call(Hash.REQUEST_IPL, "hei_carrier");
            Function.Call(Hash.REQUEST_IPL, "hei_carrier_DistantLights");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int1");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int2");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int3");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int4");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int5");
            Function.Call(Hash.REQUEST_IPL, "hei_Carrier_int6");
            Function.Call(Hash.REQUEST_IPL, "hei_carrier_LODLights");
            UI.ShowSubtitle("~r~Warning: This mission requires additional content.~w~\nChoose an entry point.", 10);
        }
    }
}