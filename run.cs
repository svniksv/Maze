using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    //test
    public static char[] Name = { 'A', 'B', 'C', 'D' };
    public static int[] Enterences = { 2, 4, 6, 8 };
    public static int[] EnergyCost = { 1, 10, 100, 1000 };
    static int Solve(List<string> lines)
    {
        var originalMaze = new Maze(lines); // получаем текущий лабиринт
        var visited = new HashSet<Maze>(); // будем хранить все движения в лабиринте
        var queue = new PriorityQueue<Maze, int>(); // приоритетная очередь по затраченной энергии
        List<Maze> mazes;

        queue.Enqueue(originalMaze, originalMaze.Energy); // вносим первый элемент очереди для старта

        // получаем все возможные движения в лабиринте с помощью алгоритма Дейкстра и вносим их в очередь
        while (queue.Count > 0)
        {
            var currentMaze = queue.Dequeue(); // берем следующий элемент из очереди

            if (currentMaze.IsDone()) return currentMaze.Energy; // проверяем пройден ли лабиринт 

            if (visited.Contains(currentMaze)) continue; // если такой лабиринт уже был, переходим дальше. 
            visited.Add(currentMaze);

            for (int i = 0; i < 4; i++)
            {
                if (IsRoomDone(currentMaze, i) || IsRoomAvailable(currentMaze, i)) continue; // не трогаем готовые комнаты и те, где объекты уже стоят корректно
                else
                {
                    //ходим из комнаты влево
                    mazes = MoveOutAndLeft(currentMaze, i);
                    if (mazes != null)
                        foreach (Maze maze in mazes) queue.Enqueue(maze, maze.Energy);

                    //ходим вправо
                    mazes = MoveOutAndRight(currentMaze, i);
                    if (mazes != null)
                        foreach (Maze maze in mazes) queue.Enqueue(maze, maze.Energy);
                }
            }
            //заходим в комнату
            mazes = MoveInRoom(currentMaze);
            if (mazes != null)
                foreach (Maze maze in mazes) queue.Enqueue(maze, maze.Energy);
        }

        return 0;
    }

    //выход из комнаты влево
    public static List<Maze> MoveOutAndLeft(Maze maze, int room)
    {
        var list = new List<Maze>();
        int pos = GetIndexOfTopObject(maze, room);
        if (pos == -1) return list;
        int energyOfObject = EnergyCost[Array.IndexOf(Name, maze.Rooms[room, pos])];
        Maze currentMaze = maze.Copy();

        // проверяем, можем ли выйти из комнаты
        if (currentMaze.Hall[room * 2 + 1] != '.') return list;
        else
        {
            currentMaze.Energy += energyOfObject * (2 + pos);
            currentMaze.Hall[room * 2 + 1] = currentMaze.Rooms[room, pos];
            currentMaze.Rooms[room, pos] = '.';
            list.Add(currentMaze.Copy());
        }

        //проверяем возможность движения влево
        for (int i = room * 2; i >= 0; i--)
        {
            if (currentMaze.Hall[i] != '.') break;
            else
            {
                currentMaze.Energy += energyOfObject;
                currentMaze.Hall[i] = currentMaze.Hall[i + 1];
                currentMaze.Hall[i + 1] = '.';
                if (!Enterences.Contains(i)) list.Add(currentMaze.Copy());
            }
        }
        return list;
    }

    //выход из комнаты вправо
    public static List<Maze> MoveOutAndRight(Maze maze, int room)
    {
        var list = new List<Maze>();
        int pos = GetIndexOfTopObject(maze, room);
        if (pos == -1) return list;
        int energyOfObject = EnergyCost[Array.IndexOf(Name, maze.Rooms[room, pos])];
        Maze currentMaze = maze.Copy();

        // проверяем, можем ли выйти из комнаты
        if (currentMaze.Hall[room * 2 + 3] != '.') return list;
        else
        {
            currentMaze.Energy += energyOfObject * (2 + pos);
            currentMaze.Hall[room * 2 + 3] = currentMaze.Rooms[room, pos];
            currentMaze.Rooms[room, pos] = '.';
            list.Add(currentMaze.Copy());
        }

        //проверяем возможность движения вправо
        for (int i = room * 2 + 4; i < currentMaze.Hall.Length; i++)
        {
            if (currentMaze.Hall[i] != '.') break;
            else
            {
                currentMaze.Energy += energyOfObject;
                currentMaze.Hall[i] = currentMaze.Hall[i - 1];
                currentMaze.Hall[i - 1] = '.';
                if (!Enterences.Contains(i)) list.Add(currentMaze.Copy());
            }
        }
        return list;
    }

    //заходим в комнату
    public static List<Maze> MoveInRoom(Maze maze)
    {
        var list = new List<Maze>();
        for (int i = 0; i < maze.Hall.Length; i++) //проходим по каждому элементу корридора 
        {
            if (maze.Hall[i] != '.') // находим объекты в корриодре
            {
                var pos = Array.IndexOf(Name, maze.Hall[i]); //индекс целевой комнаты
                var indexOfObject = GetIndexOfTopObject(maze, pos);
                var energyOfObject = EnergyCost[pos]; // затрачиваемая энергия у объекта
                Maze currentMaze = maze.Copy();
                if (IsRoomAvailable(maze, pos))
                {
                    if (i < pos * 2 + 2)
                    {
                        for (int k = i + 1; k <= pos * 2 + 2; k++)
                        {
                            if (maze.Hall[k] != '.') break;
                            else
                            {
                                currentMaze.Energy += energyOfObject;
                                if (k == pos * 2 + 2)
                                {
                                    currentMaze.Energy += energyOfObject * (indexOfObject == -1 ? maze.RoomSize : indexOfObject);
                                    currentMaze.Rooms[pos, (indexOfObject == -1 ? maze.RoomSize - 1 : indexOfObject - 1)] = currentMaze.Hall[i];
                                    currentMaze.Hall[i] = '.';
                                    list.Add(currentMaze.Copy());
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int k = i - 1; k >= pos * 2 + 2; k--)
                        {
                            if (maze.Hall[k] != '.') break;
                            else
                            {
                                currentMaze.Energy += energyOfObject;
                                if (k == pos * 2 + 2)
                                {
                                    currentMaze.Energy += energyOfObject * (indexOfObject == -1 ? maze.RoomSize : indexOfObject);
                                    currentMaze.Rooms[pos, (indexOfObject == -1 ? maze.RoomSize - 1 : indexOfObject - 1)] = currentMaze.Hall[i];
                                    currentMaze.Hall[i] = '.';
                                    list.Add(currentMaze.Copy());
                                }
                            }
                        }
                    }
                }
            }
        }
        return list;
    }

    //проверям заполнена ли целевая комната корректно
    public static bool IsRoomDone(Maze maze, int i)
    {
        for (int j = 0; j < maze.RoomSize; j++)
            if (!(maze.Rooms[i, j] == Name[i])) return false;
        return true;
    }

    //можно ли войти в комнату
    public static bool IsRoomAvailable(Maze maze, int room)
    {
        for (int i = 0; i < maze.RoomSize; i++)
            if (maze.Rooms[room, i] != '.' && maze.Rooms[room, i] != Name[room]) return false;
        return true;
    }

    // получаем индекс верхнего объекта в комнате,
    public static int GetIndexOfTopObject(Maze maze, int i)
    {
        for (int j = 0; j < maze.RoomSize; j++)
            if (maze.Rooms[i, j] != '.') return j;
        return -1; // комната пуста
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }

        int result = Solve(lines);
        Console.WriteLine(result);
    }


}

