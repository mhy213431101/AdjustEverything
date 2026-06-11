using System.Drawing;

namespace AdjustEverything;

internal static class SurveyCoordinateMapper
{
    public static (double X, double Y) FromCanvas(PointF canvasLocation)
    {
        return (XFromCanvas(canvasLocation), YFromCanvas(canvasLocation));
    }

    public static double XFromCanvas(PointF canvasLocation)
    {
        return -canvasLocation.Y;
    }

    public static double YFromCanvas(PointF canvasLocation)
    {
        return canvasLocation.X;
    }
}
