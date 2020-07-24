#define TESTINGAPP
#undef TESTINGAPP

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyAutoVersioning
{
    class Program
    {
        static string projectName = "";
        static string buildDir, projDir;

        static string newVersion, currentVersion;
        static string newFileMD5, currentFileMD5;

        static void Main(string[] args)
        {
            foreach (var item in args)
            {
                Console.WriteLine("args: " + item);
                projectName = item;
            }

            SetupProjectPath();

            Console.WriteLine("+[Versioning] Create/Update Version File...");

            Console.WriteLine("+[Versioning] Current Manifest Location: " + GetAppFile());

            Console.WriteLine();
            Console.WriteLine("+[Versioning] Current Manifest_chksum: " + GetAppFile_MD5());

            // check version
            if (args.Length == 0)
            {
                //var versionString = Assembly.GetEntryAssembly()
                //                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                //                        .InformationalVersion
                //                        .ToString();
            }
            currentVersion =
                Assembly.GetEntryAssembly().FullName.Substring(Assembly.GetEntryAssembly().FullName.IndexOf("Version=") + 8, Assembly.GetEntryAssembly().FullName.IndexOf(", Culture") - 18 - 8);

            Console.WriteLine("+[Versioning] Current Version: " + currentVersion);
            Console.WriteLine();

            // create/update assemblyfile with versioning from custom start datetime
            CreateAssemblyVersionFile();
        }

        static void SetupProjectPath()
        {
#if TESTINGAPP || DEBUG
            // AppDomain.CurrentDomain.BaseDirectory --> bin/Debug/...
            // Environment.CurrentDirectory --> bin/Debug/....
            Console.WriteLine("\ntest1:" + AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("test2:" + Environment.CurrentDirectory);
            Console.WriteLine("test3:" + Directory.GetCurrentDirectory());
#endif

#if TESTINGAPP
            buildDir = AppDomain.CurrentDomain.BaseDirectory;
#else
            buildDir = Directory.GetCurrentDirectory();
#endif

            
#if TESTINGAPP || DEBUG
            Console.WriteLine("\nbuildDir " + buildDir);
#endif

            projDir = Directory.GetParent(buildDir).Parent.Parent.FullName;

#if DEBUG
            Console.WriteLine("\nprojDir " + projDir);
#endif
            Console.WriteLine();
        }

        static void CreateAssemblyVersionFile()
        {
            string assemblyDir = Path.Combine(projDir, "properties");
            string assemblyFile = "myassemblyinfo.cs";
            string assemblyFilePath = Path.Combine(assemblyDir, assemblyFile);

            string versionFile = "myversion.json";
            string versionFilePath = Path.Combine(assemblyDir, versionFile);

            // create my assembly file directory
            Directory.CreateDirectory(assemblyDir);

            // if assembly file already exists
            string content = "";
            if (File.Exists(assemblyFilePath))
            {
                // read old file content
                content = File.ReadAllText(assemblyFilePath);
                //Console.WriteLine("Current content of file:");
                //Console.WriteLine(content);

                //currentFileMD5 = content.Substring(content.IndexOf("AssemblyInformationalVersion(") + 30, 32);
                //Console.WriteLine("++[Versioning] CurrentFile_MD5: " + currentFileMD5);
            }
            if (File.Exists(versionFilePath))
            {
                currentFileMD5 = File.ReadAllText(versionFilePath); 
            }

            // create new 
            newVersion = GetNewVersion();
            newFileMD5 = GetAppFile_MD5();

            Console.WriteLine("++[Versioning] NewFile_MD5: " + newFileMD5);

            Console.WriteLine();
            Console.WriteLine("+++[Versioning] OldVersion: " + currentVersion);
            Console.WriteLine("+++[Versioning] NewVersion: " + newVersion);

            string assemblyFileContent =
           @"using System.Reflection;" + '\n' +
           Environment.NewLine +
           "[assembly: AssemblyVersion(\"" + newVersion + "\")]" + '\n' +
           "[assembly: AssemblyInformationalVersion(\"" + newFileMD5 + "\")]";

            string updateStatus = "";
            if (!string.Equals(newFileMD5, currentFileMD5))
            {
                if (!string.Equals(newVersion, currentVersion))
                {
                    updateStatus = "(new)";
                }
            }


            if (updateStatus != default)
            {
                // create file version from content
                File.WriteAllText(assemblyFilePath, assemblyFileContent);
                File.WriteAllText(versionFilePath, newFileMD5);
            }

            Console.WriteLine();
            Console.WriteLine("++++[Versioning] Updated Manifest_chksum2:" + GetAppFile_MD5());
            Console.WriteLine($"++++[Versioning] Updated Version: {newVersion} {updateStatus}");
        }

        static string GetNewVersion()
        {
            const int Major = 1;
            const int Minor = 0;
            //int BuildNumber = 0;
            //int Revision = 0;

            DateTime ProjectStartedDate = new DateTime(year: 2020, month: 7, day: 1);
            int BuildNumber = (int)((DateTime.UtcNow - ProjectStartedDate).TotalDays) + 1; // DaysSinceProjectStarted

            int Revision = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds; // MinutesSinceMidnight

            return $"{Major}.{Minor}.{BuildNumber}.{Revision}";
        }

        static string GetAppFile_MD5()
        {
            using (var stream = File.OpenRead(GetAppFile()))
            {
                return CreateMD5(stream);
            }
        }
        static string GetAppFile()
        {

            //return Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            //return Path.Combine(buildDir, Assembly.GetEntryAssembly().ManifestModule.Name);

            if (!string.IsNullOrEmpty(projectName))
            {
                return Path.Combine(buildDir, projectName + ".dll");
            }
            return Path.Combine(buildDir, Assembly.GetEntryAssembly().GetName().Name + ".dll");


            //StackFrame[] frames = new StackTrace().GetFrames();
            //string initialAssembly = (from f in frames
            //                          select f.GetMethod().ReflectedType.AssemblyQualifiedName
            //                         ).Distinct().Last();

            //foreach (var item in frames)
            //{
            //    Console.WriteLine(item.GetMethod().ReflectedType.AssemblyQualifiedName);
            //}
            
            //return initialAssembly;
        }

        public static string CreateMD5(dynamic input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = default;

                Type inputType = input.GetType();

                if (inputType == typeof(string))
                {
                    inputBytes = System.Text.Encoding.ASCII.GetBytes(input.ToString());
                }
                else if (inputType == typeof(byte))
                {
                    inputBytes = input;
                }
                else if (inputType == typeof(FileStream))
                {
                    inputBytes = ReadFully(input);
                }

                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
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
