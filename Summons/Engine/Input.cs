using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    //This is a singleton class
    class Input
    {
        MouseState previousMouseState;
        static Input instance = new Input();

        private Input()
        {

        }

        public static Input getInstance()
        {
            return instance;
        }

        public void HandleMouseInput(MouseState mouseState, List<Actor> actorCollection)
        {
            Camera camera = Camera.getInstance();

            if ((previousMouseState == null || previousMouseState.LeftButton == ButtonState.Released) && mouseState.LeftButton == ButtonState.Pressed)
            {
                bool actorClicked = false;

                // Find out if we clicked on any Actors
                foreach (Actor actor in actorCollection)
                {
                    if (mouseState.X + camera.X > (actor.TileX * Settings.TILE_SIZE) && mouseState.X + camera.X < ((actor.TileX + 1) * Settings.TILE_SIZE) &&
                        mouseState.Y + camera.Y > (actor.TileY * Settings.TILE_SIZE) && mouseState.Y + camera.Y < ((actor.TileY + 1) * Settings.TILE_SIZE))
                    {
                        actor.Select();
                        actorClicked = true;
                    }
                }

                // If we didn't click on an Actor, we must be trying to move the currently selected actor to this location
                if (!actorClicked)
                {
                    int tileX = Convert.ToInt32(Math.Floor((mouseState.X + camera.X) / Settings.TILE_SIZE));
                    int tileY = Convert.ToInt32(Math.Floor((mouseState.Y + camera.Y) / Settings.TILE_SIZE));

                    foreach (Actor actor in actorCollection)
                    {
                        if (actor.Selected)
                        {
                            actor.SetDestination(tileX, tileY);
                        }
                    }
                }
            }
            previousMouseState = Mouse.GetState();
        }
    }
}
