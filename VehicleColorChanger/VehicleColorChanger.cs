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
        public string Name
        {
            get { return "Vehicle Color Changer"; }
        }

        public string Description
        {
            get { return "Customize vehicle colors."; }
        }

        private bool m_initialized = false;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;

            m_initialized = true;
            LoadConfig();
        }

        public override void OnLevelUnloading()
        {
            if(m_initialized)
            {
                SaveConfig();
                m_initialized = false;
            }
        }

        public override void OnReleased()
        {
            if (m_initialized)
            {
                SaveConfig();
                m_initialized = false;
            }
        }

        public void LoadConfig()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(VehicleColorInfo[]));
            VehicleColorInfo[] colors;

            try
            {
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
            catch (InvalidOperationException e)
            {
                    Debug.LogException(e);
                    return;
            }
            catch (InvalidCastException e)
            {
                    Debug.LogException(e);
                    return;
            }


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

        public void SaveConfig()
        {
            int count = PrefabCollection<VehicleInfo>.LoadedCount();

            List<VehicleColorInfo> list = new List<VehicleColorInfo>();

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

            using (FileStream stream = new FileStream("VehicleColorChanger.xml", FileMode.OpenOrCreate))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(VehicleColorInfo[]));
                xmlSerializer.Serialize(stream, list.ToArray());
            }
        }

    }
}
