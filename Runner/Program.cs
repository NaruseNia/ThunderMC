using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Runner.MinecraftUtil;

namespace Runner
{
    internal class Program
    {
        private static readonly Logger _log = new Logger();

        private static void Main(string[] args)
        {
            Console.Title = "Runner | Developer Tool for ThunderMC";
            SelectMenu();
            Console.ReadKey();
        }

        private static void SelectMenu()
        {
            Console.Clear();
            _log.Init().Add("Select option in below ").Add("(1-5)", ConsoleColor.White).Newline(2).Put();
            _log.Init().Add("1. Check Java").Newline().Put();
            _log.Init().Add("2. Get Minecraft Latest Version").Newline().Put();
            _log.Init().Add("3. Get All Minecraft Versions").Newline().Put();
            _log.Init().Add("4. Get URL Minecraft Server File").Newline(2).Put();
            _log.Init().Add("(1-5)>", ConsoleColor.White).Put();
            GetOptionNumber();
        }

        private static MinecraftVersionType SelectType()
        {
            MinecraftVersionType type;

            _log.Init().Add("Select option in below ").Add("(1-5)", ConsoleColor.White).Newline(2).Put();
            _log.Init().Add("1. Release Only").Newline().Put();
            _log.Init().Add("2. Snapshot Only").Newline().Put();
            _log.Init().Add("3. Both").Newline(2).Put();
            _log.Init().Add("(1-3)>", ConsoleColor.White).Put();
            string input = Console.ReadLine();
            uint option = 0;
            if (!Validate(input, ref option)) return MinecraftVersionType.Both;
            type = option switch
            {
                1 => MinecraftVersionType.Release,
                2 => MinecraftVersionType.Snapshot,
                3 => MinecraftVersionType.Both,
                _ => MinecraftVersionType.Both
            };

            return type;
        }

        private static void GetOptionNumber()
        {
            string input = Console.ReadLine();
            uint option = 60;
            bool k = Validate(input, ref option);

            Console.WriteLine($"{k} : {option}");

            if (!k || option > 5)
            {
                Console.Clear();
                _log.Init().Add("Please input number correctly.").Newline().Put();
                SelectMenu();
            }

            switch (option)
            {
                case 2: GetLatest(); break;
                case 3: GetAll(); break;
                default:
                    break;
            }
        }

        private async static void GetAll()
        {
            Console.Clear();

            MinecraftVersionType type = SelectType();
            Console.WriteLine(type);

            Console.Clear();
            _log.Init().Add("How many do you want?>").Put();
            string input = Console.ReadLine();
            uint count = 0;
            if (!Validate(input, ref count)) return;

            Console.Clear();
            _log.Init().Add("Waiting...", ConsoleColor.DarkGray).Newline(2).Put();

            DateTime startTime = DateTime.Now;
            List<string> res = await GetAllVersionsAsync(type);
            TimeSpan done = DateTime.Now - startTime;

            _log.Init().Add("Responce : ").Newline().Put();
            res.Select(id => id).Take((int)count).ToList().ForEach(id => _log.Init().Add(id, ConsoleColor.Cyan).Newline().Put());
            _log.Init().Add("...", ConsoleColor.DarkGray).Newline().Put();
            _log.Init().Add("done in ").Add(done, ConsoleColor.Green).Newline(2).Put();
        }

        private async static void GetLatest()
        {
            Console.Clear();

            MinecraftVersionType type = SelectType();

            Console.Clear();
            _log.Init().Add("Waiting...", ConsoleColor.DarkGray).Newline(2).Put();

            DateTime startTime = DateTime.Now;
            string res = await GetLatestVersionAsync(type);
            TimeSpan done = DateTime.Now - startTime;

            _log.Init().Add("Responce : ").Newline().Add($"{res}", ConsoleColor.Cyan).Newline(2).Put();
            _log.Init().Add("done in ").Add(done, ConsoleColor.Green).Newline(2).Put();
        }

        private static bool Validate<T>(string input, ref T result)
        {
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

                if (converter == null) return false;

                result = (T)converter.ConvertFromString(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}