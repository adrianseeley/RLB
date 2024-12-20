namespace RLB
{
    public static class Utility
    {
        public static float RandomRange(Random random, float min, float max)
        {
            return (random.NextSingle() * (max - min)) + min;
        }

        public static float[] RandomAction(Random random, GameSettings gameSettings)
        {
            float[] action = new float[gameSettings.actionSize];
            for (int a = 0; a < action.Length; a++)
            {
                action[a] = RandomRange(random, gameSettings.actionMins[a], gameSettings.actionMaxs[a]);
            }
            return action;
        }

        public static void RandomActionInPlace(Random random, GameSettings gameSettings, ref float[] action)
        {
            for (int a = 0; a < action.Length; a++)
            {
                action[a] = RandomRange(random, gameSettings.actionMins[a], gameSettings.actionMaxs[a]);
            }
        }

        public static float Clip(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float Wrap(float value, float min, float max)
        {
            // wrap x so m <= x <= M
            float diff = max - min;
            while (value > max)
            {
                value -= diff;
            }
            while (value < min)
            {
                value += diff;
            }
            return value;
        }
    }
}
