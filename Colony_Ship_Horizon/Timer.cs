using Microsoft.Xna.Framework;
using System;

namespace Colony_Ship_Horizon
{
    public class Timer
    {
        public float Delay { get; private set; }
        public float TimeRemaining { get; private set; }

        public Timer(float delay)
        {
            TimeRemaining = delay;
            Delay = delay;
        }

        public bool Update(float deltaTime)
        {
            TimeRemaining += deltaTime;

            if (TimeRemaining > Delay)
            {
                TimeRemaining = 0;
                return true;
            }
            return false;
        }
    }
}