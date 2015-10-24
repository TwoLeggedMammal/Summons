using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Summons.Engine;

namespace Summons
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        Map map;
        Camera camera;
        MonsterManager monsterManager;
        UI ui;
        EventsManager eventsManager;
        PlayerManager playerManager;
        Combat combat;

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
            this.IsMouseVisible = true;
            camera = Camera.getInstance();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Assets.getInstance().LoadTextures(Content, GraphicsDevice);
            PostContent();
        }

        public void PostContent()
        {
            monsterManager = MonsterManager.getInstance();
            ui = UI.getInstance();
            ui.Initialize(GraphicsDevice);
            eventsManager = EventsManager.getInstance();
            playerManager = PlayerManager.getInstance();
            combat = Combat.getInstance();

            // Loading up our test map
            map = Map.getInstance();
            map.LoadMap("map0.txt", GraphicsDevice);

            // The camera relies on the map being loaded to operate
            camera.Width = Settings.SCREEN_WIDTH;
            camera.Height = Settings.SCREEN_HEIGHT;
            camera.XMax = map.width * Settings.TILE_SIZE;
            camera.YMax = map.height * Settings.TILE_SIZE;
            
            // This creates a basic scenario with a couple players and monsters each, for testing
            playerManager.SpawnPlayers();
            
            // Kick things off
            eventsManager.RecordEvent(EventsManager.Event.GAME_STARTED);
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
            double timeSinceLastFrame = gameTime.ElapsedGameTime.Milliseconds / 1000.0;
            this.Window.Title = Convert.ToString(1.0 / timeSinceLastFrame);

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

            // Mouse input
            Input.getInstance().HandleMouseInput(Mouse.GetState());

            // Keyboard input
            Input.getInstance().HandleKeyboardInput(Keyboard.GetState());

            // Update our camera
            camera.Update(timeSinceLastFrame);

            // Update our actors
            if (eventsManager.CurrentScene == EventsManager.Scene.OVERWORLD)
            {
                monsterManager.Update(timeSinceLastFrame);
            }
            else if (eventsManager.CurrentScene == EventsManager.Scene.COMBAT)
            {
                monsterManager.UIUpdate(timeSinceLastFrame);
                combat.Update(timeSinceLastFrame);
            }

            // Update our UI
            ui.Update(timeSinceLastFrame);

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

            // Draw our characters
            monsterManager.Draw(GraphicsDevice);
            
            // Draw the UI last so it's on top
            ui.Draw();

            base.Draw(gameTime);
        }
    }
}
