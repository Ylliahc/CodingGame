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
        List<Creature> creatures = new List<Creature>();

        for (int i = 0; i < creatureCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            creatures.Add(new Creature()
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
            List<Drone> playerDrones = new List<Drone>();
            for (int i = 0; i < myScanCount; i++)
            {
                int creatureId = int.Parse(Console.ReadLine());
                creatures.Find(c => c.Id == creatureId).Scanned = true;
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
                playerDrones.Add(
                    new Drone()
                    {
                        Id = int.Parse(inputs[0]),
                        Position = new Point( int.Parse(inputs[1]),  int.Parse(inputs[2])),
                        Emergency = int.Parse(inputs[3]),
                        BatteryLevel =  int.Parse(inputs[4])
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
            }
            int visibleCreatureCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < visibleCreatureCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var creature = creatures.First(c => c.Id ==int.Parse(inputs[0]));
                if(creature == null) continue;
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
            }


            

            playerDrones.ForEach(
                d => {
                    var creature = creatures.Where(c => c.Scanned == false)
                        .MinBy(c => Math.Sqrt(Math.Pow(c.Position.X - d.Position.X,2)+Math.Pow(c.Position.Y - d.Position.Y,2)));
                    if(creature != null)
                        d.Move(creature.Position, false);
                    else
                        d.Wait(false);
                }
            );
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
    public bool Scanned {get;set;}
}

public class Drone
{
    public int Id { get; set; }
    public Point Position { get; set; } = new Point();
    public int BatteryLevel { get; set; }
    public const int MaxBatteryLevel = 30;
    public int Emergency {get;set;}

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