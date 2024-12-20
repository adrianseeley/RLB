﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RLB
{
    public static class Utility
    {
        public static float RandomRange(Random random, float min, float max)
        {
            return (random.NextSingle() * (max - min)) + min;
        }

        public static float Clip(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}