using ICities;

using System;
using System.IO;

namespace VehicleColorChanger
{
    public class VehicleColorUpdater : ThreadingExtensionBase
    {
        private FileInfo info = new FileInfo("VehicleColorChanger.xml");
        private DateTime time;

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);

            if (info.Exists)
                time = info.LastWriteTime;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);

            if(!VehicleColorChanger.Initialized) return;

            info.Refresh();
            if (info.Exists && info.LastWriteTime > time)
                VehicleColorChanger.LoadConfig();
        }
    }
}
