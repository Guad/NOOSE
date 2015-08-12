using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;

namespace NOOSE
{
    public enum Gamemodes
    {
        TargetElimination,
        BombDefusal,
    }

    public class CustomMission
    {
        public List<Enemy> BadGuys { get; set; }
        public List<Entity> CleanupBag { get; set; }
        public bool EntryComplete { get; set; }

        public string MissionName { get; private set; }
        public string MissionDescription { get; private set; }

        private readonly List<Tuple<Vector3, float>> _positions;

        public Vector3[] EntryPoints { get; private set; }

        public string[] EntryNames { get; private set; }

        public Vector3 CameraPosition { get; private set; }

        public Gamemodes Gamemode { get; set; }

        public int BombTime;

        public Tuple<Vector3, float> CurrentBomb;

        public List<Gamemodes> AvailableGamemodes = new List<Gamemodes>();

        public Polygon GameArea;

        //

        private readonly Dictionary<string, bool> _requiredIpLs;

        private readonly List<DecorativeVehicle> _decorativeVehicles;
        private readonly List<EntryPoint> _entryData;
        private readonly List<Tuple<Vector3, float>> _bombLocations;
        private int _maxEnemies;

        public CustomMission(string name, Vector3 cameraPos, List<EntryPoint> entries, List<Tuple<Vector3, float>> enemyPositions, int maxEnemies, List<DecorativeVehicle> decorativeVehicles, List<Tuple<Vector3, float>> bombLocations, int bombTime, string description = "", Dictionary<string, bool> requiredIpl = null, Polygon gamearea = null)
        {
            BadGuys = new List<Enemy>();
            CleanupBag = new List<Entity>();
            _bombLocations = new List<Tuple<Vector3, float>>(bombLocations);
            MissionName = name;
            MissionDescription = description;
            CameraPosition = cameraPos;
            _maxEnemies = maxEnemies;
            BombTime = bombTime;
            GameArea = gamearea;

            EntryPoints = new Vector3[entries.Count];
            EntryNames = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                if(entries[i].Type == EntryTypes.Rappel)
                    EntryPoints[i] = new Vector3(entries[i].Position.X, entries[i].Position.Y, 5f);
                else
                    EntryPoints[i] = entries[i].Position;
                EntryNames[i] = entries[i].Name;
            }
            _entryData = new List<EntryPoint>(entries);

            _positions = new List<Tuple<Vector3, float>>(enemyPositions);

            _requiredIpLs = requiredIpl != null
                ? new Dictionary<string, bool>(requiredIpl)
                : new Dictionary<string, bool>();

            _decorativeVehicles = new List<DecorativeVehicle>(decorativeVehicles);

            AvailableGamemodes.Add(Gamemodes.TargetElimination);
            if(bombLocations.Count > 0)
                AvailableGamemodes.Add(Gamemodes.BombDefusal);

            EntryComplete = false;
        }

