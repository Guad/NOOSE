using System.Collections.Generic;
using GTA.Math;

namespace NOOSE
{
    public enum EntryTypes
    {
        Teleport,
        Rappel,
        Plane,
    }

    public class EntryPoint
    {
        public string Name { get; private set; }
        public Vector3 Position { get; private set; }
        public EntryTypes Type { get; private set; }

        public float Heading { get; private set; }

        public List<Vector3> Helipads { get; private set; }
        
        public Vector3 PlaneSpawn { get; private set; }
        public float PlaneSpawnHeading { get; private set; }
        public Vector3 Approach { get; private set; }
        public Vector3 RunwayStart { get; private set; }
        public Vector3 RunwayEnd { get; private set; }

        public EntryPoint(string name, Vector3 pos, EntryTypes type, float heading) : this(name, pos, type, new List<Vector3>(), new Vector3(0,0,0), 0f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), heading)
        {
            
        }

        public EntryPoint(string name, Vector3 pos, EntryTypes type, List<Vector3> helipads) : this(name, pos, type, helipads, new Vector3(0, 0, 0), 0f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0f)
        { }

        public EntryPoint(string name, Vector3 pos, EntryTypes type, Vector3 planeSpawn,
            float planeSpawnHeading, Vector3 approach, Vector3 runwayStart, Vector3 runwayEnd)
            : this(name, pos, type, new List<Vector3>(), planeSpawn, planeSpawnHeading, approach, runwayStart, runwayEnd, 0f)
        {
        }

        public EntryPoint(string name, Vector3 pos, EntryTypes type, List<Vector3> helipads, Vector3 planeSpawn,
            float planeSpawnHeading, Vector3 approach, Vector3 runwayStart, Vector3 runwayEnd, float heading)
        {
            Name = name;
            Position = pos;
            Type = type;

            Heading = heading;
            
            Helipads = new List<Vector3>(helipads);

            PlaneSpawn = planeSpawn;
            PlaneSpawnHeading = planeSpawnHeading;
            Approach = approach;
            RunwayStart = runwayStart;
            RunwayEnd = runwayEnd;
        }
    }
}