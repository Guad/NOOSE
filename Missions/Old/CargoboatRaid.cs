using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE.Missions
{
    public class CargoboatRaid : IMission
    {
        public string MissionName => "Cargo Raid";
        public string MissionDescription => "Assault the cargoship.";

        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }
        
        private readonly Tuple<Vector3, float>[] _positions = new[]
        {
            new Tuple<Vector3, float>(new Vector3(-129.72f, -2366.27f, 9.32f), 0.6f),
            new Tuple<Vector3, float>(new Vector3(-123.72f, -2366.27f, 9.32f), 181f),
            new Tuple<Vector3, float>(new Vector3(-147.54f, -2365.27f, 9.32f), 262f),
            new Tuple<Vector3, float>(new Vector3(-163.66f, -2359.27f, 9.32f), 7f),
            new Tuple<Vector3, float>(new Vector3(-163.66f, -2372.27f, 9.32f), 177f),
            new Tuple<Vector3, float>(new Vector3(-182.41f, -2371.16f, 9.32f), 192f),
            new Tuple<Vector3, float>(new Vector3(-181.52f, -2358.9f, 9.32f), 2f),
            new Tuple<Vector3, float>(new Vector3(-193.31f, -2373.94f, 9.32f), 246f),
            new Tuple<Vector3, float>(new Vector3(-193.31f, -2356.93f, 9.32f), 290f),
            new Tuple<Vector3, float>(new Vector3(-210.28f, -2357.99f, 9.32f), 195f),
            new Tuple<Vector3, float>(new Vector3(-209.28f, -2372.84f, 9.32f), 356.76f),
            new Tuple<Vector3, float>(new Vector3(-222.63f, -2376.762f, 13.33f), 246f),
            new Tuple<Vector3, float>(new Vector3(-211.39f, -2367.043f, 17.332f), 257.83f),
            new Tuple<Vector3, float>(new Vector3(-221.16f, -2354.905f, 13.33f), 296f),
        };

        public Vector3[] EntryPoints => new[]
        {
            new Vector3(-92.5652f, -2362.343f, 13.9f),
            new Vector3(-249.4635f, -2366.84f, 9.319f)
        };

        public string[] EntryNames => new[]
        {
            "Helicopter",
            "Boat",
        };

        public Vector3 CameraPosition => new Vector3(-154.1309f, -2358.141f, 123.7619f);

        public CargoboatRaid()
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            EntryComplete = false;
        }

        public void OnStartMission()
        {
            if(Function.Call<bool>(Hash.IS_IPL_ACTIVE, "sunkcargoship"))
                Function.Call(Hash.REMOVE_IPL, "sunkcargoship");
            Function.Call(Hash.REQUEST_IPL, "cargoship");
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
            Function.Call(Hash.REMOVE_IPL, "cargoship");
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
        }

        public void MissionTick()
        {
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
        
        public void StartEntry(int entryPoint, Ped[] team)
        {
            switch (entryPoint)
            {
                case 0: // Rappel
                    Vector3[] helipads = new[]
                    {
                        new Vector3(311.313f, -1466f, 46.9f),
                        new Vector3(299.75f, -1453f, 46.9f),
                    };
                    for (int i = 0; i < Convert.ToInt32(team.Length/2); i++)
                    {
                        var rappel = new RappelEntry(helipads[i], new Vector3(-92.5652f, -2362.343f, 31.9f));
                        var team2 = team.Reverse().ToList();
                        for (int d = 0 + (2*i); d < 2 + (2*i); d++)
                        {
                            Ped t = team2[d];
                            rappel.AddPassenger(t);
                        }
                        rappelUpdates.Add(rappel);
                    }
                    break;
                case 1: //boat
                    BadGuys[BadGuys.Count-1].Character.Delete();
                    BadGuys.RemoveAt(BadGuys.Count-1);

                    var boatModel = new Model(VehicleHash.Dinghy2);
                    int counter = 0;
                    do
                    {
                        boatModel.Request();
                        Script.Yield();
                        counter++;
                    } while (!boatModel.IsLoaded && counter < 2000);

                    var boat = World.CreateVehicle(boatModel, new Vector3(-253.971f, -2367.186f, -0.89f), 185.44f);
                    boat.MarkAsNoLongerNeeded();
                    CleanupBag.Add(boat);

                    foreach (Ped t in team)
                    {
                        t.Position = new Vector3(-249.4635f, -2366.84f, 9.319f);
                        t.Heading = -180f;
                    }
                    EntryComplete = true;
                    break;

            }
        }

        public void DrawBriefingMap()
        {
            if (Function.Call<bool>(Hash.IS_IPL_ACTIVE, "sunkcargoship"))
                Function.Call(Hash.REMOVE_IPL, "sunkcargoship");
            Function.Call(Hash.REQUEST_IPL, "cargoship");
        }
    }
}