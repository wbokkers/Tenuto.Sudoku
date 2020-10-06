using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Tenuto.Asteroids.Actors;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;

namespace Tenuto.Asteroids
{
    public class Engine
    {
        private readonly Syringe _syringe = new Syringe();
        private bool _isLeftPressed;
        private bool _isRestartPressed;
        private bool _isRightPressed;
        private bool _isAccelerationPressed;
        private int _firePressed;
        private bool _gameOver;
        private bool _gameWon;
        private int _lives;
        private float _startingTime;
        private readonly Syringe[] _lifeSyringes = new Syringe[GameConstants.ExtraLives];
        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private readonly List<Coronaid> _asteroids = new List<Coronaid>();

        //private readonly Stopwatch _stopwatch = new Stopwatch();
        //private int _frameCount;
        //private int _fps;

        public Engine()
        {
            ResetGame();
        }


        private void ResetGame()
        {
            //_stopwatch.Restart();
            //_frameCount = 0;

            // Initilize the main ship
            _syringe.Reset();

            // Initializes 7 big asteroids
            _asteroids.Clear();
            for (int i = 0; i < 7; i++)
            {
                _asteroids.Add(new Coronaid());
            }

            _projectiles.Clear();

            // Initializes 3 ships representing lives left. 
            //These are purely for drawing the lives left on the screen, we don't actually control them or check for collisions
            _lives = _lifeSyringes.Length;
            for (int i = 0; i < _lives; i++)
            {
                var lifeSyringe = new Syringe(i);
                _lifeSyringes[i] = lifeSyringe;
            }

            // Reset keys
            _firePressed = 0;
            _isAccelerationPressed = false;
            _isLeftPressed = false;
            _isRightPressed = false;
            _isRestartPressed = false;

            _startingTime = 0.0f;
            _gameOver = false;
            _gameWon = false;
        }


        internal void GameLogic(float elapsedTime)
        {
            if ((_gameOver || _gameWon) && _isRestartPressed)
            {
                ResetGame();
                return;
            }

            if (_startingTime < 100)
                _startingTime += elapsedTime;

            ////////////////
            // Ship control
            ////////////////
            ControlTheShip(elapsedTime);


            //////////
            // Ship  
            /////////
            _syringe.Advance(elapsedTime);
            if (_syringe.IsExploded && _syringe.ExplosionTime > GameConstants.ExplosionDuration)
            {
                // If the ship is exploded and some time has passed, we reset it and decrease the lives.
                _syringe.Reset();
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

            // Remove exploded asteroids from the list (some time after explosion)
            _asteroids.RemoveAll(a => a.Size == 0 && a.ExplosionTime > GameConstants.ExplosionDuration);

            if (_asteroids.Count == 0)
            {
                // You won!
                _gameOver = true;
                _gameWon = true;
            }

            ShipToAsteroidCollision();

            ProjectileToAsteroidCollision();
        }

        private void ShipToAsteroidCollision()
        {
            // Ship to asteroid collisions
            // If the ship is already exploded, it doesn't matter
            if (!_syringe.IsExploded && !_syringe.IsGhost)
            {
                // We go through all the steroids
                foreach (var asteroid in _asteroids)
                {
                    var distance = Math.Pow(asteroid.Position.X - _syringe.Position.X, 2)
                                  + Math.Pow(asteroid.Position.Y - _syringe.Position.Y, 2);
                    // Asteroid's size + ship's size
                    var size = Math.Pow(asteroid.Size * Coronaid.CoronaOuterSizeMultiplier + _syringe.Size, 2);

                    // If we have a collision
                    if (distance < size)
                    {
                        // Ship explosion
                        _syringe.Explode();

                        break;
                    }
                }
            }
        }

        private void ControlTheShip(float elapsedTime)
        {
            if (!_syringe.IsExploded)
            {
                if (_isLeftPressed)
                {
                    _syringe.ApplyLeftRotation(elapsedTime);
                }

                if (_isRightPressed)
                {
                    _syringe.ApplyRightRotation(elapsedTime);
                }

                if (_isAccelerationPressed)
                {
                    _syringe.ApplyAcceleration(elapsedTime);
                }

                if (_firePressed == 1 && !_syringe.IsGhost)
                {
                    var top = _syringe.NeedleTop;
                    if (_projectiles.Count < 20) // allow max 20 projectiles on the screen
                    {
                        // If we pressed fire, we create a projectile, 
                        // starting from the position of the ship and going in the direction the ship is faced
                        var projectile = new Projectile(top, _syringe.Rotation);
                        _projectiles.Add(projectile);
                    }

                    _firePressed = 2; // do not keep firing when the key remains pressed 
                }
            }
        }

        private void ProjectileToAsteroidCollision()
        {
            // Projectile to asteroid collisions
            bool foundCollision = false;
            Projectile hitProjectile = null;
            Coronaid hitAsteroid = null;
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
                    double size = Math.Pow(asteroid.Size * Coronaid.CoronaOuterSizeMultiplier, 2);

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

                    var newAsteroid1 = new Coronaid(hitAsteroid.Position, hitAsteroid.Size / 2, newSpeed1);

                    // New asteroid 2
                    Vector2 newSpeed2;
                    newSpeed2.X = -cSpeed.Y * 1.5f;
                    newSpeed2.Y = -cSpeed.X * 1.5f;
                    var newAsteroid2 = new Coronaid(hitAsteroid.Position, hitAsteroid.Size / 2, newSpeed2);

                    _asteroids.Remove(hitAsteroid);
                    _asteroids.Add(newAsteroid1);
                    _asteroids.Add(newAsteroid2);
                }
            }
        }