public class Maze
{
    public char[] Hall { get; set; }
    public char[,] Rooms { get; set; }
    public int RoomSize { get; set; }
    public int Energy { get; set; }

    public Maze(List<string> data)
    {
        RoomSize = (data.Count == 5) ? 2 : 4;
        Energy = 0;
        Hall = new char[11];
        Rooms = new char[4, RoomSize];
        for (int i = 1; i <= 11; i++) Hall[i - 1] = data[1][i];
        for (int i = 1; i <= 4; i++)
        {
            for (int j = 2; j < 2 + RoomSize; j++)
            {
                Rooms[i - 1, j - 2] = data[j][i * 2 + 1];
            }
        }
    }

    public Maze Copy()
    {
        Maze maze = (Maze)MemberwiseClone();
        maze.Rooms = (char[,])Rooms.Clone();
        maze.Hall = (char[])Hall.Clone();
        return maze;
    }

    public bool IsDone()
    {
        //bool done = false;
        for (int i = 0; i < RoomSize; i++)
            if (!(Rooms[0, i] == 'A' && Rooms[1, i] == 'B' && Rooms[2, i] == 'C' && Rooms[3, i] == 'D')) return false;
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Maze maze)
        {
            for (int i = 0; i < 11; i++)
                if (Hall[i] != maze.Hall[i]) return false;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < RoomSize; j++)
                    if (Rooms[i, j] != maze.Rooms[i, j]) return false;
            }

            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (char room in Rooms) hash.Add(room);
        foreach (char h in Hall) hash.Add(h);
        return hash.ToHashCode();
    }
}