using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using Tenuto.Asteroids.Actors;
using Windows.System;

namespace Tenuto.Asteroids
{
    public class Engine
    {
        private readonly Ship _ship;
        private bool _isLeftPressed;
        private bool _isRightPressed;
        private bool _isAccelerationPressed;
        private int _firePressed;
        private bool _gameOver;
        private bool _gameWon;
        private int _lives;
        private Ship[] _lifeShips = new Ship[3];
        private List<Projectile> _projectiles = new List<Projectile>();
        private List<Asteroid> _asteroids = new List<Asteroid>();

        public Engine()
        {
            // Initilize the main ship
            _ship = new Ship();

            // Initializes 6 big asteroids
            for (int i = 0; i < 6; i++)
            {
                _asteroids.Add(new Asteroid());
            }

            // Initializes 3 ships representing lives left. 
            //These are purely for drawing the lives left on the screen, we don't actually control them or check for collisions
            _lives = 3;
            for (int i = 0; i < _lives; i++)
            {
                var lifeShip = new Ship(i);
                _lifeShips[i] = lifeShip;
            }

            // Reset keys
            _firePressed = 0;
        }

        internal void Advance(float elapsedTime)
        {
            ////////////////
            // Ship control
            ////////////////
            if (!_ship.IsExploded)
            {
                if (_isLeftPressed)
                {
                    _ship.ApplyLeftRotation(elapsedTime);
                }

                if (_isRightPressed)
                {
                    _ship.ApplyRightRotation(elapsedTime);
                }

                if (_isAccelerationPressed)
                {
                    _ship.ApplyAcceleration(elapsedTime);
                }

                if (_firePressed == 1)
                {
                    if (_projectiles.Count < 20) // allow max 20 projectiles on the screen
                    {
                        // If we pressed fire, we create a projectile, 
                        // starting from the position of the ship and going in the direction the ship is faced
                        var projectile = new Projectile(_ship.Position, _ship.Rotation);
                        _projectiles.Add(projectile);
                    }

                    _firePressed = 2; // do not keep firing when the key remains pressed 
                }
            }


            //////////
            // Ship  
            /////////
            _ship.Advance(elapsedTime);
            if (_ship.IsExploded && _ship.ExplosionTime > 0.5)
            {
                // If the ship is exploded and 0.5 seconds has passed, we reset it and decrease the lives.
                _ship.Reset();
                _lives--;
                if (_lives < 0)
                {
                    // Game Over
                    _gameOver = true;
                    _gameWon = false;
                }
            }

            /////////////////////
            // Projectile logic
            /////////////////////
            foreach (var projectile in _projectiles)
            {
                // Move the projectiles
                projectile.Advance(elapsedTime);
            }

            // Remove projectiles that are out
            _projectiles.RemoveAll(p => p.IsOut());

            //////////////////
            // Asteroid logic 
            //////////////////
            foreach (var asteroid in _asteroids)
            {
                // Move the asteroids
                asteroid.Advance(elapsedTime);

            }

            // Remove exploded asteroids from the list (0.5 s after explosion)
            _asteroids.RemoveAll(a => a.Size == 0 && a.ExplosionTime > 0.5);

            if (_asteroids.Count == 0)
            {
                // You won!
                _gameOver = true;
                _gameWon = true;
            }

            ProjectileToAsteroidCollision();

            // Ship to asteroid collisions
            // If the ship is already exploded, it doesn't matter
            if (!_ship.IsExploded)
            {
                // We go through all the steroids
                foreach (var asteroid in _asteroids)
                {
                    var distance = Math.Pow(asteroid.Position.X - _ship.Position.X, 2)
                                  + Math.Pow(asteroid.Position.Y - _ship.Position.Y, 2);
                     // Asteroid's size + ship's size
                    var size = Math.Pow(asteroid.Size * Asteroid.AsteroidSizeMultiplier + 5, 2);

                    // If we have a collision
                    if (distance < size)
                    {
                        // Ship explosion
                        _ship.Explode();

                        break;
                    }
                }
            }
        }

