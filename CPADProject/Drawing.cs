using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace CPADProject
{
    internal class Drawing : IDrawable
    {
        public double daylight { get; set; } // sky colour change
        public double scrollEffect { get; set; } //scrolling effect for road

        public Image Visual { get; private set; }

        public async void Draw(ICanvas canvas, RectF rect)
        {   
            //top and bottom colours of the sky
            var (top, bottom) = GetSkyColors(daylight);

            //vertical gradient colours for sky
            var gradient = new LinearGradientPaint
            {
                StartColor = top, //top colour
                EndColor = bottom, //bottom colour
                StartPoint = new Point(0, 0), //gradient starting at the top
                EndPoint = new Point(0, rect.Height) //gradient ending at the bottom
            };

            canvas.SetFillPaint(gradient, rect);
            canvas.FillRectangle(rect);

            float w = rect.Width; //canvas width
            float h = rect.Height; //canvas height

            float roadWidth = w * 0.55f; //roads width 55% of screen width
            float roadLeft = (w - roadWidth) / 2f; //position for centered road
            float roadRight = roadLeft + roadWidth; //edge of road

            //asphalt colour
            canvas.FillColor = Colors.DarkSlateGray;
            canvas.FillRectangle(roadLeft, 0, roadWidth, h);

            //stripes for road 
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 8;

            float stripeHeight = 70; //length of stripe
            float stripeGap = 90; //gap between stripes
            float cycle = stripeHeight + stripeGap; //repeat of length
            float offset = (float)(scrollEffect % cycle); //movement 

            //loop to draw centered stripes on middle of the road
            for (float y = -offset; y < h; y += cycle)
            {
                // center stripe
                canvas.DrawLine(
                    w / 2, //X start
                    y,     //Y start
                    w / 2, //X end
                    y + stripeHeight //Y end for vertical stripe
                );
            }

            //buildings trees etc
            float sideObjectWidth = 45;
            float sideObjectHeight = 100;
            float spacing = 200;

            float sideCycle = sideObjectHeight + spacing;
            float sideOffset = (float)(scrollEffect % sideCycle);

            for (float y = -sideOffset; y < h; y += sideCycle)
            {
                Visual = new Image()
                {
                    Source = "apartment.png",
                    
                    WidthRequest = sideObjectWidth,
                    HeightRequest = sideObjectHeight,
                };
            }
        }
        

        private (Color top, Color bottom) GetSkyColors(double t)
        {
            if (t < 0.25)
                return LerpSky(t / 0.25,
                    Color.FromRgb(255, 69, 0),    // sunset top orange
                    Color.FromRgb(139, 0, 0),     // sunset bottom red
                    Color.FromRgb(255, 165, 0),   // deep orange
                    Color.FromRgb(25, 25, 112));  // midnight blue

            if (t < 0.5)
                return LerpSky((t - 0.25) / 0.25,
                    Color.FromRgb(25, 25, 112),   // deep night blue
                    Color.FromRgb(0, 0, 0),       // black
                    Color.FromRgb(0, 0, 128),     // navy
                    Color.FromRgb(0, 0, 32));     // very dark blue

            if (t < 0.75)
                return LerpSky((t - 0.5) / 0.25,
                    Color.FromRgb(128, 0, 128),   // purple dawn
                    Color.FromRgb(255, 165, 0),   // orange
                    Color.FromRgb(255, 105, 180), // hot pink
                    Color.FromRgb(255, 215, 0));  // gold

            return LerpSky((t - 0.75) / 0.25,
                Color.FromRgb(135, 206, 235),     // sky blue
                Color.FromRgb(255, 215, 0),       // gold
                Color.FromRgb(0, 191, 255),       // deep sky blue
                Color.FromRgb(255, 255, 255));    // white
        }


        private (Color, Color) LerpSky(double t, Color aTop, Color aBottom, Color bTop, Color bBottom)
        {
            Color Lerp(Color a, Color b) =>
                Color.FromRgba(
                    (byte)(a.Red * 255 + (b.Red * 255 - a.Red * 255) * t),
                    (byte)(a.Green * 255 + (b.Green * 255 - a.Green * 255) * t),
                    (byte)(a.Blue * 255 + (b.Blue * 255 - a.Blue * 255) * t),
                    (byte)(a.Alpha * 255 + (b.Alpha * 255 - a.Alpha * 255) * t)
                );

            return (Lerp(aTop, bTop), Lerp(aBottom, bBottom));
        }


    }
}
