namespace RLB
{
    public static class GameCartPole
    {
        public const int STATE_SIZE = 4;
        public const int OBSERVATION_SIZE = 4;
        public const int ACTION_SIZE = 2;
        public static int RENDER_WIDTH = 400;
        public static int RENDER_HEIGHT = 400;
        public static int MAX_STEPS = 500;
        public static float GRAVITY = 9.8f;
        public static float MASS_CART = 1.0f;
        public static float MASS_POLE = 0.1f;
        public static float LENGTH = 0.5f;
        public static float FORCE_MAG = 10.0f;
        public static float TAU = 0.02f;
        public static float TOTAL_MASS = MASS_CART + MASS_POLE;
        public static float POLE_MASS_LENGTH = MASS_POLE * LENGTH;
        public static float THETA_THRESHOLD_RADS = 12f * 2f * MathF.PI / 360f;
        public static float X_THRESHOLD = 2.4f;
        public static float[] ACTION_MINS = new float[] { 0f, 0f };
        public static float[] ACTION_MAXS = new float[] { 1f, 1f };

        public static GameSettings Create()
        {
            return new GameSettings(
                stateSize: STATE_SIZE,
                observationSize: OBSERVATION_SIZE,
                actionSize: ACTION_SIZE,
                actionMins: ACTION_MINS,
                actionMaxs: ACTION_MAXS,
                renderWidth: RENDER_WIDTH,
                renderHeight: RENDER_HEIGHT,
                resetInPlace: ResetInPlace,
                reset: Reset,
                stepInPlace: StepInPlace,
                step: Step,
                renderInPlace: RenderInPlace,
                render: Render
            );
        }

        public static void StateToObservation(in float[] state, ref float[] observation)
        {
            observation[0] = state[0];
            observation[1] = state[1];
            observation[2] = state[2];
            observation[3] = state[3];
        }

        public static void ResetInPlace(in Random random, ref int step, ref float[] state, ref float[] observation)
        {
            step = 0;
            state[0] = Utility.RandomRange(random, -0.05f, 0.05f);
            state[1] = Utility.RandomRange(random, -0.05f, 0.05f);
            state[2] = Utility.RandomRange(random, -0.05f, 0.05f);
            state[3] = Utility.RandomRange(random, -0.05f, 0.05f);
            StateToObservation(in state, ref observation);
        }

        public static void Reset(in Random random, out int step, out float[] state, out float[] observation)
        {
            step = -1;
            state = new float[STATE_SIZE];
            observation = new float[OBSERVATION_SIZE];
            ResetInPlace(in random, ref step, ref state, ref observation);
        }

        public static void StepInPlace(in Random random, ref int step, ref float[] state, ref float[] observation, in float[] action, ref float reward, ref bool ended, ref bool terminated)
        {
            int aIndex = 0;
            float maxVal = action[0];
            for (int i = 1; i < ACTION_SIZE; i++)
            {
                if (action[i] > maxVal)
                {
                    maxVal = action[i];
                    aIndex = i;
                }
            }
            float force = (aIndex == 1) ? FORCE_MAG : -FORCE_MAG;
            float x = state[0];
            float x_dot = state[1];
            float theta = state[2];
            float theta_dot = state[3];
            float costheta = MathF.Cos(theta);
            float sintheta = MathF.Sin(theta);
            float temp = (force + POLE_MASS_LENGTH * (theta_dot * theta_dot) * sintheta) / TOTAL_MASS;
            float thetaacc = (GRAVITY * sintheta - costheta * temp) / (LENGTH * (4.0f / 3.0f - MASS_POLE * (costheta * costheta) / TOTAL_MASS));
            float xacc = temp - (POLE_MASS_LENGTH * thetaacc * costheta / TOTAL_MASS);
            x = x + TAU * x_dot;
            x_dot = x_dot + TAU * xacc;
            theta = theta + TAU * theta_dot;
            theta_dot = theta_dot + TAU * thetaacc;
            state[0] = x;
            state[1] = x_dot;
            state[2] = theta;
            state[3] = theta_dot;
            StateToObservation(in state, ref observation);
            step++;
            bool outOfBounds = (x < -X_THRESHOLD) || (x > X_THRESHOLD) || (theta < -THETA_THRESHOLD_RADS) || (theta > THETA_THRESHOLD_RADS);
            bool timeUp = step >= MAX_STEPS;
            if (outOfBounds || timeUp)
            {
                ended = true;
                terminated = outOfBounds;
                reward = 1.0f;
            }
            else
            {
                ended = false;
                terminated = false;
                reward = 1.0f;
            }
        }

        public static void Step(in Random random, in int step, in float[] state, in float[] observation, in float[] action, out int nextStep, out float[] nextState, out float[] nextObservation, out float reward, out bool ended, out bool terminated)
        {
            nextState = new float[STATE_SIZE];
            Array.Copy(state, nextState, STATE_SIZE);
            nextObservation = new float[OBSERVATION_SIZE];
            nextStep = step;
            reward = float.NaN;
            ended = false;
            terminated = false;
            StepInPlace(in random, ref nextStep, ref nextState, ref nextObservation, in action, ref reward, ref ended, ref terminated);
        }

        public static void RenderInPlace(in Random random, in int step, in float[] state, in float[] observation, in float[] action, ref byte[,,] pixels)
        {
            // clear background
            Bitmap.Clear(pixels, 255, 255, 255);

            float x = state[0];
            float theta = state[2];
            float world_width = X_THRESHOLD * 2f;
            float scale = RENDER_WIDTH / world_width;
            float cartY = RENDER_HEIGHT / 2;
            float cartX = x * scale + (RENDER_WIDTH / 2f);
            float cartWidth = 50f;
            float cartHeight = 30f;
            float poleLen = 100;

            // draw line cart is on
            Bitmap.DrawLine(pixels, 0, (int)cartY, RENDER_WIDTH, (int)cartY, 0, 0, 0);

            // draw cart rect in brown
            Bitmap.DrawRect(pixels, (int)(cartX - cartWidth / 2), (int)(cartY - cartHeight / 2), (int)cartWidth, (int)cartHeight, 139, 69, 19);

            // draw pole 
            float poleTopX = cartX + MathF.Sin(theta) * poleLen;
            float poleTopY = cartY - MathF.Cos(theta) * poleLen;
            Bitmap.DrawLine(pixels, (int)cartX, (int)cartY, (int)poleTopX, (int)poleTopY, 255, 0, 0);

            // draw action indicator (we have 2 actions)
            int aIndex = 0;
            float maxVal = action[0];
            for (int i = 1; i < ACTION_SIZE; i++)
            {
                if (action[i] > maxVal)
                {
                    maxVal = action[i];
                    aIndex = i;
                }
            }

            Bitmap.DrawArgmaxActionIndicator(pixels, aIndex, ACTION_SIZE);
        }

        public static void Render(in Random random, in int step, in float[] state, in float[] observation, in float[] action, out byte[,,] pixels)
        {
            pixels = new byte[RENDER_WIDTH, RENDER_HEIGHT, 3];
            RenderInPlace(in random, in step, in state, in observation, in action, ref pixels);
        }
    }
}
