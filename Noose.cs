using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using Control = GTA.Control;
using System.IO;
using System.Xml;
using MapEditor;
using MapEditor.API;

namespace NOOSE
{
    public class Noose : Script
    {
        public static bool IsSquadmenuOpen = false;

        private readonly UIMenu _squadMenu;
        private readonly UIMenu _loadoutMenu;
        private readonly UIMenu _firstWeaponMenu;
        private readonly UIMenu _secondWeaponMenu;
        private readonly UIMenu _thirdWeaponMenu;
        private readonly UIMenu _missionOptions;

        private MenuPool _menuPool;


        private Teammate[] _mates = new Teammate[3];

        private WeaponHash[] firstArray = new[]
        {
            WeaponHash.SMG,
            WeaponHash.MicroSMG, 
            WeaponHash.AssaultSMG, 
            WeaponHash.CombatPDW, 
            WeaponHash.CarbineRifle, 
            WeaponHash.SpecialCarbine, 
            WeaponHash.BullpupRifle, 
            WeaponHash.PumpShotgun, 
            WeaponHash.BullpupShotgun, 
        };

        private WeaponHash[] secondArray = new[]
        {
            WeaponHash.Pistol,
            WeaponHash.Pistol,
            WeaponHash.CombatPistol, 
            WeaponHash.Pistol50, 
            WeaponHash.SNSPistol, 
            WeaponHash.HeavyPistol, 
            WeaponHash.StunGun, 
        };

        private WeaponHash[] thirdArray = new[]
        {
            WeaponHash.SmokeGrenade,
            WeaponHash.Grenade, 
            WeaponHash.StickyBomb, 
            WeaponHash.BZGas, 
            WeaponHash.Molotov, 
        };

        public List<CustomMission> MissionList = new List<CustomMission>();
        public static int SecondCount = 0;
        public List<Tuple<string, int>> MissionObjectives = new List<Tuple<string, int>>();

        private DateTime _oldTime = DateTime.Now;
        private bool _soloMission = false;
        private Gamemodes _selectedGamemode = Gamemodes.TargetElimination;
        private bool _loadOnlineContent = false;


