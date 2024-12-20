namespace RLB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            GameSettings gameSettings = GameAcrobot.Create();
            FFMPEG ffmpeg = new FFMPEG("output.gif", 30, FFMPEG.EncodeAs.GIF, gameSettings);

            float[] action = [0, 0, 0];
            float reward = float.NaN;
            bool ended = false;
            bool terminated = false;
            gameSettings.reset(in random, out int step, out float[] state, out float[] observation);
            while (!ended)
            {
                action[0] = random.NextSingle();
                action[1] = random.NextSingle();
                action[2] = random.NextSingle();
                gameSettings.render(in random, in step, in state, in observation, in action, out byte[,,] pixels);
                ffmpeg.AddFrame(pixels);
                gameSettings.stepInPlace(in random, ref step, ref state, ref observation, in action, ref reward, ref ended, ref terminated);
            }
            ffmpeg.Finish();
        }
    }
}
