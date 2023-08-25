using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using File = System.IO.File;

namespace BackupService 
{
    public class Backup
    {
        public static void DoBackup()
        {
            int filesAdded = 0, filesOverwritten = 0, filesKept = 0, filesDeleted = 0;
            StringBuilder logBuilder = new StringBuilder();

            if (Program.mainFolder == "") {
                Program.GetBackupInfo();
            }

            Console.WriteLine("**** START BACKUP PROCESS **** " + Program.GetCurrentDate() + " it may take a while");

            InitializeBackupLog(logBuilder);

            //cicle trough all the files in the Main folder
            string[] files = Directory.GetFiles(Program.mainFolder);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string destinationPath = Path.Combine(Program.backupFolder, fileName);

                //check if the new files exists
                if (File.Exists(destinationPath))
                {

                    //check if the files are the same or not
                    if (!IsSameFile(filePath, destinationPath))
                    {

                        //just easy copy the file
                        File.Copy(filePath, destinationPath);
                        logBuilder.AppendLine("OVERWRITTEN successfully file name: " + fileName);
                        Console.WriteLine("OVERWRITTEN successfully file name: " + fileName);
                        filesOverwritten++;
                    }
                    else
                    {
                        logBuilder.AppendLine("File name: " + fileName + " is the same. No action required");
                        filesKept++;
                    }
                }
                else
                {
                    //just add the file
                    File.Copy(filePath, destinationPath);
                    logBuilder.AppendLine("ADDED successfully file name: " + fileName);
                    Console.WriteLine("ADDED successfully file name: " + fileName);
                    filesAdded++;
                }
            }

            Console.WriteLine("**** PROCESSING ***** \n");

           //now cleanup, check if the backup folder contains files that are no longer in the main folder
            files = Directory.GetFiles(Program.backupFolder);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string sourceFilePath = Path.Combine(Program.mainFolder, fileName);

                //check if the source file no longer exists
                if (!File.Exists(sourceFilePath))
                {
                    File.Delete(filePath);
                    logBuilder.AppendLine("DELETED from backup: " + fileName);
                    Console.WriteLine("DELETED from backup: " + fileName);
                    filesDeleted++;
                }
            }
            AppendInfoToBackupLog(logBuilder, files.Length, filesAdded, filesOverwritten, filesKept, filesDeleted);

            FinalizeBackupLog(logBuilder);

            Console.WriteLine("    BACKUP RESULT: OK" + Environment.NewLine);
            Console.WriteLine("**** BACKUP PROCESS ENDED **** \nFor more infos check log.txt at " + Program.logPath + Environment.NewLine);
        }

        private static void InitializeBackupLog(StringBuilder logBuilder)
        {
            logBuilder.AppendLine("\n-----------------------------------------------------");
            logBuilder.AppendLine("**** START BACKUP PROCESS **** " + Program.GetCurrentDate());
            logBuilder.AppendLine("BACKUP FREQUENCY: " + Program.backupFrequency);
            logBuilder.AppendLine("MAIN FOLDER: " + Program.mainFolder);
            logBuilder.AppendLine("BACKUP FOLDER: " + Program.backupFolder + "\n");
        }

        private static void AppendInfoToBackupLog(StringBuilder logBuilder, int filesLength, int filesAdded, int filesOverwritten, int filesKept, int filesDeleted)
        {
            logBuilder.AppendLine("\n____TOTAL FILES: " + filesLength);
            logBuilder.AppendLine("________FILES ADDED: " + filesAdded);
            logBuilder.AppendLine("________FILES OVERWRITTEN: " + filesOverwritten);
            logBuilder.AppendLine("________FILES DELETED: " + filesDeleted);
            logBuilder.AppendLine("________FILES KEPT: " + filesKept + Environment.NewLine);
        }

        private static void FinalizeBackupLog(StringBuilder logBuilder)
        {
            logBuilder.AppendLine("BACKUP RESULT: OK");
            logBuilder.AppendLine("**** BACKUP PROCESS ENDED **** " + Program.GetCurrentDate());
            logBuilder.AppendLine("-----------------------------------------------------" + "\n");

            File.AppendAllText(Program.logPath, logBuilder.ToString());
        }
        static bool IsSameFile(string path1, string path2)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream1 = File.OpenRead(path1))
                using (var stream2 = File.OpenRead(path2))
                {
                    byte[] hash1 = md5.ComputeHash(stream1);
                    byte[] hash2 = md5.ComputeHash(stream2);

                    return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
                }
            }
        }
    }
}