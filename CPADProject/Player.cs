using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPADProject
{
    public class Player
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; private set; } = 100;

        public Image Visual { get; set; }


        public double Rotation
        {
            get
            {
                return Visual.Rotation;
            }
        }

        public Player(double x, double y)
        {
            X = x;
            Y = y;

            Visual = new Image()
            {
                Source = "car.png",
                WidthRequest = Size,
                HeightRequest = Size,
            };
        }

        public void MoveTo(double targetX, double targetY)
        {
            X = targetX;
            Y = targetY;
        }

    }
}
