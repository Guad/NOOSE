using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE
{
    public class Teammate
    {
        public Ped Character { get; set; }

        public static int NOOSEGroup = World.AddRelationshipGroup("MOD_NOOSE");

        public Teammate(Vector3 spawnpoint)
        {
            World.SetRelationshipBetweenGroups(Relationship.Respect, NOOSEGroup, Game.Player.Character.RelationshipGroup);

            World.SetRelationshipBetweenGroups(Relationship.Hate, NOOSEGroup, Enemy.EnemyGroup);
            World.SetRelationshipBetweenGroups(Relationship.Hate, Enemy.EnemyGroup, NOOSEGroup);

            var swatmodel = new Model(PedHash.Swat01SMY);
            int counter = 0;
            do
            {
                swatmodel.Request();
                Script.Yield();
                counter++;
            } while (!swatmodel.IsLoaded && counter < 10000);

            Character = Function.Call<Ped>(Hash.CREATE_PED, 26, swatmodel.Hash, spawnpoint.X, spawnpoint.Y, spawnpoint.Z, 0f, false, false);
            Character.Health = 200;
            Character.Armor = 200;
            Character.RelationshipGroup = NOOSEGroup;
            Character.AlwaysDiesOnLowHealth = false;
        }
    }
}