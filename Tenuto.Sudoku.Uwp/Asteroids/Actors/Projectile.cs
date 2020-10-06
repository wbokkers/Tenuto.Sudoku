using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.UI;

namespace Tenuto.Asteroids.Actors
{
    public class Projectile
    {
        private const float ProjectileSpeed = 400;
        private Vector2 _position;
        private Vector2 _velocity;

        public Projectile(Vector2 position, float rotation)
        {
            _position = position;

            _velocity.X = (float)Math.Sin(rotation) * ProjectileSpeed;
            _velocity.Y = -(float)Math.Cos(rotation) * ProjectileSpeed;
        }

        public Vector2 Position => _position;

        internal void Advance(float elapsedTime)
        {
            // Projectile moves
            _position.X += elapsedTime * _velocity.X;
            _position.Y += elapsedTime * _velocity.Y;
        }

        internal bool IsOut()
        {
            // Returns true if the projectile is out of the screen area so we can remove it
            return _position.X < -10 || _position.X > GameConstants.DesignWidth + 10
                || _position.Y < -10 || _position.Y > GameConstants.DesignHeight + 10;
        }

        public void Draw(CanvasDrawingSession ds)
        {
            ds.FillCircle(_position.X, _position.Y, 5, Colors.Red);
        }
    }
}
