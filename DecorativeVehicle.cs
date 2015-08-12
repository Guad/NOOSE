using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE
{
    public class DecorativeVehicle
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public VehicleHash Model { get; set; }
        public int EntryPoint { get; set; }
        public bool SirenActive { get; set; }
        
        public List<VehicleDoor> OpenDoors { get; set; } = new List<VehicleDoor>();

        public DecorativeVehicle(VehicleHash hash, Vector3 position, float heading, int entry, bool sirenActive, string doors)
        {
            Position = position;
            Heading = heading;
            Model = hash;
            EntryPoint = entry;
            SirenActive = sirenActive;

            if (doors[0] == '1') OpenDoors.Add(VehicleDoor.FrontLeftDoor);
            if (doors[1] == '1') OpenDoors.Add(VehicleDoor.FrontRightDoor);
            if (doors[2] == '1') OpenDoors.Add(VehicleDoor.BackLeftDoor);
            if (doors[3] == '1') OpenDoors.Add(VehicleDoor.BackRightDoor);
        } 
    }
}