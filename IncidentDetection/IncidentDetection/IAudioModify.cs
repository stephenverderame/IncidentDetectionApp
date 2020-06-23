using System;
using System.Collections.Generic;
using System.Text;

namespace IncidentDetection
{
    public interface IAudioModify
    {
        //Overrides current volume levels to ensure sound played at max volume
        void setVolumeMax();
    }
}
