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
        
        // Media locations
        public static String MEDIA_ROOT = "../../../Media/";
        public static String MAPS_ROOT = MEDIA_ROOT + "Maps/";
        public static String TILES_ROOT = MEDIA_ROOT + "Tiles/";

        // Tileset mapping
        public static int TILE_SIZE = 64;
        public static List<KeyValuePair<char, String>> TILE_MAPPING = new List<KeyValuePair<char, String>>()
        { 
            new KeyValuePair<char, String>('0', "water64.jpg"),
            new KeyValuePair<char, String>('1', "grass64.jpg")
        };

        private Settings() {}

        public static Settings getInstance()
        {
            return instance;
        }
    }
}
