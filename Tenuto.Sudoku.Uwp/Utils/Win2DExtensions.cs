using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Tenuto.Sudoku.Uwp.Utils
{
    public static class Win2DExtensions
    {
        public static Vector2 ScaleFactorHD(this CanvasControl canvas)
        {
            return new Vector2((float)canvas.ActualWidth / 1920, (float)canvas.ActualHeight / 1080);
        }
        public static ICanvasEffect ScaleToFitCanvas(this CanvasBitmap bmp, CanvasControl canvas)
        {
            if (bmp == null)
                return null;
      
            var scaleW = canvas == null ? 1 : canvas.ActualWidth / bmp.Bounds.Width;
            var scaleH = canvas == null ? 1 : canvas.ActualHeight / bmp.Bounds.Height;

          
            return new ScaleEffect { Source = bmp, Scale = new Vector2((float)scaleW, (float)scaleH) };


            //return new Transform2DEffect 
            //{
            //    Source = bmp,
            //    TransformMatrix = Matrix3x2.CreateScale((float)scaleW, (float)scaleH)
            //};
        }

        public static ICanvasEffect Scale(this CanvasBitmap bmp, Vector2 sf)
        {
            return new ScaleEffect { Source = bmp, Scale = sf };
        }
    }
}
