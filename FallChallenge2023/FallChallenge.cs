using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Score points by scanning valuable fish faster than your opponent.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int creatureCount = int.Parse(Console.ReadLine());
        Dictionary<int, Creature> creatures = new Dictionary<int, Creature>();

        for (int i = 0; i < creatureCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            creatures.Add(int.Parse(inputs[0]), new Creature()
            {
                Id = int.Parse(inputs[0]),
                Color = int.Parse(inputs[1]),
                Type = int.Parse(inputs[2])
            });

        }

        Dictionary<int, Drone> playerDrones = new Dictionary<int, Drone>();

        // game loop
        while (true)
        {
            int myScore = int.Parse(Console.ReadLine());
            int foeScore = int.Parse(Console.ReadLine());
            int myScanCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < myScanCount; i++)
            {
                int creatureId = int.Parse(Console.ReadLine());
            }
            int foeScanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < foeScanCount; i++)
            {
                int creatureId = int.Parse(Console.ReadLine());
            }
            int myDroneCount = int.Parse(Console.ReadLine());
            int swimStart = 0;
            int swimlaneLength = Zones.XMax / myDroneCount;
            for (int i = 0; i < myDroneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var droneId = int.Parse(inputs[0]);
                var droneX = int.Parse(inputs[1]);
                var droneY = int.Parse(inputs[2]);
                var emergency = int.Parse(inputs[3]);
                var batteryLevel = int.Parse(inputs[4]);

                if (playerDrones.ContainsKey(droneId))
                {
                    var drone = playerDrones[droneId];
                    drone.Emergency = emergency;
                    drone.Position.X = droneX;
                    drone.Position.Y = droneY;
                    drone.BatteryLevel = batteryLevel;
                }
                else
                {
                    int swimlaneStop = swimStart + swimlaneLength;
                    if (i == myDroneCount - 1)
                        swimlaneStop = Zones.XMax;
                    playerDrones.Add(droneId,
                        new Drone()
                        {
                            Id = droneId,
                            Position = new Point(droneX, droneY),
                            Emergency = emergency,
                            BatteryLevel = batteryLevel,
                            Swimlane = new Swimlane(swimStart, swimlaneStop)
                        }
                    );

                    swimStart += swimlaneLength;
                }
            }
            int foeDroneCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < foeDroneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int droneX = int.Parse(inputs[1]);
                int droneY = int.Parse(inputs[2]);
                int emergency = int.Parse(inputs[3]);
                int battery = int.Parse(inputs[4]);
            }
            int droneScanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < droneScanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int creatureId = int.Parse(inputs[1]);
                if (playerDrones.ContainsKey(droneId))
                {
                    playerDrones[droneId].ScannedNotSaved.Add(
                        creatures[creatureId]);
                    creatures[creatureId].Scanned = true;
                }
            }

            int visibleCreatureCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < visibleCreatureCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var creatureId = int.Parse(inputs[0]);
                var creature = creatures[creatureId];
                if (creature == null) continue;
                creature.Position.X = int.Parse(inputs[1]);
                creature.Position.Y = int.Parse(inputs[2]);
                int creatureVx = int.Parse(inputs[3]);
                int creatureVy = int.Parse(inputs[4]);

            }
            int radarBlipCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < radarBlipCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int creatureId = int.Parse(inputs[1]);
                string radar = inputs[2];
                var drone = playerDrones[droneId];

                if (!drone.RadarEntries.ContainsKey(radar))
                    drone.RadarEntries.Add(radar, new List<Creature>());

                drone.RadarEntries[radar].Add(
                    creatures[creatureId]
                );
            }

            /*most work is done here*/
            foreach (var droneItem in playerDrones)
            {
                var drone = droneItem.Value;
                drone.Act();
            }
        }
    }
}

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point()
    {

    }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Swimlane
{
    public Swimlane(int start, int stop)
    {
        Start = start;
        Stop = stop;
    }

    public int Start { get; }
    public int Stop { get; }
}

public class Creature
{
    public int Id { get; set; }
    public int Color { get; set; }
    public int Type { get; set; }
    public Point Position { get; set; } = new Point();
    public bool Scanned { get; set; }
    public bool Visible { get; set; }
    public int XVelocity { get; set; }
    public int YVelocity { get; set; }
}

public class Drone
{
    public int Id { get; set; }
    public Point Position { get; set; } = new Point();
    public Swimlane Swimlane { get; set; } = new Swimlane(0, 0);
    public int BatteryLevel { get; set; }
    public const int MaxBatteryLevel = 30;
    public const int MaxDistance = 600;
    public const int MinLightRange = 800;
    public const int MaxLightRange = 2000;
    public int Emergency { get; set; }
    public DroneState State { get; set; }
    public List<Creature> ScannedNotSaved { get; set; } = new List<Creature>();

    public Dictionary<string, List<Creature>> RadarEntries { get; set; } = new Dictionary<string, List<Creature>>();

    public void Act()
    {
        var strategy = ChoseStragegy();

        var result = strategy.Process();

        if (result.Succeeded)
        {
            NexStep();
        }

        Console.Error.WriteLine($"Drone {Id} {State} {result.Succeeded} {result.Action}");
        if (result.Action == ActionType.MOVE)
        {
            Move(result.Target, result.UseLight);
        }
        else
        {
            Wait(result.UseLight);
        }
    }

    private IStrategy ChoseStragegy() => State switch
    {
        DroneState.GoingToStartPoint => new GoingToStartPointStrategy(this),
        DroneState.ReportingScan => new ReportScanStrategy(this),
        DroneState.SearchingFish => new SearchingFishStrategy(this),
        _ => new WaitingStrategy()
    };


