using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model
{
    class pathDetailsMasterData
    {
        public int pathPkId { get; set; }
        public string eesId { get; set; }
        public string vlcId { get; set; }
        public string lcmId { get; set; }
        public string ucmId { get; set; }
        public int priority { get; set; }
        public int needRelocation { get; set; }
        public int queueId { get; set; }
        public int isActive { get; set; }
        public int floor { get; set; }
        public int aisle { get; set; }
        public int row { get; set; }
        public string pvlId { get; set; }
        public string pstId { get; set; }
        public int isStore { get; set; }
        public string ees { get; set; }
        public string lcm { get; set; }
        public string vlc { get; set; }
        public string ucm { get; set; }
        public int activity { get; set; }
        public string carAt { get; set; }
        public string execCommand { get; set; }
        public string pvl { get; set; }

    }
}
