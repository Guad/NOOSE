using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using NativeUI;
using Font = GTA.Font;

namespace NOOSE
{
    public class MissionFailedScreen
    {
        public string Reason { get; set; }
        public bool Visible { get; set; }

        public MissionFailedScreen(string reason)
        {
            Reason = reason;
            Visible = false;
        }
        
        public void Show()
        {
            Visible = true;
        }

        public void Draw()
        {
            if (!Visible) return;
            
            SizeF res = UIMenu.GetScreenResolutionMantainRatio();
            int middle = Convert.ToInt32(res.Width/2);

            new Sprite("mpentry", "mp_modenotselected_gradient", new Point(0, 30), new Size(Convert.ToInt32(res.Width), 300),
                0f, Color.FromArgb(230, 255, 255, 255)).Draw();
            
            new UIResText("mission failed", new Point(middle, 100), 2.5f, Color.FromArgb(255, 148, 27, 46), Font.Pricedown, UIResText.Alignment.Centered).Draw();

            new UIResText(Reason, new Point(middle, 230), 0.5f, Color.White, Font.ChaletLondon, UIResText.Alignment.Centered).Draw();
            
            var scaleform = new Scaleform(0);
            scaleform.Load("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL");
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            scaleform.CallFunction("CREATE_CONTAINER");

            scaleform.CallFunction("SET_DATA_SLOT", 0, Function.Call<string>(Hash._0x0499D7B09FC9B407, 2, (int)Control.FrontendAccept, 0), "Continue");
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            scaleform.Render2D();
            if (Game.IsControlJustPressed(0, Control.FrontendAccept))
            {
                Game.PlaySound("SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
        }
    }
}