        private void ProjectileToAsteroidCollision()
        {
            // Projectile to asteroid collisions
            bool foundCollision = false;
            Projectile hitProjectile = null;
            Asteroid hitAsteroid = null;
            // We go through each asteroid and projectile to check for collisions
            foreach (var asteroid in _asteroids)
            {
                if (foundCollision)
                    break;

                foreach (var projectile in _projectiles)
                {
                    if (foundCollision)
                        break; // One collision detection per frame is enough

                    // Distance between the center of the asteroid and projectile
                    double distance = Math.Pow(asteroid.Position.X - projectile.Position.X, 2)
                                    + Math.Pow(asteroid.Position.Y - projectile.Position.Y, 2);
                    // Size of the asteroid
                    double size = Math.Pow(asteroid.Size * Asteroid.AsteroidSizeMultiplier, 2) * 1.2;

                    if (distance < size)
                    { // We have a collision

                        hitProjectile = projectile;

                        // Explode asteroid and create 2 new if needed
                        if (asteroid.Size > 1)
                        {
                            hitAsteroid = asteroid;
                        }
                        else
                        {
                            // If asteroid size was 1, we set it to explosion mode
                            asteroid.Explode();
                        }

                        foundCollision = true;
                    }

                }
            }

            if (foundCollision)
            {
                if (hitProjectile != null)
                {
                    _projectiles.Remove(hitProjectile);
                }

                if (hitAsteroid != null)
                {
                    // If the asteroid's size is higher than 1, we can split it into 2
                    // That means creating 2 smaller asteroids and removing this one
                    var cSpeed = hitAsteroid.Velocity;

                    // New asteroid 1
                    Vector2 newSpeed1;
                    newSpeed1.X = cSpeed.Y * 1.5f;
                    newSpeed1.Y = cSpeed.X * 1.5f;

                    var newAsteroid1 = new Asteroid(hitAsteroid.Position, hitAsteroid.Size / 2, newSpeed1);

                    // New asteroid 2
                    Vector2 newSpeed2;
                    newSpeed2.X = -cSpeed.Y * 1.5f;
                    newSpeed2.Y = -cSpeed.X * 1.5f;
                    var newAsteroid2 = new Asteroid(hitAsteroid.Position, hitAsteroid.Size / 2, newSpeed2);

                    _asteroids.Remove(hitAsteroid);
                    _asteroids.Add(newAsteroid1);
                    _asteroids.Add(newAsteroid2);
                }
            }
        }

        internal void Draw(CanvasDrawingSession ds)
        {
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(ds);
            }

            if (!_gameOver || _gameWon)
            {
                _ship.Draw(ds);
            }

            foreach (var asteroid in _asteroids)
            {
                asteroid.Draw(ds);
            }

            for (int i = 0; i < _lives; i++)
            {
                _lifeShips[i].Draw(ds);
            }

            if (_gameOver)
            {
                if (_gameWon)
                {
                    // TODO
                }
                else
                {
                    // TODO
                }
            }
        }

        internal void KeyDown(VirtualKey key)
        {
            SetKeyPressed(key, true);
        }

        internal void KeyUp(VirtualKey key)
        {
            SetKeyPressed(key, false);
        }

        private void SetKeyPressed(VirtualKey key, bool isPressed)
        {
            if (_gameOver || _gameWon)
                return;

            switch (key)
            {
                case VirtualKey.Left:
                    _isLeftPressed = isPressed;
                    break;
                case VirtualKey.Right:
                    _isRightPressed = isPressed;
                    break;
                case VirtualKey.Up:
                    _isAccelerationPressed = isPressed;
                    break;
                case VirtualKey.Space:
                    if (isPressed)
                    {
                        if (_firePressed == 0)
                            _firePressed = 1;
                    }
                    else
                    {
                        if (_firePressed == 2)
                            _firePressed = 0;
                    }
                    break;
            }
        }

    }
}
