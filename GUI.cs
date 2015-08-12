using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using Font = GTA.Font;

namespace NOOSE
{
    public static class GUI
    {
        public static void DrawHUD(Teammate[] mates, int seconds = -1)
        {
            string[] photos = new[]
            {
                "hc_n_gus",
                "hc_n_kar",
                "hc_n_edd",
                "hc_n_pac",
            };

            SizeF res = UIMenu.GetScreenResolutionMantainRatio();
            Point safe = UIMenu.GetSafezoneBounds();
            new Sprite("heisthud", "main_gradient", new Point(Convert.ToInt32(res.Width) - safe.X - 300, Convert.ToInt32(res.Height) - safe.Y - (seconds == -1 ? 370 : 470)), new Size(400, (seconds == -1 ? 300 : 400)), 90f, Color.White).Draw();
            for (int i = 0; i < mates.Length; i++)
            {
                new UIResRectangle(new Point(Convert.ToInt32(res.Width) - 100 - safe.X, Convert.ToInt32(res.Height) - 110 - safe.Y - 100 * i), new Size(100, 20), Color.FromArgb(200, 57, 98, 116)).Draw();
                if (mates[i].Character.IsAlive)
                    new UIResRectangle(new Point(Convert.ToInt32(res.Width) - 100 - safe.X, Convert.ToInt32(res.Height) - 110 - safe.Y - 100 * i), new Size(Convert.ToInt32((mates[i].Character.Armor / 200f) * 100), 20), Color.FromArgb(200, 75, 140, 172)).Draw();


                new UIResRectangle(new Point(Convert.ToInt32(res.Width) - 100 - safe.X, Convert.ToInt32(res.Height) - 140 - safe.Y - 100*i), new Size(100, 20), Color.FromArgb(200, 60, 103, 52)).Draw();
                if(mates[i].Character.IsAlive)
                    new UIResRectangle(new Point(Convert.ToInt32(res.Width) - 100 - safe.X, Convert.ToInt32(res.Height) - 140 - safe.Y - 100 * i), new Size(Convert.ToInt32((mates[i].Character.Health/200f)*100), 20), Color.FromArgb(200, 90, 155, 85)).Draw();
                

                var photoSprite = new Sprite("heisthud", photos[i], new Point(Convert.ToInt32(res.Width) - 180 - safe.X, Convert.ToInt32(res.Height) - 150 - safe.Y - 100*i), new Size(70, 70));
                photoSprite.Draw();
                if (mates[i].Character.IsDead)
                {
                    new Sprite("heisthud", "kiaoverlay", new Point(Convert.ToInt32(res.Width) - 180 - safe.X, Convert.ToInt32(res.Height) - 130 - safe.Y - 100*i),
                        new Size(70, 50)).Draw();
                }
            }
            if(seconds != -1)
                DrawBombTimer(seconds);
        }

        public static void DrawBombTimer(int seconds)
        {
            SizeF res = UIMenu.GetScreenResolutionMantainRatio();
            Point safe = UIMenu.GetSafezoneBounds();
            string timeStr = Noose.FormatTimeSpan(seconds, false);
            if (seconds == -2) timeStr = "~r~XX:XX";
            new Sprite("char_detonatebomb", "char_detonatebomb", new Point(Convert.ToInt32(res.Width) - 180 - safe.X, Convert.ToInt32(res.Height) - safe.Y - 450), new Size(70, 70)).Draw();
            new UIResText(timeStr, new Point(Convert.ToInt32(res.Width) - 100 - safe.X, Convert.ToInt32(res.Height) - safe.Y - 440), 0.65f).Draw();
        }

        public static bool InBriefing { get; set; }
        
        private static Camera _mainCamera;
        public static int SelectedEntry = 0;
        public static void DrawBriefing(CustomMission currentMission)
        {
            UI.ShowSubtitle("Choose an entry point.", 10);
            if (currentMission.EntryPoints.Length - 1 < SelectedEntry)
                SelectedEntry = 0;
            Function.Call(Hash._SHOW_CURSOR_THIS_FRAME);
            DisEnableControls(false, true);
            currentMission.DrawBriefingMap();
            Game.Player.Character.Position = currentMission.CameraPosition + new Vector3(0, 0, 10f);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, Game.Player.Character.Handle, true);
            if (_mainCamera == null || (_mainCamera.Position != currentMission.CameraPosition))
            {
                World.DestroyAllCameras();
                _mainCamera = World.CreateCamera(currentMission.CameraPosition, new Vector3(0, 0, 0), 60f);
            }
            _mainCamera.PointAt(currentMission.CameraPosition - new Vector3(0,0, 30f));
            World.RenderingCamera = _mainCamera;
            SizeF res = UIMenu.GetScreenResolutionMantainRatio();

