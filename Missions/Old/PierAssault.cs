using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE.Missions
{
    public class PierAssault : IMission
    {
        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }

        public string MissionName => "Pier Assault";
        public string MissionDescription => "Assault the pier.";

        private readonly Tuple<Vector3, float>[] _positions = new[]
        {
            new Tuple<Vector3, float>(new Vector3(-1814.42f, -1212.90f, 13.017f), 0f),
            new Tuple<Vector3, float>(new Vector3(-1803.76f, -1211.67f, 19.16f), 0f),
            new Tuple<Vector3, float>(new Vector3(-1814.18f, -1206.72f, 19.16f), 0f),
            new Tuple<Vector3, float>(new Vector3(-1823.8f, -1204.67f, 13.01f), 0f),
            new Tuple<Vector3, float>(new Vector3(-1840.34f, -1206.64f, 14.16f), 0f),
            new Tuple<Vector3, float>(new Vector3(-1862.76f, -1207.67f, 13.16f), 228f),
        };

        public Vector3[] EntryPoints => new[]
        {
            new Vector3(-1793.309f, -1196.424f, 12.017f),
            new Vector3(-1800.013f, -1227.414f, 1.66f),
            new Vector3(-1837.12f, -1254.43f, 6.18f),
        };

        public string[] EntryNames => new[]
        {
            "Front",
            "Boat",
            "Helicopter"
        };

        public Vector3 CameraPosition => new Vector3(-1819.837f, -1208.235f, 115.4621f);

        public PierAssault()
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            EntryComplete = false;
        }

        public void OnStartMission()
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                var tmp = new Enemy(_positions[i].Item1, _positions[i].Item2);
                
                tmp.Character.Task.GuardCurrentPosition();
                BadGuys.Add(tmp);
                CleanupBag.Add(tmp.Character);
            }
            var polmodel = new Model(VehicleHash.Police2);
            polmodel.Request(10);

            var polcar1 = World.CreateVehicle(polmodel, new Vector3(-1769f, -1157.036f, 12.633f), 68.7f);
            polcar1.SirenActive = true;
            var polcar2 = World.CreateVehicle(polmodel, new Vector3(-1774.87f, -1153.445f, 12.633f), 202.47f);
            polcar2.SirenActive = true;
            CleanupBag.Add(polcar1);
            CleanupBag.Add(polcar2);
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
            float heading = 0f;
            switch (entryPoint)
            {
                case 0: // Front
                    var entryPos = new Vector3(-1793.309f, -1196.424f, 12.017f);
                    heading = 51.61f;
                    Ped lastped = new Ped(0);
                    for (int index = 0; index < team.Length; index++)
                    {
                        team[index].Position = entryPos;
                        if (lastped.Exists()) team[index].Position -= lastped.ForwardVector*2;
                        team[index].Heading = heading;
                        lastped = team[index];
                    }
                    var enforcermodel = new Model(VehicleHash.Riot);
                    enforcermodel.Request(10);
                    var enforcer = World.CreateVehicle(enforcermodel, new Vector3(-1794.152f, -1188.071f, 12.74f), 9.35f);
                    enforcer.SirenActive = true;
                    enforcer.OpenDoor(VehicleDoor.BackLeftDoor, false, true);
                    enforcer.OpenDoor(VehicleDoor.BackRightDoor, false, true);
                    CleanupBag.Add(enforcer);
                    EntryComplete = true;
                    break;
                case 1: // Water
                    var model = new Model(VehicleHash.Predator);
                    model.Request(10);
                    var car = World.CreateVehicle(model, new Vector3(-1799.512f, -1230.37f, 0.568f), 139.47f
                        );
                    CleanupBag.Add(car);

                    entryPos = new Vector3(-1800.013f, -1227.414f, 1.66f);
                    heading = 149.15f;
                    foreach (Ped ped in team)
                    {
                        ped.Position = entryPos;
                        ped.Heading = heading;
                    }
                    EntryComplete = true;
                    break;
                case 2: // Rappel
                    for (int i = 0; i < Convert.ToInt32(team.Length/2); i++)
                    {
                        var rappel = new RappelEntry(new Vector3(-1492.215f, -1230.166f, 2.87f).Around(20f), new Vector3(-1837.12f, -1254.43f, 20.18f));
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
            
        }
    }
}