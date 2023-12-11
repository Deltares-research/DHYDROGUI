/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 23/06/2022
 * Time: 10:08
 * 
 * To change this template use Tools > Options > Coding > Edit standard headers.
 */
using System;
using System.Globalization;
using Ranorex;

namespace DHYDRO.Code
{
    /// <summary>
    /// Description of MapCalibration.
    /// </summary>
    public static class MapCalibration
    {
        public static void Execute()
        {
            Mouse.DefaultMoveTime = 300;
            Keyboard.DefaultKeyPressTime = 0;
            Delay.SpeedFactor = 0.0;
            
            var centralMap = DHYDRO1D2DRepository.Instance.DSWindow.ListView.CentralMapContainer.CentralMap.Element.ScreenRectangle;
            var mapWidth = centralMap.Width;
            var mapHeight = centralMap.Height;

            var pixelPointUL = new Point(10, 10);
            var worldPointUL = GetWorldPointWithRetry(pixelPointUL);

            var pixelPointLR = new Point(mapWidth - 10, mapHeight - 10);
            var worldPointLR = GetWorldPointWithRetry(pixelPointLR);

            Current.MapTransformation = new Transformation(pixelPointUL, worldPointUL, pixelPointLR, worldPointLR);
        }

        private static Point GetWorldPointWithRetry(Point pixelsPoint)
        {
            for (var i = 0; i < 5; i++)
            {
                var worldPoint = GetWorldPoint(pixelsPoint);

                if (worldPoint.IsEmpty)
                {
                    Report.Warn("Failed to retrieve map coordinates, retrying...");
                    Delay.Duration(200, false);
                }
                else
                {
                    return worldPoint;
                }
            }
            
            return Point.Empty;
        }
        
        private static Point GetWorldPoint(Point pixelsPoint)
        {
            DHYDRO1D2DRepository.Instance.DSWindow.ListView.CentralMapContainer.CentralMap.MoveTo($"{pixelsPoint.X};{pixelsPoint.Y}");
            Delay.Duration(20, false);
            
            // The <statusBarText> will be in the following form:
            // "Current map coordinates: 123.123, 456.456" or "Current map coordinates: 123,123, 456,456" depending on the Culture.
                
            var statusBarText = DHYDRO1D2DRepository.Instance.DSWindow.ListView.StatusBar.Element.GetAttributeValueText("Text");
            Report.Info("statusBarText= " + statusBarText );

            return string.IsNullOrEmpty(statusBarText) ? Point.Empty : ToPoint(statusBarText);
        }

        private static Point ToPoint(string coordinates)
        {
            var split = coordinates.Split(new[] {"coordinates:", ", "}, StringSplitOptions.RemoveEmptyEntries);
            var x = split[1].Trim();
            var y = split[2].Trim();

            return new Point(ToDouble(x), ToDouble(y));
        }

        private static double ToDouble(string doubleStr)
        {
            return double.Parse(doubleStr, CultureInfo.CurrentCulture);
        }
    }
}
