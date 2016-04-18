﻿using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.ViewportAdapters;

namespace Demo.SpriteSheetAnimations
{
    public class Game1 : Game
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once NotAccessedField.Local
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private Camera2D _camera;
        private SpriteBatch _spriteBatch;
        private TiledMap _tiledMap;
        private ViewportAdapter _viewportAdapter;
        private CollisionWorld _world;
        private Zombie _zombie;
        private KeyFrameAnimation _animation;
        private Sprite _fireballSprite;
        private KeyFrameAnimationPlayer _motwPlayer;
        private Sprite _motwSprite;

        public Game1()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 800, 480);
            _camera = new Camera2D(_viewportAdapter)
            {
                MinimumZoom = 0.1f,
                MaximumZoom = 2.0f,
                Zoom = 0.7833337f,
                Origin = new Vector2(400, 240),
                Position = new Vector2(408, 270)
            };

            Window.Title = $"MonoGame.Extended - {GetType().Name}";
            Window.Position = Point.Zero;
            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Content.Load<BitmapFont>("Fonts/courier-new-32");
            _tiledMap = Content.Load<TiledMap>("Tilesets/level01");

            _world = new CollisionWorld(new Vector2(0, 900));
            _world.CreateGrid(_tiledMap.GetLayer<TiledTileLayer>("Tile Layer 1"));

            var animationGroup = Content.Load<SpriteSheetAnimationGroup>("Sprites/zombie-animations");
            _zombie = new Zombie(animationGroup);
            var zombieActor = _world.CreateActor(_zombie);
            zombieActor.Position = new Vector2(462.5f, 896f);

            var fireballTexture = Content.Load<Texture2D>("Sprites/fireball");
            var fireballAtlas = TextureAtlas.Create(fireballTexture, 130, 50);
            _animation = new KeyFrameAnimation(0.1f, fireballAtlas.Regions.ToArray());
            _fireballSprite = new Sprite(_animation.CurrentFrame)
            {
                Position = _zombie.Position
            };

            var motwTexture = Content.Load<Texture2D>("motw");
            var motwAtlas = TextureAtlas.Create(motwTexture, 52, 72);
            _motwPlayer = new KeyFrameAnimationPlayer();
            _motwPlayer.Add("idle", new KeyFrameAnimation(0.2f, new[] { motwAtlas[1] }));
            _motwPlayer.Add("walkSouth", new KeyFrameAnimation(0.2f, new[] { motwAtlas[0], motwAtlas[1], motwAtlas[2], motwAtlas[1] }, isLooping: false));
            _motwPlayer.Add("walkWest", new KeyFrameAnimation(0.2f, new[] { motwAtlas[12], motwAtlas[13], motwAtlas[14], motwAtlas[13] }, isLooping: false));
            _motwPlayer.Add("walkEast", new KeyFrameAnimation(0.2f, new[] { motwAtlas[24], motwAtlas[25], motwAtlas[26], motwAtlas[25] }, isLooping: false));
            _motwPlayer.Add("walkNorth", new KeyFrameAnimation(0.2f, new[] { motwAtlas[36], motwAtlas[37], motwAtlas[38], motwAtlas[37] }, isLooping: false));
            _motwSprite = _motwPlayer.CreateSprite(_zombie.Position - new Vector2(0, 100));
        }

        protected override void UnloadContent()
        {
            _tiledMap.Dispose();
            _world.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            // motw
            if (keyboardState.IsKeyDown(Keys.W))
                _motwPlayer.Play("walkNorth");

            if (keyboardState.IsKeyDown(Keys.A))
                _motwPlayer.Play("walkWest");

            if (keyboardState.IsKeyDown(Keys.S))
                _motwPlayer.Play("walkSouth");

            if (keyboardState.IsKeyDown(Keys.D))
                _motwPlayer.Play("walkEast");

            // camera
            if (keyboardState.IsKeyDown(Keys.R))
                _camera.ZoomIn(deltaSeconds);

            if (keyboardState.IsKeyDown(Keys.F))
                _camera.ZoomOut(deltaSeconds);

            // zombie
            if (keyboardState.IsKeyDown(Keys.Left))
                _zombie.Walk(-1.0f);

            if (keyboardState.IsKeyDown(Keys.Right))
                _zombie.Walk(1.0f);

            if (keyboardState.IsKeyDown(Keys.Space))
                _zombie.Attack();

            if (keyboardState.IsKeyDown(Keys.Up))
                _zombie.Jump();

            if (keyboardState.IsKeyDown(Keys.Enter))
                _zombie.Die();

            // update must be called before collision detection
            _zombie.Update(gameTime);
            _world.Update(gameTime);
            _camera.LookAt(_zombie.Position);

            _animation.Update(deltaSeconds);
            _fireballSprite.TextureRegion = _animation.CurrentFrame;

            _motwPlayer.Update(deltaSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
            _tiledMap.Draw(_spriteBatch, _camera);
            _zombie.Draw(_spriteBatch);
            _spriteBatch.Draw(_fireballSprite);
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());

            _spriteBatch.Draw(_motwSprite);
            _spriteBatch.End();


            //_spriteBatch.Begin();
            //_spriteBatch.DrawString(_bitmapFont, string.Format("FPS: {0} Zoom: {1}", _fpsCounter.AverageFramesPerSecond, _camera.Zoom), new Vector2(5, 5), new Color(0.5f, 0.5f, 0.5f));
            //_spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}