    private void NexStep()
    {
        switch (State)
        {
            case DroneState.GoingToStartPoint:
            case DroneState.ReportingScan:
                State = DroneState.SearchingFish;
                break;
            case DroneState.SearchingFish:
                State = DroneState.ReportingScan;
                break;
            default:
                State = DroneState.GoingToStartPoint;
                break;
        }
    }

    public void Move(Point target, bool useLight)
    {
        var light = useLight ? 1 : 0;
        Console.WriteLine($"MOVE {target.X} {target.Y} {light}");
    }

    public void Wait(bool useLight)
    {
        var light = useLight ? 1 : 0;
        Console.WriteLine($"WAIT {light}");
    }
}

public class RadarValues
{
    public const string TopLeft = "TL";
    public const string TopRight = "TR";
    public const string BottomRight = "BR";
    public const string BottomLeft = "BL";
}

public class Zones
{
    public const int SurfaceLimit = 500;
    public const int YMinType0Zone = 2500;
    public const int YMaxType0Zone = 5000;
    public const int YMinType1Zone = 5000;
    public const int YMaxType1Zone = 7500;
    public const int YMin = 0;
    public const int YMax = 9999;
    public const int XMin = 0;
    public const int XMax = 9999;
}

public enum DroneState
{
    GoingToStartPoint,
    SearchingFish,
    ReportingScan
}

public interface IStrategy
{
    StrategyResult Process();
}

public class StrategyResult
{
    public Point? Target { get; set; }
    public ActionType Action { get; set; }
    public bool UseLight { get; set; }
    public bool Succeeded { get; set; }
}

public enum ActionType
{
    MOVE,
    WAIT
}

public class GoingToStartPointStrategy : IStrategy
{
    private Drone _drone;
    public GoingToStartPointStrategy(Drone drone)
    {
        _drone = drone;
    }

    public StrategyResult Process()
    {
        Point target;
        bool succeeded = false;
        bool useLight = false;
        var action = ActionType.MOVE;
        if (_drone.Swimlane.Start == Zones.XMin)
        {
            target = new Point(Zones.XMin, Zones.YMaxType0Zone);
        }
        else if (_drone.Swimlane.Stop == Zones.XMax)
        {
            target = new Point(Zones.XMax, Zones.YMaxType1Zone);
        }
        else
        {
            var length = (_drone.Swimlane.Stop - _drone.Swimlane.Start) / 2;
            target = new Point(x: _drone.Swimlane.Start + length,
                                    y: Zones.YMaxType0Zone);
        }
        Console.Error.WriteLine($"{target.X} | {_drone.Swimlane.Start} - {_drone.Swimlane.Stop}");
        if (_drone.Position.X == target.X
            && _drone.Position.Y == target.Y)
        {
            succeeded = true;
            action = ActionType.WAIT;
            useLight = true;
            _drone.ScannedNotSaved.Clear();
        }

        return new StrategyResult()
        {
            Target = target,
            Action = action,
            UseLight = useLight,
            Succeeded = succeeded
        };
    }
}

public class ReportScanStrategy : IStrategy
{
    private Drone _drone;
    public ReportScanStrategy(Drone drone)
    {
        _drone = drone;
    }

    public StrategyResult Process()
    {
        Point target = new Point(_drone.Position.X, Zones.SurfaceLimit);
        bool succeeded = false;
        var action = ActionType.MOVE;
        Console.Error.WriteLine($"{target.X} | {_drone.Swimlane.Start} - {_drone.Swimlane.Stop}");
        
        if (_drone.Position.Y <= Zones.SurfaceLimit)
        {
            succeeded = true;
            action = ActionType.WAIT;
            _drone.ScannedNotSaved.Clear();
        }

        return new StrategyResult()
        {
            Target = target,
            Action = action,
            UseLight = false,
            Succeeded = succeeded
        };
    }
}


public class SearchingFishStrategy : IStrategy
{
    private Drone _drone;

    public SearchingFishStrategy(Drone drone)
    {
        _drone = drone;
    }
    public StrategyResult Process()
    {
        bool useLight = _drone.BatteryLevel > 10 ? true : false;
        bool succeeded = false;
        var action = ActionType.MOVE;

        var direction = _drone.RadarEntries.MaxBy(creatures => creatures.Value.Where(c => c.Scanned == false).Count()).Key;

        var target = direction switch
        {
            RadarValues.TopLeft => new Point(x: _drone.Position.X - Drone.MaxDistance,
                                                y: _drone.Position.Y - Drone.MaxDistance),
            RadarValues.TopRight => new Point(x: _drone.Position.X + Drone.MaxDistance,
                                                y: _drone.Position.Y - Drone.MaxDistance),
            RadarValues.BottomLeft => new Point(x: _drone.Position.X - Drone.MaxDistance,
                                                    y: _drone.Position.Y + Drone.MaxDistance),
            RadarValues.BottomRight => new Point(x: _drone.Position.X + Drone.MaxDistance,
                                                    y: _drone.Position.Y + Drone.MaxDistance),
            _ => new Point(_drone.Position.X, _drone.Position.Y)
        };

        if (_drone.ScannedNotSaved.Count >= 8)
        {
            succeeded = true;
            action = ActionType.WAIT;
        }

        return new StrategyResult()
        {
            Action = action,
            Target = target,
            UseLight = useLight,
            Succeeded = succeeded
        };
    }
}

public class WaitingStrategy : IStrategy
{
    public StrategyResult Process()
    {
        return new StrategyResult()
        {
            Action = ActionType.WAIT,
            UseLight = false,
            Succeeded = true
        };
    }
}

