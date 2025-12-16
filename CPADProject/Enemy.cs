using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPADProject
{
    public class Enemy
    {
        public double X {get; private set;}
        public double Y {get; private set;}
        public double Size { get; private set; } = 200;
        public Image Visual {get; private set;}
        private Random random = new Random();

        private double velocityX;
        private double velocityY;
        private double speed = 3.0;

        public Enemy(double x, double y)
        {
            X = x;
            Y = y;

            int whichBus = random.Next(1, 3);
            string busIMG = $"bus{whichBus}.png";
            Visual = new Image()
            {
                Source = busIMG,
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
