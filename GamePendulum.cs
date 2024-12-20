namespace RLB
{
    public static class GamePendulum
    {
        public const int STATE_SIZE = 2;
        public const int OBSERVATION_SIZE = 3;
        public const int ACTION_SIZE = 1;
        public static int RENDER_WIDTH = 500;
        public static int RENDER_HEIGHT = 500;
        public static int MAX_STEPS = 200;
        public static float MAX_SPEED = 8f;
        public static float MAX_TORQUE = 2.0f;
        public static float DT = 0.05f;
        public static float G = 10.0f;
        public static float M = 1.0f;
        public static float L = 1.0f;
        public static float[] ACTION_MINS = new float[] { -MAX_TORQUE };
        public static float[] ACTION_MAXS = new float[] { MAX_TORQUE };

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
            observation[0] = MathF.Sin(state[0]);
            observation[1] = MathF.Cos(state[0]);
            observation[2] = state[1];
        }

        public static void ResetInPlace(in Random random, ref int step, ref float[] state, ref float[] observation)
        {
            step = 0;
            state[0] = Utility.RandomRange(random, -MathF.PI, MathF.PI);
            state[1] = Utility.RandomRange(random, -1f, 1f);
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
            float theta = state[0];
            float thetaDot = state[1];
            float torque = Utility.Clip(action[0], -MAX_TORQUE, MAX_TORQUE);
            float normalizedAngle = (theta + MathF.PI) % (2 * MathF.PI) - MathF.PI;
            reward = -((normalizedAngle * normalizedAngle) + (0.1f * thetaDot * thetaDot) + (0.001f * torque * torque));
            float newThetaDot = Utility.Clip(thetaDot + (3 * G / (2 * L) * MathF.Sin(theta) + 3.0f / (M * L * L) * torque) * DT, -MAX_SPEED, MAX_SPEED);
            float newTheta = theta + newThetaDot * DT;
            state[0] = newTheta;
            state[1] = newThetaDot;
            StateToObservation(in state, ref observation);
            step++;
            ended = step >= MAX_STEPS;
            terminated = false;
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
            Bitmap.Clear(pixels, 255, 255, 255);
            
            // find center
            int cx = RENDER_WIDTH / 2;
            int cy = RENDER_HEIGHT / 2;
            
            // fix angle
            float angle = state[0] + MathF.PI;

            // find end of pendulum
            int x1 = cx + (int)(MathF.Sin(angle) * L * 150);
            int y1 = cy + (int)(MathF.Cos(angle) * L * 150);

            // draw cross in center
            Bitmap.DrawLine(pixels, cx - 20, cy, cx + 20, cy, 0, 0, 0);
            Bitmap.DrawLine(pixels, cx, cy - 20, cx, cy + 20, 0, 0, 0);

            // draw pendulum line
            Bitmap.DrawLine(pixels, cx, cy, x1, y1, 255, 0, 0);

            // draw circle around center
            Bitmap.DrawCircle(pixels, cx, cy, 10, 0, 0, 0);

            // draw action indicator
            Bitmap.DrawContinuousActionIndicators(pixels, action, ACTION_MINS, ACTION_MAXS);
        }

        public static void Render(in Random random, in int step, in float[] state, in float[] observation, in float[] action, out byte[,,] pixels)
        {
            pixels = new byte[RENDER_WIDTH, RENDER_HEIGHT, 3];
            RenderInPlace(in random, in step, in state, in observation, in action, ref pixels);
        }
    }
}
