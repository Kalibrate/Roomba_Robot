using System;
using System.Collections.Generic;
using System.Text;

namespace Roomba_Robot.Model
{
    public class ResultModel
    {       
        public List<XY> Visited { get; set; }
        public List<XY> Cleaned { get; set; }      
        public final final { get; set; }
        public string Battery { get; set; }
    }
    public class final
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string facing { get; set; }

    }
    public class XY
    {
        public int X { get; set; }

        public int Y { get; set; }
    }
}
