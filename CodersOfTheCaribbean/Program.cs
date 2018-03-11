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
            var entityIds = new List<int>();
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
                switch (entityType)
                {
                    case Constants.ShipEntity:
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
                        break;
                    case Constants.BarrelEntity:
                        if (!context.Entities.Any(e => e.Id == entityId))
                        {
                            var barrel = new Barrel();
                            barrel.Id = entityId;
                            barrel.Position = new Position(x, y);
                            barrel.RumAmount = arg1;
                            context.Barrels.Add(barrel);
                            context.Entities.Add(barrel);
                        }
                        else
                        {
                            var barrel = context.Entities.FirstOrDefault(e => e.Id == entityId) as Barrel;
                            barrel.Position = new Position(x, y);
                            barrel.RumAmount = arg1;
                        }
                        break;
                    case Constants.MineEntity:
                        var mine = new Mine();
                        mine.Id = entityId;
                        mine.Position = new Position(x, y);
                        context.Mines.Add(mine);
                        break;
                    case Constants.CannonballEntity:

                        break;
                }
                entityIds.Add(entityId);
                #endregion Handle Input
            }

            context.RemoveEntities(entityIds);

            for (int i = 0; i < myShipCount; i++)
            {
                var ship = context.MyShips[i];
                if (ship != null)
                {
                    context.UpdateStats(ship);
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
    public const int MaxSpeed = 2;
    public const int ShipWidth = 1;
    public const int ShipLength = 3;
    public const int MaxUnitsOfRum = 100;
    public const int FireRange = 10;
    public const int MineCooldown = 5;
    public const int CannonballCooldown = 2;
    public const string MoveCommand = "MOVE";
    public const string WaitCommand = "WAIT";
    public const string SlowerCommand = "SLOWER";
    public const string FireCommand = "FIRE";
    public const string MineCommand = "MINE";
    public const string ShipEntity = "SHIP";
    public const string BarrelEntity = "BARREL";
    public const string MineEntity = "MINE";
    public const string CannonballEntity = "CANNONBALL";
}

public static class Utils
{
    public static double DeltaXFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.NW:
            case Direction.SW:
                return 0.5;
            case Direction.W:
                return 1;
            case Direction.E:
                return -1;
            case Direction.NE:
            case Direction.SE:
                return -0.5;
            default:
                return 0;
        }
    }

    public static int DeltaYFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.NW:
            case Direction.NE:
                return 1;
            case Direction.SW:
            case Direction.SE:
                return -1;
            default:
                return 0;
        }
    }

    public static int GetCannonballFlightInTurns(Position source, Position target)
    {
        return (int)Math.Round(1 + CalculateDistance(source, target) / 3);
    }

    public static double CalculateDistance(Position p1, Position p2)
    {
        return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }

    public static Direction GetDirection(Position p1, Position p2)
    {
        var n = 270 - (Math.Atan2(p2.Y - p1.Y, p2.X - p1.X)) * 180 / Math.PI;
        var angleInDegrees = n % 360;

        if (angleInDegrees < 60)
        {
            return Direction.NE;
        }
        if (angleInDegrees < 120)
        {
            return Direction.E;
        }
        if (angleInDegrees < 180)
        {
            return Direction.SE;
        }
        if (angleInDegrees < 240)
        {
            return Direction.SW;
        }
        if (angleInDegrees < 300)
        {
            return Direction.W;
        }

        return Direction.NW;
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
        this.Mines = new List<Mine>();
    }

    public IList<Ship> MyShips { get; set; }
    public IList<Ship> EnemyShips { get; set; }
    public IList<Barrel> Barrels { get; set; }
    public IList<Mine> Mines { get; set; }
    public IList<Entity> Entities { get; set; }

    public void ProcessTurn(Ship ship)
    {
        if (this.AvoidMines(ship))
        {
            return;
        }

        if (this.SlowDown(ship))
        {
            return;
        }

        if (this.FireAtEnemyShips(ship))
        {
            return;
        }

        //if (this.PlaceMines(ship))
        //{
        //    return;
        //}

        if (this.MoveToClosestBarrel(ship))
        {
            return;
        }

        var closestEnemy = this.GetClosestEntity<Ship>(ship, this.EnemyShips, x => true);
        if (closestEnemy != null)
        {
            ship.Move(closestEnemy.Position);
        }
        else
        {
            ship.Wait();
        }
    }

    private bool SlowDown(Ship ship)
    {
        var closestBarrel = GetClosestEntity<Barrel>(ship, this.Barrels, x => true);
        var isBarrelClose = closestBarrel != null &&
            Utils.CalculateDistance(ship.Position, closestBarrel.Position) < 5;
        if (isBarrelClose &&
            ship.Speed > 1)
        {
            ship.Slower();
            return true;
        }
        else if (isBarrelClose && ship.Speed == 1)
        {
            ship.Move(closestBarrel.Position);
            return true;
        }

        return false;
    }

    private bool AvoidMines(Ship ship)
    {
        var closestMine = this.GetClosestEntity<Mine>(ship, this.Mines, x => true);
        if (closestMine != null)
        {
            var distance = Utils.CalculateDistance(ship.Position, closestMine.Position);
            if (distance < ship.Speed * 3)
            {
                ship.Fire(closestMine.Position);
                return true;
            }
        }

        return false;
        // Calculate Path
        // Check if any mines are in the path
        // Change direction if any mines in path
    }

    public void UpdateStats(Ship ship)
    {
        if (ship.MineCooldown > 0)
        {
            ship.MineCooldown--;
        }

        if (ship.CannonballCooldown > 0)
        {
            ship.CannonballCooldown--;
        }
    }

    private bool PlaceMines(Ship ship)
    {
        if (ship.MineCooldown == 0)
        {
            ship.Mine();
            return true;
        }

        return false;
    }

    private bool FireAtEnemyShips(Ship ship)
    {
        foreach (var enemyShip in this.EnemyShips)
        {
            var distance = Utils.CalculateDistance(ship.Position, enemyShip.Position);
            var direction = Utils.GetDirection(ship.Position, enemyShip.Position);
            if (distance < Constants.FireRange && ship.CannonballCooldown == 0)
            {
                Position cannonBallTarget;
                if (enemyShip.Speed == 0)
                {
                cannonBallTarget = enemyShip.Position;
                }
                else
                {
                    var turns = Utils.GetCannonballFlightInTurns(ship.Position, enemyShip.Position);
                    var deltaX = Utils.DeltaXFromDirection(enemyShip.Direction) * turns * enemyShip.Speed;
                    var deltaY = Utils.DeltaYFromDirection(enemyShip.Direction) * turns * enemyShip.Speed;
                    var newX = (int)Math.Max(enemyShip.Position.X + deltaX, 0);
                    var newY = (int)Math.Max(enemyShip.Position.Y + deltaY, 0);
                    cannonBallTarget = new Position(Math.Min(newX, Constants.FieldWidth - 1), Math.Min(newY, Constants.FieldHeight - 1));
                }

                ship.Fire(cannonBallTarget);
                return true;
            }
        }

        return false;
    }

    private bool MoveToClosestBarrel(Ship ship)
    {
        if (this.Barrels.Count > 0)
        {
            var allBarrels = new List<Barrel>(this.Barrels);
            var closestBarrel = this.GetClosestEntity<Barrel>(ship, this.Barrels, (x => true));
            if (closestBarrel != null)
            {
                ship.Move(closestBarrel.Position);
                return true;
            }
            else
            {
                ship.Wait();
                return true;
            }
        }

        return false;
    }

    public T GetClosestEntity<T>(Ship ship, IList<T> entities, Func<T, bool> predicate)
        where T : Entity
    {
        T closestEntity = entities.FirstOrDefault(predicate);
        if (closestEntity != null)
        {
            foreach (var entity in entities)
            {
                var d1 = Utils.CalculateDistance(ship.Position, closestEntity.Position);
                var d2 = Utils.CalculateDistance(ship.Position, entity.Position);

                if (d2 < d1)
                {
                    closestEntity = entity;
                }
            }
        }

        return closestEntity;
    }

    internal void RemoveEntities(List<int> entityIds)
    {
        var entititesToRemove = GetEntitiesToRemove(entityIds, this.Entities);
        foreach (var entity in entititesToRemove)
        {
            if (this.Barrels.Contains(entity))
            {
                var barrel = entity as Barrel;
                this.Barrels.Remove(barrel);
            }
            else if (this.MyShips.Contains(entity))
            {
                var ship = entity as Ship;
                this.MyShips.Remove(ship);
            }
            else if (this.EnemyShips.Contains(entity))
            {
                var ship = entity as Ship;
                this.EnemyShips.Remove(ship);
            }
            else if (this.Mines.Contains(entity))
            {
                var mine = entity as Mine;
                this.Mines.Remove(mine);
            }

            this.Entities.Remove(entity);
        }
    }

    private IList<Entity> GetEntitiesToRemove(List<int> entityIds, IList<Entity> entities)
    {
        var entitiesToRemove = new List<Entity>();
        foreach (var entity in entities)
        {
            if (!entityIds.Contains(entity.Id))
            {
                entitiesToRemove.Add(entity);
            }
        }

        return entitiesToRemove;
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

public class Mine : Entity
{ }

public class Ship : Entity
{
    public int Speed { get; set; }

    public Direction Direction { get; set; }

    public int RumAmount { get; set; }

    public int MineCooldown { get; set; }
    public int CannonballCooldown { get; set; }

    public void Move(Position p)
    {
        Console.WriteLine($"{Constants.MoveCommand} {p.X} {p.Y}");
    }

    public void Fire(Position p)
    {
        this.CannonballCooldown = Constants.CannonballCooldown;
        Console.WriteLine($"{Constants.FireCommand} {p.X} {p.Y}");
    }

    public void Mine()
    {
        this.MineCooldown = Constants.MineCooldown;
        Console.WriteLine($"{Constants.MineCommand}");
    }

    public void Wait()
    {
        Console.WriteLine($"{Constants.WaitCommand}");
    }

    public override string ToString()
    {
        return $"{this.Position}";
    }

    public void Slower()
    {
        Console.WriteLine($"{Constants.SlowerCommand}");
    }
}

public class Barrel : Entity
{
    public int RumAmount { get; set; }

    public bool IsTaken { get; set; }
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

    public override int GetHashCode()
    {
        return 23 * this.X * 91 * this.Y;
    }

    public override string ToString()
    {
        return $"{this.X} {this.Y}";
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
