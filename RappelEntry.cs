using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using System.IO;
using NativeUI;


namespace NOOSE
{
    public class RappelEntry
    {
        private Vehicle heli;
        private Ped pilot;
        private Vector3 _target;

        public RappelEntry(Vector3 helipad, Vector3 target)
        {
            _target = target;
            int counter = 0;
            var mav = new Model(VehicleHash.Polmav);
            do
            {
                mav.Request();
                Script.Yield();
                counter++;
            } while (!mav.IsLoaded && counter < 20000);
            counter = 0;
            var pedmod = new Model(PedHash.Pilot01SMY);
            do
            {
                pedmod.Request();
                Script.Yield();
                counter++;
            } while (!pedmod.IsLoaded && counter < 20000);
            heli = Function.Call<Vehicle>(Hash.CREATE_VEHICLE, mav.Hash, helipad.X, helipad.Y, helipad.Z, 0f, false, false);
            heli.Livery = 0;
            pilot = Function.Call<Ped>(Hash.CREATE_PED, 26, pedmod.Hash, helipad.X, helipad.Y, helipad.Z, 0f, false, false);
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, pilot.Handle, 1);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, pilot.Handle, 0, 0);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, pilot.Handle, 17, 1);
            Function.Call(Hash.SET_PED_INTO_VEHICLE, pilot.Handle, heli.Handle, (int)VehicleSeat.Driver);
            /*
            void TASK_HELI_MISSION(Ped ped, Vehicle vehicle, Any p2, Any p3,
              float posX, float posY, float posZ, int mode, float speed1,
                float radius, float heading, int height1, int height2, float p13, int p14)

            modes:
            0, 1 - no takeoff?
            4 - Get height first, then fly mantaining heading
            6 - fly to point first, achieve heading later.
            20 - ignore height
            */
        }

        private List<Ped> _backupPeds = new List<Ped>(); 

        public void AddPassenger(Ped ped)
        {
            var ped1 = heli.GetPedOnSeat(VehicleSeat.LeftRear);
            var ped2 = heli.GetPedOnSeat(VehicleSeat.RightRear);

            if (ped1 == null) Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, heli.Handle, (int)VehicleSeat.LeftRear);
            else if (ped2 == null)
                Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, heli.Handle, (int) VehicleSeat.RightRear);
            else
            {
                ped.Position = heli.Position + heli.ForwardVector*10f;
                ped.FreezePosition = true;
                ped.IsVisible = false;
                if(ped.CurrentBlip != null)
                    ped.CurrentBlip.Alpha = 0;
                _backupPeds.Add(ped);
            }
        }

        public void Start()
        {
            Function.Call(Hash.TASK_HELI_MISSION, pilot.Handle, heli.Handle, 0, 0, _target.X, _target.Y, _target.Z, 6, 40f, 1f, 36f, 15, 15, -1f, 1);
            _disembarkStarted = false;
            _disembarkTime = Noose.SecondCount;
            EntryInProgress = true;
            _totalTimeToWait = (15 + (15*Convert.ToInt32(Math.Ceiling(_backupPeds.Count/2m))));
        }

        private bool _disembarkStarted;
        private int _disembarkTime;

        private int _timeToWait = 15;
        private int _totalTimeToWait;

        public bool EntryInProgress = false;

        public void Tick()
        {
            if (!EntryInProgress) return;
            if (heli.IsInRangeOf(_target, 5f) && heli.Speed < 1f && !_disembarkStarted)
            {
                var ped1 = heli.GetPedOnSeat(VehicleSeat.LeftRear);
                var ped2 = heli.GetPedOnSeat(VehicleSeat.RightRear);

                if (ped1 != null) Function.Call((Hash)678138103285225033, ped1.Handle, 1);
                if (ped2 != null) Function.Call((Hash)678138103285225033, ped2.Handle, 1);
                _disembarkStarted = true;
                _disembarkTime = Noose.SecondCount;
            }

            if (_disembarkStarted && Noose.SecondCount - _disembarkTime > _totalTimeToWait)
            {
                EntryInProgress = false;
                Function.Call(Hash.TASK_HELI_MISSION, pilot.Handle, heli.Handle, 0, 0, 0f, 0f, 0f, 6, 40f, 1f, 36f, 15, 15, -1f, 1);
            }

            if (_backupPeds.Count > 0 && _disembarkStarted && Noose.SecondCount - _disembarkTime > _timeToWait)
            {
                _backupPeds[0].FreezePosition = false;
                _backupPeds[0].IsVisible = true;
                if (_backupPeds[0].CurrentBlip != null)
                    _backupPeds[0].CurrentBlip.Alpha = 255;
                Function.Call(Hash.SET_PED_INTO_VEHICLE, _backupPeds[0].Handle, heli.Handle, (int)VehicleSeat.LeftRear);
                Function.Call((Hash)678138103285225033, _backupPeds[0].Handle, 1);
                if (_backupPeds.Count >= 2)
                {
                    _backupPeds[1].FreezePosition = false;
                    _backupPeds[1].IsVisible = true;
                    if (_backupPeds[1].CurrentBlip != null)
                        _backupPeds[1].CurrentBlip.Alpha = 255;
                    Function.Call(Hash.SET_PED_INTO_VEHICLE, _backupPeds[1].Handle, heli.Handle,
                        (int) VehicleSeat.RightRear);
                    Function.Call((Hash)678138103285225033, _backupPeds[1].Handle, 1);
                    _backupPeds.RemoveAt(1);
                }
                _backupPeds.RemoveAt(0);
                _timeToWait += 15;
            }
            

            if (!heli.IsInRangeOf(Game.Player.Character.Position, 200f) && _disembarkStarted)
            {
                pilot.Delete();
                heli.Delete();
                _disembarkStarted = false;
                EntryInProgress = false;
                return;
            }

            if (pilot.IsDead || heli.IsDead)
            {
                pilot.Delete();
                heli.Delete();
                _disembarkStarted = false;
                EntryInProgress = false;
                return;
            }
        }

        public void Clean()
        {
            pilot?.Delete();
            heli?.Delete();
        }
    }
}