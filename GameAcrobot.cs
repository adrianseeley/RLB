namespace RLB
{
    public static class GameAcrobot
    {
        public const int STATE_SIZE = 4;
        public const int OBSERVATION_SIZE = 6;
        public const int ACTION_SIZE = 3;
        public static int RENDER_WIDTH = 500;
        public static int RENDER_HEIGHT = 500;
        public static int MAX_STEPS = 500;
        public static float DT = 0.2f;
        public static float LINK_LENGTH_1 = 1.0f;
        public static float LINK_LENGTH_2 = 1.0f;
        public static float LINK_MASS_1 = 1.0f;
        public static float LINK_MASS_2 = 1.0f;
        public static float LINK_COM_POS_1 = 0.5f;
        public static float LINK_COM_POS_2 = 0.5f;
        public static float LINK_MOI = 1.0f;
        public static float MAX_VEL_1 = 4 * MathF.PI;
        public static float MAX_VEL_2 = 9 * MathF.PI;
        public static float G = 9.8f;
        public static float TORQUE_NOISE_MAX = 0f;
        public static float[] TORQUES = new float[] { -1f, 0f, 1f };
        public static float[] ACTION_MINS = new float[] { 0f, 0f, 0f };
        public static float[] ACTION_MAXS = new float[] { 1f, 1f, 1f };

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
            float theta1 = state[0];
            float theta2 = state[1];
            float dtheta1 = state[2];
            float dtheta2 = state[3];
            observation[0] = MathF.Cos(theta1);
            observation[1] = MathF.Sin(theta1);
            observation[2] = MathF.Cos(theta2);
            observation[3] = MathF.Sin(theta2);
            observation[4] = dtheta1;
            observation[5] = dtheta2;
        }

        public static void ResetInPlace(in Random random, ref int step, ref float[] state, ref float[] observation)
        {
            step = 0;
            state[0] = Utility.RandomRange(random, -0.1f, 0.1f);
            state[1] = Utility.RandomRange(random, -0.1f, 0.1f);
            state[2] = Utility.RandomRange(random, -0.1f, 0.1f);
            state[3] = Utility.RandomRange(random, -0.1f, 0.1f);
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
            float torque = TORQUES[aIndex];
            float theta1 = state[0];
            float theta2 = state[1];
            float dtheta1 = state[2];
            float dtheta2 = state[3];
            float[] s = new float[4] { theta1, theta2, dtheta1, dtheta2 };
            float[] s_aug = new float[5] { theta1, theta2, dtheta1, dtheta2, torque };
            float[] nextState = RK4(s_aug, DT);
            float ntheta1 = Utility.Wrap(nextState[0], -MathF.PI, MathF.PI);
            float ntheta2 = Utility.Wrap(nextState[1], -MathF.PI, MathF.PI);
            float ndtheta1 = Utility.Clip(nextState[2], -MAX_VEL_1, MAX_VEL_1);
            float ndtheta2 = Utility.Clip(nextState[3], -MAX_VEL_2, MAX_VEL_2);
            state[0] = ntheta1;
            state[1] = ntheta2;
            state[2] = ndtheta1;
            state[3] = ndtheta2;
            StateToObservation(in state, ref observation);
            step++;
            bool doneGoal = (-MathF.Cos(ntheta1) - MathF.Cos(ntheta2 + ntheta1)) > 1.0f;
            bool doneTime = step >= MAX_STEPS;
            if (doneGoal)
            {
                reward = 0.0f;
                ended = true;
                terminated = true;
            }
            else if (doneTime)
            {
                reward = -1.0f;
                ended = true;
                terminated = false;
            }
            else
            {
                reward = -1.0f;
                ended = false;
                terminated = false;
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

            // define a scale to draw the links
            float scale = 100f;
            int cx = RENDER_WIDTH / 2;
            int cy = RENDER_HEIGHT / 2;

            float theta1 = state[0];
            float theta2 = state[1];

            // compute link endpoints
            // link 1 end
            int x1 = cx + (int)(MathF.Sin(theta1) * LINK_LENGTH_1 * scale);
            int y1 = cy + (int)(MathF.Cos(theta1) * LINK_LENGTH_1 * scale);

            // link 2 end
            int x2 = x1 + (int)(MathF.Sin(theta1 + theta2) * LINK_LENGTH_2 * scale);
            int y2 = y1 + (int)(MathF.Cos(theta1 + theta2) * LINK_LENGTH_2 * scale);

            // draw the pivot as a small circle
            Bitmap.DrawCircle(pixels, cx, cy, 5, 0, 0, 0);

            // draw first link
            Bitmap.DrawLine(pixels, cx, cy, x1, y1, 255, 0, 0);

            // draw second link
            Bitmap.DrawLine(pixels, x1, y1, x2, y2, 255, 0, 0);

            // draw target line: when (-cos(theta1)-cos(theta2+theta1))>1 means success
            // since max height is 2 (fully up), threshold at 1 means halfway up.
            // we draw a horizontal line at cy - scale to indicate the target height
            Bitmap.DrawLine(pixels, 0, cy - (int)scale, RENDER_WIDTH - 1, cy - (int)scale, 255, 0, 0);

            // draw action indicator
            // we have 3 actions: index 0 (-1 torque), index 1 (0 torque), index 2 (1 torque)
            // select the max index
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

        private static float[] RK4(float[] s_aug, float dt)
        {
            float[] y0 = new float[5];
            Array.Copy(s_aug, y0, 5);

            // we apply 4 steps
            float[] k1 = DSDT(y0);
            float[] k2Input = Add(y0, Mul(k1, dt * 0.5f));
            float[] k2 = DSDT(k2Input);
            float[] k3Input = Add(y0, Mul(k2, dt * 0.5f));
            float[] k3 = DSDT(k3Input);
            float[] k4Input = Add(y0, Mul(k3, dt));
            float[] k4 = DSDT(k4Input);

            // yout = y0 + dt/6*(k1+2*k2+2*k3+k4)
            float[] sum = Add(k1, Add(Mul(k2, 2f), Add(Mul(k3, 2f), k4)));
            float[] yout = Add(y0, Mul(sum, dt / 6f));

            // we only return the first 4 elements (the state)
            float[] result = new float[4];
            Array.Copy(yout, 0, result, 0, 4);
            return result;
        }

        private static float[] DSDT(float[] s_aug)
        {
            float m1 = LINK_MASS_1;
            float m2 = LINK_MASS_2;
            float l1 = LINK_LENGTH_1;
            float l2 = LINK_LENGTH_2;
            float lc1 = LINK_COM_POS_1;
            float lc2 = LINK_COM_POS_2;
            float I1 = LINK_MOI;
            float I2 = LINK_MOI;
            float g = G;
            float a = s_aug[4];
            float theta1 = s_aug[0];
            float theta2 = s_aug[1];
            float dtheta1 = s_aug[2];
            float dtheta2 = s_aug[3];

            float d1 = m1 * lc1 * lc1 + m2 * (l1 * l1 + lc2 * lc2 + 2 * l1 * lc2 * MathF.Cos(theta2)) + I1 + I2;
            float d2 = m2 * (lc2 * lc2 + l1 * lc2 * MathF.Cos(theta2)) + I2;
            float phi2 = m2 * lc2 * g * MathF.Cos(theta1 + theta2 - MathF.PI / 2);
            float phi1 = -m2 * l1 * lc2 * dtheta2 * dtheta2 * MathF.Sin(theta2)
                         - 2 * m2 * l1 * lc2 * dtheta1 * dtheta2 * MathF.Sin(theta2)
                         + (m1 * lc1 + m2 * l1) * g * MathF.Cos(theta1 - MathF.PI / 2)
                         + phi2;

            // book version
            float denominator = (m2 * lc2 * lc2 + I2 - (d2 * d2 / d1));
            float ddtheta2 = (a + (d2 / d1) * phi1 - m2 * l1 * lc2 * dtheta1 * dtheta1 * MathF.Sin(theta2) - phi2) / denominator;
            float ddtheta1 = -(d2 * ddtheta2 + phi1) / d1;

            return new float[] { dtheta1, dtheta2, ddtheta1, ddtheta2, 0f };
        }

        private static float[] Add(float[] a, float[] b)
        {
            float[] c = new float[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] + b[i];
            }
            return c;
        }

        private static float[] Mul(float[] a, float scalar)
        {
            float[] c = new float[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] * scalar;
            }
            return c;
        }
    }
}
