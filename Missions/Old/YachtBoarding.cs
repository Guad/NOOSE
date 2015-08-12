using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE.Missions
{
    public class YachtBoarding : IMission
    {
        public string MissionName => "Boarding Party";
        public string MissionDescription => "Assault the yacht.";

        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }
        
        private readonly Tuple<Vector3, float>[] _positions = new[]
        {
            new Tuple<Vector3, float>(new Vector3(-2095.21f, -1014.529f, 8.980454f), 254.6685f),
            new Tuple<Vector3, float>(new Vector3(-2088.654f, -1014.833f, 8.971187f), 53.26372f),
            new Tuple<Vector3, float>(new Vector3(-2089.107f, -1019.663f, 8.971214f), 59.5147f),
            new Tuple<Vector3, float>(new Vector3(-2068.266f, -1030.879f, 8.971484f), 203.552f),
            new Tuple<Vector3, float>(new Vector3(-2050.54f, -1027.787f, 8.971493f), 253.1365f),
            new Tuple<Vector3, float>(new Vector3(-2063.967f, -1017.624f, 8.971493f), 346.4969f),
            new Tuple<Vector3, float>(new Vector3(-2043.094f, -1027.089f, 8.971489f), 205.0295f),
            new Tuple<Vector3, float>(new Vector3(-2057.475f, -1023.555f, 11.90752f), 243.8363f),
            new Tuple<Vector3, float>(new Vector3(-2060.493f, -1030.129f, 11.90751f), 293.0046f),
            new Tuple<Vector3, float>(new Vector3(-2066.532f, -1021.296f, 11.90952f), 181.7644f),
            new Tuple<Vector3, float>(new Vector3(-2085.112f, -1017.847f, 12.78191f), 76.60201f),
            new Tuple<Vector3, float>(new Vector3(-2023.627f, -1037.542f, 5.576131f), 22.59287f),
            new Tuple<Vector3, float>(new Vector3(-2031.613f, -1033.119f, 5.883026f), 272.627f),
            new Tuple<Vector3, float>(new Vector3(-2029.798f, -1042.656f, 2.56635f), 345.5269f),
            new Tuple<Vector3, float>(new Vector3(-2038.944f, -1037.97f, 2.563906f), 348.8367f),
            new Tuple<Vector3, float>(new Vector3(-2044.829f, -1032.102f, 2.58486f), 56.84675f),
            new Tuple<Vector3, float>(new Vector3(-2057.889f, -1033.588f, 3.017608f), 285.5529f),
            new Tuple<Vector3, float>(new Vector3(-2065.094f, -1028.157f, 3.061059f), 278.4578f),
            new Tuple<Vector3, float>(new Vector3(-2070.482f, -1022.374f, 3.061459f), 134.485f),
            new Tuple<Vector3, float>(new Vector3(-2062.749f, -1017.854f, 5.882016f), 342.0445f),
            new Tuple<Vector3, float>(new Vector3(-2079.463f, -1018.967f, 5.881791f), 284.9947f),
            new Tuple<Vector3, float>(new Vector3(-2073.653f, -1028.266f, 5.882463f), 174.1384f),
            new Tuple<Vector3, float>(new Vector3(-2107.997f, -1015.018f, 5.887006f), 319.7578f),
            new Tuple<Vector3, float>(new Vector3(-2096.069f, -1009.585f, 5.884347f), 206.588f),
            //new Tuple<Vector3, float>(new Vector3(-2082.51f, -1011.933f, 5.884128f), 8.348411f),
        };

        public Vector3[] EntryPoints => new[]
        {
            new Vector3(-2015.249f, -1041.006f, 2.072449f),
            new Vector3(-2121.114f, -1007.282f, 2.83442f)
        };

        public string[] EntryNames => new[]
        {
            "Boat",
            "Helicopter",
        };

        public Vector3 CameraPosition => new Vector3(-2065.032f, -1023.529f, 108.1314f);

        public YachtBoarding()
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            EntryComplete = false;
        }

        public void OnStartMission()
        {
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bar");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bedrm");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bridge");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_DistantLights");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_enginrm");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_LODLights");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Lounge");
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
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_Bar");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_Bedrm");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_Bridge");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_DistantLights");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_enginrm");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_LODLights");
            Function.Call(Hash.REMOVE_IPL, "hei_yacht_heist_Lounge");
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
                case 0: //boat
                    BadGuys[BadGuys.Count-1].Character.Delete();
                    BadGuys.RemoveAt(BadGuys.Count-1);

                    var boatModel = new Model(VehicleHash.Dinghy2);
                    int counter = 0;
                    do
                    {
                        boatModel.Request();
                        //Script.Yield();
                        counter++;
                    } while (!boatModel.IsLoaded && counter < 2000);

                    var boat = World.CreateVehicle(boatModel, new Vector3(-2015.075f, -1040.253f, 0.5699487f), 341.2882f);
                    boat.MarkAsNoLongerNeeded();
                    boat.FreezePosition = true;
                    CleanupBag.Add(boat);
                    foreach (Ped t in team)
                    {
                        t.Position = new Vector3(-2015.249f, -1041.006f, 2.072449f);
                        t.Heading = 63.90365f;
                    }
                    EntryComplete = true;
                    break;
                case 1: // Rappel
                    for (int i = 0; i < Convert.ToInt32(team.Length/2); i++)
                    {
                        var rappel = new RappelEntry(new Vector3(-1492.215f, -1230.166f, 2.87f).Around(20f), new Vector3(-2109.796f, -1009.942f, 22.83442f));
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
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bar");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bedrm");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Bridge");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_DistantLights");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_enginrm");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_LODLights");
            Function.Call(Hash.REQUEST_IPL, "hei_yacht_heist_Lounge");
            UI.ShowSubtitle("~r~Warning: This mission requires additional content.~w~\nChoose an entry point.", 10);
        }
    }
}