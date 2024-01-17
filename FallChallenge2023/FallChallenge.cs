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

        for (int i = 0; i < creatureCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            GameContext.Instance.Creatures
                                    .Add(int.Parse(inputs[0])
                                            , new Creature()
                                            {
                                                Id = int.Parse(inputs[0]),
                                                Color = int.Parse(inputs[1]),
                                                Type = int.Parse(inputs[2])
                                            });

        }
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

                if (GameContext.Instance.PlayerDrones.ContainsKey(droneId))
                {
                    var drone = GameContext.Instance.PlayerDrones[droneId];
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
                    GameContext.Instance.PlayerDrones.Add(droneId,
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
                if (GameContext.Instance.PlayerDrones.ContainsKey(droneId))
                {
                    GameContext.Instance.PlayerDrones[droneId]
                                            .ScannedNotSaved.Add(
                                                GameContext.Instance.Creatures[creatureId]);
                    GameContext.Instance.Creatures[creatureId].Scanned = true;
                }
            }

            int visibleCreatureCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < visibleCreatureCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var creatureId = int.Parse(inputs[0]);
                var creature = GameContext.Instance.Creatures[creatureId];
                if (creature == null) 
                    continue;
                creature.Position.X = int.Parse(inputs[1]);
                creature.Position.Y = int.Parse(inputs[2]);
                creature.XVelocity = int.Parse(inputs[3]);
                creature.YVelocity = int.Parse(inputs[4]);
                Console.Error.WriteLine($"{creature.Id} {creature.Type} {creature.Position.X} {creature.Position.Y}");
            }
            int radarBlipCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < radarBlipCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int droneId = int.Parse(inputs[0]);
                int creatureId = int.Parse(inputs[1]);
                string radar = inputs[2];
                var drone = GameContext.Instance.PlayerDrones[droneId];

                if (!drone.RadarEntries.ContainsKey(radar))
                    drone.RadarEntries.Add(radar, new List<Creature>());

                drone.RadarEntries[radar].Add(
                    GameContext.Instance.Creatures[creatureId]
                );
            }

            /*most work is done here*/
            foreach (var droneItem in GameContext.Instance.PlayerDrones)
            {
                var drone = droneItem.Value;
                drone.EvaluateDanger();
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
    public const int DangerousType = -1;
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

    private DroneState _state;
    public DroneState State 
    { 
        get{ return _state; } 
        set
        { 
            if(_state != DroneState.Fleeing)
            {
                PreviousState = _state;
            }
            _state = value;
        } 
    }
    public DroneState PreviousState {get;set;}
    public List<Creature> ScannedNotSaved { get; set; } = new List<Creature>();

    public Dictionary<string, List<Creature>> RadarEntries { get; set; } = new Dictionary<string, List<Creature>>();

    public Drone()
    {
        _state = DroneState.ComingDown;
        PreviousState = DroneState.GoingToStartPoint;
    }

    public bool IsInDanger()
    {
        return GameContext.Instance.Creatures.Any(c => c.Value.Type == Creature.DangerousType && GetDistance(c.Value.Position) < 2000);
    }

    public Creature GetNearestDanger()
    {
        return GameContext.Instance.Creatures
            .Where(c => c.Value.Type == Creature.DangerousType)
            .MinBy(c => GetDistance(c.Value.Position)).Value;
    }

    public void EvaluateDanger()
    {
        if(IsInDanger())
        {
            State = DroneState.Fleeing;
            Console.Error.WriteLine("Is in danger");
        }
            
    }

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
        DroneState.ComingDown => new ComingDownStrategy(this),
        DroneState.Fleeing => new FleeingStrategy(this),
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
            case DroneState.ComingDown:
                State = DroneState.ReportingScan;
                break;
            case DroneState.Fleeing:
                State = PreviousState;
                break;
            default:
                State = DroneState.ComingDown;
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

    public int GetDistance(Point point)
    {
        return (int)Math.Ceiling(Math.Sqrt(Math.Pow(point.X - Position.X,2) + Math.Pow(point.Y - Position.Y,2)));
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
    ComingDown,
    SearchingFish,
    ReportingScan,
    Fleeing
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
    public string Message {get;set;}
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

public class ComingDownStrategy : IStrategy
{
    private Drone _drone;

    public ComingDownStrategy(Drone drone)
    {
        _drone = drone;
    }

    public StrategyResult Process()
    {
        
        bool useLight = _drone.BatteryLevel > 10 ? true : false;
        bool succeeded = false;
        var action = ActionType.MOVE;
        
        var target = new Point(x: _drone.Position.X
                                    ,y: _drone.Position.Y + (useLight ? Drone.MaxDistance : Drone.MinLightRange));

        if(_drone.Position.Y + Drone.MaxDistance >= Zones.YMax)
            succeeded = true;

        return new StrategyResult()
        {
            Action = ActionType.MOVE,
            Target = target,
            UseLight = useLight,
            Succeeded = succeeded
        };
    }
}

public class FleeingStrategy : IStrategy
{
    private Drone _drone;
    public FleeingStrategy(Drone drone)
    {
        _drone = drone;
    }

    public StrategyResult Process()
    {
        bool succeeded = false;
        bool useLight = _drone.BatteryLevel > 10 ? true : false;
        var action = ActionType.MOVE;
        var creature = _drone.GetNearestDanger();

        //var direction =  _drone.RadarEntries.First(re => re.Value.Contains(creature)).Key;

        var target = new Point(
            x: _drone.Position.X + creature.XVelocity - (_drone.Position.X + creature.Position.X),
            y: _drone.Position.Y + creature.YVelocity - (_drone.Position.Y + creature.Position.Y)
        );

        // var target = direction switch
        // {
        //     RadarValues.BottomRight => new Point(x: _drone.Position.X, y: _drone.Position.Y - Drone.MaxDistance),
        //     RadarValues.BottomLeft => new Point(x: _drone.Position.X, y: _drone.Position.Y - Drone.MaxDistance),
        //     RadarValues.TopRight => new Point(x: _drone.Position.X - Drone.MaxDistance,
        //                                             y: _drone.Position.Y - Drone.MaxDistance),
        //     RadarValues.TopLeft => new Point(x: _drone.Position.X + Drone.MaxDistance,
        //                                             y: _drone.Position.Y - Drone.MaxDistance),
        //     _ => new Point(_drone.Position.X, _drone.Position.Y)
        // };

        if (!_drone.IsInDanger())
        {
            succeeded = true;
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

public sealed class GameContext
{
    private static Lazy<GameContext> _instance 
                            = new Lazy<GameContext>(() => new GameContext());

    public readonly Dictionary<int, Creature> Creatures;
    public readonly Dictionary<int, Drone> PlayerDrones;

    private GameContext()
    {
        Creatures = new Dictionary<int, Creature>();
        PlayerDrones = new Dictionary<int, Drone>();
    }

    public static GameContext Instance
    {
        get
        {
            return _instance.Value;
        }
    }
}