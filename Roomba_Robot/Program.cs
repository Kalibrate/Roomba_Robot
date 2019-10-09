using Roomba_Robot.Controller;
using Roomba_Robot.Service;
using System;

namespace Roomba_Robot
{
    class Program
    {
        static void Main(string[] args)
        {
            Logic logic = new Logic();
            RobotController robot = new RobotController(logic);
            robot.Excecute();

            Console.WriteLine("Robot Has Cleaned Room");
        }
    }
}
