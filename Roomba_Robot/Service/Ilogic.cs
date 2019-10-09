using System;
using System.Collections.Generic;
using System.Text;
using Roomba_Robot.Model;

namespace Roomba_Robot.Service
{
    public interface Ilogic
    {
        InformationModel ReadFile();
        ResultModel Cleaning(InformationModel values);
        void WriteFile(ResultModel values);
    } 
}
