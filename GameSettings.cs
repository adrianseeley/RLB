namespace RLB
{
    public class GameSettings
    {
        public delegate void ResetInPlaceDelegate(in Random random, ref int step, ref float[] state, ref float[] observation);
        public delegate void ResetDelegate(in Random random, out int step, out float[] state, out float[] observation);
        public delegate void StepInPlaceDelegate(in Random random, ref int step, ref float[] state, ref float[] observation, in float[] action, ref float reward, ref bool ended, ref bool terminated);
        public delegate void StepDelegate(in Random random, in int step, in float[] state, in float[] observation, in float[] action, out int nextStep, out float[] nextState, out float[] nextObservation, out float reward, out bool ended, out bool terminated);
        public delegate void RenderInPlaceDelegate(in Random random, in int step, in float[] state, in float[] observation, in float[] action, ref byte[,,] pixels);
        public delegate void RenderDelegate(in Random random, in int step, in float[] state, in float[] observation, in float[] action, out byte[,,] pixels);

        public int stateSize;
        public int observationSize;
        public int actionSize;
        public float[] actionMins;
        public float[] actionMaxs;
        public int renderWidth;
        public int renderHeight;
        public ResetInPlaceDelegate resetInPlace;
        public ResetDelegate reset;
        public StepInPlaceDelegate stepInPlace;
        public StepDelegate step;
        public RenderInPlaceDelegate renderInPlace;
        public RenderDelegate render;

        public GameSettings(int stateSize, int observationSize, int actionSize, float[] actionMins, float[] actionMaxs, int renderWidth, int renderHeight, ResetInPlaceDelegate resetInPlace, ResetDelegate reset, StepInPlaceDelegate stepInPlace, StepDelegate step, RenderInPlaceDelegate renderInPlace, RenderDelegate render)
        {
            this.stateSize = stateSize;
            this.observationSize = observationSize;
            this.actionSize = actionSize;
            this.actionMins = actionMins;
            this.actionMaxs = actionMaxs;
            this.renderWidth = renderWidth;
            this.renderHeight = renderHeight;
            this.resetInPlace = resetInPlace;
            this.reset = reset;
            this.stepInPlace = stepInPlace;
            this.step = step;
            this.renderInPlace = renderInPlace;
            this.render = render;
        }
    }
}
