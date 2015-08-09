using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Summons
{
    /// <summary>
    /// Holds all the global variables we'll want to use through the project.
    /// Created as a Singleton for easy access.
    /// </summary>
    class Settings
    {
        static Settings instance = new Settings();
        
        // Screen resolution
        public static int SCREEN_WIDTH = 1600;
        public static int SCREEN_HEIGHT = 1080;
        
        // Media locations
        public static String MEDIA_ROOT = "../../../Content/";
        public static String MAPS_ROOT = MEDIA_ROOT + "Maps/";
        public static String TILES_ROOT = MEDIA_ROOT + "Tiles/";

        // Tilesets
        public static int TILE_SIZE = 64;
        public static int UI_TILE_SIZE = 32;

        private Settings() {}

        public static Settings getInstance()
        {
            return instance;
        }
    }
}
