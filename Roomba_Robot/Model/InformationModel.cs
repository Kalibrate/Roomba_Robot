using System;
using System.Collections.Generic;
using System.Text;

namespace Roomba_Robot.Model
{
    public class InformationModel
    {
       
        public ICollection<ICollection<RoomInfo>> Map { get; set; }     
        public ICollection<string> commands { get; set; }
        public start start { get; set; }
        public string Battery { get; set; }
    }

    public class start
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string facing { get; set; }

    }
    public enum RoomInfo
    {
        S,
        C,
        Null
    }

    public enum facing
    {
        N,
        E,
        S,
        W
    }
}
