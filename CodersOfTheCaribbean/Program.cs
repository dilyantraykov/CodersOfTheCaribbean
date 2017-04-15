using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        var context = new Context();
        // game loop
        while (true)
        {
            int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)
            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int arg1 = int.Parse(inputs[4]);
                int arg2 = int.Parse(inputs[5]);
                int arg3 = int.Parse(inputs[6]);
                int arg4 = int.Parse(inputs[7]);

                #region Handle Input
                if (entityType == Constants.ShipEntity)
                {
                    if (!context.Entities.Any(e => e.Id == entityId))
                    {
                        var ship = new Ship();
                        ship.Id = entityId;
                        ship.Position = new Position(x, y);
                        ship.Direction = (Direction)arg1;
                        ship.Speed = arg2;
                        ship.RumAmount = arg3;
                        if (arg4 == 1)
                        {
                            context.MyShips.Add(ship);
                        }
                        else
                        {
                            context.EnemyShips.Add(ship);
                        }

                        context.Entities.Add(ship);
                    }
                    else
                    {
                        var ship = context.Entities.FirstOrDefault(e => e.Id == entityId) as Ship;
                        ship.Position = new Position(x, y);
                        ship.Direction = (Direction)arg1;
                        ship.Speed = arg2;
                        ship.RumAmount = arg3;
                    }
                }
                else if (entityType == Constants.BarrelEntity)
                {
                    if (!context.Entities.Any(e => e.Id == entityId))
                    {
                        var barrel = new Barrel();
                        barrel.Id = entityId;
                        barrel.Position = new Position(x, y);
                        barrel.RumAmount = arg1;
                        context.Barrels.Add(barrel);
                    }
                    else
                    {
                        var barrel = context.Entities.FirstOrDefault(e => e.Id == entityId) as Barrel;
                        barrel.Position = new Position(x, y);
                        barrel.RumAmount = arg1;
                    }
                }
#endregion Handle Input
            }
            for (int i = 0; i < myShipCount; i++)
            {
                var ship = context.MyShips[i];
                if (ship != null)
                {
                    context.ProcessTurn(ship);
                }
            }
        }
    }
}

public static class Constants
{
    public const int FieldWidth = 23;
    public const int FieldHeight = 21;
    public const int ShipWidth = 1;
    public const int ShipLength = 3;
    public const int MaxUnitsOfRum = 100;
    public const string MoveCommand = "MOVE";
    public const string WaitCommand = "WAIT";
    public const string SlowerCommand = "SLOWER";
    public const string ShipEntity = "SHIP";
    public const string BarrelEntity = "BARREL";
}

public static class Utils
{
    public static double CalculateDistance(Position p1, Position p2)
    {
        return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }
}

public class Context
{
    public Context()
    {
        this.Entities = new List<Entity>();
        this.MyShips = new List<Ship>();
        this.EnemyShips = new List<Ship>();
        this.Barrels = new List<Barrel>();
    }

    public IList<Ship> MyShips { get; set; }
    public IList<Ship> EnemyShips { get; set; }
    public IList<Barrel> Barrels { get; set; }
    public IList<Entity> Entities { get; set; }

    public void ProcessTurn(Ship ship)
    {
        
    }
}

public class Entity
{
    public int Id { get; set; }
    public Position Position { get; set; }

    public override bool Equals(object obj)
    {
        var otherEntity = obj as Entity;
        if (otherEntity.Id == this.Id)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}

public class Ship : Entity
{
    public int Speed { get; set; }

    public Direction Direction { get; set; }

    public int RumAmount { get; set; }
}

public class Barrel : Entity
{
    public int RumAmount { get; set; }
}

public struct Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        var p2 = (Position)obj;
        return this.X == p2.X && this.Y == p2.Y;
    }
}

public enum Direction
{
    W = 0,
    NW = 1,
    NE = 2,
    E = 3,
    SE = 4,
    SW = 5
}