        internal void Draw(CanvasDrawingSession ds)
        {
           // DrawFps(ds);

            foreach (var projectile in _projectiles)
            {
                projectile.Draw(ds);
            }

            if (!_gameOver || _gameWon)
            {
                _syringe.Draw(ds);
                if (_syringe.IsGhost)
                {
                    var format = BigTextFormat();
                    format.FontSize = 20;
                    ds.DrawText("Get Ready!", FullWindowRect, Colors.White, format);
                }
            }

            foreach (var asteroid in _asteroids)
            {
                asteroid.Draw(ds);
            }

            for (int i = 0; i < _lives; i++)
            {
                _lifeSyringes[i].Draw(ds);
            }

            if (_gameOver)
            {
                var rect = FullWindowRect;
                var textFormat = BigTextFormat();
                    

                if (_gameWon)
                {
                    ds.DrawText("You Win!", rect, Colors.White, textFormat);
                }
                else
                {
                    ds.DrawText("Game Over!",  rect, Colors.White, textFormat);
                }

                rect.Height -= 20;
                textFormat.FontSize = 20;
                textFormat.VerticalAlignment = CanvasVerticalAlignment.Bottom;
                ds.DrawText("Press S for New Game", rect, Colors.Yellow, textFormat);
            }
        }

        private Rect FullWindowRect => new Rect(0, 0, GameConstants.DesignWidth, GameConstants.DesignHeight);
        private CanvasTextFormat BigTextFormat()
        {
            return new CanvasTextFormat
            {
                FontFamily = "Verdana",
                FontWeight = FontWeights.Normal,
                FontStyle = FontStyle.Normal,
                FontStretch = FontStretch.Normal,
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 60
            };
        }
        //private void DrawFps(CanvasDrawingSession ds)
        //{
        //    _frameCount++;
        //    var ms = _stopwatch.ElapsedMilliseconds;
        //    if(ms >= 1000)
        //    {
        //        _fps = _frameCount;
        //        _stopwatch.Restart();
        //        _frameCount = 0;
        //    }

        //    ds.DrawText(_fps + " FPS", 20, 20, Colors.White);
        //}

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
            {
                if (key == VirtualKey.S)
                    _isRestartPressed = isPressed;
                return;
            }

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
