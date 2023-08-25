using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Win32.TaskScheduler;


namespace BackupService
{
    public class Program
    {
        public static string backupFrequency = "", mainFolder = "", backupFolder = "", logPath = "";
        static string configPath = "config.txt";
        
        static void Main(string[] args)
        {
            if (File.Exists(configPath))
            {
                if (args.Length > 0 && args[0] == "onlybackup")
                {
                    Backup.DoBackup();
                }
                else
                {
                    Menu();
                }
            }
            else
            {
                Setup();
                Backup.DoBackup();
            }
        }

       private static void Menu()
        {
            GetBackupInfo();
            
            ShowInfos();

            Console.WriteLine("Want do you want to do? ");
            Console.WriteLine("1. Force Backup");
            Console.WriteLine("2. Delete Process");
            Console.WriteLine("3. Quit");
            switch (GetUserInput("1", "2", "3"))
            {
                case "1":
                    Backup.DoBackup();
                    break;

                case "2":
                    DeleteBackupProcess();
                    break;

                case "3":
                    Environment.Exit(0);
                    break;
            }
        }

        static void ShowInfos()
        {
            Console.WriteLine("Welcome back in BACKUP AUTOMATED SERVICES. Your BACKUP SERVICE is currently running: " + Environment.NewLine);
            Console.WriteLine("BACKUP FREQUENCY: " + backupFrequency);
            Console.WriteLine("MAIN FOLDER: " + mainFolder);
            Console.WriteLine("BACKUP FOLDER: " + backupFolder);
            Console.WriteLine("LOG.TXT PATH: " + logPath + Environment.NewLine);
        }

        public static void GetBackupInfo()
        {   
            backupFrequency = GetValueConfigFile("BACKUP FREQUENCY");
            mainFolder = GetValueConfigFile("MAIN");
            backupFolder = GetValueConfigFile("BACKUP");
            logPath = GetValueConfigFile("LOG PATH");
        }

        private static void DeleteBackupProcess()
        {
            Console.WriteLine("Delete process? Backup folder will remain. (Y/N)");
            switch (GetUserInput("Y", "N").ToUpper())
            {
                case "Y":
                    File.Delete(configPath);
                    DeleteWindowsTask();
                    LogExit();
                    break;
                case "N":
                    Console.WriteLine("Ok ciao");
                    break;
            }
        }

        /* SETUP PROCESS */
        static void Setup()
        {
            Console.WriteLine("Welcome in BACKUP AUTOMATED SERVICES");

            //BACKUP FREQUENCY
            backupFrequency = GetBackupFrequency();
            Console.WriteLine("\nBACKUP FREQUENCY: " + backupFrequency);

            //MAIN FOLDER 
            mainFolder = GetMainFolder();
            Console.WriteLine("\nMAIN FOLDER: " + mainFolder);

            //BACKUP FOLDER
            backupFolder = GetBackupFolder();
            Console.WriteLine("\nBACKUP FOLDER: " + backupFolder);

            //LOG PATH
            logPath = GetLogPath();
            LogEntry();
            Console.WriteLine("\nLOG FILE: " + logPath + "\n");

            CreateWindowsTaskScheduler(backupFrequency);
            CreateConfigFile();
        }

        private static string GetBackupFrequency()
        {
            string frequency;

            Console.WriteLine("Please insert the backup frequency");
            Console.WriteLine("1. Hourly");
            Console.WriteLine("2. Dayly");
            Console.WriteLine("3. Weekly");

            frequency = GetUserInput("1", "2", "3", "HOURLY", "DAYLY", "WEEKLY");
            frequency = BackupFrequencyConversion(frequency);

            return frequency;
        }

        //MAIN FOLDER
        private static string GetMainFolder()
        {
            string folder;
            do
            {
                Console.WriteLine("\nPlease insert the path of the folder that you would like to backup");
                folder = Console.ReadLine();
                if (!Directory.Exists(folder))
                {
                    Console.WriteLine("Invalid folder path. Please provide a valid folder path:");
                }
            } while (!Directory.Exists(folder));

            folder = GetAbsolutePath(folder);

            return folder;
        }

        //BACKUP FOLDER
        private static string GetBackupFolder()
        {
            string path;
            do
            {
                Console.WriteLine("\nPlease insert the path of the backup folder");
                path = Console.ReadLine();
                path = GetAbsolutePath(path);
            } while (CheckOverwrite(path, "FOLDER"));

            return path;
        }

        //LOG PATH
        private static string GetLogPath()
        {
            string path;
            do
            {
                Console.WriteLine("\nPlease insert the path of the log.txt file");
                path = Console.ReadLine();
                path = GetAbsolutePath(path);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.Write("\nFOLDER CREATED: " + path);
                }

                path = path + "\\log.txt";
            } while (CheckOverwrite(path, "FILE"));

            return path;
        }

