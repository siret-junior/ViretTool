namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class Ellipse
    {
        public enum State { All, Any }
        public State EllipseState { get; private set; }

        public double PositionX { get; private set; }
        public double PositionY { get; private set; }
        public double HorizontalAxis { get; private set; }
        public double VerticalAxis { get; private set; }
        public int Rotation { get; private set; }

        public int ColorR { get; private set; }
        public int ColorG { get; private set; }
        public int ColorB { get; private set; }


        public Ellipse(
            State ellipseState,
            double positionX, double positionY,
            double horizontalAxis, double verticalAxis, 
            int rotation, 
            int colorR, int colorG, int colorB)
        {
            EllipseState = ellipseState;
            PositionX = positionX;
            PositionY = positionY;
            HorizontalAxis = horizontalAxis;
            VerticalAxis = verticalAxis;
            Rotation = rotation;
            ColorR = colorR;
            ColorG = colorG;
            ColorB = colorB;
        }

        public override bool Equals(object obj)
        {
            return obj is Ellipse ellipse &&
                   EllipseState == ellipse.EllipseState &&
                   PositionX == ellipse.PositionX &&
                   PositionY == ellipse.PositionY &&
                   HorizontalAxis == ellipse.HorizontalAxis &&
                   VerticalAxis == ellipse.VerticalAxis &&
                   Rotation == ellipse.Rotation &&
                   ColorR == ellipse.ColorR &&
                   ColorG == ellipse.ColorG &&
                   ColorB == ellipse.ColorB;
        }

        public override int GetHashCode()
        {
            int hashCode = -1855929513;
            hashCode = hashCode * -1521134295 + EllipseState.GetHashCode();
            hashCode = hashCode * -1521134295 + PositionX.GetHashCode();
            hashCode = hashCode * -1521134295 + PositionY.GetHashCode();
            hashCode = hashCode * -1521134295 + HorizontalAxis.GetHashCode();
            hashCode = hashCode * -1521134295 + VerticalAxis.GetHashCode();
            hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorR.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorG.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorB.GetHashCode();
            return hashCode;
        }
    }
}