            for (int i = 0; i < currentMission.EntryPoints.Length; i++)
            {
                Vector3 entryPoint = currentMission.EntryPoints[i];

                var xPointer = new OutputArgument();
                var yPointer = new OutputArgument();
                bool successful = Function.Call<bool>(Hash._WORLD3D_TO_SCREEN2D, entryPoint.X, entryPoint.Y, entryPoint.Z, xPointer,
                    yPointer);
                var x = xPointer.GetResult<float>()*res.Width;
                var y = yPointer.GetResult<float>()*res.Height;

                if (!successful)
                {
                    x = 64f;
                    y = res.Height - 64f - 64*i;
                }

                bool hovering = UIMenu.IsMouseInBounds(new Point(Convert.ToInt32(x) - 32, Convert.ToInt32(y) - 32),
                    new Size(64, 64));

                new Sprite("trafficcam", SelectedEntry == i ? "traffic_junction_box" : "cursor",
                    new Point(Convert.ToInt32(x) - 32, Convert.ToInt32(y) - 32), new Size(64, 64), 0f, SelectedEntry == i ? Color.DarkGreen : Color.DarkRed)
                    .Draw();

                if (Game.IsControlPressed(0, GTA.Control.Attack) && hovering)
                    SelectedEntry = i;
                new UIResText(currentMission.EntryNames[i], new Point(Convert.ToInt32(x) + 32, Convert.ToInt32(y) - 32), 1f, Color.White, Font.Monospace,
                    UIResText.Alignment.Left).Draw();
            }
            new UIResText(currentMission.MissionName, new Point(Convert.ToInt32(res.Width), 20), 3f, Color.White,
                GTA.Font.Pricedown, UIResText.Alignment.Right).Draw();
            
            DrawScaleform();
            Function.Call(Hash.FREEZE_ENTITY_POSITION, Game.Player.Character.Handle, false);
        }

        public static void DisEnableControls(bool enable, bool menuEnalbe)
        {
            Hash thehash = enable ? Hash.ENABLE_CONTROL_ACTION : Hash.DISABLE_CONTROL_ACTION;
            foreach (var con in Enum.GetValues(typeof(Control)))
            {
                Function.Call(thehash, 0, (int)con);
            }

            if (!menuEnalbe) return;
            var list = new List<Control>
            {
                Control.FrontendAccept,
                Control.FrontendAxisX,
                Control.FrontendAxisY,
                Control.FrontendDown,
                Control.FrontendUp,
                Control.FrontendLeft,
                Control.FrontendRight,
                Control.FrontendCancel,
                Control.FrontendSelect,
                Control.CursorScrollDown,
                Control.CursorScrollUp,
                Control.CursorX,
                Control.CursorY,
                Control.MoveUpDown,
                Control.MoveLeftRight,
                Control.Sprint,
                Control.Jump,
                Control.Enter,
                Control.VehicleExit,
                Control.VehicleAccelerate,
                Control.VehicleBrake,
                Control.VehicleMoveLeftRight,
                Control.VehicleFlyYawLeft,
                Control.FlyLeftRight,
                Control.FlyUpDown,
                Control.VehicleFlyYawRight,
                Control.VehicleHandbrake,
            };
            foreach (var control in list)
            {
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, (int)control);
            }
        }

        public static void DrawScaleform()
        {
            var scaleform = new Scaleform(0);
            scaleform.Load("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL");
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            scaleform.CallFunction("CREATE_CONTAINER");

            scaleform.CallFunction("SET_DATA_SLOT", 0, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.Jump, 0), "Begin Assault");
            scaleform.CallFunction("SET_DATA_SLOT", 1, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.FrontendCancel, 0), "Cancel");
            scaleform.CallFunction("SET_DATA_SLOT", 2, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.PhoneRight, 0), "");
            scaleform.CallFunction("SET_DATA_SLOT", 3, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.PhoneLeft, 0), "Change Mission");
            scaleform.CallFunction("SET_DATA_SLOT", 4, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.FrontendRb, 0), "");
            scaleform.CallFunction("SET_DATA_SLOT", 5, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.FrontendLb, 0), "Change Entry Point");
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            scaleform.Render2D();
        }
    }
}