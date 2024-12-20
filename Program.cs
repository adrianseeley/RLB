namespace RLB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int width = 500;
            int height = 500;
            byte[,,] pixels = new byte[height, width, 3]; // [y, x, rgb]

            // Clear the canvas with a white background
            Bitmap.Clear(pixels, 255, 255, 255);

            // Draw some shapes
            Bitmap.DrawLine(pixels, 10, 10, 190, 10, 0, 0, 0);            // horizontal line
            Bitmap.DrawLine(pixels, 10, 10, 10, 190, 0, 0, 0);            // vertical line
            Bitmap.DrawLine(pixels, 10, 190, 190, 10, 255, 0, 0);         // diagonal line
            Bitmap.DrawRect(pixels, 50, 50, 100, 60, 0, 0, 255);          // rectangle
            Bitmap.DrawCircle(pixels, 100, 100, 40, 0, 255, 0);           // circle

            Bitmap.FillRect(pixels, 150, 150, 100, 60, 255, 0, 20);         // filled rectangle
            Bitmap.FillCircle(pixels, 200, 200, 30, 0, 255, 50);           // filled circle

            // Save to BMP
            Bitmap.SaveToBMP("output.bmp", pixels);
        }
    }
}
