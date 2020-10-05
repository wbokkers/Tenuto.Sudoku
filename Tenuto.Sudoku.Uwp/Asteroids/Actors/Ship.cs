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
        private float _timeInGame;
        private readonly bool _isLifeLeftShip;
        public Ship()
        {
            Reset();
        }

        public Ship(int lifeNo)
        {
            // This is used for the "lives" ships, just to display the number of lives left in the corner of the screen
            Reset();
        
            // Sets position in the corner of the screen
            _position.X = 30 + lifeNo * 20;
            _position.Y = 40;
            _isLifeLeftShip = true;
        }

        public Vector2 Position => _position;

        public bool IsExploded { get; private set; }
        public bool IsGhost => _timeInGame <= GameConstants.ShipGhostTime;
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
            if (_timeInGame < 100)
                _timeInGame += elapsedTime;

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

        public float Size => 8;

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

                var len1 = _isLifeLeftShip ? 20 : 30;
                var len2 = _isLifeLeftShip ? 10 : 15;

                points[0] = new Vector2(_position.X + len1 * sinr, _position.Y - len1 * cosr);
                points[1] = new Vector2(_position.X + len2 * sinmin120, _position.Y - len2 * cosmin120);
                points[2] = new Vector2(_position.X + len2 * sin120, _position.Y - len2 * cos120);

                var geom = CanvasGeometry.CreatePolygon(ds, points);
                if (IsGhost && !_isLifeLeftShip)
                    ds.DrawGeometry(geom, Colors.Green, 3,  new CanvasStrokeStyle { DashStyle = CanvasDashStyle.DashDot, MiterLimit = 1 });
                else
                    ds.FillGeometry(geom, Colors.Green);
            }
            else
            {
                // If it's in explosion mode, we draw 20 points moving away from the center, simulating an explosion
                var noFragments = 20;
                float angleStep = 360f / noFragments;
                for (int i = 0; i < noFragments; i++)
                {
                    ds.DrawCircle(
                        _position.X + ExplosionTime * 100 * (float)Math.Sin(i * angleStep *Math.PI / 180),
                        _position.Y - ExplosionTime * 100 * (float)Math.Cos(i * angleStep * Math.PI / 180),
                       (GameConstants.ExplosionDuration-ExplosionTime)/GameConstants.ExplosionDuration * 8 , Colors.Yellow);
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
            _velocity.X += 250 * elapsedTime * (float)Math.Sin(Rotation);
            if (_velocity.X > 100)
            {
                _velocity.X = 100;
            }
            else if (_velocity.X < -100)
            {
                _velocity.X = -100;
            }

            // y-component of acceleration
            _velocity.Y -= 250 * elapsedTime * (float)Math.Cos(Rotation);
            if (_velocity.Y > 100)
            {
                _velocity.Y = 100;
            }
            else if (_velocity.Y < -100)
            {
                _velocity.Y = -100;
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
            _timeInGame = 0.0f;
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
