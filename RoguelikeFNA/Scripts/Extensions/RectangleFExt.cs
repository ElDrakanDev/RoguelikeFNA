using Nez;


namespace RoguelikeFNA
{
    public static class RectangleFExt
    {
        public static RectangleF Transformed(this RectangleF rect, Transform transform)
        {
            var transformed = new RectangleF();
            transformed.Location = rect.Location * transform.Scale + transform.Position;
            transformed.Size = rect.Size * transform.Scale;

            if(transformed.Width < 0)
            {
                transformed.X += transformed.Width;
                transformed.Width *= -1;
            }
            if(transformed.Height < 0)
            {
                transformed.Y += transformed.Height;
                transformed.Height *= -1;
            }
            return transformed;
        }
    }
}
