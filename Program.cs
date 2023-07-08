using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;

namespace StudyLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("若想自律，添加开机自启");
            static void GenerateDefaultWebsFile(string filePath)
            {
                var defaultContent = "名称;链接或者是路径，然后换行\n名称;链接或者是路径，然后换行\n";
                System.IO.File.WriteAllText(filePath, defaultContent);
                Console.WriteLine("成功生成默认的 Webs.txt 文件。");
            }
            static string GetDefaultWebsFileContent()
            {
                return "名称;链接或者是路径，然后换行\n名称;链接或者是路径，然后换行\n";
            }


            var directoryPath = Directory.GetCurrentDirectory();
            var websFolderPath = Path.Combine(directoryPath, "Webs");
            var websFilePath = Path.Combine(websFolderPath, "Webs.txt");

            if (!Directory.Exists(websFolderPath))
            {
                Directory.CreateDirectory(websFolderPath);
            }

            if (!System.IO.File.Exists(websFilePath))
            {
                GenerateDefaultWebsFile(websFilePath);
            }

            var websFileContent = System.IO.File.ReadAllText(websFilePath);
            if (websFileContent.Trim() == GetDefaultWebsFileContent().Trim())
            {
                Console.WriteLine("Webs.txt 文件内容为默认内容，请修改该文件以适应您的需求。");
                Console.WriteLine("Webs.txt 文件内容为默认内容，请修改该文件以适应您的需求。");
                // 此处可以添加提醒用户如何修改文件的逻辑
                Thread.Sleep(10000);
                Environment.Exit(0);
            }




            if (!Directory.Exists(websFolderPath))
            {
                Directory.CreateDirectory(websFolderPath);
            }

            if (!System.IO.File.Exists(websFilePath))
            {
                GenerateDefaultWebsFile(websFilePath);
            }


            bool regenerateBatFiles = false;

            if (Directory.GetFiles(directoryPath, "*.bat").Length == 0)
            {
                regenerateBatFiles = true;
            }
            else
            {
                regenerateBatFiles = AskYesNoQuestion("是否要重新生成.bat文件？(Y/N) ");
            }

            if (regenerateBatFiles)
            {
                GenerateBatFiles();
            }

            RunBatFiles();
        }

        static bool AskYesNoQuestion(string question)
        {
            while (true)
            {
                Console.Write(question);
                string input = Console.ReadLine().Trim().ToLower();

                if (input == "y" || input == "yes")
                {
                    return true;
                }
                else if (input == "n" || input == "no")
                {
                    return false;
                }

                Console.WriteLine("无效的输入，请输入 Y 或 N。");
            }
        }

        static void GenerateBatFiles()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Webs");
            var filePath = Path.Combine(directoryPath, "Webs.txt");
            var transportDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Transport");
            if (!Directory.Exists(transportDirectoryPath))
            {
                Directory.CreateDirectory(transportDirectoryPath);
            }

            var lines = System.IO.File.ReadAllLines(filePath);
            var invalidLines = new List<int>();

            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length != 2)
                {
                    invalidLines.Add(i + 1);
                    continue;
                }

                var batFileName = $"{i + 1}_{parts[0]}.bat";
                var batFilePath = Path.Combine(directoryPath, batFileName);
                var batFileCommand = parts[1].Trim();

                if (batFileCommand.Contains(" ") || ContainsChineseCharacter(batFileCommand))
                {
                    var shortcutPath = Path.Combine(transportDirectoryPath, $"{i + 1}_{parts[0]}.lnk");
                    CreateShortcut(batFileCommand, shortcutPath);
                    batFileCommand = shortcutPath;
                }

                System.IO.File.WriteAllText(batFilePath, batFileCommand);
            }

            Console.WriteLine("成功生成.bat文件，并保存到 Webs 文件夹中。");

            if (invalidLines.Count > 0)
            {
                Console.WriteLine("以下行的格式不正确：");
                foreach (var line in invalidLines)
                {
                    Console.WriteLine($"第 {line} 行");
                }
            }
        }

        static void CreateShortcut(string targetPath, string shortcutPath)
        {
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.Save();
        }

        static bool ContainsChineseCharacter(string str)
        {
            foreach (char c in str)
            {
                if (c >= 0x4E00 && c <= 0x9FFF)
                {
                    return true;
                }
            }
            return false;
        }

        static void RunBatFiles()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Webs");
            var filePath = Path.Combine(directoryPath, "Webs.txt");
            var lines = System.IO.File.ReadAllLines(filePath);

            Console.WriteLine("以下是文件内容：");

            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length != 2)
                {
                    Console.WriteLine($"错误：第 {i + 1} 行格式不正确。");
                    continue;
                }

                var batFileName = $"{parts[0]}.bat";
                Console.WriteLine($"{i + 1}. {batFileName}");
            }

            bool allFilesRenamed = false;
            while (!allFilesRenamed)
            {
                Console.WriteLine("请输入要运行的.bat文件序号：");
                var input2 = Console.ReadLine();
                int index;
                while (!int.TryParse(input2, out index) || index < 1 || index > lines.Length)
                {
                    Console.WriteLine("输入的序号无效，请重新输入：");
                    input2 = Console.ReadLine();
                }

                var fileName = Path.Combine(directoryPath, $"{index}_{lines[index - 1].Split(';')[0]}.bat");
                if (System.IO.File.Exists(fileName))
                {
                    System.Diagnostics.Process.Start(fileName);
                }
                else
                {
                    Console.WriteLine("指定的.bat文件不存在，请重新输入序号。");
                    continue;
                }

                allFilesRenamed = true;
                for (int i = 0; i < lines.Length; i++)
                {
                    var batFileName = $"{i + 1}_{lines[i].Split(';')[0]}.bat";
                    var batFilePath = Path.Combine(directoryPath, batFileName);
                    if (!System.IO.File.Exists(batFilePath))
                    {
                        allFilesRenamed = false;
                        break;
                    }
                }
            }
        }
    }
}
//如果报错没有引用的话：在Visual Studio中，右键单击项目，然后选择“添加” -> “引用...”。
//在“引用管理器”对话框中，切换到“COM”选项卡。
//在搜索框中输入“Windows Script Host Object Model”。
//勾选搜索结果中的“Windows Script Host Object Model”并点击“确定”按钮。

/*将Webs.txt文件移动到Webs文件夹下。确保Webs文件夹与Webs.txt文件在同一目录下。

在GenerateBatFiles方法中，将读取Webs.txt文件的部分代码修改如下：
var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Webs");
var filePath = Path.Combine(directoryPath, "Webs.txt");
var lines = File.ReadAllLines(filePath);

在RunBatFiles方法中，将读取Webs.txt文件的部分代码修改如下：
var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Webs");
var filePath = Path.Combine(directoryPath, "Webs.txt");
var lines = File.ReadAllLines(filePath);
请根据上述修改，将代码中对应的部分进行调整。这样可以确保程序读取的是Webs文件夹下的Webs.txt文件。如果您有任何其他问题，请随时提问。感谢您的理解与支持！*/