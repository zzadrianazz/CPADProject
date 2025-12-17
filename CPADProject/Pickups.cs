using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPADProject
{
    public class Pickups
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Size { get; private set; } = 70;
        public Image Visual { get; private set; }
        public PickupType Type { get; private set; }

        private double speed = 3.0;

        public enum PickupType
        {
            Coin,
            Fuel
        }
        public Pickups(double x, double y, PickupType type)
        {
            X = x;
            Y = y;
            Type = type;

            string image = type == PickupType.Coin
                ? "coin.png"
                : "fuel.png";

            Visual = new Image
            {
                Source = image,
                WidthRequest = Size,
                HeightRequest = Size
            };
        }

        public void Update(double screenWidth, double screenHeight)
        {
            Y += speed;
        }
    }
}

