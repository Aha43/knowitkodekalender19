using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Jul19
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //int tall = 8991;
           

           

            var luke = 13;

            if (luke == 9)
            {
                var numbers = await Luke9_GetNumbersAsync("https://julekalender.knowit.no/resources/2019-luke09/krampus.txt");
                Console.WriteLine((from n in numbers select Luke9_Value(n)).Sum());
            }
            else if (luke == 10)
            {
                var forbruk = await Luke10("https://julekalender.knowit.no/resources/2019-luke10/logg.txt");
                Console.WriteLine(forbruk);
            }
            else if (luke == 11)
            {
                var luken = new Luke11();
                var resultat = await luken.OpenAsync();
                Console.WriteLine(resultat);
            }
            else if (luke == 12)
            {
                var luken = new Luke12();
                var resultat = await luken.OpenAsync();
                Console.WriteLine(resultat);
            }
            else if (luke == 13)
            {
                var luken = new Luke13();
                var resultat = await luken.OpenAsync();
                Console.WriteLine(resultat);
            }

            Console.ReadLine();
        }

        #region luke9
        private static int Luke9_Value(int n)
        {
            long l = n;
            long nn = l * l;
            int digits = (int)Math.Floor(Math.Log10(nn) + 1);
            for (int i = 1; i < digits; i++)
            {
                var div = (long)Math.Pow(10, i);
                var a = nn / div;
                var b = nn % div;
                if (a != 0 && b != 0)
                {
                    if (a + b == n) return n;
                }
            }

            return 0;
        }

        private static async Task<IEnumerable<int>> Luke9_GetNumbersAsync(string uri)
        {
            var retVal = new List<int>();
            using var client = new HttpClient();
            var r = await client.GetAsync(uri);
            var c = await r.Content.ReadAsStringAsync();
            using var reader = new StringReader(c);
            var line = reader.ReadLine();
            while (line != null)
            {
                retVal.Add(int.Parse(line));
                line = reader.ReadLine();
            }
            return retVal;
        }
        #endregion

        #region luke10
        private static async Task<int> Luke10(string uri)
        {
            var parser = new PeterMeterLogParser();
            var produkter = await parser.ParseAsync(uri, 2018);

            var totTankrem = 0.0;
            var totSjampo = 0.0;
            var totPapir = 0.0;
            var sunTotSjampo = 0;
            var wedTotPapir = 0;

            foreach (var p in produkter)
            {
                totTankrem += p.TannkremMl;
                totSjampo += p.SjampoMl;
                totPapir += p.ToalettPapirM;

                if (p.Dato.DayOfWeek == DayOfWeek.Sunday)
                {
                    sunTotSjampo += p.SjampoMl;
                }
                else if (p.Dato.DayOfWeek == DayOfWeek.Wednesday)
                {
                    wedTotPapir += p.ToalettPapirM;
                }
            }

            var tannkremTub = (int)(totTankrem / 125);
            var sjampoTub = (int)(totSjampo / 300);
            var papirRul = (int)(totPapir / 25);

            return tannkremTub*sjampoTub*papirRul*sunTotSjampo*wedTotPapir;
        }
        #endregion

    }

    interface KnowItJuleKalenderLuke
    {
        Task<int> OpenAsync();
    }

    class Luke13 : KnowItJuleKalenderLuke
    {
        public Task<int> OpenAsync()
        {
            var maze = new Maze();

            var arthurBoot = new ArthurBoot(maze);
            var res = arthurBoot.Search();
            Console.WriteLine("Arthur: " + res.Item1 + ", " + res.Item2);

            var isaacBoot = new IsaacBoot(maze);
            res = isaacBoot.Search();
            Console.WriteLine("Isaac: " + res.Item1 + ", " + res.Item2);

            return Task.FromResult(0);
        }



    }

    //

    abstract class BaseRobot
    {
        protected Maze Maze;

        protected Room Current { get; set; } = null;

        private HashSet<Room> Visited = new HashSet<Room>();

        private Stack<Room> Trail = new Stack<Room>();

        public BaseRobot(Maze maze)
        {
            Maze = maze;
        }

        public (bool, int) Search()
        {
            Visited.Clear();
            Trail.Clear();

            Current = Maze.Entry;
            while (!Current.Goal)
            {
                if (!Visited.Contains(Current))
                {
                    Visited.Add(Current);
                    Console.WriteLine(Current);
                }
                
                Room next = null;
                for (int i = 0; i < 4 && next == null; i++)
                {
                    var room = Next(i);
                    if (room != null)
                    {
                        if (!Visited.Contains(room))
                        {
                            next = room;
                        }
                    }
                }

                if (next == null)
                {
                    if (!Trail.Any())
                    {
                        return (false, Visited.Count());
                    }
                    else
                    {
                        Current = Trail.Pop();
                    }
                }
                else
                {
                    Trail.Push(Current);
                    Current = next;
                }
            }
            Visited.Add(Current);

            return (true, Visited.Count);
        }

        public abstract Room Next(int i);
    }

    class ArthurBoot : BaseRobot
    {
        public ArthurBoot(Maze maze) : base(maze) {}

        public override Room Next(int i)
        {
            switch (i)
            {
                case 0: return Maze.TryGoDown(Current);
                case 1: return Maze.TryGoRight(Current);
                case 2: return Maze.TryGoLeft(Current);
                case 3: return Maze.TryGoUp(Current);
                default: throw new Exception();
            }
        }

    }

    class IsaacBoot : BaseRobot
    {
        public IsaacBoot(Maze maze) : base(maze) {}

        public override Room Next(int i)
        {
            switch (i)
            {
                case 0: return Maze.TryGoRight(Current);
                case 1: return Maze.TryGoDown(Current);
                case 2: return Maze.TryGoLeft(Current);
                case 3: return Maze.TryGoUp(Current);
                default: throw new Exception();
            }
        }

    }

    class Room
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Top { get; set; }
        public bool Left { get; set; }
        public bool Bottom { get; set; }
        public bool Right { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            var o = obj as Room;
            return o == null ? false : o.X == X && o.Y == Y;
        }
        
        public override int GetHashCode()
        {
            int retVal = 7;
            retVal = 31 * retVal + X;
            retVal = 31 * retVal + X;
            return retVal;
        }

        public override string ToString()
        {
            return "[" + X.ToString() + ',' + Y.ToString() + "]";
        }

        public bool Goal => X == 499 && Y == 499;

    }

    class Maze
    {
        private Room[,] _maze = new Room[500, 500];

        public Maze()
        {
            foreach (var r in LoadRooms()) _maze[r.X, r.Y] = r;
        }

        public Room TryGoUp(Room from)
        {
            var x = from.X;
            var y = from.Y;
            var room = _maze[x, y];
            return room.Top ? null : _maze[x, y - 1];
        }

        public Room TryGoRight(Room from)
        {
            var x = from.X;
            var y = from.Y;
            var room = _maze[x, y];
            return room.Right ? null : _maze[x + 1, y];
        }

        public Room TryGoDown(Room from)
        {
            var x = from.X;
            var y = from.Y;
            var room = _maze[x, y];
            return room.Bottom ? null : _maze[x, y + 1];
        }

        public Room TryGoLeft(Room from)
        {
            var x = from.X;
            var y = from.Y;
            var room = _maze[x, y];
            return room.Left ? null : _maze[x - 1, y];
        }

        public Room Entry => _maze[0, 0];

        private IEnumerable<Room> LoadRooms()
        {
            var retVal = new List<Room>();

            char[] sep = { ' ', ':', ',' };

            string line = "";
            var r = new StreamReader(@"C:\Users\arha\source\repos\Jul\data\MAZE.TXT");
            Room room = null;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("\"x\""))
                {
                    room = new Room();
                    retVal.Add(room);
                    var data = line.Split(sep);
                    room.X = int.Parse(data[2]);
                }
                else if (line.StartsWith("\"y\""))
                {
                    var data = line.Split(sep);
                    room.Y = int.Parse(data[2]);
                }
                else if (line.StartsWith("\"top\""))
                {
                    var data = line.Split(sep);
                    room.Top = bool.Parse(data[2]);
                }
                else if (line.StartsWith("\"left\""))
                {
                    var data = line.Split(sep);
                    room.Left = bool.Parse(data[2]);
                }
                else if (line.StartsWith("\"right\""))
                {
                    var data = line.Split(sep);
                    room.Right = bool.Parse(data[2]);
                }
                else if (line.StartsWith("\"bottom\""))
                {
                    var data = line.Split(sep);
                    room.Bottom = bool.Parse(data[2]);
                }
            }

            return retVal;
        }
    }

    
    //

    class Luke12 : KnowItJuleKalenderLuke
    {
        public Task<int> OpenAsync()
        {
            var resultat = 0;
            for (int i = 1000; i < 10000; i++)
            {
                int steg = AntallSteg(i);
                if (steg == 7) resultat++;
            }

            return Task.FromResult(resultat);
        }

        private int AntallSteg(Luke12Tall lukeTallet)
        {
            int steg = 0;
            while (lukeTallet != 6174)
            {
                steg++;
                var lukeTall = lukeTallet.LukeTall;
                lukeTallet = (lukeTall.Item1 < lukeTall.Item2) ? lukeTall.Item2 - lukeTall.Item1 : lukeTall.Item1 - lukeTall.Item2;
                if (lukeTallet == 0) return -1;
            }

            return steg;
        }
    }

    // C# koden ble jo kort og lesbar :) .... gitt denne abstrakte typen :D
    // Driver ikke vanligvis å operator overloader så dette var litt kult.
    class Luke12Tall
    {
        private readonly int[] _sifre = new int[4];

        public int Verdi { get; private set; }
       
        public Luke12Tall(int[] n)
        {
            for (int i = 0; i < _sifre.Length; i++) _sifre[i] = n[i];
            Verdi = int.Parse(ToString());
        }

        public Luke12Tall(int n)
        {
            Verdi = n;
            var rep = n.ToString();
            int i = _sifre.Length - 1;
            for (var j = rep.Length - 1; j >= 0; j--, i--)
            {
                var cs = rep[j].ToString();
                _sifre[i] = int.Parse(cs);
            }
            while (i >= 0)
            {
                _sifre[i--] = 0;
            }
        }

        private (Luke12Tall, Luke12Tall) _lukeTall = default;
        public (Luke12Tall, Luke12Tall) LukeTall
        {
            get
            {
                if (_lukeTall == default)
                {
                    var a = _sifre.OrderByDescending(e => e).ToArray();
                    var b = new int[_sifre.Length];
                    for (int i = 0, j = _sifre.Length - 1; j >= 0; j--, i++)
                    {
                        b[i] = a[j];
                    }

                    var lt1 = new Luke12Tall(a);
                    var lt2 = new Luke12Tall(b);

                    _lukeTall = (lt1, lt2);
                }

                return _lukeTall;
            }
        }

        public override string ToString()
        {
            var retVal = "";
            for (int i = 0; i < _sifre.Length; i++) retVal += _sifre[i];
            return retVal;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            var o = obj as Luke12Tall;
            return (o == null) ? false : Verdi == o.Verdi;
        }

        public override int GetHashCode()
        {
            return Verdi;
        }

        public static implicit operator Luke12Tall(int n) => new Luke12Tall(n);

        public static bool operator <(Luke12Tall a, Luke12Tall b)
        {
            return a.Verdi < b.Verdi;
        }

        public static bool operator >(Luke12Tall a, Luke12Tall b)
        {
            return a.Verdi > b.Verdi;
        }

        public static Luke12Tall operator -(Luke12Tall a, Luke12Tall b)
        {
            return new Luke12Tall(a.Verdi - b.Verdi);
        }

        public static bool operator ==(Luke12Tall luke12Tall, int n)
        {
            return luke12Tall.Verdi == n;
        }

        public static bool operator !=(Luke12Tall luke12Tall, int n)
        {
            return luke12Tall.Verdi != n;
        }



    }
    
    class Luke11 : KnowItJuleKalenderLuke
    {
        public async Task<int> OpenAsync()
        {
            using var client = new HttpClient();
            var r = await client.GetAsync("https://julekalender.knowit.no/resources/2019-luke11/terreng.txt");
            var landscape = await r.Content.ReadAsStringAsync();
            return landscape.BreakingProfile(10703437).Count();
        }
    }

    static class SantaSpeedExtensions
    {
        public static IEnumerable<int> BreakingProfile(this string landscape, int speed)
        {
            var iceFactor = 1;
            var oppover = true;
            
            for (var i = 0; i < landscape.Length && speed > 0; i++)
            {
                var c = landscape[i];
                switch (c)
                {
                    case 'G':
                        speed -= 27;
                        yield return speed;
                        iceFactor = 1;
                        break;
                    case 'I':
                        speed += 12 * iceFactor;
                        yield return speed;
                        iceFactor++;
                        break;
                    case 'A':
                        speed -= 59;
                        yield return speed;
                        iceFactor = 1;
                        break;
                    case 'S':
                        speed -= 212;
                        yield return speed;
                        iceFactor = 1;
                        break;
                    case 'F': 
                        if (oppover)
                        {
                            speed -= 70;
                            yield return speed;
                        }
                        else
                        {
                            speed += 35;
                            yield return speed;
                        }
                        oppover = !oppover;
                        iceFactor = 1;
                        break;
                    default: throw new Exception("uknown landscape code: " + c);
                }
            }
        }
    }

    class Luke10 : KnowItJuleKalenderLuke
    {
        public async Task<int> OpenAsync()
        {
            var parser = new PeterMeterLogParser();
            var produkter = await parser.ParseAsync("https://julekalender.knowit.no/resources/2019-luke10/logg.txt", 2018);

            var totTankrem = 0.0;
            var totSjampo = 0.0;
            var totPapir = 0.0;
            var sunTotSjampo = 0;
            var wedTotPapir = 0;

            foreach (var p in produkter)
            {
                totTankrem += p.TannkremMl;
                totSjampo += p.SjampoMl;
                totPapir += p.ToalettPapirM;

                if (p.Dato.DayOfWeek == DayOfWeek.Sunday)
                {
                    sunTotSjampo += p.SjampoMl;
                }
                else if (p.Dato.DayOfWeek == DayOfWeek.Wednesday)
                {
                    wedTotPapir += p.ToalettPapirM;
                }
            }

            var tannkremTub = (int)(totTankrem / 125);
            var sjampoTub = (int)(totSjampo / 300);
            var papirRul = (int)(totPapir / 25);

            return tannkremTub * sjampoTub * papirRul * sunTotSjampo * wedTotPapir;
        }
    }

    class PeterMeterProduct
    {
        public DateTime Dato { get; set; }
        public int TannkremMl { get; set; }
        public int SjampoMl { get; set; }
        public int ToalettPapirM { get; set; }
    }

    class PeterMeterLogParser
    {
        public async Task<IEnumerable<PeterMeterProduct>> ParseAsync(string uri, int year)
        {
            using var client = new HttpClient();
            var r = await client.GetAsync(uri);
            var log = await r.Content.ReadAsStringAsync();
            return Parse(log, 2018);
        }

        private IEnumerable<PeterMeterProduct> Parse(string log, int year)
        {
            char[] dateSplit = { ' ', ':' };
            char[] dataSplit = { '\t', ' ', '*', };

            var retVal = new List<PeterMeterProduct>();

            var state = 0;

            using var reader = new StringReader(log);
            var line = reader.ReadLine();
            PeterMeterProduct p = default;
            while (line != null)
            {
                switch (state)
                {
                    case 0:
                        {
                            p = new PeterMeterProduct();
                            var split = line.Split(dateSplit, StringSplitOptions.RemoveEmptyEntries);
                            var m = MonthToInt(split[0]);
                            var d = int.Parse(split[1]);
                            p.Dato = new DateTime(year, m, d);
                        }
                        break;
                    case 1:
                    case 2:
                    case 3:
                        {
                            var split = line.Split(dataSplit, StringSplitOptions.RemoveEmptyEntries);
                            if (split[2].Equals("tannkrem"))
                            {
                                p.TannkremMl = int.Parse(split[0]);
                            }
                            else if (split[2].Equals("toalettpapir"))
                            {
                                p.ToalettPapirM = int.Parse(split[0]);
                            }
                            else if (split[2].Equals("sjampo"))
                            {
                                p.SjampoMl = int.Parse(split[0]);
                            }
                            else
                            {
                                throw new Exception("uknown product: " + split[2]);
                            }

                            if (state == 3)
                            {
                                retVal.Add(p);
                            }
                        }
                        break;
                    default: throw new Exception("illegal state");
                }
                state = ++state % 4;
                line = reader.ReadLine();
            }

            return retVal;
        }

        private int MonthToInt(string m)
        {
            switch (m)
            {
                case "Jan": return 1;
                case "Feb": return 2;
                case "Mar": return 3;
                case "Apr": return 4;
                case "May": return 5;
                case "Jun": return 6;
                case "Jul": return 7;
                case "Aug": return 8;
                case "Sep": return 9;
                case "Oct": return 10;
                case "Nov": return 11;
                case "Dec": return 12;
                default: throw new Exception("uknown month: " + m);
            }
        }

    }

}
