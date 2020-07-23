#define TestingApp 
//#undef TestingApp

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyAutoVersioning
{
    class Program
    {
        static string newVersion, currentVersion;
        static string newFileMD5, currentFileMD5;

        static void Main(string[] args)
        {
            Console.WriteLine("+[Versioning] Create/Update Version File...");

            Console.WriteLine("+[Versioning] Manifest: " + GetAppFile());

            Console.WriteLine("+[Versioning] Manifest_chksum: " + GetAppFile_MD5());

            // check version
            if (args.Length == 0)
            {
                //var versionString = Assembly.GetEntryAssembly()
                //                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                //                        .InformationalVersion
                //                        .ToString();

                currentVersion =
                Assembly.GetEntryAssembly().FullName.Substring(Assembly.GetEntryAssembly().FullName.IndexOf("Version=") + 8, Assembly.GetEntryAssembly().FullName.IndexOf(", Culture") - 18 - 8);

                Console.WriteLine("+[Versioning] Current Version:" + currentVersion);
            }

            // create/update assemblyfile with versioning from custom start datetime
            CreateAssemblyVersionFile();

            Console.WriteLine("+[Versioning] Manifest_chksum2:" + GetAppFile_MD5());
        }

        static void CreateAssemblyVersionFile()
        {
#if Debug
            // AppDomain.CurrentDomain.BaseDirectory --> bin/Debug/...
            // Environment.CurrentDirectory --> bin/Debug/....
            Console.WriteLine("\ntest1:" + AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("\ntest2:" + Environment.CurrentDirectory);
#endif

            string buildDir = Directory.GetCurrentDirectory();

#if !TestingApp
            buildDir = Directory.GetParent(buildDir).Parent.Parent.FullName;
#endif

#if Debug
            Console.WriteLine("\ntest3:" + buildDir);
#endif

#if Debug
            string projDir = Directory.GetParent(buildDir).Parent.Parent.FullName;
#else
            string projDir = buildDir;
#endif


#if Debug
            Console.WriteLine(projDir);
#endif
            string assemblyDir = Path.Combine(projDir, "properties");
            string assemblyFile = "myassemblyinfo.cs";
            string assemblyFilePath = Path.Combine(assemblyDir, assemblyFile);

            // create my assembly file directory
            Directory.CreateDirectory(assemblyDir);
            
            // if assembly file already exists
            string content = "";
            if (File.Exists(assemblyFilePath))
            {
                // read old file content
                content = File.ReadAllText(assemblyFilePath);
                Console.WriteLine("Current content of file:");
                Console.WriteLine(content);

                currentFileMD5 = content.Substring(content.IndexOf("AssemblyInformationalVersion(", 16));
                Console.WriteLine("CurrentFileMD5:" + currentFileMD5);
            }

            // create new 
            newVersion = GetNewVersion();
            newFileMD5 = GetAppFile_MD5();

            string assemblyFileContent =
           @"using System.Reflection;" +
           Environment.NewLine +
           "[assembly: AssemblyVersion(\"" + newVersion + "\")]" +
           "[assembly: AssemblyInformationalVersion(\"" + newFileMD5 + "\")]";

            string updateStatus = "";
            if (!string.Equals(newVersion, currentVersion))
            {
                updateStatus = "(new)";
            }

            if (updateStatus != default)
            {
                // create file version from content
                File.WriteAllText(assemblyFilePath, assemblyFileContent);
            }
            
            Console.WriteLine($"+[Versioning] Updated Version:{newVersion} {updateStatus}");
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
            return  Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
        }

        public static string CreateMD5(dynamic input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = default;

                Type inputType = input.GetType();

                if (inputType == typeof(string) )
                {
                    inputBytes = System.Text.Encoding.ASCII.GetBytes(input.ToString());
                }
                else if(inputType == typeof(byte) )
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
