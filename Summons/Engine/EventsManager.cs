using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Summons.Engine
{
    /// <summary>
    /// EventsManager is responsible for communicating with the player at the right times.
    /// Other objects report their actions to it, and it should know if circumstances warrant
    /// feedback to the player.
    /// 
    /// A singleton.
    /// </summary>
    class EventsManager
    {
        static EventsManager instance = new EventsManager();
        UI ui;
        public Scene CurrentScene = Scene.OVERWORLD;

        public enum Event
        {
            GAME_STARTED,
            ACTOR_CLICKED,
            MAP_CLICKED,
            INVALID_ACTOR_DESTINATION,
            BATTLE_ENGAGED,
            MONSTER_EVOLVED
        }
        public enum Scene
        {
            TITLE_SCREEN,
            OPTIONS_MENU,
            OVERWORLD,
            COMBAT
        }
        Dictionary<Event, bool> triggered;

        private EventsManager()
        {
            ui = UI.getInstance();
            triggered = new Dictionary<Event,bool>();
        }

        public static EventsManager getInstance()
        {
            return instance;
        }

        public void RecordEvent(Event e)
        {
            if (e == Event.GAME_STARTED && !PreviouslyTriggered(e))
            {
                ui.OpenTextDialog(64, 64, 384, "Welcome to Summons! The point of the game is to crush your enemies with brute force!");
                ui.OpenTextDialog(64, 64, 384, "You can click on the little mage dude there to move him.");
            }
            else if (e == Event.ACTOR_CLICKED && !PreviouslyTriggered(e))
            {
                ui.OpenTextDialog(64, 64, 384, "Now you can click a location on the map and he'll move there if he is able.");
            }
            else if (e == Event.INVALID_ACTOR_DESTINATION)
            {
                ui.OpenTextDialog(64, 64, 384, "That location can't be reached by this character!");
            }
            else if (e == Event.BATTLE_ENGAGED && CurrentScene != Scene.COMBAT)
            {
                Console.WriteLine("Battle Start!");
                CurrentScene = Scene.COMBAT;
                ui.ShowMessage("Fight it out!");
            }

            if (triggered.ContainsKey(e))
                triggered[e] = true;
            else triggered.Add(e, true);
        }

        bool PreviouslyTriggered(Event e)
        {
            return triggered.ContainsKey(e) && triggered[e];
        }
    }
}
