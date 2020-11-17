using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace ExecuteRandomFromDir
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            int input = 0;
            do
            {
                DisplayMenu();

                string userInput;
                userInput = Console.ReadLine();

                try
                {
                    input = Convert.ToInt32(userInput);
                }
                catch (Exception) { }
                
                switch (input)
                {
                    case 1:
                        createList(false);
                        break;
                    case 2:
                        createList(true);
                        break;
                    case 3:
                        selectExe();
                        break;
                    case 4: 
                        addToBlackList();
                        break;
                }
            } while (true);
        }

        static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("1. new List");
            Console.WriteLine("2. Update List");
            Console.WriteLine("3. Choose random .exe");
            Console.WriteLine("4. add to blacklist");
        }

        static void createList(Boolean add)
        {
            var foldersList = File.Exists("folders.txt") ? File.ReadAllLines("folders.txt").ToList() : new List<string>();

            using (FolderBrowserDialog mainFolder = new FolderBrowserDialog())
            {
                DialogResult result = mainFolder.ShowDialog();

                if (add && foldersList.IndexOf(mainFolder.SelectedPath) > -1)
                {
                    Console.Clear();
                    Console.WriteLine($"path \"{mainFolder.SelectedPath}\" already exists");
                    Console.ReadLine();
                    return;
                }

                if (result == DialogResult.OK)
                {
                    if (add) {
                        foldersList.Add(mainFolder.SelectedPath);
                        File.WriteAllLines("folders.txt", foldersList);
                    }
                    else File.WriteAllText("folders.txt", mainFolder.SelectedPath);

                    string[] exefiles = GetAllSafeFiles(mainFolder.SelectedPath, "*.exe");

                    if (add)
                    {
                        List<string> exeList = File.ReadAllLines("output.txt").ToList();

                        exeList.AddRange(exefiles);
                        File.WriteAllLines("output.txt", exeList);
                    }
                    else File.WriteAllLines("output.txt", exefiles);
                    
                }
            }
        }

        static void selectExe()
        {
            if (File.Exists("output.txt"))
            {
                var exeList = File.ReadAllLines("output.txt").ToList();
                int index = new Random().Next(exeList.Count);
                manageExe(exeList, index);
            }
            else {
                Console.Clear();
                Console.WriteLine("File list not found");
                Console.ReadLine();
            }
        }

        public static string[] GetAllSafeFiles(string path, string searchPattern = "*.*")
        {
            List<string> allFiles = new List<string>();
            string[] root = Directory.GetFiles(path, searchPattern);
            allFiles.AddRange(root);
           
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                try
                {
                    if (!IsIgnorable(folder))
                    {
                        allFiles.AddRange(Directory.GetFiles(folder, searchPattern, SearchOption.AllDirectories));
                    }
                }
                catch { } // Don't know what the problem is, don't care...
            }

            allFiles = allFiles.Where((x) =>
            {
                if (File.Exists("BlackList.txt"))
                {
                    var blacklist = File.ReadAllLines("BlackList.txt").ToList();
                    for (int i = 0; i < blacklist.Count; i++)
                    {
                        if (x.ToUpper().Contains(blacklist[i].ToUpper())) return false;
                    }
                    return true;
                }
                else return true;
            }).ToList();

            return allFiles.ToArray();
        }

        private static bool IsIgnorable(string dir)
        {
            if (dir.EndsWith("System Volume Information")) return true;
            if (dir.Contains("$RECYCLE.BIN")) return true;
            return false;
        }    

        static void manageExe(List<string> exeList, int index) {
            Console.Clear();
            Console.WriteLine($"selected exe => {exeList[index]}, run?");
            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No, delete");

            var i = 0;
            do
            {
                try {
                    string input = Console.ReadLine();
                    i = Convert.ToInt32(input);
                }
                catch (Exception) { }            
            } while (i < 1 || i > 2);

            switch (i) {
                case 1:
                    var canLaunch = true;

                    Console.Clear();
                    Console.WriteLine($"executing exe => {exeList[index]}");
                    try  {
                        //System.Diagnostics.Process.Start(exeList[index]);
                        //asdasd
                        StartProcess(exeList[index]);
                    }
                    catch (Exception) {
                        Console.Clear();
                        Console.WriteLine($"unnable to run exe => {exeList[index]}");
                        DeleteExeFromList(exeList, index);
                        canLaunch = false;
                    }

                    if (canLaunch) {
                        int input = 0;
                        do
                        {
                            Console.WriteLine("1. keep exe");
                            Console.WriteLine("2. delete exe");

                            string userInput;
                            userInput = Console.ReadLine();

                            try
                            {
                                input = Convert.ToInt32(userInput);
                            }
                            catch (Exception) { }

                            if (input == 2) {
                                Console.Clear();
                                DeleteExeFromList(exeList, index);
                            } 
                        } while (input < 1 || input > 2);
                    }

                    Console.ReadLine();
                    break;
                case 2:
                    Console.Clear();
                    DeleteExeFromList(exeList, index);
                    Console.ReadLine();
                    break;
            }       
        }

        static void StartProcess(string exeFile) {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = exeFile;
            processInfo.WorkingDirectory = Path.GetDirectoryName(exeFile);
            processInfo.ErrorDialog = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = false;
            processInfo.RedirectStandardError = false;
            System.Diagnostics.Process.Start(processInfo);
            //p.WaitForExit();
        }

        static void DeleteExeFromList(List<string> exeList, int index) {
            Console.WriteLine($"exe deleted => {exeList[index]}");
            var list = exeList.Where(x => x != exeList[index]).ToList();
            File.WriteAllLines("output.txt", list);
        }

        static void addToBlackList(){
            var BlackList = new List<string>();
            if (File.Exists("BlackList.txt")) BlackList = File.ReadAllLines("BlackList.txt").ToList();

            Console.Clear();
            Console.WriteLine("Enter blacklist word");
            var input = Console.ReadLine();

            if (BlackList.IndexOf(input) > -1) {
                Console.Clear();
                Console.WriteLine($"\"{input}\" already exists in blacklist");
                Console.ReadLine();
            }
            else {
                BlackList.Add(input);
                var exeFiles = File.ReadAllLines("output.txt").ToList();

                exeFiles = exeFiles.Where((x) =>
                {
                    for (int i = 0; i < BlackList.Count; i++)
                    {
                        if (x.ToUpper().Contains(BlackList[i].ToUpper())) return false;
                    }
                    return true;
                }).ToList();

                File.WriteAllLines("BlackList.txt", BlackList);
                File.WriteAllLines("output.txt", exeFiles);
            }
        }
    }
}