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
        Dictionary<int,Creature> creatures = new Dictionary<int, Creature>();

        for (int i = 0; i < creatureCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            creatures.Add(int.Parse(inputs[0]),new Creature()
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
            Dictionary<int,Drone> playerDrones = new Dictionary<int, Drone>();
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
            for (int i = 0; i < myDroneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                playerDrones.Add(int.Parse(inputs[0]),
                    new Drone()
                    {
                        Id = int.Parse(inputs[0]),
                        Position = new Point(int.Parse(inputs[1]),
                                                int.Parse(inputs[2])),
                        Emergency = int.Parse(inputs[3]),
                        BatteryLevel = int.Parse(inputs[4])
                    }
                );
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
                if(playerDrones.ContainsKey(droneId))
                    playerDrones[droneId].ScannedNotSaved.Add(
                        creatures[creatureId]
                    );
            }

            int visibleCreatureCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < visibleCreatureCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var creatureId = int.Parse(inputs[0]);
                var creature = creatures[creatureId];
                if (creature == null) continue;
                creature.Scanned = true;
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

                if(!drone.RadarEntries.ContainsKey(radar))
                    drone.RadarEntries.Add(radar, new List<Creature>());

                drone.RadarEntries[radar].Add(
                    creatures[creatureId]
                );
            }

            /*most work is done here*/
            foreach(var droneItem in playerDrones)
            {
                var drone = droneItem.Value;
                var radarEntry = drone.RadarEntries.MaxBy(re => re.Value.Where(c => c.Scanned == false).Count());
                Point? target = null;
                
                if(drone.Position.Y <= 200)
                    drone.ScannedNotSaved.Clear();

                if(drone.ScannedNotSaved.Count >= 3 
                    && drone.Position.Y > 200)
                {
                    target = new Point(drone.Position.X, drone.Position.Y - Drone.MaxDistance);
                }
                else if(radarEntry.Value.Any(c => c.Scanned == false && c.Position.X != 0 && c.Position.Y != 0))
                {
                    target = radarEntry.Value.First(c => c.Scanned == false && c.Position.X != 0 && c.Position.Y != 0).Position;
                    drone.Move(target,false);
                }
                else
                {
                    target = radarEntry.Key switch 
                    {
                        RadarValues.TopLeft => new Point(drone.Position.X - Drone.MaxDistance, drone.Position.Y - Drone.MaxDistance),
                        RadarValues.TopRight => new Point(drone.Position.X + Drone.MaxDistance, drone.Position.Y - Drone.MaxDistance),
                        RadarValues.BottomLeft => new Point(drone.Position.X - Drone.MaxDistance, drone.Position.Y + Drone.MaxDistance),
                        RadarValues.BottomRight => new Point(drone.Position.X + Drone.MaxDistance, drone.Position.Y + Drone.MaxDistance),
                        _ => null
                    };
                }

                if(target is null)
                    drone.Wait(false);
                else
                    drone.Move(target,false);
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

public class Creature
{
    public int Id { get; set; }
    public int Color { get; set; }
    public int Type { get; set; }
    public Point Position { get; set; } = new Point();
    public bool Scanned { get; set; }
    public bool Visible { get; set; }
}

public class Drone
{
    public int Id { get; set; }
    public Point Position { get; set; } = new Point();
    public int BatteryLevel { get; set; }
    public const int MaxBatteryLevel = 30;
    public const int MaxDistance = 600;
    public const int MinLightRange = 800;
    public const int MaxLightRange = 2000;
    public int Emergency { get; set; }

    public List<Creature> ScannedNotSaved {get;set;} = new List<Creature>();

    public Dictionary<string, List<Creature>> RadarEntries {get;set;} = new Dictionary<string, List<Creature>>();

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