        public Noose()
        {
            KeyDown += OnKeyDown;
            Tick += OnTick;

            _menuPool = new MenuPool();
            _squadMenu = new UIMenu("", "SQUAD COMMANDER", new Point(0, -107));
            _squadMenu.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0,0,0,0)));
            _squadMenu.AddItem(new UIMenuItem("Hold Position"));
            _squadMenu.AddItem(new UIMenuItem("Regroup"));
            _squadMenu.AddItem(new UIMenuItem("Engage targets"));
            var box = new UIMenuCheckboxItem("Night Vision", false);
            box.CheckboxEvent += (item, czech) =>
            {
                Game.Nightvision = czech;
            };
            _squadMenu.AddItem(box);
            _squadMenu.AddItem(new UIMenuItem("Stop Assault"));
            _squadMenu.OnItemSelect += OnSquadMenuSelect;
            _squadMenu.RefreshIndex();
            
            _firstWeaponMenu = new UIMenu("", "~b~PRIMARY WEAPON", new Point(0, -107));
            _firstWeaponMenu.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0, 0, 0, 0)));
            var r = new UIMenuItem("SMG");
            r.SetRightBadge(UIMenuItem.BadgeStyle.Gun);
            _firstWeaponMenu.AddItem(r);
            _firstWeaponMenu.AddItem(new UIMenuItem("Micro SMG"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Assault SMG"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Combat PDW"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Carbine Rifle"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Special Carbine"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Bullpup Rifle"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Pump Shotgun"));
            _firstWeaponMenu.AddItem(new UIMenuItem("Bullpup Shotgun"));
            _firstWeaponMenu.DisableInstructionalButtons(true);
            _firstWeaponMenu.RefreshIndex();
            _menuPool.Add(_firstWeaponMenu);

            _secondWeaponMenu = new UIMenu("", "~b~SECONDARY WEAPON", new Point(0, -107));
            _secondWeaponMenu.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0, 0, 0, 0)));
            var s = new UIMenuItem("Pistol");
            s.SetRightBadge(UIMenuItem.BadgeStyle.Gun);
            _secondWeaponMenu.AddItem(s);
            _secondWeaponMenu.AddItem(new UIMenuItem("Silenced Pistol"));
            _secondWeaponMenu.AddItem(new UIMenuItem("Combat Pistol"));
            _secondWeaponMenu.AddItem(new UIMenuItem("Pistol .50"));
            _secondWeaponMenu.AddItem(new UIMenuItem("SNS Pistol"));
            _secondWeaponMenu.AddItem(new UIMenuItem("Heavy Pistol"));
            _secondWeaponMenu.AddItem(new UIMenuItem("Stun Gun"));
            _secondWeaponMenu.DisableInstructionalButtons(true);
            _secondWeaponMenu.RefreshIndex();
            _menuPool.Add(_secondWeaponMenu);

            _thirdWeaponMenu = new UIMenu("", "~b~THROWABLE WEAPON", new Point(0, -107));
            _thirdWeaponMenu.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0, 0, 0, 0)));
            var g = new UIMenuItem("Tear Gas");
            g.SetRightBadge(UIMenuItem.BadgeStyle.Gun);
            _thirdWeaponMenu.AddItem(g);
            _thirdWeaponMenu.AddItem(new UIMenuItem("Grenade"));
            _thirdWeaponMenu.AddItem(new UIMenuItem("Sticky Bomb"));
            _thirdWeaponMenu.AddItem(new UIMenuItem("BZ Gas"));
            _thirdWeaponMenu.AddItem(new UIMenuItem("Molotov"));
            _thirdWeaponMenu.DisableInstructionalButtons(true);
            _thirdWeaponMenu.RefreshIndex();
            _menuPool.Add(_thirdWeaponMenu);

            _firstWeaponMenu.OnItemSelect += SelectHandler;
            _secondWeaponMenu.OnItemSelect += SelectHandler;
            _thirdWeaponMenu.OnItemSelect += SelectHandler;

            _missionOptions = new UIMenu("", "~b~MISSION SETTINGS", new Point(0, -107));
            _missionOptions.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0,0,0,0)));
            var soloItem = new UIMenuCheckboxItem("Solo Mission", false);
            soloItem.CheckboxEvent += (i, c) =>
            {
                _soloMission = c;
            };
            var weatherList = new List<dynamic>(Enum.GetNames(typeof(Weather)));
            var weatherItem = new UIMenuListItem("Weather", weatherList, 0);
            var timeList = new List<dynamic> {"Day", "Sunset", "Night", "Sunrise" };
            var timeItem = new UIMenuListItem("Time", timeList, 0);
            var gmItem = new UIMenuListItem("Gamemode", new List<dynamic>(Enum.GetNames(typeof (Gamemodes))), 0);
            
            gmItem.OnListChanged += (item, index) =>
            {
                Gamemodes output;
                Enum.TryParse(item.IndexToItem(index), out output);
                if (_briefingMission != null && !_briefingMission.AvailableGamemodes.Contains(output))
                {
                    item.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    item.Description = "This map does not have this gamemode.";
                }
                else
                {
                    item.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    item.Description = "";
                    _selectedGamemode = output;
                }
            };

            weatherItem.OnListChanged += (item, index) =>
            {
                Weather result;
                if (Enum.TryParse(item.IndexToItem(index), out result))
                    World.Weather = result;
            };

            timeItem.OnListChanged += (item, index) =>
            {
                string str = item.IndexToItem(index);
                switch (str)
                {
                    case "Day":
                        World.CurrentDayTime = new TimeSpan(14, 00, 00);
                        break;
                    case "Night":
                        World.CurrentDayTime = new TimeSpan(2, 00, 00);
                        break;
                    case "Sunset":
                        World.CurrentDayTime = new TimeSpan(20, 00, 00);
                        break;
                    case "Sunrise":
                        World.CurrentDayTime = new TimeSpan(06, 00, 00);
                        break;
                }
            };

            _missionOptions.AddItem(gmItem);
            _missionOptions.AddItem(weatherItem);
            _missionOptions.AddItem(timeItem);
            _missionOptions.AddItem(soloItem);
            _missionOptions.DisableInstructionalButtons(true);
            _missionOptions.RefreshIndex();
            _menuPool.Add(_missionOptions);

            _loadoutMenu = new UIMenu("", "~b~CHOOSE LOADOUT", new Point(0, -107));
            _loadoutMenu.SetBannerType(new UIResRectangle(new Point(), new Size(), Color.FromArgb(0,0,0,0)));
            var one = new UIMenuItem("Primary Weapon");
            var two = new UIMenuItem("Secondary Weapon");
            var three = new UIMenuItem("Throwable Weapon");
            var four = new UIMenuItem("Mission Settings");
            _loadoutMenu.AddItem(one);
            _loadoutMenu.AddItem(two);
            _loadoutMenu.AddItem(three);
            _loadoutMenu.AddItem(four);
            _loadoutMenu.DisableInstructionalButtons(true);
            _loadoutMenu.RefreshIndex();

            _loadoutMenu.BindMenuToItem(_firstWeaponMenu, one);
            _loadoutMenu.BindMenuToItem(_secondWeaponMenu, two);
            _loadoutMenu.BindMenuToItem(_thirdWeaponMenu, three);
            _loadoutMenu.BindMenuToItem(_missionOptions, four);
            _menuPool.Add(_loadoutMenu);



            // Set up mouse removin'
            _squadMenu.ControlDisablingEnabled = false;
            _squadMenu.MouseEdgeEnabled = false;
            _squadMenu.ResetKey(UIMenu.MenuControls.Back);
            _squadMenu.SetKey(UIMenu.MenuControls.Back, Control.FrontendCancel);
            _squadMenu.FormatDescriptions = false;

            // Load Mission XMLs
            LoadMissions();

            //Load Online content.
            if (_loadOnlineContent)
            {
                Function.Call((Hash)0x0888C3502DBBEEF5);
                Function.Call(Hash._0x9BAE5AD2508DF078, 1);
            }

			if (File.Exists("scripts\\MapEditor.dll"))
				AttachMapEditor();
		}

	    private void AttachMapEditor()
	    {
		    var thisMod = new ModListener()
		    {
				ButtonString = "Create a NOOSE Mission",
				Description = "Create a mission for the NOOSE mod.",
				Name = "NOOSE",
		    };
			ModManager.SuscribeMod(thisMod);
		    thisMod.OnMapSaved += SaveMission;
	    }

	    private void SaveMission(Map map, string filename)
	    {
		    if (filename.EndsWith(".xml"))
			    filename = filename.Replace(".xml", "");
			// Prog. Note: I know this fucking horrible but that's what you get when you use a XML parser written at 5 AM.
			
		    string nl = "\r\n";
		    string output = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + nl;
		    output += "<Mission>" + nl;
		    output += "\t<MetaInfo>" + nl;
		    output += "\t\t<Name>" + filename + "</Name>" + nl;
		    output += "\t\t<Info></Info>" + nl;
		    output += "\t\t<Enabled>true</Enabled>" + nl;
		    var firstProp = map.Objects.First(obj => obj.Type == ObjectTypes.Prop);
		    output += $"\t\t<CameraPos x=\"{firstProp.Position.X}\" y=\"{firstProp.Position.Y}\" z=\"{firstProp.Position.Z}\" />" + nl;
		    output += "\t</MetaInfo>" + nl;
		    output += "\t<EntryPoints>" + nl;
		    foreach (Marker marker in map.Markers)
		    {
				if(marker.Type == MarkerType.PlaneModel) continue;
			    string type = marker.Type == MarkerType.HorizontalCircleSkinny_Arrow ? "teleport" : "rappel";
			    output += $"\t\t<EntryPoint type=\"{type}\">" + nl;
			    if (type == "teleport")
			    {
				    output += "\t\t\t<Name>Front</Name>" + nl;
				    output += $"\t\t\t<Position x=\"{marker.Position.X}\" y=\"{marker.Position.Y}\" z=\"{marker.Position.Z}\" heading=\"{marker.Rotation.Z}\"/>" + nl;
			    }
				else
			    {
				    output += "\t\t\t<Name>Helicopter</Name>" + nl;
				    output += $"\t\t\t<Destination x=\"{marker.Position.X}\" y=\"{marker.Position.Y}\" z=\"{marker.Position.Z + 15}\"/>" + nl;
					output += $"\t\t\t<Helipad x=\"{map.Markers.First(x => x.Type == MarkerType.PlaneModel).Position.X}\" y=\"{map.Markers.First(x => x.Type == MarkerType.PlaneModel).Position.Y}\" z=\"{map.Markers.First(x => x.Type == MarkerType.PlaneModel).Position.Z}\"/>" + nl;
                }
			    output += "\t\t</EntryPoint>" + nl;
		    }
		    output += "\t</EntryPoints>" + nl;
		    output += "\t<Enemies>" + nl;
		    foreach (MapObject o in map.Objects.Where(obj => obj.Type == ObjectTypes.Ped))
		    {
			    output += $"\t\t<Enemy x=\"{o.Position.X}\" y=\"{o.Position.Y}\" z=\"{o.Position.Z}\" heading=\"{o.Rotation.Z}\"/>" + nl;
		    }
		    output += "\t</Enemies>" + nl;
		    if (map.Objects.Count(obj => obj.Type == ObjectTypes.Prop) > 1)
		    {
			    output += "\t<Bombs time=\"180\">" + nl;
			    output = map.Objects.Where(obj => obj.Type == ObjectTypes.Prop).Skip(1).Aggregate(output, (current, o) => current + ($"\t\t<Bomb x=\"{o.Position.X}\" y=\"{o.Position.Y}\" z=\"{o.Position.Z + 0.5f}\" heading=\"{o.Rotation.Z}\"/>" + nl));
			    output += "\t</Bombs>" + nl;
		    }
		    output += "\t<DecorativeVehicles>" + nl;
		    foreach (MapObject o in map.Objects.Where(x => x.Type == ObjectTypes.Vehicle))
		    {
			    var name = MapEditor.ObjectDatabase.VehicleDb.First(pair => pair.Value == o.Hash);
			    output += $"\t<Vehicle model=\"{name.Key}\" x=\"{o.Position.X}\" y=\"{o.Position.Y}\" z=\"{o.Position.Z}\" heading=\"{o.Rotation.Z}\" spawn=\"-1\" doors=\"0000\" siren=\"{o.SirensActive}\" />" + nl;
            }
		    output += "\t</DecorativeVehicles>" + nl;
		    output += "</Mission>";
		    var path = "scripts\\NOOSEMissions\\" + filename + ".xml";
            File.WriteAllText(path, output);
		    UI.Notify("Mission saved to " + path);
	    }

        private static float ParseFloat(string s)
        {
            return float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private void LoadMissions()
        {
            if (!Directory.Exists("scripts\\NOOSEMissions")) return;
            int counter = 0;
            foreach (string path in Directory.GetFiles("scripts\\NOOSEMissions", "*.xml"))
            {
                string location = "MetaInfo";
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);
                    var mainDictionary = XMLParser.Parse(xmlDoc.DocumentElement);
                    /*
                        Literally it just werks: the code
                     */

                    if (!Convert.ToBoolean(mainDictionary["MetaInfo"]["Children"]["Enabled"]["Value"])) continue;

                    var name = mainDictionary["MetaInfo"]["Children"]["Name"]["Value"];
                    var desc = mainDictionary["MetaInfo"]["Children"]["Info"]["Value"];
                    var camera = new Vector3(ParseFloat(mainDictionary["MetaInfo"]["Children"]["CameraPos"]["Attributes"]["x"]), ParseFloat(mainDictionary["MetaInfo"]["Children"]["CameraPos"]["Attributes"]["y"]), ParseFloat(mainDictionary["MetaInfo"]["Children"]["CameraPos"]["Attributes"]["z"]));
                    Dictionary<string, bool> ipls = new Dictionary<string, bool>();
                    if (mainDictionary["MetaInfo"]["Children"].ContainsKey("RequiredIPL"))
                    {
                        if (
                            mainDictionary["MetaInfo"]["Children"]["RequiredIPL"]["Attributes"].ContainsKey("loadOnline") &&
                            Convert.ToBoolean(
                                mainDictionary["MetaInfo"]["Children"]["RequiredIPL"]["Attributes"]["loadOnline"]))
                            _loadOnlineContent = true;

                        foreach (var keyValuePair in mainDictionary["MetaInfo"]["Children"]["RequiredIPL"]["Children"])
                        {
                            bool rem = false;
                            if (keyValuePair.Value["Attributes"].ContainsKey("remove"))
                            {
                                rem = Convert.ToBoolean(keyValuePair.Value["Attributes"]["remove"]);
                            }
                            ipls.Add(keyValuePair.Value["Value"], rem);
                        }
                    }
                    
                    List<PointF> vertices = new List<PointF>();
                    Polygon area = null;
                    if (mainDictionary["MetaInfo"]["Children"].ContainsKey("Area"))
                    {
                        foreach (var point in mainDictionary["MetaInfo"]["Children"]["Area"]["Children"])
                        {
                            vertices.Add(new PointF(ParseFloat(point.Value["Attributes"]["x"]), ParseFloat(point.Value["Attributes"]["y"])));
                        }
                    }
                    if(vertices.Count > 2)
                        area = new Polygon(vertices);


                    List<EntryPoint> entries = new List<EntryPoint>();
                    location = "EntryPoints";
                    foreach (var point in mainDictionary["EntryPoints"]["Children"])
                    {
                        switch ((string)point.Value["Attributes"]["type"])
                        {
                            case "teleport":
                                entries.Add(new EntryPoint(point.Value["Children"]["Name"]["Value"], new Vector3(ParseFloat(point.Value["Children"]["Position"]["Attributes"]["x"]), ParseFloat(point.Value["Children"]["Position"]["Attributes"]["y"]), ParseFloat(point.Value["Children"]["Position"]["Attributes"]["z"])), EntryTypes.Teleport, ParseFloat(point.Value["Children"]["Position"]["Attributes"]["heading"])));
                                break;
                            case "rappel":
                                var helipads = new List<Vector3>();
                                if (point.Value["Children"].ContainsKey("Helipads"))
                                    foreach (var helipad in point.Value["Children"]["Helipads"]["Children"])
                                    {
                                        helipads.Add(new Vector3(ParseFloat(helipad.Value["Attributes"]["x"]),
                                            ParseFloat(helipad.Value["Attributes"]["y"]),
                                            ParseFloat(helipad.Value["Attributes"]["z"])));
                                    }
                                else
                                {
                                    helipads.Add(new Vector3(ParseFloat(point.Value["Children"]["Helipad"]["Attributes"]["x"]), ParseFloat(point.Value["Children"]["Helipad"]["Attributes"]["y"]), ParseFloat(point.Value["Children"]["Helipad"]["Attributes"]["z"])));
                                }
                                entries.Add(new EntryPoint(point.Value["Children"]["Name"]["Value"],
                                    new Vector3(ParseFloat(point.Value["Children"]["Destination"]["Attributes"]["x"]), ParseFloat(point.Value["Children"]["Destination"]["Attributes"]["y"]), ParseFloat(point.Value["Children"]["Destination"]["Attributes"]["z"])),
                                    EntryTypes.Rappel,
                                    helipads));
                                break;
                            case "plane":
                                var planeSpawnPoint = new Vector3(ParseFloat(point.Value["Children"]["PlaneSpawn"]["Attributes"]["x"]), ParseFloat(point.Value["Children"]["PlaneSpawn"]["Attributes"]["y"]), ParseFloat(point.Value["Children"]["PlaneSpawn"]["Attributes"]["z"]));
                                var planeHeading = ParseFloat(point.Value["Children"]["PlaneHeading"]["Value"]);
                                var approach = new Vector3(ParseFloat(point.Value["Children"]["Approach"]["Attributes"]["x"]), ParseFloat(point.Value["Children"]["Approach"]["Attributes"]["y"]), ParseFloat(point.Value["Children"]["Approach"]["Attributes"]["z"]));
                                var runwayStart =
                                    new Vector3(ParseFloat(point.Value["Children"]["RunwayStart"]["Attributes"]["x"]),
                                        ParseFloat(point.Value["Children"]["RunwayStart"]["Attributes"]["y"]),
                                        ParseFloat(point.Value["Children"]["RunwayStart"]["Attributes"]["z"]));
                                var runwayEnd =
                                    new Vector3(ParseFloat(point.Value["Children"]["RunwayEnd"]["Attributes"]["x"]),
                                        ParseFloat(point.Value["Children"]["RunwayEnd"]["Attributes"]["y"]),
                                        ParseFloat(point.Value["Children"]["RunwayEnd"]["Attributes"]["z"]));
                                entries.Add(new EntryPoint(point.Value["Children"]["Name"]["Value"], runwayEnd,
                                    EntryTypes.Plane, planeSpawnPoint, planeHeading, approach, runwayStart, runwayEnd));
                                break;
                        }
                    }
                    var enemies = new List<Tuple<Vector3, float>>();
                    location = "Enemies";
                    foreach (var enemy in mainDictionary["Enemies"]["Children"])
                    {
                        enemies.Add(new Tuple<Vector3, float>(new Vector3(ParseFloat(enemy.Value["Attributes"]["x"]), ParseFloat(enemy.Value["Attributes"]["y"]), ParseFloat(enemy.Value["Attributes"]["z"])), ParseFloat(enemy.Value["Attributes"]["heading"])));
                    }
                    int maxEnemies = enemies.Count;
                    if (mainDictionary["Enemies"]["Attributes"].ContainsKey("max") && mainDictionary["Enemies"]["Attributes"]["max"] != "random")
                        maxEnemies = Convert.ToInt32(mainDictionary["Enemies"]["Attributes"]["max"], CultureInfo.InvariantCulture);
                    else if ((mainDictionary["Enemies"]["Attributes"].ContainsKey("max") &&
                              mainDictionary["Enemies"]["Attributes"]["max"] == "random"))
                        maxEnemies = -1;
                    var cars = new List<DecorativeVehicle>();
                    location = "DecorativeVehicles";
                    foreach (var vehicle in mainDictionary["DecorativeVehicles"]["Children"])
                    {
                        VehicleHash hash;
                        bool success = Enum.TryParse(vehicle.Value["Attributes"]["model"], out hash);
                        if (!success) continue;
                        bool siren = vehicle.Value["Attributes"].ContainsKey("siren") ? Convert.ToBoolean(vehicle.Value["Attributes"]["siren"]) : false;
                        string doors = vehicle.Value["Attributes"].ContainsKey("doors") ? vehicle.Value["Attributes"]["doors"] : "0000";

                        cars.Add(new DecorativeVehicle(hash, new Vector3(ParseFloat(vehicle.Value["Attributes"]["x"]), ParseFloat(vehicle.Value["Attributes"]["y"]), ParseFloat(vehicle.Value["Attributes"]["z"])), ParseFloat(vehicle.Value["Attributes"]["heading"]), Convert.ToInt32(vehicle.Value["Attributes"]["spawn"]), siren, doors));
                    }
                    // Optional: bomb gamemode
                    var bombLocations = new List<Tuple<Vector3, float>>();
                    int bombTime = 120;
                    if (mainDictionary.ContainsKey("Bombs"))
                    {
                        location = "Bomb Gamemode";
                        foreach (var bomb in mainDictionary["Bombs"]["Children"])
                        {
                            bombLocations.Add(new Tuple<Vector3, float>(new Vector3(ParseFloat(bomb.Value["Attributes"]["x"]), ParseFloat(bomb.Value["Attributes"]["y"]), ParseFloat(bomb.Value["Attributes"]["z"])), ParseFloat(bomb.Value["Attributes"]["heading"])));
                        }
                        if (mainDictionary["Bombs"]["Attributes"].ContainsKey("time"))
                            bombTime = Convert.ToInt32(mainDictionary["Bombs"]["Attributes"]["time"], CultureInfo.InvariantCulture);
                    }

                    location = "Mission Initialization";
                    var customMission = new CustomMission(name, camera, entries, enemies, maxEnemies, cars, bombLocations, bombTime, desc, ipls, area);
                    MissionList.Add(customMission);
                    counter++;
                }
                catch (Exception e)
                {
                    UI.Notify("~r~Error~w~ while loading a mission, this incident has been logged.");
                    UI.Notify(e.Message);
                    var errorLog = String.Format(@"==============={0}=================
- ERROR WILE LOADING MISSION {1}
- MESSAGE: {2}
- STACK TRACE: {3}
- SOURCE: {4}
- TARGET: {5}
- LOCATION: {6}
", DateTime.Now, path, e.Message, e.StackTrace, e.Source, e.TargetSite, location);
                    File.AppendAllText("scripts\\NOOSEMissions\\loading.log", errorLog);
                }
            }

            UI.Notify("~b~NOoSE~w~\nLoaded ~r~" + counter + "~w~ mission(s).");
        }

        private Helptip _maintip;
        private void SelectHandler(UIMenu sender, UIMenuItem selected, int index)
        {
            foreach (var item in sender.MenuItems)
            {
                item.SetRightBadge(UIMenuItem.BadgeStyle.None);
            }
            selected.SetRightBadge(UIMenuItem.BadgeStyle.Gun);
        }
        
        private Model _characterUsed;
        private TimeSpan _lastTime;
        private readonly Dictionary<WeaponHash, int> _playerWeapons = new Dictionary<WeaponHash, int>();
        private void StartMission(CustomMission mission, int entrypoint)
        {
            try
            {
                _characterUsed = Game.Player.Character.Model;
                _lastTime = World.CurrentDayTime;

                _killedEnemies = 0;
                _killedCivs.Clear();
                _woundedCivs.Clear();

                MissionFailed = false;

                foreach (var hashObj in Enum.GetValues(typeof(WeaponHash)))
                {
                    var hash = (WeaponHash) hashObj;
                    var weapon = Game.Player.Character.Weapons[hash];
                    if(weapon == null) continue;
                    _playerWeapons.Add(hash, weapon.Ammo);
                }
                var swat = new Model(PedHash.Swat01SMY);
                int counter = 0;
                do
                {
                    swat.Request();
                    Yield();
                    counter++;
                } while (!swat.IsLoaded && counter < 10000);
                _menuPool.CloseAllMenus();
                Function.Call(Hash.SET_MAX_WANTED_LEVEL, 0);
                if (swat.IsLoaded)
                    Function.Call(Hash.SET_PLAYER_MODEL, new InputArgument(Game.Player), new Model(PedHash.Swat01SMY));
                int firstInx = 0;
                int secondInx = 0;
                int thirdInx = 0;
                for (int i = 0; i < _firstWeaponMenu.Size; i++)
                {
                    if (_firstWeaponMenu.MenuItems[i].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        firstInx = i;
                }

                for (int i = 0; i < _secondWeaponMenu.Size; i++)
                {
                    if (_secondWeaponMenu.MenuItems[i].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        secondInx = i;
                }

                for (int i = 0; i < _thirdWeaponMenu.Size; i++)
                {
                    if (_thirdWeaponMenu.MenuItems[i].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        thirdInx = i;
                }
                Game.Player.Character.Weapons.Give(secondArray[secondInx], 100, false, true);
                Game.Player.Character.Weapons.Give(thirdArray[thirdInx], 4, false, true);
                Game.Player.Character.Weapons.Give(firstArray[firstInx], 500, true, true);
                if (secondInx == 1)
                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, (int) WeaponHash.Pistol, 0x65EA7EBB);
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, (int)secondArray[secondInx], 0x359B7AAE);
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, (int)firstArray[firstInx], 0x7BC4CDDC);

                Game.Player.Character.Health = 200;
                Game.Player.Character.Armor = 200;
                _currentMission?.OnEndMission();
                _currentMission = null;
                mission.OnStartMission(_selectedGamemode);
                if(!_soloMission)
                    AddMates();
                else
                    _mates = new Teammate[0];
                Ped[] team;
                if (!_soloMission)
                {
                    team = new Ped[4];
                    team[0] = Game.Player.Character;
                    for (int i = 1; i < 4; i++)
                    {
                        team[i] = _mates[i - 1].Character;
                    }
                }
                else
                {
                    team = new[] {Game.Player.Character};
                }
                mission.StartEntry(entrypoint, team);
                _currentMission = mission;
                _maintip = new Helptip("Open the ~b~Squad Commander~w~ menu to see your objectives.");
                int sc = 0;
                while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "swat") && sc < 5000)
                {
                    Function.Call(Hash.REQUEST_ANIM_DICT, "swat");
                    sc++;
                }

                MissionObjectives.Clear();
                // 0 - in progress, 1 - complete, 2 - failed
                MissionObjectives.Add(new Tuple<string, int>("~h~Objectives~h~~s~~w~\n", 0));
                if (_selectedGamemode == Gamemodes.TargetElimination)
                {
                    MissionObjectives.Add(new Tuple<string, int>("Neutralize all targets\n", 0));
                    MissionObjectives.Add(new Tuple<string, int>("Minimize civilian casualties", 0));
                }
                if (_selectedGamemode == Gamemodes.BombDefusal)
                {
                    MissionObjectives.Add(new Tuple<string, int>("Neutralize all targets\n", 0));
                    MissionObjectives.Add(new Tuple<string, int>("Minimize civilian casualties\n", 0));
                    MissionObjectives.Add(new Tuple<string, int>("Defuse the bomb", 0));
                }
            }
            finally
            {
                _stopMission = null;
                _missionStart = SecondCount;
                Game.FadeScreenIn(1000);
            }
        }

        private void StopMission()
        {
            Function.Call(Hash.SET_MAX_WANTED_LEVEL, 5);
            Function.Call(Hash.SET_PLAYER_MODEL, new InputArgument(Game.Player), _characterUsed);
            _currentMission?.OnEndMission();
            RemoveMates();
            _currentMission = null;
            _killedEnemies = 0;
            _killedCivs?.Clear();
            _woundedCivs?.Clear();
            World.RenderingCamera = null;
            Game.Nightvision = false;
            if(_squadMenu.Visible) _squadMenu.Visible = false;
            _maintip = null;
            Game.Player.Character.Position = oldPos - new Vector3(0f, 0f, 1f);
            World.CurrentDayTime = _lastTime;
            foreach (KeyValuePair<WeaponHash, int> pair in _playerWeapons)
            {
                Game.Player.Character.Weapons.Give(pair.Key, pair.Value, false, true);
            }
            _playerWeapons.Clear();
            Game.Player.Character.FreezePosition = false;
            Game.FadeScreenIn(500);
        }

        private void RemoveMates()
        {
            foreach (var teammate in _mates)
            {
                teammate?.Character?.CurrentBlip?.Remove();
                teammate?.Character?.Delete();
            }
            _mates = new Teammate[3];
        }

        private void AddMates()
        {
            for (int i = 0; i < 3; i++)
            {
                _mates[i] = new Teammate(Game.Player.Character.Position);
                _mates[i].Character.AddBlip();
                _mates[i].Character.CurrentBlip.Color = BlipColor.Blue;
                _mates[i].Character.CurrentBlip.Scale = 0.75f;

                int firstInx = 0;
                int secondInx = 0;
                int thirdInx = 0;
                for (int g = 0; g < _firstWeaponMenu.Size; g++)
                {
                    if (_firstWeaponMenu.MenuItems[g].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        firstInx = g;
                }

                for (int g = 0; g < _secondWeaponMenu.Size; g++)
                {
                    if (_secondWeaponMenu.MenuItems[g].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        secondInx = g;
                }

                for (int g = 0; g < _thirdWeaponMenu.Size; g++)
                {
                    if (_thirdWeaponMenu.MenuItems[g].RightBadge == UIMenuItem.BadgeStyle.Gun)
                        thirdInx = g;
                }

                _mates[i].Character.Weapons.Give(secondArray[secondInx], 100, false, true);
                _mates[i].Character.Weapons.Give(thirdArray[thirdInx], 4, false, true);
                _mates[i].Character.Weapons.Give(firstArray[firstInx], 500, true, true);
                if (secondInx == 1)
                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, _mates[i].Character.Handle, (int)WeaponHash.Pistol, 0x65EA7EBB);
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, _mates[i].Character.Handle, (int)secondArray[secondInx], 0x359B7AAE);
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, _mates[i].Character.Handle, (int)firstArray[firstInx], 0x7BC4CDDC);
            }
        }

        private Dictionary<int, int> _pedTracking = new Dictionary<int, int>(); // Ped Hash - Last health.
        
        /// <summary>
        /// Stats tracking
        /// </summary>
        private List<int> _woundedCivs = new List<int>();
        private List<int> _killedCivs = new List<int>();
        private int _killedEnemies = 0;
        private int _missionStart;

        private void TrackCivillians()
        {
            Ped[] civs = World.GetNearbyPeds(Game.Player.Character, 300f);
            foreach (Ped civ in civs)
            {
                if(_currentMission.BadGuys.Any(x => x.Character.Handle == civ.Handle) || _mates.Any(x => x.Character.Handle == civ.Handle)) continue;
                if (_pedTracking.ContainsKey(civ.Handle))
                {
                    if (civ.Health < _pedTracking[civ.Handle] && !_woundedCivs.Contains(civ.Handle))
                        _woundedCivs.Add(civ.Handle);
                    if (!civ.IsDead || !_woundedCivs.Contains(civ.Handle) || _killedCivs.Contains(civ.Handle)) continue;
                    _woundedCivs.Remove(civ.Handle);
                    _killedCivs.Add(civ.Handle);
                }
                else
                    _pedTracking.Add(civ.Handle, civ.Health);
            }
        }

        public void OnTick(object sender, EventArgs e)
        {
            if (_oldTime.Second != DateTime.Now.Second)
            {
                SecondCount++;
                _oldTime = DateTime.Now;
                if (_currentMission != null && _currentMission.Gamemode == Gamemodes.BombDefusal && !_currentMission.BombDefused && _currentMission.BombTime - (SecondCount - _missionStart) > 0)
                {
                    Function.Call(Hash.PLAY_SOUND_FROM_COORD, -1, "HACKING_DOWNLOADED", 
                        _currentMission.CurrentBomb.Item1.X, 
                        _currentMission.CurrentBomb.Item1.Y, 
                        _currentMission.CurrentBomb.Item1.Z, 0, 0, 0, 0);
                    Function.Call(Hash.DRAW_LIGHT_WITH_RANGE,
                        _currentMission.CurrentBomb.Item1.X,
                        _currentMission.CurrentBomb.Item1.Y,
                        _currentMission.CurrentBomb.Item1.Z + 0.1f, 255, 10, 10, 1f, 40f);
                }
            }

            IsSquadmenuOpen = _squadMenu.Visible;

            Ped player = Game.Player.Character;
            if(!_squadMenu.Visible && !_menuPool.IsAnyMenuOpen())
                _maintip?.Draw();
            _currentMission?.MissionTick();
            _menuPool.ProcessMenus();
            _squadMenu.ProcessControl();
            _squadMenu.Draw();

            if (_squadMenu.Visible)
            {
                _squadMenu.DisEnableControls(false);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Aim);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Jump);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.LookLeftRight);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.LookUpDown);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.SelectWeapon);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.WeaponWheelLeftRight);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.WeaponWheelUpDown);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.WeaponWheelNext);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.WeaponWheelPrev);
            }

            if (Game.Player.Character.IsInRangeOf(new Vector3(2477.207f, -384.1893f, 94.40262f), 1.5f))
            {
                Function.Call(Hash.REQUEST_ADDITIONAL_TEXT, "ass_cs", 0);
                Function.Call(Hash.DISPLAY_HELP_TEXT_THIS_FRAME, "ASS_CS_ELEHELP");
            }

            if (Game.IsControlJustPressed(0, GTA.Control.Context) && _currentMission == null && Game.Player.Character.IsInRangeOf(new Vector3(2477.207f, -384.1893f, 94.40262f), 1.5f))
            {
                if (!GUI.InBriefing)
                    oldPos = Game.Player.Character.Position;
                _briefingMission = MissionList[_briefingIndex % MissionList.Count];
                GUI.InBriefing = !GUI.InBriefing;
                _loadoutMenu.Visible = GUI.InBriefing;
                Function.Call(Hash.DISPLAY_RADAR, !GUI.InBriefing);
                if (!GUI.InBriefing)
                {
                    World.RenderingCamera = null;
                    Game.Player.Character.Position = oldPos - new Vector3(0f, 0f, 1f);
                }
            }
            Function.Call(Hash.DRAW_MARKER, 2, 2477.207f, -384.1893f, 94.40262f, 0, 0, 0, 0, 180f, 90f, 0.75f, 0.75f, 0.75f, Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, 50, true, false, 0, false, false, false, false);

            if (GUI.InBriefing)
            {
                Function.Call(Hash.DISPLAY_RADAR, false);
                _briefingMission = MissionList[_briefingIndex%MissionList.Count];
                GUI.DrawBriefing(_briefingMission);

                if (Game.IsControlJustPressed(0, GTA.Control.PhoneLeft) &&
                    (!_missionOptions.Visible ||
                     (_missionOptions.Visible && _missionOptions.CurrentSelection != 0 &&
                      _missionOptions.CurrentSelection != 1 &&
                      _missionOptions.CurrentSelection != 2)))
                {
                    _briefingMission.UnloadIPL();
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).Index = 0;
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).Description = "";
                    _selectedGamemode = Gamemodes.TargetElimination;
                    _briefingIndex--;
                }
                if (Game.IsControlJustPressed(0, GTA.Control.PhoneRight) &&
                    (!_missionOptions.Visible ||
                     (_missionOptions.Visible && _missionOptions.CurrentSelection != 0 &&
                      _missionOptions.CurrentSelection != 1 &&
                      _missionOptions.CurrentSelection != 2)))
                {
                    _briefingMission.UnloadIPL();
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).Index = 0;
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    ((UIMenuListItem) _missionOptions.MenuItems[0]).Description = "";
                    _selectedGamemode = Gamemodes.TargetElimination;
                    _briefingIndex++;
                }
                if (Game.IsControlJustPressed(0, GTA.Control.FrontendLb))
                {
                    GUI.SelectedEntry = (1000 - (1000 % _briefingMission.EntryPoints.Length) + (GUI.SelectedEntry - 1))%_briefingMission.EntryPoints.Length;
                }

                if (Game.IsControlJustPressed(0, GTA.Control.FrontendRb))
                {
                    GUI.SelectedEntry = (1000 - (1000 % _briefingMission.EntryPoints.Length) + (GUI.SelectedEntry + 1))%_briefingMission.EntryPoints.Length;
                }

                if (Game.IsControlJustPressed(0, GTA.Control.PhoneCancel) && _loadoutMenu.Visible)
                {
                    GUI.InBriefing = false;
                    World.RenderingCamera = null;
                    Function.Call(Hash.DISPLAY_RADAR, true);
                    _menuPool.CloseAllMenus();
                    Game.Player.Character.Position = oldPos - new Vector3(0f, 0f, 1f);
                }
                if (Game.IsControlJustPressed(0, GTA.Control.Jump))
                {
                    GUI.InBriefing = false;
                    Game.FadeScreenOut(500);
                    Wait(1000);
                    Function.Call(Hash.DISPLAY_RADAR, true);
                    World.RenderingCamera = null;
                    _menuPool.CloseAllMenus();
                    StartMission(_briefingMission, GUI.SelectedEntry);
                }
            }

            if (_currentMission != null)
            {
                if (Game.Player.Character.Health <= 0)
                {
                    StopMission();
                    Game.Player.Character.Kill();
                    return;
                }

                MissionObjectives.Clear();
                // 0 - in progress, 1 - complete, 2 - failed
                MissionObjectives.Add(new Tuple<string, int>("~h~Objectives~h~~s~~w~\n", 0));
                if (_selectedGamemode == Gamemodes.TargetElimination)
                {
                    MissionObjectives.Add(new Tuple<string, int>("Neutralize all targets\n", _currentMission.BadGuys.Count == 0 ? 1 : 0));
                    MissionObjectives.Add(new Tuple<string, int>("Minimize civilian casualties", _woundedCivs.Count == 0 ? 1 : 2));
                }
                if (_selectedGamemode == Gamemodes.BombDefusal)
                {
                    MissionObjectives.Add(new Tuple<string, int>("Neutralize all targets\n", _currentMission.BadGuys.Count == 0 ? 1 : 0));
                    MissionObjectives.Add(new Tuple<string, int>("Minimize civilian casualties\n", _woundedCivs.Count == 0 ? 1 : 2));
                    MissionObjectives.Add(new Tuple<string, int>("Defuse the bomb", _currentMission.BombDefused ? 1 : 0));
                }

                foreach (UIMenuItem item in _squadMenu.MenuItems)
                {
                    string output = "";
                    foreach (Tuple<string, int> tuple in MissionObjectives)
                    {
                        output += tuple.Item2 == 0
                            ? "~w~"
                            : tuple.Item2 == 1
                                ? "~g~"
                                : "~r~";
                        output += tuple.Item1;
                    }
                    item.Description = output;
                }
            }

            if (_currentMission != null && !MissionFailed)
            {
                TrackCivillians();

                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.InteractionMenu);
                if(!_soloMission)
                    foreach (Teammate mate in _mates.Where(mate => mate.Character.IsDead))
                    {
                        mate.Character.CurrentBlip?.Remove();
                    }

                if (Game.IsControlJustPressed(0, GTA.Control.InteractionMenu))
                {
                    _squadMenu.Visible = !_squadMenu.Visible;
                }
                Game.Player.WantedLevel = 0;
                if (!_soloMission)
                {
                    GUI.DrawHUD(_mates,
                        _currentMission.Gamemode == Gamemodes.BombDefusal
                            ? _currentMission.BombDefused ? -2 : _currentMission.BombTime - (SecondCount - _missionStart)
                            : -1);
                }
                else
                {
                    if(_currentMission.Gamemode == Gamemodes.BombDefusal)
                        GUI.DrawBombTimer(_currentMission.BombDefused ? -2 : _currentMission.BombTime - (SecondCount - _missionStart));
                }

                for (int i = _currentMission.BadGuys.Count - 1; i >= 0; i--)
                {
                    if (_currentMission.BadGuys[i].Character.IsDead)
                    {
                        _currentMission.BadGuys[i].Character.CurrentBlip?.Remove();
                        _currentMission.BadGuys.RemoveAt(i);
                        _killedEnemies++;
                        continue;
                    }
                    if (_currentMission.GameArea != null && !_currentMission.GameArea.Contains(new PointF(
                            _currentMission.BadGuys[i].Character.Position.X,
                            _currentMission.BadGuys[i].Character.Position.Y)) &&
                            _currentMission.BadGuys[i].Character.CurrentBlip == null)
                    {
                        _currentMission.BadGuys[i].Character.AddBlip();
                        _currentMission.BadGuys[i].Character.CurrentBlip.Color = BlipColor.Red;
                    }
                    else if(_currentMission.GameArea != null && _currentMission.GameArea.Contains(new PointF(
                            _currentMission.BadGuys[i].Character.Position.X,
                            _currentMission.BadGuys[i].Character.Position.Y)) &&
                            _currentMission.BadGuys[i].Character.CurrentBlip != null)
                    {
                        _currentMission.BadGuys[i].Character.CurrentBlip?.Remove();
                    }
                }

                if (_currentMission.Gamemode == Gamemodes.BombDefusal)
                {
                    int secondsLeft = _currentMission.BombTime - (SecondCount - _missionStart);
                    if (secondsLeft <= 0 && !_currentMission.BombDefused)
                    {
                        World.AddExplosion(_currentMission.CurrentBomb.Item1, ExplosionType.BigExplosion1, 200f, 3f);
                        MissionFailed = true;
                        MissionFailedReason = "The bomb has detonated.";
                        Function.Call(Hash._START_SCREEN_EFFECT, "DeathFailOut", 0, true);
                        return;
                    }
                }

                if (_currentMission.BadGuys.Count == 0 && (_currentMission.Gamemode != Gamemodes.BombDefusal || _currentMission.BombDefused))
                {
                    GUI.DisEnableControls(false, false);
                    if (_stopMission == null)
                        _stopMission = SecondCount;
                    /*
                    What do we want
                    - Time elapsed
                    - Enemies killed
                    - Check whether no civillian damage
                    - Check whether no teammate deaths
                    - Check Number of civillians wounded
                    - Number of civs dead
                    */
                    string timeElapsed = FormatTimeSpan(_stopMission.Value - _missionStart);
                    int enemiesKilled = _killedEnemies;
                    int civWounded = _woundedCivs.Count;
                    int civKilled = _killedCivs.Count;
                    bool noCivDamage = _woundedCivs.Count == 0 && _killedCivs.Count == 0;
                    bool noTeamDamage = !_mates.Any(x => x.Character.IsDead);

                    int score = 100;
                    if (!noCivDamage) score -= 10;
                    if (!noTeamDamage) score -= 20;
                    _killedCivs.ForEach(x => score -= 10);
                    _woundedCivs.ForEach(x => score -= 5);

                    var tmpMiss = new MissionPassedScreen(_currentMission.MissionName, score,
                        score > 70
                            ? score > 90 ? MissionPassedScreen.Medal.Gold : MissionPassedScreen.Medal.Silver
                            : MissionPassedScreen.Medal.Bronze);
                    tmpMiss.AddItem("Mission Time", timeElapsed, MissionPassedScreen.TickboxState.None);
                    tmpMiss.AddItem("Enemies Killed", enemiesKilled + "/" + _currentMission.MaxEnemies, enemiesKilled == _currentMission.MaxEnemies ? MissionPassedScreen.TickboxState.Tick : MissionPassedScreen.TickboxState.Empty);
                    tmpMiss.AddItem("No Civilian Damage", "", noCivDamage ? MissionPassedScreen.TickboxState.Tick : MissionPassedScreen.TickboxState.Cross);
                    tmpMiss.AddItem("Civilians Wounded", civWounded.ToString(), civWounded == 0 ? MissionPassedScreen.TickboxState.Tick : MissionPassedScreen.TickboxState.Empty);
                    tmpMiss.AddItem("Civilians Dead", civKilled.ToString(), civKilled == 0? MissionPassedScreen.TickboxState.Tick : MissionPassedScreen.TickboxState.Empty);
                    tmpMiss.AddItem("No Team Deaths", "", noTeamDamage ? MissionPassedScreen.TickboxState.Tick : MissionPassedScreen.TickboxState.Cross);
                    tmpMiss.Show();
                    tmpMiss.Draw();
                    
                    if (Game.IsControlJustPressed(0, Control.FrontendAccept))
                    {
                        Game.FadeScreenOut(700);
                        Wait(1500);
                        StopMission();
                    }
                }
            }

            else if (_currentMission != null && MissionFailed && Game.Player.Character.IsAlive)
            {
                var tmp = new MissionFailedScreen(MissionFailedReason);
                tmp.Show();
                tmp.Draw();
                Function.Call(Hash.DISPLAY_RADAR, false);
                GUI.DisEnableControls(false, false);

                if (Game.IsControlJustPressed(0, Control.PhoneSelect))
                {
                    Game.FadeScreenOut(700);
                    Wait(1500);
                    Function.Call(Hash.DISPLAY_RADAR, true);
                    Function.Call(Hash._STOP_SCREEN_EFFECT, "DeathFailOut");
                    StopMission();
                }
            }
        }

        public bool MissionFailed = false;
        public string MissionFailedReason = "";

        public static string FormatTimeSpan(int seconds, bool useHours = true)
        {
            int minutes = Convert.ToInt32(Math.Floor(seconds / 60f));
            int hours = Convert.ToInt32(Math.Floor(minutes/60f));
            int realsecs = seconds%60;
            return String.Format(useHours ? "{0}:{1}:{2}" : "{1}:{2}", hours.ToString("00"), minutes.ToString("00"), realsecs.ToString("00"));
        }


        private int? _stopMission = null;
        private CustomMission _currentMission;
        private CustomMission _briefingMission;
        private int _briefingIndex = 1000;
        private Vector3 oldPos;

        public void OnKeyDown(object sender, KeyEventArgs key)
        {
            if (key.KeyCode == Keys.Decimal)
            {
                File.AppendAllText("scripts\\NOOSEMissions\\Coordinates.txt", String.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\" heading=\"{3}\"\n", Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, Game.Player.Character.Heading));
            }
        }

        public void OnSquadMenuSelect(UIMenu sender, UIMenuItem item, int index)
        {
            switch (index)
            {
                case 0: //Hold position
                    if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "swat"))
                        Function.Call(Hash.REQUEST_ANIM_DICT, "swat");
                    foreach (var teammate in _mates.Where(teammate => !teammate.Character.IsInVehicle()))
                    {
                        teammate.Character.Task.ClearAll();
                        teammate.Character.Task.GuardCurrentPosition();
                    }
                    if(!_currentMission.IsDefusingBomb && !Function.Call<bool>(Hash.IS_PED_IN_COVER, Game.Player.Character.Handle, 0))
                        Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character.Handle, "swat", "freeze", 8f, -8f, 2000, 145, 8f, 0,0,0);
                    break;
                case 1: //Regroup
                    if(!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "swat"))
                        Function.Call(Hash.REQUEST_ANIM_DICT, "swat");
                    foreach (var teammate in _mates.Where(teammate => !teammate.Character.IsInVehicle()))
                    {
                        teammate.Character.Task.ClearAll();
                        teammate.Character.Task.RunTo(Game.Player.Character.Position.Around(3f));
                    }
                    if (!_currentMission.IsDefusingBomb && !Function.Call<bool>(Hash.IS_PED_IN_COVER, Game.Player.Character.Handle, 0))
                        Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character.Handle, "swat", "rally_point", 8f, -8f, 1500, 145, 8f, 0, 0, 0);
                    break;
                case 2: //Engage
                    if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "swat"))
                        Function.Call(Hash.REQUEST_ANIM_DICT, "swat");
                    foreach (var teammate in _mates.Where(teammate => !teammate.Character.IsInVehicle()))
                    {
                        teammate.Character.Task.ClearAll();
                        teammate.Character.Task.FightAgainstHatedTargets(300f);
                    }
                    if (!_currentMission.IsDefusingBomb && !Function.Call<bool>(Hash.IS_PED_IN_COVER, Game.Player.Character.Handle, 0))
                        Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character.Handle, "swat", "go_fwd", 8f, -8f, 2000, 145, 8f, 0, 0, 0);
                    break;
                case 4: // Stop Assault
                    StopMission();
                    break;
            }
        }
    }

}
