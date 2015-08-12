using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NOOSE
{
    public class PlaneEntry
    {
        private Vehicle _plane;
        private Ped _pilot;
        private Vector3 _runwayStart;
        private Vector3 _runwayEnd;
        private Vector3 _corridor;

        public PlaneEntry(Vector3 spawnPoint, float spawnHeading, Vector3 approach, Vector3 runwayStart, Vector3 runwayEnd)
        {
            _runwayStart = runwayStart;
            _runwayEnd = runwayEnd;
            _corridor = approach;

            var mav = new Model(VehicleHash.Velum2);
            mav.Request(10);
            var pedmod = new Model(PedHash.Pilot01SMY);
            pedmod.Request(10);

            _plane = World.CreateVehicle(mav, spawnPoint, spawnHeading);
            //_plane.Livery = 0;
            _pilot = World.CreatePed(pedmod, spawnPoint);

            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, _pilot.Handle, 1);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, _pilot.Handle, 0, 0);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, _pilot.Handle, 17, 1);

            Function.Call(Hash.SET_PED_INTO_VEHICLE, _pilot.Handle, _plane.Handle, (int)VehicleSeat.Driver);
            _plane.MarkAsNoLongerNeeded();
            
        }

        public void AddPassenger(Ped ped)
        {
            var ped1 = _plane.GetPedOnSeat(VehicleSeat.Passenger);
            var ped2 = _plane.GetPedOnSeat(VehicleSeat.LeftRear);
            var ped3 = _plane.GetPedOnSeat(VehicleSeat.RightRear);
            var ped4 = _plane.GetPedOnSeat((VehicleSeat)3);
            if (ped1 == null) Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, _plane.Handle, (int)VehicleSeat.Passenger);
            else if (ped2 == null) Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, _plane.Handle, (int) VehicleSeat.LeftRear);
            else if (ped3 == null) Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, _plane.Handle, (int)VehicleSeat.RightRear);
            else if (ped4 == null) Function.Call(Hash.SET_PED_INTO_VEHICLE, ped.Handle, _plane.Handle, 3);
        }

        public void Start()
        {
            _pilot.BlockPermanentEvents = true;
            Function.Call(Hash.TASK_PLANE_MISSION, _pilot.Handle, _plane.Handle, 0, 0, _corridor.X, _corridor.Y, _corridor.Z, 4, 30f, 50f, -1f, 100, 20);
            disembarkStarted = false;
            EntryInProgress = true;
        }

        private bool disembarkStarted;

        public bool EntryInProgress = false;

        public void Tick()
        {
            if (!EntryInProgress) return;

            if (_plane.IsInRangeOf(_corridor, 10f))
            {
                Function.Call(Hash.TASK_PLANE_LAND, _pilot.Handle, _plane.Handle, _runwayStart.X, _runwayStart.Y, _runwayStart.Z, _runwayEnd.X, _runwayEnd.Y, _runwayEnd.Z);
                UI.Notify("~b~ATC:~w~ Cleared to land, over.");
                disembarkStarted = true;
            }

            if (_plane.Speed < 1f && disembarkStarted)
            {
                var ped1 = _plane.GetPedOnSeat(VehicleSeat.Passenger);
                var ped2 = _plane.GetPedOnSeat(VehicleSeat.LeftRear);
                var ped3 = _plane.GetPedOnSeat(VehicleSeat.RightRear);
                var ped4 = _plane.GetPedOnSeat((VehicleSeat)3);

                ped1?.Task.LeaveVehicle();
                ped2?.Task.LeaveVehicle();
                ped3?.Task.LeaveVehicle();
                ped4?.Task.LeaveVehicle();
                EntryInProgress = false;
            }
            
            if (!_plane.IsInRangeOf(Game.Player.Character.Position, 200f) && disembarkStarted)
            {
                _pilot.Delete();
                _plane.Delete();
                disembarkStarted = false;
                EntryInProgress = false;
                return;
            }

            if (_pilot.IsDead || _plane.IsDead)
            {
                _pilot.Delete();
                _plane.Delete();
                disembarkStarted = false;
                EntryInProgress = false;
                return;
            }
        }

        public void Clean()
        {
            _pilot?.Delete();
            _plane?.Delete();
        }
    }
}
 