using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MainLauncher
{
    class Program
    {
        // 导入 user32.dll 中的 GetForegroundWindow 和 ShowWindow 函数
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // 定义 ShowWindow 函数中的常量
        private const int SW_MAXIMIZE = 3;


        static void Main()
        {
            IntPtr hWnd = GetForegroundWindow(); // 获取当前活动窗口的句柄
            ShowWindow(hWnd, SW_MAXIMIZE); // 最大化窗口

        MainEnterance:
            Console.WriteLine("选择模式,1启动学习或录入错题，3复习错题；4为退出");
            string MainMode = Console.ReadLine();

            while (MainMode != "1" && MainMode != "3" && MainMode != "4")
            {
                Console.WriteLine("输入错误，重新输入");
                MainMode = Console.ReadLine();
            }
            if (MainMode == "1")
            {
                Console.WriteLine("若想自律，添加开机自启");

                var directoryPath = Directory.GetCurrentDirectory();
                var websFolderPath = Path.Combine(directoryPath, "Webs");
                var websFilePath = Path.Combine(websFolderPath, "Webs.txt");
                var lastModifiedTimePath = Path.Combine(directoryPath, "LastModified.txt");

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
                    Console.ReadKey();
                    // 此处可以添加提醒用户如何修改文件的逻辑
                    Environment.Exit(0);
                }

                DateTime lastModifiedTime = System.IO.File.GetLastWriteTime(websFilePath);
                if (System.IO.File.Exists(lastModifiedTimePath))
                {
                    DateTime savedModifiedTime = DateTime.Parse(System.IO.File.ReadAllText(lastModifiedTimePath));
                    if (lastModifiedTime <= savedModifiedTime)
                    {
                        Console.WriteLine("Webs.txt 文件未修改，无需重新生成.bat文件。");
                    }
                    else
                    {
                        GenerateBatFiles();
                        System.IO.File.WriteAllText(lastModifiedTimePath, lastModifiedTime.ToString());
                    }
                }
                else
                {
                    GenerateBatFiles();
                    System.IO.File.WriteAllText(lastModifiedTimePath, lastModifiedTime.ToString());
                }

                RunBatFiles();

            }

            #region
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

                    string targetPath = parts[1].Trim();
                    string targetPathWithStart = $"start {targetPath}"; // 在此处添加 "start "

                    if (targetPath.Contains(" ") || ContainsChineseCharacter(targetPath))
                    {
                        var shortcutPath = Path.Combine(transportDirectoryPath, $"{i + 1}_{parts[0]}.lnk");
                        CreateShortcut(targetPath, shortcutPath);
                        targetPathWithStart = $"start {shortcutPath}";
                    }

                    var batFileCommand = $"{targetPathWithStart}\n{parts[1].Trim()}"; // 在此处添加 "start "
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
                    var input2 = Console.ReadLine().Trim(); // 修剪输入的字符串，去除前后的空格
                    int index;
                    bool isHiddenString = input2 == "I REALLY Do Want To Have A Break";

                    while ((!int.TryParse(input2, out index) || (index < 1 || index > lines.Length)) && !isHiddenString)
                    {
                        Console.WriteLine("输入的序号无效，请重新输入：");
                        input2 = Console.ReadLine().Trim(); // 修剪输入的字符串，去除前后的空格
                        isHiddenString = input2 == "I REALLY Do Want To Have A Break";
                    }

                    // 检查用户是否输入了隐藏字符串
                    if (isHiddenString)
                    {
                        Console.WriteLine("请输入您想要启动的网址：");
                        Console.WriteLine("在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n" +
                            "在这之前，请你确认再确认你是否真的能够休息，不能休息的话，请输入“0”以重启学习\n");
                        var url = Console.ReadLine().Trim(); // 修剪输入的字符串，去除前后的空格

                        if (url == "0")
                        {
                            continue; // 重新询问用户想要启动的 .bat 程序
                        }

                        // 在默认浏览器中打开网址
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });

                        // 设置 allFilesRenamed 为 true 以退出循环时
                        allFilesRenamed = true;
                    }
                    else
                    {
                        var fileName = Path.Combine(directoryPath, $"{index}_{lines[index - 1].Split(';')[0]}.bat");
                        if (System.IO.File.Exists(fileName))
                        {
                            System.Diagnostics.Process.Start(fileName);
                            allFilesRenamed = true;
                        }
                        else
                        {
                            Console.WriteLine("指定的.bat文件不存在，请重新输入序号。");
                            continue;
                        }
                    }
                }
            }

            #endregion

            if (MainMode == "3")
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "CorrectedNotebook");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);


                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("请按照Readme文本文档来配置好文件后使用");
                    // 扫描程序目录下的 Readme.txt 并打开
                    string readmePath = Path.Combine(Directory.GetCurrentDirectory(), "Readme.txt");
                    if (System.IO.File.Exists(readmePath))
                    {
                        System.Diagnostics.Process.Start(readmePath);
                    }
                    Console.WriteLine("请按照Readme文本文档来配置好文件后使用");
                    Console.ReadKey();
                    return;



                }
                // 功能1: 如果有，就询问用户“启动配置模式或是正常模式”
                Console.WriteLine("启动配置模式或是正常模式？(输入1表示配置模式，输入2表示正常模式)");
                string mode = Console.ReadLine();
                while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (mode != "1" && mode != "2"))
                {
                    Console.WriteLine("无效的输入，请重新输入：");
                    mode = Console.ReadLine(); // 获取用户重新输入的答案
                }
                if (mode == "1")
                {
                    // 功能2: 打开文件所在目录下的文件夹
                    OpenFolder(folderPath);
                }
                else if (mode == "2")
                {
                //添加传送点（标记）
                RB:
                    // 功能3: 扫描CorrectedNotebook文件夹下面的一级子文件夹的名字，分行显示在在屏幕上，询问用户要复习哪一个文件夹中的错题
                    var subfolders = Directory.GetDirectories(folderPath).Select(Path.GetFileName).ToArray();
                    for (int i = 0; i < subfolders.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {subfolders[i]}");
                    }
                    Console.WriteLine("请选择要复习哪一个文件夹中的错题(输入序号): ");
                    int index = Convert.ToInt32(Console.ReadLine()) - 1;
                    string selectedFolder = Path.Combine(folderPath, subfolders[index]);

                    // 用户输入完毕以后,是否按照分类抽取
                    Console.WriteLine("是否按照分类抽取？(输入y表示是，输入n表示否)");
                    string answer = Console.ReadLine();
                    while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (answer != "y" && answer != "n"))
                    {
                        Console.WriteLine("无效的输入，请重新输入：");
                        answer = Console.ReadLine(); // 获取用户重新输入的答案
                    }
                    if (answer == "n")
                    {
                        ExtractImages(selectedFolder);
                    }
                    else if (answer == "y")
                    {
                        SelectFolderAndExtractImages(selectedFolder);

                    }




                }


            }

            #region
            static void OpenFolder(string folderPath)
            {
                System.Diagnostics.Process.Start("explorer.exe", folderPath);//这样可以制定程序
                //System.Diagnostics.Process.Start(folderPath);这样是使用默认程序
            }

            static void SelectFolderAndExtractImages(string folderPath)
            {
                var subfolders = Directory.GetDirectories(folderPath).Select(Path.GetFileName).ToArray();
                if (subfolders.Length == 0)
                {
                    Console.WriteLine("没有更小的文件夹了");
                    ExtractImages(folderPath);
                    return;
                }

                for (int i = 0; i < subfolders.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {subfolders[i]}");
                }
                Console.WriteLine("请选择要复习哪一个文件夹中的错题(输入序号): ");
                int index = Convert.ToInt32(Console.ReadLine()) - 1;
                string selectedFolder = Path.Combine(folderPath, subfolders[index]);

                Console.WriteLine("是否按照分类抽取？(输入y表示是，输入n表示否)");
                string answer = Console.ReadLine();
                while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (answer != "y" && answer != "n"))
                {
                    Console.WriteLine("无效的输入，请重新输入：");
                    answer = Console.ReadLine(); // 获取用户重新输入的答案
                }
                if (answer == "n")
                {

                    ExtractImages(selectedFolder);
                    return;
                }
                else if (answer == "y")
                {

                    SelectFolderAndExtractImages(selectedFolder);
                }



            }

            static void ExtractImages(string folderPath)
            {
                // 先询问用户要从收藏夹中抽取多少张图片
                string bookmarkFolder = Path.Combine(folderPath, "Bookmarks");
                if (Directory.Exists(bookmarkFolder))
                {
                    Console.WriteLine("要从收藏夹中抽取多少张图片？(输入一个整数)");
                    int count = Convert.ToInt32(Console.ReadLine());
                    var bookmarkImages = GetImageFiles(bookmarkFolder);
                    Random random = new Random();
                    for (int i = 0; i < count; i++)
                    {
                        int randomIndex = random.Next(bookmarkImages.Length);
                        string selectedImage = bookmarkImages[randomIndex];
                        OpenImage(selectedImage);
                    }
                }

                // 再从文件夹中随机抽取图片
                var imageFiles = GetImageFiles(folderPath);
                Random random2 = new Random();
                List<string> usedImages = new List<string>();
                while (true)
                {
                    int randomIndex = random2.Next(imageFiles.Length);
                    string selectedImage = imageFiles[randomIndex];
                    if (usedImages.Contains(selectedImage))
                    {
                        continue;
                    }
                    usedImages.Add(selectedImage);
                    OpenImage(selectedImage);

                    // 然后问用户做对了没，让用户回答，然后问是否将这个题复制到收藏夹
                    Console.WriteLine("做对了吗？(输入y表示是，输入n表示否)");
                    string correct = Console.ReadLine();
                    while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (correct != "n" && correct != "y"))
                    {
                        Console.WriteLine("无效的输入，请重新输入：");
                        correct = Console.ReadLine(); // 获取用户重新输入的答案
                    }
                    if (correct == "n")
                    {
                        Console.WriteLine("是否将这个题复制到收藏夹？(输入y表示是，输入n表示否)");
                        string bookmark = Console.ReadLine();
                        while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (bookmark != "n" && bookmark != "y"))
                        {
                            Console.WriteLine("无效的输入，请重新输入：");
                            bookmark = Console.ReadLine(); // 获取用户重新输入的答案
                        }
                        if (bookmark == "y")
                        {
                            if (!Directory.Exists(bookmarkFolder))
                            {
                                Directory.CreateDirectory(bookmarkFolder);
                            }
                            string destFile = Path.Combine(bookmarkFolder, Path.GetFileName(selectedImage));
                            System.IO.File.Copy(selectedImage, destFile);
                        }
                    }

                    // 然后问用户要不要结束程序
                    Console.WriteLine("要不要结束程序？(输入y表示是，输入n表示否)");
                    string exit = Console.ReadLine();
                    while (/*!int.TryParse(Console.ReadLine(), out answer)*←这是啥意思*/ (exit != "n" && exit != "y"))
                    {
                        Console.WriteLine("无效的输入，请重新输入：");
                        exit = Console.ReadLine(); // 获取用户重新输入的答案
                    }
                    if (exit == "y")
                    {
                        return;
                    }
                }
            }

            static void OpenImage(string imagePath)
            {
                System.Diagnostics.Process.Start("mspaint.exe", imagePath);
            }

            static string[] GetImageFiles(string folderPath)
            {
                return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".bmp"))
                            .ToArray();
            }

            #endregion

            if(MainMode == "4")
            {
                Environment.Exit(0);
            }

        }
    }
}