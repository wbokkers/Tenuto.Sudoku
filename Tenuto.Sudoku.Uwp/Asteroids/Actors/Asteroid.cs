using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Tenuto.Asteroids.Actors
{
    public class Asteroid
    {
        private static readonly Random _rnd = new Random();
        private const float AsteroidMaxRotation = 90f * (float)Math.PI / 180.0f;
        private const float AsteroidSizeVariation = 20;
        private const float AsteroidSpeed = 50;
        public const float AsteroidSizeMultiplier = 20;
        public const int AsteroidCorners = 9;

		private Vector2 _position;
        private Vector2 _velocity;
        private float _rotation;
        private float _rotationSpeed;
        private readonly float[] _sizeVariation = new float[AsteroidCorners];



        public Asteroid()
        {
            // Initialize position randomly on the screen
            _position.X = (float)_rnd.NextDouble() * GameConstants.DesignWidth;
            _position.Y = (float)_rnd.NextDouble() * GameConstants.DesignHeight;

            // Initialize fixed speed in a random direction
            double rotationAngle = _rnd.NextDouble() * 2.0 * Math.PI;
            _velocity.X = (float)Math.Sin(rotationAngle) * AsteroidSpeed;
            _velocity.Y = -(float)Math.Cos(rotationAngle) * AsteroidSpeed;

            // Initial size : 4
            Size = 4;

            // Initializes a random rotation speed
            _rotation = 0;
            _rotationSpeed = (float)_rnd.NextDouble() * AsteroidMaxRotation  - (AsteroidMaxRotation / 2.0f);

            // Generates random shape of the asteroid
            int variation = (int)(AsteroidSizeVariation * Size / 4);
            for (int i = 0; i < _sizeVariation.Length; i++)
            {
                _sizeVariation[i] = (float)_rnd.NextDouble() * variation - (variation / 2.0f);
            }
        }

        public Asteroid(Vector2 position, int size, Vector2 velocity)
        {
            _position = position;
            _velocity = velocity;
            Size = size;

            // Initializes a random rotation speed
            _rotation = 0;
            _rotationSpeed = (float)_rnd.NextDouble() * AsteroidMaxRotation - (AsteroidMaxRotation / 2.0f);

            // Generates random shape of the asteroid
            int variation = (int)(AsteroidSizeVariation * Size / 4);
            for (int i = 0; i < _sizeVariation.Length; i++)
            {
                _sizeVariation[i] = (float)_rnd.NextDouble() * variation - (variation / 2.0f);
            }
        }

        public int Size { get; private set; }
        public float ExplosionTime { get; private set; }
        public Vector2 Position => _position;
        public Vector2 Velocity => _velocity;

        internal void Advance(float elapsedTime)
        {
            // If size > 0, we have an asteroid moving around
            // If size == 0, our asteroid is exploded
            if (Size > 0)
            {
                // We move it around, and if it goes outside the screen, we make it pop up on the other side
                _position.X += elapsedTime * _velocity.X;
                if (_position.X < -10)
                {
                    _position.X = GameConstants.DesignWidth + 10;
                }
                else if (_position.X > GameConstants.DesignWidth + 10)
                {
                    _position.X = -10;
                }

                _position.Y += elapsedTime * _velocity.Y;
                if (_position.Y < -10)
                {
                    _position.Y = GameConstants.DesignHeight + 10;
                }
                else if (_position.Y > GameConstants.DesignHeight + 10)
                {
                    _position.Y = -10;
                }

                // We also rotate it
                _rotation += _rotationSpeed * elapsedTime;
            }
            else
            {
                // We use explosionTime to generate a visual explosion and remove it after 0.5 seconds
                ExplosionTime += elapsedTime;
            }
        }

        public void Explode()
        {
            // Asteroids goes into explosion mode
            Size = 0;
            ExplosionTime = 0;
        }

        internal void Draw(CanvasDrawingSession ds)
        {
            if (Size > 0)
            {
                // If it's not exploded, we draw the asteroid's shape


                var points = new Vector2[AsteroidCorners];
               
  
                float angleStep = 2.0f * (float)Math.PI / AsteroidCorners;
                for (int i = 0; i < AsteroidCorners; i++)
                {
                    var point = new Vector2(_position.X + (Size * AsteroidSizeMultiplier + _sizeVariation[i]) * (float)Math.Sin(_rotation + i * angleStep), 
                                            _position.Y - (Size * AsteroidSizeMultiplier + _sizeVariation[i]) * (float)Math.Cos(_rotation + i * angleStep));
                    points[i] = point;
                }

                var geom = CanvasGeometry.CreatePolygon(ds, points.ToArray());
                ds.FillGeometry(geom, Colors.Blue);
            }
            else
            {
                // In case of an explosion, we draw 9 points moving away from the center
                float angleStep = 2.0f * (float)Math.PI / AsteroidCorners;
                for (int i = 1; i < AsteroidCorners; i++)
                {
                    ds.FillCircle(_position.X + (ExplosionTime * (100 + 20 * _sizeVariation[i])) * (float)Math.Sin(i * angleStep),
                                  _position.Y - (ExplosionTime * (100 + 20 * _sizeVariation[i])) * (float)Math.Cos(i * angleStep),
                        4, Colors.Yellow);
                }
            }
        }
    }
}
