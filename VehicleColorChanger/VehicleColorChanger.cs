using ICities;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace VehicleColorChanger
{
    public class VehicleColorChanger : LoadingExtensionBase, IUserMod
    {
        #region IUserMod implementation
        public string Name
        {
            get { return "Vehicle Color Changer"; }
        }

        public string Description
        {
            get { return "Customize vehicle colors."; }
        }

        public static bool Initialized
        {
            get { return m_initialized; }
        }
        #endregion

        private static bool m_initialized = false;
        private static FileSystemWatcher m_watcher = new FileSystemWatcher();

        #region LoadingExtensionBase overrides
        /// <summary>
        /// Called when the level (game, map editor, asset editor) is loaded
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Is it an actual game ?
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;

            m_initialized = true;

            // Watching configuration file for any changes
            if (!m_watcher.EnableRaisingEvents)
            {
                m_watcher.Filter = "VehicleColorChanger.xml";
                m_watcher.NotifyFilter = NotifyFilters.LastWrite;
                m_watcher.Changed += new FileSystemEventHandler(OnFileChanged);

                m_watcher.EnableRaisingEvents = true;
            }

            // Loading the configuration
            LoadConfig();
        }

        /// <summary>
        /// Called when the level is unloaded
        /// </summary>
        public override void OnLevelUnloading()
        {
            // Saving the configuration when leaving the game (via Load Game)
            if(m_initialized)
            {
                SaveConfig();
                m_initialized = false;
            }
        }

        /// <summary>
        /// Called when the instance of the class is released
        /// </summary>
        public override void OnReleased()
        {
            // Saving the configuration when leaving the game (via Exit Game)
            if (m_initialized)
            {
                SaveConfig();
                m_initialized = false;
            }
        }
        #endregion

        /// <summary>
        /// FileSystemWatcher callback
        /// </summary>
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            // Applying changes
            if (m_initialized) LoadConfig();
        }

        /// <summary>
        /// Load and apply the configuration file
        /// </summary>
        public static void LoadConfig()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(VehicleColorInfo[]));
            VehicleColorInfo[] colors;

            try
            {
                // Trying to deserialize the configuration file
                using (FileStream stream = new FileStream("VehicleColorChanger.xml", FileMode.Open))
                {
                    colors = xmlSerializer.Deserialize(stream) as VehicleColorInfo[];
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Log("Configuration file not found. Creating new configuration file.");
                SaveConfig();
                return;
            }
            catch (Exception e)
            {
                // Couldn't deserialize (XML malformed?)
                Debug.LogException(e);
                return;
            }

            // Applying new colors to each prefabs
            for (int i = 0; i < colors.Length; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.FindLoaded(colors[i].name);
                if (prefab != null)
                {
                    prefab.m_useColorVariations = true;
                    prefab.m_color0 = colors[i].color0;
                    prefab.m_color1 = colors[i].color1;
                    prefab.m_color2 = colors[i].color2;
                    prefab.m_color3 = colors[i].color3;
                }
            }
        }

        /// <summary>
        /// Save the configuration file
        /// </summary>
        public static void SaveConfig()
        {
            int count = PrefabCollection<VehicleInfo>.LoadedCount();

            List<VehicleColorInfo> list = new List<VehicleColorInfo>();

            // Compiling each prefab colors into a list
            for (uint i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetLoaded(i);

                VehicleColorInfo info = new VehicleColorInfo();
                info.name = prefab.name;
                info.color0 = prefab.m_color0;
                info.color1 = prefab.m_color1;
                info.color2 = prefab.m_color2;
                info.color3 = prefab.m_color3;

                list.Add(info);
            }

            // The list shouldn't be empty
            if (list.Count == 0) return;

            // Serializing the list
            using (FileStream stream = new FileStream("VehicleColorChanger.xml", FileMode.OpenOrCreate))
            {
                stream.SetLength(0); // Emptying the file !!!
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(VehicleColorInfo[]));
                xmlSerializer.Serialize(stream, list.ToArray());
            }
        }

    }
}
