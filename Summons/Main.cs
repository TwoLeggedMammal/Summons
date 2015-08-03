using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Summons
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        Camera camera;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = Settings.SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = Settings.SCREEN_HEIGHT;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            camera = Camera.getInstance();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Assets.getInstance().LoadTextures(Content);
            map = Map.getInstance();
            map.LoadMap("map0.txt", GraphicsDevice);
            camera.Width = Settings.SCREEN_WIDTH;
            camera.Height = Settings.SCREEN_HEIGHT;
            camera.XMax = map.width;
            camera.YMax = map.height;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {}

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            this.Window.Title = Convert.ToString(1000.0 / gameTime.ElapsedGameTime.Milliseconds);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Move the camera
            if (GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Right))
                camera.X += 500.0 * Convert.ToDouble(gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            if (GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Left))
                camera.X -= 500.0 * Convert.ToDouble(gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            if (GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Down))
                camera.Y += 500.0 * Convert.ToDouble(gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            if (GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Up))
                camera.Y -= 500.0 * Convert.ToDouble(gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Have the map render
            map.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
