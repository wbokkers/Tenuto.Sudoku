using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Tenuto.Asteroids
{
    public sealed partial class AsteroidsGame : UserControl
    {
        private CanvasBitmap _bgImage;
        private bool _resourcesCreated;

        private readonly Engine _engine = new Engine();

        public AsteroidsGame()
        {
            this.InitializeComponent();
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        private void CanvasTapped(object sender, TappedRoutedEventArgs e)
        {
            //  _ship.Explode();
        }

        private void CanvasControl_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            var elapsedTime = (float)args.Timing.ElapsedTime.TotalSeconds;

            if (!_resourcesCreated)
            {
                ds.DrawText("Loading...", 200, 200, Colors.Yellow, new CanvasTextFormat() { FontWeight = FontWeights.Bold, FontSize = 100f });
                return;
            }

            ds.DrawImage(_bgImage, 0, 0);


            _engine.Advance(elapsedTime);
            _engine.Draw(ds);


            // remove all the bullets that are not longer on the screen
            //    _bullets.RemoveAll(bullet => !bullet.IsInBounds(0, 0, DesignWidth, DesignHeight));
        }

        private async void CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            _bgImage = await CanvasBitmap.LoadAsync(sender, @"./Asteroids/background-space.jpg");
            _resourcesCreated = true;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _engine.KeyDown(e.Key);
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            _engine.KeyUp(e.Key);
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            canvas.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
    }
}