        public void OnStartMission(Gamemodes gamemode)
        {
            Gamemode = gamemode;
            LoadIPL();
            foreach (var veh in _decorativeVehicles.Where(x => x.EntryPoint == -1))
            {
                var tmpMod = new Model(veh.Model);
                int counter = 0;
                do
                {
                    tmpMod.Request();
                    counter++;
                    Script.Yield();
                } while (!tmpMod.IsLoaded && counter < 10000);
                if(counter >= 10000) continue;

                Vehicle tmpVeh = new Vehicle(Function.Call<int>(Hash.CREATE_VEHICLE, tmpMod.Hash, veh.Position.X, veh.Position.Y,veh.Position.Z, veh.Heading, false, false));
                tmpVeh.SirenActive = veh.SirenActive;
                foreach (VehicleDoor door in veh.OpenDoors)
                {
                    tmpVeh.OpenDoor(door, false, false);
                }
                CleanupBag.Add(tmpVeh);
                }
            var tmpposs = new List<Tuple<Vector3, float>>(_positions);
            var tmpMax = _maxEnemies;
            if (_maxEnemies == -1) tmpMax = Enemy.Dice.Next(_positions.Count/2, _positions.Count);
            MaxEnemies = tmpMax;
            for(int i = 0; i < tmpMax; i++)
            {
                Enemy tmp;
                int ourPick = Enemy.Dice.Next(tmpposs.Count);
                Tuple<Vector3, float> ourTuple = tmpposs[ourPick];
                tmp = new Enemy(ourTuple.Item1, ourTuple.Item2);
                tmpposs.RemoveAt(ourPick);

                tmp.Character.Task.GuardCurrentPosition();
                BadGuys.Add(tmp);
                CleanupBag.Add(tmp.Character);
            }
            if (Gamemode == Gamemodes.BombDefusal)
            {
                BombDefused = false;
                _defusePercentage = 0;
                _defuseStart = null;
                IsDefusingBomb = false;

                var ourPick = _bombLocations[Enemy.Dice.Next(_bombLocations.Count)];
                CurrentBomb = new Tuple<Vector3, float>(ourPick.Item1, ourPick.Item2);
                int counter = 0;
                var barrelM = new Model(1298403575);
                do
                {
                    barrelM.Request();
                    counter++;
                    Script.Yield();
                } while (!barrelM.IsLoaded && counter < 2000);
                var barrel = World.CreateProp(barrelM, CurrentBomb.Item1 - new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, CurrentBomb.Item2), false, false);
                barrel.FreezePosition = true;
                counter = 0;
                var bombM = new Model(929047740);
                do
                {
                    bombM.Request();
                    counter++;
                    Script.Yield();
                } while (!bombM.IsLoaded && counter < 2000);
                var bomb = World.CreateProp(bombM, CurrentBomb.Item1 - new Vector3(0f, 0f, 0.2f), new Vector3(-90f, 0f, CurrentBomb.Item2), false, false);
                bomb.FreezePosition = true;
                CleanupBag.Add(barrel);
                CleanupBag.Add(bomb);
            }
        }

        public int MaxEnemies { get; private set; }

        public void OnEndMission()
        {
            foreach (var badGuy in CleanupBag)
            {
                (badGuy as Ped)?.CurrentBlip?.Remove();
                badGuy?.Delete();
            }
            CleanupBag.Clear();

            foreach (var entry in _rappelUpdates)
            {
                entry?.Clean();
            }
            foreach (var rappelEntry in _rappelStarted)
            {
                rappelEntry?.Clean();
            }
            _planeEntry?.Clean();
            UnloadIPL();
        }

        public void MissionTick()
        {
            _planeEntry?.Tick();   
            for (int i = _rappelUpdates.Count - 1; i >= 0; i--)
            {
                if (!_rappelUpdates[i].EntryInProgress && !_rappelStarted.Contains(_rappelUpdates[i]))
                {
                    _rappelUpdates[i].Start();
                    _rappelStarted.Add(_rappelUpdates[i]);
                }
                if (_rappelUpdates[i].EntryInProgress)
                {
                    _rappelUpdates[i].Tick();
                    break;
                }
                if (!_rappelUpdates[i].EntryInProgress && _rappelStarted.Contains(_rappelUpdates[i]))
                {
                    _rappelUpdates.RemoveAt(i);
                }
            }
            if (_rappelStarted.Count == 2 && _rappelStarted.All(f => !f.EntryInProgress) && !EntryComplete)
                EntryComplete = true;

            if (Gamemode == Gamemodes.BombDefusal && !BombDefused)
            {
                //SC_START
                if (Game.Player.Character.IsInRangeOf(CurrentBomb.Item1, 1.2f))
                {
                    if (!IsDefusingBomb)
                    {
                        Function.Call(Hash.REQUEST_ADDITIONAL_TEXT, "global", 0);
                        Function.Call(Hash.DISPLAY_HELP_TEXT_THIS_FRAME, "SC_START");
                    }
                    if (!IsDefusingBomb && Game.IsControlJustPressed(0, Control.Context))
                    {
                        Game.Player.Character.FreezePosition = true;
                        IsDefusingBomb = true;
                        var angle = Math.Atan2(CurrentBomb.Item1.Y - Game.Player.Character.Position.Y,
                                               CurrentBomb.Item1.X - Game.Player.Character.Position.X);
                        angle = angle*(180/Math.PI);
                        angle -= 90;
                        if (angle < 0)
                            angle = 360 - (-angle);
                        Game.Player.Character.Heading = (float)angle;
                        Game.Player.Character.Weapons.Select(Game.Player.Character.Weapons[(WeaponHash) 0xA2719263]);
                        Game.Player.Character.Task.PlayAnimation("missah_2_ext_altleadinout", "hack_loop", 8f, -1, true, 8f);
                        _defuseStart = Noose.SecondCount;
                        _defusePercentage = 0;
                    }
                    if (IsDefusingBomb &&
                        (Game.IsControlJustPressed(0, Control.PhoneCancel) ||
                         Game.IsControlJustPressed(0, Control.FrontendPauseAlternate)) && !Noose.IsSquadmenuOpen)
                    {
                        Game.Player.Character.FreezePosition = false;
                        IsDefusingBomb = false;
                        Game.Player.Character.Task.ClearAll();
                        _defuseStart = null;
                        _defusePercentage = 0;
                    }
                    if (IsDefusingBomb && _defuseStart.HasValue)
                    {
                        const int bombDefusedConst = 15;
                        _defusePercentage = Noose.SecondCount - _defuseStart.Value;
                        if (_defusePercentage > bombDefusedConst)
                        {
                            IsDefusingBomb = false;
                            BombDefused = true;
                            Game.Player.Character.FreezePosition = false;
                            Game.Player.Character.Task.ClearAll();
                            return;
                        }
                        int width = Convert.ToInt32((_defusePercentage/(float)bombDefusedConst) * 590);
                        SizeF res = UIMenu.GetScreenResolutionMantainRatio();
                        int center = Convert.ToInt32(res.Width/2);
                        new Sprite("timerbars", "damagebar_128",
                            new Point(center - 300, Convert.ToInt32(res.Height - 200)), new Size(600, 50), 0f,
                            Color.FromArgb(200, 200, 200, 0)).Draw();
                        new Sprite("timerbars", "damagebarfill_128",
                            new Point(center - 290, Convert.ToInt32(res.Height - 200)), new Size(width, 50), 0f,
                            Color.FromArgb(200, 200, 200, 0)).Draw();
                    }
                }

            }
        }

        public bool IsDefusingBomb = false;
        private int? _defuseStart = null;
        private int _defusePercentage = 0;

        public bool BombDefused = false;

        private readonly List<RappelEntry> _rappelUpdates = new List<RappelEntry>();
        private readonly List<RappelEntry> _rappelStarted = new List<RappelEntry>();
        private PlaneEntry _planeEntry;
        public void StartEntry(int entryPoint, Ped[] team)
        {
            var entry = _entryData[entryPoint];
            switch (entry.Type)
            {
                case EntryTypes.Teleport:
                    foreach (var ped in team)
                    {
                        ped.Position = entry.Position;
                        ped.Heading = entry.Heading;
                    }
                    break;
                case EntryTypes.Rappel:
                    if (team.Length == 1)
                    {
                        var rappel = new RappelEntry(entry.Helipads[0], entry.Position);
                        rappel.AddPassenger(team[0]);
                        _rappelUpdates.Add(rappel);
                    }
                    else
                    {
                        var rappel = new RappelEntry(entry.Helipads[0], entry.Position);
                        foreach (Ped ped in team)
                        {
                            rappel.AddPassenger(ped);
                        }
                        _rappelUpdates.Add(rappel);
                    }
                    break;
                case EntryTypes.Plane:
                    _planeEntry = new PlaneEntry(entry.PlaneSpawn, entry.PlaneSpawnHeading, entry.Approach, entry.RunwayStart, entry.RunwayEnd);
                    foreach (Ped ped in team)
                    {
                        _planeEntry.AddPassenger(ped);
                    }
                    _planeEntry.Start();
                    break;
            }
            foreach (var source in _decorativeVehicles.Where(x => x.EntryPoint == entryPoint))
            {
                var tmpMod = new Model(source.Model);
                int counter = 0;
                do
                {
                    tmpMod.Request();
                    Script.Yield();
                    counter++;
                } while (!tmpMod.IsLoaded && counter < 10000);
                var tmpVeh = Function.Call<Vehicle>(Hash.CREATE_VEHICLE, tmpMod.Hash, source.Position.X, source.Position.Y, source.Position.Z,source.Heading, false, false);
                tmpVeh.SirenActive = source.SirenActive;
                foreach (VehicleDoor door in source.OpenDoors)
                {
                    tmpVeh.OpenDoor(door, false, false);
                }
                CleanupBag.Add(tmpVeh);
            }
        }

        public void DrawBriefingMap()
        {
            LoadIPL();
            if (!String.IsNullOrWhiteSpace(MissionDescription))
                UI.ShowSubtitle(MissionDescription + "\nChoose an entry point.", 10);
        }

        public void LoadIPL()
        {
            foreach (var l in _requiredIpLs)
            {
                if (l.Value)
                {
                    if (Function.Call<bool>(Hash.IS_IPL_ACTIVE, l.Key))
                        Function.Call(Hash.REMOVE_IPL, l.Key);
                }
                else
                    Function.Call(Hash.REQUEST_IPL, l.Key);
            }
        }

        public void UnloadIPL()
        {
            foreach (var l in _requiredIpLs)
            {
                if (!l.Value)
                {
                    if (Function.Call<bool>(Hash.IS_IPL_ACTIVE, l.Key))
                        Function.Call(Hash.REMOVE_IPL, l.Key);
                }
                else
                    Function.Call(Hash.REQUEST_IPL, l.Key);
            }
            //Function.Call((Hash)0xD7C10C4A637992C9);
        }
    }
}