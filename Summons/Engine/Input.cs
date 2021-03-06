﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    //This is a singleton class
    public class Input
    {
        MouseState previousMouseState;
        KeyboardState previousKeyboardState;
        MonsterManager monsterManager;
        static Input instance = new Input();
        public ClickAction clickAction = ClickAction.NO_ACTION;
        public Monster selectedMonster;
        public Type summonType;

        public enum ClickAction
        {
            NO_ACTION,
            MOVE_MONSTER,
            SUMMON_MONSTER
        }

        private Input()
        {
            monsterManager = MonsterManager.getInstance();
        }

        public static Input getInstance()
        {
            return instance;
        }

        public void HandleKeyboardInput(KeyboardState keyboardState)
        {
            // Summon monster hotkey
            if (keyboardState.GetPressedKeys().Contains(Keys.S) && !previousKeyboardState.GetPressedKeys().Contains(Keys.S))
            {
                UI.getInstance().OpenSummonDialog();
            }

            if (keyboardState.GetPressedKeys().Contains(Keys.Enter) && !previousKeyboardState.GetPressedKeys().Contains(Keys.Enter))
            {
                foreach (Button button in UI.getInstance().playerActionDialog.buttonCollection)
                {
                    if (typeof(PassTurnButton) == button.GetType())
                        button.ClickHandler();
                }
            }

            previousKeyboardState = keyboardState;
        }

        public void HandleMouseInput(MouseState mouseState)
        {
            Camera camera = Camera.getInstance();
            bool inputCaptured = false;

            // Button hovering
            foreach (Button button in UI.getInstance().buttonCollection)
            {
                button.hovered = !inputCaptured && (mouseState.X >= button.x && mouseState.X < button.x + button.width &&
                                mouseState.Y >= button.y && mouseState.Y < button.y + button.height);
                inputCaptured = inputCaptured || button.hovered;
            }

            // Actor hovering
            foreach (Actor actor in monsterManager.monsterCollection)
            {
                actor.Hovered = !inputCaptured && (mouseState.X + camera.X > (actor.TileX * Settings.TILE_SIZE) && mouseState.X + camera.X < ((actor.TileX + 1) * Settings.TILE_SIZE) &&
                                mouseState.Y + camera.Y > (actor.TileY * Settings.TILE_SIZE) && mouseState.Y + camera.Y < ((actor.TileY + 1) * Settings.TILE_SIZE));
                inputCaptured = inputCaptured || actor.Hovered;
            }

            // Map hovering when planning a move for a monster
            if (this.clickAction == ClickAction.MOVE_MONSTER)
            {
                Coordinate mapCoordinate = MouseToMap(mouseState);
                Map.getInstance().PlanRoute(this.selectedMonster, Convert.ToInt32(mapCoordinate.x), Convert.ToInt32(mapCoordinate.y));
            }

            // Handle left clicks
            if ((previousMouseState == null || previousMouseState.LeftButton == ButtonState.Released) && mouseState.LeftButton == ButtonState.Pressed)
            {
                LeftClickHandler(mouseState);   
            }

            // Right button is down
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                camera.Panning = true;
            }

            // Right button was released
            if ((previousMouseState != null && previousMouseState.RightButton == ButtonState.Pressed) && mouseState.RightButton == ButtonState.Released)
            {
                camera.Panning = false;
                camera.calculateMomentum();
            }

            // Right button was pressed
            if ((previousMouseState != null && previousMouseState.RightButton == ButtonState.Pressed) && mouseState.RightButton == ButtonState.Pressed)
            {
                if (camera.Panning)
                {
                    double xDiff = mouseState.Position.X - previousMouseState.Position.X;
                    double yDiff = mouseState.Position.Y - previousMouseState.Position.Y;

                    camera.X -= xDiff;
                    camera.Y -= yDiff;

                    camera.momentum.Push(new Coordinate(xDiff, yDiff));
                }
            }

            previousMouseState = Mouse.GetState();
        }

        void LeftClickHandler(MouseState mouseState)
        {
            bool uiClicked = false;

            // Send input to the UI
            uiClicked = UI.getInstance().Click(mouseState);

            // Find out if we clicked on any Actors
            if (!uiClicked)
            {
                bool actorClicked = false;

                actorClicked = MonsterManager.getInstance().Click(mouseState) || actorClicked;

                // If we didn't click on an Actor, we must be trying to move the currently selected actor to this location
                if (!actorClicked)
                {
                    Coordinate mapCoordinate = MouseToMap(mouseState);

                    if (this.clickAction == ClickAction.MOVE_MONSTER)
                    {
                        foreach (Actor actor in monsterManager.monsterCollection)
                        {
                            if (actor.Selected)
                            {
                                actor.SetDestination(Convert.ToInt32(mapCoordinate.x), Convert.ToInt32(mapCoordinate.y));
                                Map.getInstance().previewRoute = null;
                            }
                        }
                        this.clickAction = ClickAction.NO_ACTION;
                    }
                    else if (this.clickAction == ClickAction.SUMMON_MONSTER)
                    {
                        if (Map.getInstance().IsSummonLocation(Convert.ToInt32(mapCoordinate.x), Convert.ToInt32(mapCoordinate.y)))
                        {
                            monsterManager.Spawn(summonType, Convert.ToInt32(mapCoordinate.x), Convert.ToInt32(mapCoordinate.y), PlayerManager.getInstance().currentPlayer);
                            this.clickAction = ClickAction.NO_ACTION;
                        }
                    }
                }
            }
        }

        Coordinate MouseToMap(MouseState mouseState)
        {
            Camera camera = Camera.getInstance();
            int tileX = Convert.ToInt32(Math.Floor((mouseState.X + camera.X) / Settings.TILE_SIZE));
            int tileY = Convert.ToInt32(Math.Floor((mouseState.Y + camera.Y) / Settings.TILE_SIZE));
            return new Coordinate(tileX, tileY);
        }
    }
}
