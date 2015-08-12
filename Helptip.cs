using System;
using System.Drawing;
using GTA;
using GTA.Native;
using NativeUI;

namespace NOOSE
{
    public class Helptip
    {
        public String Text { get; set; }
        public int Time { get; set; }
        public bool Visible { get; set; }

        private DateTime _timeStarted;

        private UIResRectangle _bg;
        private UIResText _text;

        public Helptip(string text) : this(text, 7000)
        {
        }

        public Helptip(string text, int duration)
        {
            _timeStarted = DateTime.Now;
            Time = duration;

            Point safe = UIMenu.GetSafezoneBounds();
            _text = new UIResText(FormatDescription(text, 400), new Point(safe.X + 10, safe.Y + 5), 0.38f);
            
            _bg = new UIResRectangle(new Point(safe.X, safe.Y), new Size(410, 37), Color.FromArgb(200, 0,0,0));

            Visible = true;
        }

        public void Draw()
        {
            if (!Visible) return;
            if (DateTime.Now.Subtract(_timeStarted).TotalMilliseconds >= Time)
            {
                Visible = false;
                return;
            }
            _bg.Size = new Size(410, 45 + 30 * (_text.Caption.Split('\n').Length-1));

            _bg.Draw();
            _text.Draw();
        }

        private string FormatDescription(string input, int maxWidth)
        {
            int maxPixelsPerLine = maxWidth;
            int aggregatePixels = 0;
            string output = "";
            string[] words = input.Split(' ');
            foreach (string word in words)
            {
                Function.Call((Hash)0x54CE8AC98E120CAB, "jamyfafi");
                UIResText.AddLongString(word);
                int screenw = Game.ScreenResolution.Width;
                int screenh = Game.ScreenResolution.Height;
                const float height = 1080f;
                float ratio = (float)screenw / screenh;
                var width = height * ratio;
                int offset = Convert.ToInt32(Function.Call<float>((Hash)0x85F061DA64ED2F67, 0) * width * 0.38f);

                aggregatePixels += Convert.ToInt32(offset);
                if (aggregatePixels > maxPixelsPerLine)
                {
                    output += "\n" + word + " ";
                    aggregatePixels = Convert.ToInt32(offset);
                }
                else
                {
                    output += word + " ";
                }
            }
            return output;
        }
    }
}