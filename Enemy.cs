using System;
using GTA;
using GTA.Math;
using GTA.Native;
using System.IO;

namespace NOOSE
{
    public class Enemy
    {
        public static Random Dice = new Random();

        public Ped Character { get; private set; }

        public static int EnemyGroup = World.AddRelationshipGroup("noose_targets");

        private readonly WeaponHash[] _guns = new[]
        {
            WeaponHash.SpecialCarbine,
            WeaponHash.APPistol,
            WeaponHash.CombatPistol,
            WeaponHash.Pistol50,
            WeaponHash.Pistol,
            WeaponHash.AdvancedRifle,
            WeaponHash.AssaultSMG,
            WeaponHash.AssaultShotgun,
            WeaponHash.BullpupShotgun,
            WeaponHash.CombatPDW,
            WeaponHash.PumpShotgun,
            WeaponHash.SawnOffShotgun,
            WeaponHash.SMG,
            WeaponHash.MicroSMG,
            WeaponHash.MG,
            WeaponHash.CombatMG,
        };

        private readonly PedHash[] _skins = new[]
        {
            PedHash.Dealer01SMY, 
            PedHash.MexGang01GMY, 
            PedHash.ChiBoss01GMM, 
            PedHash.ArmGoon01GMM, 
            PedHash.ArmGoon02GMY, 
            PedHash.ChiGoon01GMM, 
            PedHash.ChiGoon02GMM, 
            PedHash.SalvaGoon01GMY, 
            PedHash.SalvaGoon02GMY, 
            PedHash.SalvaGoon03GMY, 
            PedHash.PoloGoon01GMY, 
            PedHash.PoloGoon02GMY, 
            PedHash.MexGoon01GMY, 
            PedHash.MexGoon02GMY, 
            PedHash.MexGoon03GMY, 
        };
        
        public Enemy(Vector3 position, float heading)
        {
            var tmpMod = new Model(_skins[Dice.Next(_skins.Length)]);
            int counter = 0;
            do
            {
                tmpMod.Request();
                Script.Yield();
                counter++;
            } while (!tmpMod.IsLoaded && counter < 10000);
            int c2 = 0;
            do
            {
                Character = Function.Call<Ped>(Hash.CREATE_PED, 26, tmpMod.Hash, position.X, position.Y, position.Z, heading, false, false);
                c2++;
            } while (Character == null && c2 < 3000);
            Character.Accuracy = Dice.Next(30, 100);
            Character.Weapons.Give(_guns[Dice.Next(_guns.Length)], 200, true, true);
            var relation = EnemyGroup;
            var relation2 = Game.Player.Character.RelationshipGroup;
            World.SetRelationshipBetweenGroups(Relationship.Hate, relation, relation2);
            World.SetRelationshipBetweenGroups(Relationship.Hate, relation2, relation);
            Character.RelationshipGroup = relation;
        }
    }
}