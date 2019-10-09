using Newtonsoft.Json;
using Roomba_Robot.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Linq;

namespace Roomba_Robot.Service
{
    public class Logic :Ilogic
    {        
        public InformationModel ReadFile()
        {
            //you can change directory on appsettings.json
            string path = ConfigurationManager.AppSettings["Path"];
            var values = new InformationModel();
            var result = new ResultModel();
            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    values = JsonConvert.DeserializeObject<InformationModel>(json);
                }
            }

            return values;

        }

        public void WriteFile(ResultModel values)
        {
            //you can change directory on appsettings.json
            string Newpath = ConfigurationManager.AppSettings["NewPath"];
            var data = JsonConvert.SerializeObject(values);
            File.WriteAllText(Newpath, data, Encoding.UTF8);
        }

        public ResultModel Cleaning(InformationModel values)
        {
            var commands = values.commands;
            var Battery = values.Battery;
            var Maps = values.Map;
            var start = values.start;
            int X = start.X; int Y = start.Y; facing Facing =  (facing)Enum.Parse(typeof(facing), start.facing, true);
            
            //Cleaning(X, Y, Battery, Facing, Command, Maps);
            
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            try
            {
                results.Visited.Add(new XY { X = currentX, Y = currentY });
                foreach (var step in commands)
                {
                    var directions = new[] { "TL", "TR" };
                    var ValidMap = ValidateMaps(Maps, X, Y);
                    battery -= UsingBattery(step);
                    results.Battery = battery.ToString();

                    if (battery > 0 && ValidMap)
                    {

                        if (directions.Any(x => x.Contains(step)))
                        {
                            Facing = ChangeDirection(Facing, step);
                        }
                        else if (step != InstructionModel.C)
                        {
                            int oldX = currentX; int oldY = currentY;
                            List<int> moving = Moving(Facing, currentX, currentY, step);
                            currentX = moving.ElementAt(0);
                            currentY = moving.ElementAt(1);
                            ValidMap = ValidateMaps(Maps, currentX, currentY);
                            if (ValidMap)
                            {
                                var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                                if (newloc == RoomInfo.S)
                                {
                                    results.Visited.Add(new XY { X = currentX, Y = currentY });
                                }
                                else
                                {
                                    var result =  Scenario1(oldX, oldY, battery, Facing, Maps);
                                    results.Visited.AddRange(result.Visited);
                                    results.Cleaned.AddRange(result.Cleaned);
                                    currentX = result.final.X;
                                    currentY = result.final.Y;
                                    Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                    battery = Convert.ToInt32(result.Battery);                                    
                                }
                            }
                            else
                            {
                                var result = Scenario1(oldX, oldY, battery, Facing, Maps);
                                results.Visited.AddRange(result.Visited);
                                results.Cleaned.AddRange(result.Cleaned);
                                currentX = result.final.X;
                                currentY = result.final.Y;
                                Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                battery = Convert.ToInt32(result.Battery);
                            }
                        }
                        else if (step == InstructionModel.C)
                        {
                            results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        }
                    }
                    else if (battery > 0)
                    {
                        var result = Scenario1(currentX, currentY, battery, Facing, Maps);
                        results.Visited.AddRange(result.Visited);
                        results.Cleaned.AddRange(result.Cleaned);
                        currentX = result.final.X;
                        currentY = result.final.Y;
                        Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                        battery = Convert.ToInt32(result.Battery);
                    }
                    else
                    {
                        results.Battery = battery.ToString();
                    }
                }
                results.final.X = currentX;
                results.final.Y = currentY;
                results.final.facing = Facing.ToString();
                results.Visited = results.Visited.GroupBy(x => new { x.X, x.Y }).Select(y => new XY(){X = y.Key.X, Y= y.Key.Y}).OrderBy(z => z.X).ToList();
                results.Cleaned = results.Cleaned.GroupBy(x => new { x.X, x.Y }).Select(y => new XY(){X = y.Key.X, Y= y.Key.Y}).OrderBy(z => z.X).ToList();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var ada = ex;
            }

            return results;
        }
               
        #region Back of Strategy
        private ResultModel Scenario1(int X, int Y, int Battery, facing Facing, ICollection<ICollection<RoomInfo>> Maps)
        {
            var Steps = new List<string> { "TR", "A" };
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            foreach (var step in Steps)
            {
                var directions = new[] { "TL", "TR" };
                var ValidMap = ValidateMaps(Maps, X, Y);
                battery -= UsingBattery(step);
                results.Battery = battery.ToString();
                if (battery > 0 && ValidMap)
                {

                    if (directions.Any(x => x.Contains(step)))
                    {
                        Facing = ChangeDirection(Facing, step);
                    }
                    else if (step != InstructionModel.C)
                    {
                        int oldX = currentX; int oldY = currentY;
                        List<int> moving = Moving(Facing, currentX, currentY, step);
                        currentX = moving.ElementAt(0);
                        currentY = moving.ElementAt(1);
                        ValidMap = ValidateMaps(Maps, currentX, currentY);
                        if (ValidMap)
                        {
                            var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                            if (newloc == RoomInfo.S)
                            {
                                results.Visited.Add(new XY { X = currentX, Y = currentY });
                            }
                            else
                            {
                                var result = Scenario2(oldX, oldY, battery, Facing, Maps);
                                results.Visited.AddRange(result.Visited);
                                results.Cleaned.AddRange(result.Cleaned);
                                currentX = result.final.X;
                                currentY = result.final.Y;
                                Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                battery = Convert.ToInt32(result.Battery);
                            }
                        }
                        else
                        {
                            var result = Scenario2(oldX, oldY, battery, Facing, Maps);
                            results.Visited.AddRange(result.Visited);
                            results.Cleaned.AddRange(result.Cleaned);
                            currentX = result.final.X;
                            currentY = result.final.Y;
                            Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                            battery = Convert.ToInt32(result.Battery);
                        }
                    }
                    else if (step == InstructionModel.C)
                    {
                        results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        
                    }
                }
                else if (battery > 0)
                {
                    var result = Scenario2(currentX, currentY, battery, Facing, Maps);
                    results.Visited.AddRange(result.Visited);
                    
                    results.Cleaned.AddRange(result.Cleaned);
                    
                    currentX = result.final.X;
                    currentY = result.final.Y;
                    Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                    battery = Convert.ToInt32(result.Battery);
                }
                else
                {
                    results.Battery = battery.ToString();
                }
            }
            results.final.X = currentX;
            results.final.Y = currentY;
            results.final.facing = Facing.ToString();

            return results;
        }
        private ResultModel Scenario2(int X, int Y, int Battery, facing Facing, ICollection<ICollection<RoomInfo>> Maps)
        {
            var Steps = new List<string> { "TL", "B", "TR", "A" };
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            foreach (var step in Steps)
            {
                var directions = new[] { "TL", "TR" };
                var ValidMap = ValidateMaps(Maps, X, Y);
                battery -= UsingBattery(step);
                results.Battery = battery.ToString();
                if (battery > 0 && ValidMap)
                {

                    if (directions.Any(x => x.Contains(step)))
                    {
                        Facing = ChangeDirection(Facing, step);
                    }
                    else if (step != InstructionModel.C)
                    {
                        int oldX = currentX; int oldY = currentY;
                        List<int> moving = Moving(Facing, currentX, currentY, step);
                        currentX = moving.ElementAt(0);
                        currentY = moving.ElementAt(1);
                        ValidMap = ValidateMaps(Maps, currentX, currentY);
                        if (ValidMap)
                        {
                            var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                            if (newloc == RoomInfo.S)
                            {
                                results.Visited.Add(new XY { X = currentX, Y = currentY });
                            }
                            else
                            {
                                var result = Scenario3(oldX, oldY, battery, Facing, Maps);
                                results.Visited.AddRange(result.Visited);
                                
                                results.Cleaned.AddRange(result.Cleaned);
                                
                                currentX = result.final.X;
                                currentY = result.final.Y;
                                Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                battery = Convert.ToInt32(result.Battery);
                            }
                        }
                        else
                        {
                            var result = Scenario3(oldX, oldY, battery, Facing, Maps);
                            results.Visited.AddRange(result.Visited);
                            
                            results.Cleaned.AddRange(result.Cleaned);
                            
                            currentX = result.final.X;
                            currentY = result.final.Y;
                            Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                            battery = Convert.ToInt32(result.Battery);
                        }
                    }
                    else if (step == InstructionModel.C)
                    {
                        results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        
                    }
                }
                else if (battery > 0)
                {
                    var result = Scenario3(currentX, currentY, battery, Facing, Maps);
                    results.Visited.AddRange(result.Visited);
                    
                    results.Cleaned.AddRange(result.Cleaned);
                    
                    currentX = result.final.X;
                    currentY = result.final.Y;
                    Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                    battery = Convert.ToInt32(result.Battery);
                }
                else
                {
                    results.Battery = battery.ToString();
                }
            }
            results.final.X = currentX;
            results.final.Y = currentY;
            results.final.facing = Facing.ToString();

            return results;
        }
        private ResultModel Scenario3(int X, int Y, int Battery, facing Facing, ICollection<ICollection<RoomInfo>> Maps)
        {
            var Steps = new List<string> { "TL", "TL", "A"};
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            foreach (var step in Steps)
            {
                var directions = new[] { "TL", "TR" };
                var ValidMap = ValidateMaps(Maps, X, Y);
                battery -= UsingBattery(step);
                results.Battery = battery.ToString();
                if (battery > 0 && ValidMap)
                {

                    if (directions.Any(x => x.Contains(step)))
                    {
                        Facing = ChangeDirection(Facing, step);
                    }
                    else if (step != InstructionModel.C)
                    {
                        int oldX = currentX; int oldY = currentY;
                        List<int> moving = Moving(Facing, currentX, currentY, step);
                        currentX = moving.ElementAt(0);
                        currentY = moving.ElementAt(1);
                        ValidMap = ValidateMaps(Maps, currentX, currentY);
                        if (ValidMap)
                        {
                            var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                            if (newloc == RoomInfo.S)
                            {
                                results.Visited.Add(new XY { X = currentX, Y = currentY });
                            }
                            else
                            {
                                var result = Scenario4(oldX, oldY, battery, Facing, Maps);
                                results.Visited.AddRange(result.Visited);
                                
                                results.Cleaned.AddRange(result.Cleaned);
                                
                                currentX = result.final.X;
                                currentY = result.final.Y;
                                Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                battery = Convert.ToInt32(result.Battery);
                            }
                        }
                        else
                        {
                            var result = Scenario4(oldX, oldY, battery, Facing, Maps);
                            results.Visited.AddRange(result.Visited);
                            
                            results.Cleaned.AddRange(result.Cleaned);
                            
                            currentX = result.final.X;
                            currentY = result.final.Y;
                            Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                            battery = Convert.ToInt32(result.Battery);
                        }
                    }
                    else if (step == InstructionModel.C)
                    {
                        results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        
                    }
                }
                else if (battery > 0)
                {
                    var result = Scenario4(currentX, currentY, battery, Facing, Maps);
                    results.Visited.AddRange(result.Visited);
                    
                    results.Cleaned.AddRange(result.Cleaned);
                    
                    currentX = result.final.X;
                    currentY = result.final.Y;
                    Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                    battery = Convert.ToInt32(result.Battery);
                }
                else
                {
                    results.Battery = battery.ToString();
                }
            }
            results.final.X = currentX;
            results.final.Y = currentY;
            results.final.facing = Facing.ToString();

            return results;
        }
        private ResultModel Scenario4(int X, int Y, int Battery, facing Facing, ICollection<ICollection<RoomInfo>> Maps)
        {
            var Steps = new List<string> { "TR", "B", "TR", "A" };
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            foreach (var step in Steps)
            {
                var directions = new[] { "TL", "TR" };
                var ValidMap = ValidateMaps(Maps, X, Y);
                battery -= UsingBattery(step);
                results.Battery = battery.ToString();
                if (battery > 0 && ValidMap)
                {

                    if (directions.Any(x => x.Contains(step)))
                    {
                        Facing = ChangeDirection(Facing, step);
                    }
                    else if (step != InstructionModel.C)
                    {
                        int oldX = currentX; int oldY = currentY;
                        List<int> moving = Moving(Facing, currentX, currentY, step);
                        currentX = moving.ElementAt(0);
                        currentY = moving.ElementAt(1);
                        ValidMap = ValidateMaps(Maps, currentX, currentY);
                        if (ValidMap)
                        {
                            var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                            if (newloc == RoomInfo.S)
                            {
                                results.Visited.Add(new XY { X = currentX, Y = currentY });
                            }
                            else
                            {
                                var result = Scenario5(oldX, oldY, battery, Facing, Maps);
                                results.Visited.AddRange(result.Visited);
                                
                                results.Cleaned.AddRange(result.Cleaned);
                                
                                currentX = result.final.X;
                                currentY = result.final.Y;
                                Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                                battery = Convert.ToInt32(result.Battery);
                            }
                        }
                        else
                        {
                            var result = Scenario5(oldX, oldY, battery, Facing, Maps);
                            results.Visited.AddRange(result.Visited);
                            
                            results.Cleaned.AddRange(result.Cleaned);
                            
                            currentX = result.final.X;
                            currentY = result.final.Y;
                            Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                            battery = Convert.ToInt32(result.Battery);
                        }
                    }
                    else if (step == InstructionModel.C)
                    {
                        results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        
                    }
                }
                else if (battery > 0)
                {
                    var result = Scenario5(currentX, currentY, battery, Facing, Maps);
                    results.Visited.AddRange(result.Visited);
                    
                    results.Cleaned.AddRange(result.Cleaned);
                    
                    currentX = result.final.X;
                    currentY = result.final.Y;
                    Facing = (facing)Enum.Parse(typeof(facing), result.final.facing, true);
                    battery = Convert.ToInt32(result.Battery);
                }
                else
                {
                    results.Battery = battery.ToString();
                }
            }
            results.final.X = currentX;
            results.final.Y = currentY;
            results.final.facing = Facing.ToString();

            return results;

        }
        private ResultModel Scenario5(int X, int Y, int Battery, facing Facing, ICollection<ICollection<RoomInfo>> Maps)
        {
            var value = new List<string>();
            var Steps = new List<string> { "TL", "TL", "A" };
            var results = new ResultModel();
            results.Visited = new List<XY>();
            results.Cleaned = new List<XY>();
            results.final = new final();
            int battery = Convert.ToInt32(Battery);
            int currentX = X; int currentY = Y;
            foreach (var step in Steps)
            {
                var directions = new[] { "TL", "TR" };
                var ValidMap = ValidateMaps(Maps, X, Y);
                battery -= UsingBattery(step);
                results.Battery = battery.ToString();
                if (battery > 0 && ValidMap)
                {

                    if (directions.Any(x => x.Contains(step)))
                    {
                        Facing = ChangeDirection(Facing, step);
                    }
                    else if (step != InstructionModel.C)
                    {
                        int oldX = currentX; int oldY = currentY;
                        List<int> moving = Moving(Facing, currentX, currentY, step);
                        currentX = moving.ElementAt(0);
                        currentY = moving.ElementAt(1);
                        ValidMap = ValidateMaps(Maps, currentX, currentY);
                        if (ValidMap)
                        {
                            var newloc = Maps.ElementAt(currentY).ElementAt(currentX);
                            if (newloc == RoomInfo.S)
                            {
                                results.Visited.Add(new XY { X = currentX, Y = currentY });
                            }
                            else
                            {
                                return results;
                            }
                        }
                        else
                        {
                            return results;
                        }
                    }
                    else if (step == InstructionModel.C)
                    {
                        results.Cleaned.Add(new XY { X = currentX, Y = currentY });
                        
                    }
                }
                else if (battery > 0)
                {
                    return results;
                }
                else
                {
                    results.Battery = battery.ToString();
                }
            }
            results.final.X = currentX;
            results.final.Y = currentY;
            results.final.facing = Facing.ToString();

            return results;
        }
        #endregion

        #region Core Strategy
        private int UsingBattery(string Step)
        {
            int value = 0;
            switch(Step)
            {
                case InstructionModel.TL:
                    value = BatteryConsume.TL;
                    break;
                case InstructionModel.TR:
                    value = BatteryConsume.TR;
                    break;
                case InstructionModel.A:
                    value = BatteryConsume.A;
                    break;
                case InstructionModel.B:
                    value = BatteryConsume.B;
                    break;
                case InstructionModel.C:
                    value = BatteryConsume.C;
                    break;
            }

            return value;
        }

        private List<int> Moving(facing Facing, int X, int Y, string command)
        {
            switch (Facing)
            {
                case facing.N:
                    if (command == InstructionModel.A)
                        Y -= 1;
                    else if (command == InstructionModel.B)
                        Y += 1;
                    break;
                case facing.E:
                    if (command == InstructionModel.A)
                        X += 1;
                    else if (command == InstructionModel.B)
                        X -= 1;
                    break;
                case facing.S:
                    if (command == InstructionModel.A)
                         Y += 1;
                    else if (command == InstructionModel.B)
                        Y -= 1;                   
                    break;
                case facing.W:
                    if (command == InstructionModel.A)
                        X -= 1;
                    else if (command == InstructionModel.B)
                        X += 1;
                    break;
            }
                        
            var value = new List<int> {X, Y};

            return value;
        }
       
        private facing ChangeDirection(facing Facing, string command)
        {
            facing NewFacing = Facing;
            switch (Facing)
            {
                case facing.N:
                    if (command == InstructionModel.TL)
                        NewFacing = facing.W;
                    else if (command == InstructionModel.TR)
                        NewFacing = facing.E;
                    break;
                case facing.E:
                    if (command == InstructionModel.TL)
                        NewFacing = facing.N;
                    else if (command == InstructionModel.TR)
                        NewFacing = facing.S;
                    break;
                case facing.S:
                    if (command == InstructionModel.TL)
                        NewFacing = facing.E;
                    else if (command == InstructionModel.TR)
                        NewFacing = facing.W;
                    break;
                case facing.W:
                    if (command == InstructionModel.TL)
                        NewFacing = facing.S;
                    else if (command == InstructionModel.TR)
                        NewFacing = facing.N;
                    break;
            }

            return NewFacing;
        }

        private Boolean ValidateMaps (ICollection<ICollection<RoomInfo>> Maps, int X, int Y)
        {
            if (Y <= Maps.Count && Y >= 0)
            {
                if (X <= Maps.ElementAt(Y).Count -1 && X >= 0)
                {
                    return true;
                }              
                return false;
            }
            return false;
        }
        #endregion
    }
}
