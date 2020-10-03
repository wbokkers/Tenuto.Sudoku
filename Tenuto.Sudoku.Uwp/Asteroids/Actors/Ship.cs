using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.UI;

namespace Tenuto.Asteroids.Actors
{
    public class Ship
    {
        private Vector2 _velocity;
        private Vector2 _position;

        public Ship()
        {
            Reset();
        }

        public Ship(int lifeNo)
        {
            // This is used for the "lives" ships, just to display the number of lives left in the corner of the screen
            Reset();

            // Sets position in the corner of the screen
            _position.X = 30 + lifeNo * 30;
            _position.Y = 40;
        }

        public Vector2 Position => _position;

        public bool IsExploded { get; private set; }
        public float Rotation { get; private set; }
        public float ExplosionTime { get; private set; }

        public void ApplyLeftRotation(float elapsedTime)
        {
            Rotation -= elapsedTime * (float)Math.PI;  // rad
        }
        public void ApplyRightRotation(float elapsedTime)
        {
            Rotation += elapsedTime * (float)Math.PI;  // rad
        }

        public void Advance(float elapsedTime)
        {
            // s = s0 + v.dt

            _position.X += elapsedTime * _velocity.X;

            if (_position.X < -10)
            {
                _position.X = GameConstants.DesignWidth + 10;
            }
            if (_position.X > GameConstants.DesignWidth + 10)
            {
                _position.X = -10;
            }

            _position.Y += elapsedTime * _velocity.Y;
            if (_position.Y < -10)
            {
                _position.Y = GameConstants.DesignHeight + 10;
            }
            if (_position.Y > GameConstants.DesignHeight + 10)
            {
                _position.Y = -10;
            }


            if (IsExploded)
            {
                ExplosionTime += elapsedTime;
            }
        }

        public void Draw(CanvasDrawingSession ds)
        {
            if (!IsExploded)
            {
                var sinr = (float)Math.Sin(Rotation);
                var cosr = (float)Math.Cos(Rotation);
                var sin120 = (float)Math.Sin(Rotation + 120 * Math.PI / 180);
                var cos120 = (float)Math.Cos(Rotation + 120 * Math.PI / 180);
                var sinmin120 = (float)Math.Sin(Rotation - 120 * Math.PI / 180);
                var cosmin120 = (float)Math.Cos(Rotation - 120 * Math.PI / 180);
                var points = new Vector2[3];
                points[0] = new Vector2(_position.X + 40 * sinr, _position.Y - 40 * cosr);
                points[1] = new Vector2(_position.X + 20 * sinmin120, _position.Y - 20 * cosmin120);
                points[2] = new Vector2(_position.X + 20 * sin120, _position.Y - 20 * cos120);

                var geom = CanvasGeometry.CreatePolygon(ds, points);
                ds.FillGeometry(geom, Colors.Red);
            }
            else
            {
                // If it's in explosion mode, we draw 9 red points moving away from the center, simulating an explosion
                int angleStep = 360 / Asteroid.AsteroidCorners;
                for (int i = 0; i < Asteroid.AsteroidCorners; i++)
                {
                    ds.FillCircle(
                        _position.X + ExplosionTime * 120 * (float)Math.Sin(i * angleStep *Math.PI / 180),
                        _position.Y - ExplosionTime * 120 * (float)Math.Cos(i * angleStep * Math.PI / 180),
                        4, Colors.Yellow);
                 }
            }
        }

        public void Explode()
        {
            IsExploded = true;
            ExplosionTime = 0;
        }

        internal void ApplyAcceleration(float elapsedTime)
        {
            // This accellerates the ship forward. We also cap the speed
            // added accel = 300 pixels per s^2 
            // v = v0 + a.dt 

            // accelerate in direction of the nose, not in the x-direction, so
            // we need to take rotation into account.

            // x-component of acceleration
            _velocity.X += 300 * elapsedTime * (float)Math.Sin(Rotation);
            if (_velocity.X > 150)
            {
                _velocity.X = 150;
            }
            else if (_velocity.X < -150)
            {
                _velocity.X = -150;
            }

            // y-component of acceleration
            _velocity.Y -= 300 * elapsedTime * (float)Math.Cos(Rotation);
            if (_velocity.Y > 150)
            {
                _velocity.Y = 150;
            }
            else if (_velocity.Y < -150)
            {
                _velocity.Y = -150;
            }
        }

        //internal bool IsInBounds(float x, float y, float width, float height)
        //{
        //    if (Position.X < x || Position.Y < y)
        //        return false;

        //    if (Position.X > (x + width) || Position.Y > (y + height))
        //        return false;

        //    return true;
        //}

        public void Reset()
        {
            // Position in the center of the screen
            _position.X = GameConstants.DesignWidth / 2;
            _position.Y = GameConstants.DesignHeight / 2;

            // Speed = 0, the ship initially doesn't move
            _velocity.X = 0;
            _velocity.Y = 0;

            Rotation = 0;

            // The ship is not exploded ... yet
            IsExploded = false;
            ExplosionTime = 0;
        }

    }
}
