using Microsoft.AspNetCore.Mvc;
using Roomba_Robot.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Roomba_Robot.Model;
using System.Linq;

namespace Roomba_Robot.Controller
{
    public class RobotController
    {
        private readonly Ilogic _logic;
        public RobotController(Ilogic logic)
        {
            _logic = logic;
        }
        
        public void Excecute()
        {
            var exec = new Logic();
            InformationModel values =  exec.ReadFile();
            var cleaning = exec.Cleaning(values);
            exec.WriteFile(cleaning);
            //throw new NotImplementedException();
        }


    }
}
