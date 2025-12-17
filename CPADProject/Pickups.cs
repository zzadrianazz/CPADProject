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
        private Random random = new Random();

        private double velocityX;
        private double velocityY;
        private double speed = 3.0;

        public Pickups(double x, double y)
        {
            X = x;
            Y = y;

            string coinIMG = "coin.png";
            Visual = new Image()
            {
                Source = coinIMG,
                WidthRequest = Size,
                HeightRequest = Size,
            };
        }

        public void Update(double screenWidth, double screenHeight)
        {
            Y += speed;
        }
    }
}

