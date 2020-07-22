using System;
using System.IO;
using System.Reflection;

namespace MyAutoVersioning
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating Version File...");
            
            CreateAssemblyVersionFile();
        }

        static void CreateAssemblyVersionFile()
        {
            // AppDomain.CurrentDomain.BaseDirectory --> bin/Debug/...
            // Environment.CurrentDirectory --> bin/Debug/....
            Console.WriteLine("test1:" + AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("test2:" + Environment.CurrentDirectory);

            string buildDir = Directory.GetCurrentDirectory();
            Console.WriteLine(buildDir);
#if Debug
            string projDir = Directory.GetParent(buildDir).Parent.Parent.FullName;
#else
            string projDir = buildDir;
#endif
            Console.WriteLine(projDir);

            string assemblyDir = Path.Combine(projDir, "properties");
            string assemblyFile = "myassemblyinfo.cs";
            string assemblyFilePath = Path.Combine(assemblyDir, assemblyFile);
            
            // create my assembly file directory
            Directory.CreateDirectory(assemblyDir);
            
            //// if already exists
            //string content = "";
            //if (File.Exists(assemblyFilePath))
            //{
            //    // read old file content
            //    content = File.ReadAllText(assemblyFilePath);
            //    Console.WriteLine("Current content of file:");
            //    Console.WriteLine(content);
            //}

            // create new 
            string assemblyFileContent =
           @"using System.Reflection;" +
           Environment.NewLine +
           "[assembly: AssemblyVersion(\"" + GetNextVersion() + "\")]";

            // create file version from content
            File.WriteAllText(assemblyFilePath, assemblyFileContent);
        }

        static string GetNextVersion()
        {
            const int Major = 1;
            const int Minor = 0;
            //int BuildNumber = 0;
            //int Revision = 0;

            DateTime ProjectStartedDate = new DateTime(year: 2020, month: 7, day: 1);
            int BuildNumber = (int)((DateTime.UtcNow - ProjectStartedDate).TotalDays) + 1; // DaysSinceProjectStarted
            
            int Revision = (int)DateTime.UtcNow.TimeOfDay.TotalMinutes; // MinutesSinceMidnight

            return $"{Major}.{Minor}.{BuildNumber}.{Revision}";
        }

        static void Versioning(string[] args)
        {
            int Major = 1;
            int Minor = 0;
            //int BuildNumber = 0;
            //int Revision = 0;

            DateTime ProjectStartedDate = new DateTime(year: 2020, month: 7, day: 1);
            int BuildNumber = (int)((DateTime.UtcNow - ProjectStartedDate).TotalDays) + 1; // DaysSinceProjectStarted

            int Revision = (int)DateTime.UtcNow.TimeOfDay.TotalMinutes; // MinutesSinceMidnight

            //Console.WriteLine(
            //    "<#= this.Major #>.<#= this.Minor #>.<#= this.BuildNumber #>.<#= this.Revision #>"
            //    );


            // -----------------------------------------------------------------------
            if (args.Length == 0)
            {
                var versionString = Assembly.GetEntryAssembly()
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        .InformationalVersion
                                        .ToString();

                Console.WriteLine($"botsay v{versionString}");
                Console.WriteLine("-------------");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  botsay <message>");
                return;
            }

            ShowBot(string.Join(' ', args));
        }

        static void ShowBot(string message)
        {
            string bot = $"\n        {message}";
            bot += @"
    __________________
                      \
                       \
                          ....
                          ....'
                           ....
                        ..........
                    .............'..'..
                 ................'..'.....
               .......'..........'..'..'....
              ........'..........'..'..'.....
             .'....'..'..........'..'.......'.
             .'..................'...   ......
             .  ......'.........         .....
             .    _            __        ......
            ..    #            ##        ......
           ....       .                 .......
           ......  .......          ............
            ................  ......................
            ........................'................
           ......................'..'......    .......
        .........................'..'.....       .......
     ........    ..'.............'..'....      ..........
   ..'..'...      ...............'.......      ..........
  ...'......     ...... ..........  ......         .......
 ...........   .......              ........        ......
.......        '...'.'.              '.'.'.'         ....
.......       .....'..               ..'.....
   ..       ..........               ..'........
          ............               ..............
         .............               '..............
        ...........'..              .'.'............
       ...............              .'.'.............
      .............'..               ..'..'...........
      ...............                 .'..............
       .........                        ..............
        .....
";
            Console.WriteLine(bot);
        }
    }
}