        private static bool CheckOverwrite(string path, string element)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                Console.WriteLine("\nThis " + element + " already exists and will be overwritten. Want to proceed? (Y/N)");
                return GetUserInput("Y", "N").Equals("N", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                switch (element)
                {
                    case "FOLDER":
                        Directory.CreateDirectory(path);
                        break;
                    case "FILE":
                        File.WriteAllText(path, "");
                        break;
                }

                Console.Write("\n" + element + " CREATED: " + path);
            }

            return false;
        }

        private static string BackupFrequencyConversion(String userSelection)
        {
            if (userSelection.Equals("1") || userSelection.Equals("HOURLY", StringComparison.OrdinalIgnoreCase))
            {
                userSelection = "Hourly";
            }

            else if (userSelection.Equals("2") || userSelection.Equals("DAYLY", StringComparison.OrdinalIgnoreCase))
            {
                userSelection = "Daily";
            }

            else if (userSelection.Equals("3") || userSelection.Equals("WEEKLY", StringComparison.OrdinalIgnoreCase))
            {
                userSelection = "Weekly";
            }

            return userSelection;
        }

        static void CreateWindowsTaskScheduler(string backupFrequency)
        {
            Trigger trigger;
            switch (backupFrequency)
            {
                case "Hourly":
                    trigger = new TimeTrigger { Repetition = new RepetitionPattern(TimeSpan.FromHours(1), TimeSpan.Zero) };
                    break;
                case "Daily":
                    trigger = new DailyTrigger { DaysInterval = 1 };
                    break;
                case "Weekly":
                    trigger = new WeeklyTrigger { WeeksInterval = 1 };
                    break;
                default:
                    throw new ArgumentException("Invalid backup frequency.");
            }

            // Create a new task service instance
            using (TaskService taskService = new TaskService())
            {
                // Create a new task definition
                TaskDefinition taskDefinition = taskService.NewTask();

                // Set the task settings
                taskDefinition.RegistrationInfo.Description = "Automated Backup Service " + Environment.NewLine + "BACKUP FREQUENCY: " + backupFrequency + Environment.NewLine + "MAIN FOLDER: " + mainFolder + Environment.NewLine + "BACKUP FOLDER: " + backupFolder;
                taskDefinition.Triggers.Add(trigger);

                // Set the action to run a program
                string executablePath = AppDomain.CurrentDomain.BaseDirectory + "\\Backup.exe";
                string arguments = "onlybackup";  // Pass the parameter here
                taskDefinition.Actions.Add(new ExecAction(executablePath, arguments));

                // Register the task
                taskService.RootFolder.RegisterTaskDefinition("Automated Backup Service", taskDefinition);

                Console.WriteLine("Task created successfully.");
            }
        }

        static void DeleteWindowsTask()
        {
            string taskName = "Automated Backup Service";
            using (TaskService taskService = new TaskService())
            {
                taskService.RootFolder.DeleteTask(taskName, false);
                Console.WriteLine("Task " + taskName + " deleted successfully.");
            }
        }

        static void LogEntry()
        {
            File.WriteAllText(logPath, "**** **** LOG CREATED " + GetCurrentDate());
        }

        static void LogExit()
        {
            File.AppendAllText(logPath, "**** **** BACKUP PROCESS DELETED " + GetCurrentDate());
        }

        static void CreateConfigFile()
        {
            File.AppendAllText(configPath, "**** GENERATED AUTOMATICALLY **** " + GetCurrentDate() + Environment.NewLine);
            File.AppendAllText(configPath, "BACKUP FREQUENCY:'" + backupFrequency + "'" + Environment.NewLine);
            File.AppendAllText(configPath, "MAIN:'" + mainFolder + "'" + Environment.NewLine);
            File.AppendAllText(configPath, "BACKUP:'" + backupFolder + "'" + Environment.NewLine);
            File.AppendAllText(configPath, "LOG PATH:'" + logPath + "'");
        }

        static string GetValueConfigFile(string keyword)
        {
            //check trought all the lines which one starts with the Keyword give like MAIN or BACKUP 
            string path = "";
            string[] lines = File.ReadAllLines(configPath);

            foreach (string line in lines)
            {
                if (line.StartsWith(keyword))
                {
                    //nice
                    string pattern = keyword + ":'(.*?)'"; // -> (.*?) //$"{keyword}:'(.*?)'"; // -> (.*?) 
                    Match match = Regex.Match(line, pattern);
                    if (match.Success) path = match.Groups[1].Value;
                }
            }
            return path;
        }

        /* *** UTIL FUNCTIONS ***  */
        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        static string GetUserInput(params string[] validOptions)
        {
            string userInput;

            do
            {
                userInput = Console.ReadLine();

                if (Array.Exists(validOptions, option => option.Equals(userInput, StringComparison.OrdinalIgnoreCase)))
                {
                    return userInput;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please choose from the valid options: " + string.Join(", ", validOptions));
                }
            } while (true);
        }

        private static string GetAbsolutePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            return path;
        }
        /* *** END UTIL FUNCTIONS ***  */
    }
}