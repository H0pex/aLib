#region • Usings

using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с ресурсами операционной системы.
    /// </summary>
    public class aSystem
    {
        /// <summary>
        /// Системные сообщения приложения.
        /// </summary>
        public class Messages
        {
            /// <summary>
            /// Допустимые типы сообщений.
            /// </summary>
            public enum MessageTypes
            {
                Exceptions,
                Inforamtion,
                Question,
                Warning
            }

            /// <summary>
            /// Показ сообщения с отпределнным типом <paramref name="MessageType"/>.
            /// </summary>
            /// <param name="MessageText">Текст сообщения.</param>
            /// <param name="MessageType">Тип сообщения.</param>
            public static DialogResult Show(string MessageText, MessageTypes MessageType = MessageTypes.Inforamtion)
            {
                MessageBoxButtons MessageBoxButtons = default;
                MessageBoxIcon MessageBoxIcon = default;

                switch (MessageType)
                {
                    case MessageTypes.Exceptions:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Error;
                        break;
                    case MessageTypes.Inforamtion:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Information;
                        break;
                    case MessageTypes.Question:
                        MessageBoxButtons = MessageBoxButtons.YesNo;
                        MessageBoxIcon = MessageBoxIcon.Question;
                        break;
                    case MessageTypes.Warning:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Exclamation;
                        break;
                    default:
                        MessageBoxButtons = MessageBoxButtons.OK;
                        MessageBoxIcon = MessageBoxIcon.Information;
                        break;
                }

                return MessageBox.Show(MessageText, Application.ProductName, MessageBoxButtons, MessageBoxIcon);
            }
        }

        /// <summary>
        /// Перечисление типов сообщения.
        /// </summary>
        public enum NotifyType
        {
            Success,
            Error,
            Warning,
            Information
        };
        public static int NotifyCount = 0;

        /// <summary>
        /// Выводит сообщение пользователя как уведомление в правом верхнем углу.
        /// </summary>
        /// <param name="Data">Текст сообщения.</param>
        /// <param name="NotifyType">Тип сообщения; по умолчанию Information.</param>
        /// <param name="TimeOut">Тип сообщения; по умолчанию 3000 (3 сек.).</param>
        public static async void NotifyBox(string Data, NotifyType NotifyType = NotifyType.Information, int TimeOut = 3000)
        {
            #region Message

            Form Message = new Form()
            {
                Opacity = 1, // 0 для анимации
                BackColor = Color.FromArgb(17, 17, 17),
                Size = new Size(320, 100), // (1, 100) для анимации
                StartPosition = FormStartPosition.Manual,
                DesktopLocation = new Point(
                        Convert.ToInt32(GetUserSreen()[0]) - 330,
                        NotifyCount < 1 ? 10 : NotifyCount * 120),
                FormBorderStyle = FormBorderStyle.None,
                ControlBox = false,
                ShowInTaskbar = false,
                TopMost = true
            };

            #endregion

            #region Line

            var sideColor = Color.Silver;
            switch (NotifyType)
            {
                case NotifyType.Success:
                    sideColor = Color.Green;
                    break;
                case NotifyType.Error:
                    sideColor = Color.Red;
                    break;
                case NotifyType.Warning:
                    sideColor = Color.Yellow;
                    break;
                case NotifyType.Information:
                    sideColor = Color.Silver;
                    break;
                default:
                    sideColor = Color.Silver;
                    break;
            }

            PictureBox Line = new PictureBox()
            {
                Parent = Message,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Location = new Point(0, 0),
                Size = new Size(2, 100),
                BackColor = sideColor
            };

            #endregion

            #region Settings

            //PictureBox Settings = new PictureBox()
            //{
            //    Parent = Message,
            //    BackColor = Color.Transparent,
            //    Size = new Size(10, 10),
            //    Location = new Point(292, 4),
            //    SizeMode = PictureBoxSizeMode.StretchImage,
            //    Image = Properties.Resources.NSettings
            //};

            #endregion

            #region Close

            PictureBox Exit = new PictureBox()
            {
                Parent = Message,
                BackColor = Color.Transparent,
                Size = new Size(10, 10),
                Location = new Point(306, 4),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.NExit
            };
            Exit.MouseClick += (object O, MouseEventArgs E) => { Message.Dispose(); NotifyCount--; };

            #endregion

            #region Caption

            Label mCaption = new Label()
            {
                Parent = Message,
                AutoSize = true,
                Location = new Point(7, 4),
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point),
                Text = (NotifyType == NotifyType.Information
                        ? "Информация"
                        : (NotifyType == NotifyType.Error
                            ? "Ошибка"
                            : "Внимание"))
            };

            #endregion

            #region Data

            Label mData = new Label()
            {
                Parent = Message,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Size = new Size(310, 70),
                Location = new Point(7, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Text = Data
            };

            #endregion

            NotifyCount++;
            Message.Show();
            await Task.Run(() => Thread.Sleep(TimeOut));
            NotifyCount--;
            Message.Dispose();

            // Анимация
            // aAnimations.AlphaBlend.AlphaBlandMessage(Message, 10, Time);
        }

        public static async Task<bool> ExistsProcessAsync(string processName) => await Task.Run(() => Process.GetProcessesByName(processName).Any());
        public static bool ExistsProcess(string processName) => Process.GetProcessesByName(processName).Any();
        
        public static async Task KillProcessAsync(string processName)
        {
            await Task.Run(() =>
            {
                List<Process> processes = new List<Process>();
                processes.AddRange(Process.GetProcessesByName(processName));
                processes.ForEach(x => x.Kill());
            });
        }

        public static void KillProcess(string processName)
        {
            List<Process> processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName(processName));
            processes.ForEach(x => x.Kill());
        }

        /* Не дописано диалоговое окно */

        //public static DialogResult MessageBox(string Text, MessageType MessageType)
        //{
        //    #region Message

        //    Form Message = new Form()
        //    {
        //        BackColor = Color.FromArgb(17, 17, 17),
        //        Size = new Size(350, 150),
        //        StartPosition = FormStartPosition.CenterScreen,
        //        DesktopLocation = new Point(
        //                (GetUserSreen()[0] - 350) / 2,
        //                (GetUserSreen()[1] - 150) / 2),
        //        FormBorderStyle = FormBorderStyle.None,
        //        ControlBox = false,
        //        ShowInTaskbar = false,
        //        TopMost = true
        //    };

        //    #endregion

        //    #region Caption

        //    Label Caption = new Label()
        //    {
        //        Parent = Message,
        //        AutoSize = true,
        //        Location = new Point(7, 4),
        //        ForeColor = Color.Silver,
        //        Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point),
        //        Text = (MessageType == MessageType.Information
        //                ? "Информация"
        //                : (MessageType == MessageType.Error
        //                    ? "Ошибка"
        //                    : "Внимание"))
        //    };

        //    #endregion

        //    #region Data

        //    Label Data = new Label()
        //    {
        //        Parent = Message,
        //        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
        //        Size = new Size(310, 70),
        //        Location = new Point(7, 25),
        //        ForeColor = Color.White,
        //        Font = new Font("Segoe UI", 8, FontStyle.Regular),
        //        Text = Text
        //    };

        //    #endregion

        //    #region Buttons



        //    #endregion
        //}

        /// <summary>
        /// Скрытый интерпретатор командной строки.
        /// </summary>
        public class HCmd
        {
            /// <summary>
            /// Проверяет доступность интернет-соединения.
            /// </summary>
            public class IsSetWebConnection
            {
                /// <summary>
                /// Проверяет доступность серверов Google.
                /// </summary>
                public static bool Google()
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.google.ru/");
                        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                        request.Timeout = 10000;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream ReceiveStream1 = response.GetResponseStream();
                        StreamReader sr = new StreamReader(ReceiveStream1, true);
                        string responseFromServer = sr.ReadToEnd();

                        response.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                /// <summary>
                /// Проверяет доступность сервера, указанного пользователем.
                /// </summary>
                /// <param name="Url">Проверяемый сервер.</param>
                public static bool Custom(string Url)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                        request.Timeout = 10000;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream ReceiveStream1 = response.GetResponseStream();
                        StreamReader sr = new StreamReader(ReceiveStream1, true);
                        string responseFromServer = sr.ReadToEnd();

                        response.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// Выполняет введенную команду пользователя.
            /// </summary>
            /// <param name="Command">Исполняемая команда.</param>
            public static void Custom(string Command)
            {
                ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + Command);

                psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                psiOpt.RedirectStandardOutput = true;
                psiOpt.UseShellExecute = false;
                psiOpt.CreateNoWindow = true;

                Process procCommand = Process.Start(psiOpt);
                StreamReader srIncoming = procCommand.StandardOutput;
                procCommand.WaitForExit();
            }

            /// <summary>
            /// Уничтожает заданный пользователем процесс, если тот доступен.
            /// </summary>
            /// <param name="ProcessName">Уничтожаемый процесс.</param>
            public static void KillProcess(string ProcessName)
            {
                Custom("taskkill /f /im " + ProcessName);
            }

            /// <summary>
            /// Выполняет поиск заданного пользователем процесса.
            /// </summary>
            /// <param name="Name">Искомый процесс.</param>
            public static async Task<bool> ExistProcess(string Name)
            {
                bool cont = false;
                await Task.Run(() =>
                {
                    foreach (Process clsProcess in Process.GetProcesses())
                        if (clsProcess.ProcessName.StartsWith(Name))
                        {
                            cont = true;
                            break;
                        }                    
                });
                return cont;
            }
        }

        /// <summary>
        /// Импорт библиотеки.
        /// </summary>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        /// <summary>
        /// Возвращает объем доступной оперативной памяти.
        /// </summary>
        /// <param name="TotalMemoryInKilobytes">Тотальный объем оперативной памяти.</param>
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        /// <summary>
        /// Возвращает запрещенное приложение (совместно с MARK46).
        /// </summary>
        public static string CheckDBG()
        {
            string X = "0x01";
            string[] PS = GetProccessesCaption().Split('~');
            string[] xPS =
                    new string[]
                    {
                    "[F?Ш???]",             "[BlackBox]",           "Sandbox:D",
                    "IDA: Quick start",     "PE Explorer",          "AssemblyExplorer",
                    "Microsoft Debugger",   "Syser Kernel",         "syser",
                    "PEID",                 "PEiD",                 "peid",
                    "SNIFFER",              "Sniffer",              "sniffer",
                    "DUMP",                 "Dump",                 "dump",
                    "DUMPER",               "Dumper",               "dumper",
                    "PE TOOL",              "PE Tool",              "pe tool",
                    "PETOOLS",              "PETools",              "petools",
                    "DUPAR",                "Dupar",                "dupar",
                    "DUPER",                "Duper",                "duper",
                    "ENGINE",               "Engine",               "engine",
                    "OLLY",                 "Olly",                 "olly",
                    "PACKAGER",             "Packager",             "packager",
                    "PACOTES",              "Pacotes",              "pacotes",
                    "SUSPEND",              "Suspend",              "suspend",
                    "WILDPROXY",            "Wildproxy",            "wildproxy",
                    "XELERATOR",            "Xelerator",            "xeleretor",
                    "PACKET",               "Packet",               "packet",
                    "SANDBOX",              "Sandbox",              "sandbox",
                    "SANDBOXED",            "Sandboxed",            "sandboxed",
                    "DEFALTBOX",            "DefaltBox",            "defaltbox",
                    "HXD",                  "HxD",                  "hxd",
                    "BVKHEX",               "Bvkhex",               "bvkhex",
                    "EMULATOR:",            "Emulator:",            "emulator:",
                    "AUTOCLIKER",           "AutoClicker",          "autoclicker",
                    "DEBUG",                "Debug",                "debug",
                    "DEBUGER",              "Debugger",             "debugger",
                    "SYSERAPP",             "SyserApp",             "syserapp",
                    "OLLYDBG",              "OllyDgb",              "ollydbg",
                    "SPY",                  "Spy",                  "spy",
                    "ilspy",                "ILSpy",                "ILSPY",
                    "DNSPY",                "dnSpy",                "dnspy",
                    "IMMUNITY",             "Immunity",             "immunity",
                    "YDBG",                 "YDbg",                 "tdbg",
                    "SOFTICE",              "SoftICE",              "softice",
                    "GDB",                  "Gdb",                  "gdb",
                    "IDA",                  "Ida",                  "ida",
                    "HEX-RAYS",             "Hex-Rays",             "hex-rays",
                    "W32DASM",              "W32Dasm",              "w32dasm",
                    "DEDE",                 "DeDe",                 "dede",
                    "PEID",                 "PEiD",                 "peid"
                    };

            foreach (var ProcU in PS)
            {
                foreach (var ProcX in xPS)
                {
                    if (ProcU.IndexOf(ProcX) != -1)
                    {
                        X = ProcX + "|" + GetProccesPath(ProcX);
                    }
                }
            }
            return X;
        }

        /// <summary>
        /// Возвращает битность ОС Windows (x32/x64).
        /// </summary>
        public static string GetWinBit()
        {
            return Environment.Is64BitOperatingSystem ? "64" : "32";
        }

        /// <summary>
        /// Возвращает версию Windows.
        /// </summary>
        /// <param name="Ma">Мажор версии.</param>
        /// <param name="Mi">Минор версии.</param>
        /// <returns></returns>
        public static string GetWinVersion(int Ma, int Mi)
        {
            switch ((Ma + "." + Mi).ToString())
            {
                case "5.0": return "Windows 2000";
                case "5.1": return "Windows XP";
                case "5.2": return "Windows XP (R2)";
                case "6.0": return "Windows Vista";
                case "6.1": return "Windows 7";
                case "6.2": return "Windows 8";
                case "6.3": return "Windows 8.1";
                case "10.0": return "Windows 10";
                default: return "Unknow";
            }
        }

        /// <summary>
        /// Возвращает массив имен (через ~) всех запущенных процессов.
        /// </summary>
        public static string GetProccesses()
        {
            string response = null;
            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes) response += mo["Name"].ToString() + "~";
            return response;

        }

        /// <summary>
        /// Возвращает массив описаний (через ~) всех запущенных процессов.
        /// </summary>
        public static string GetProccessesCaption()
        {
            string response = null;
            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes) response += mo["Caption"].ToString() + "~";
            return response;

        }

        /// <summary>
        /// Возвращает имя процесса по ID.
        /// </summary>
        /// <param name="ID">ID процесса.</param>
        public static string GetProccesName(int ID)
        {
            return Process.GetProcessById(ID).ProcessName;
        }

        /// <summary>
        /// Возвращает ID процесса по имени.
        /// </summary>
        /// <param name="Name">Имя процесса.</param>
        public static string GetProccessID(string Name)
        {
            int response = 00;
            Name = Name.Contains(".") ? Name.Split('.')[1] == "exe" ? Name : Name + "exe" : Name + ".exe";

            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes)
            {
                if (mo["Name"].ToString() == Name)
                    response = Convert.ToInt32(mo["ProcessId"]);
            }
            return response.ToString();
        }

        /// <summary>
        /// Возвращает путь до файла процесса по ID.
        /// </summary>
        /// <param name="ID">ID процесса.</param>
        public static string GetProccesPath(int ID)
        {
            string response = null;

            string queryString =
                    "SELECT Name, ProcessId, Caption, ExecutablePath" +
                    "  FROM Win32_Process";

            SelectQuery query = new SelectQuery(queryString);
            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject mo in processes)
            {
                if (Convert.ToInt32(mo["ProcessId"]) == ID)
                    response = mo["ExecutablePath"].ToString();
            }
            return response;

        }

        /// <summary>
        /// Возвращает путь до файла процесса по имени.
        /// </summary>
        /// <param name="Name">Имя процесса.</param>
        public static string GetProccesPath(string Name)
        {
            if (Name != "0x01")
            {
                string response = string.Empty;
                //Name = Name.Contains(".") ? Name.Split('.')[1] == "exe" ? Name : Name + "exe" : Name + ".exe";
                Name = Name.Contains(".exe") ? Name.Replace(".exe", "") : Name;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT *  FROM Win32_Process");
                foreach (ManagementObject queryObj in searcher.Get())
                    if (queryObj["Name"].ToString().Contains(Name)) response = queryObj["ExecutablePath"].ToString();
                return response;
            }
            else return "";
        }

        /// <summary>
        /// Возвращает имя компьютера.
        /// </summary>
        public static string GetNamePC()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Возвращает имя пользователя.
        /// </summary>
        public static string GetNameUser()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Возвращает размер экрана пользователя.
        /// </summary>
        public static int [] GetUserSreen()
        {
            string Uscreen = string.Empty;
            foreach (var screen in Screen.AllScreens)
                Uscreen = screen.WorkingArea.ToString();
            return new int[] { int.Parse(Uscreen.Split(',')[2].Split('=')[1]), int.Parse(Uscreen.Split(',')[3].Split('=')[1].Replace("}", "")) };
        }

        /// <summary>
        /// Возвращает тотальный объем оперативной памяти.
        /// </summary>
        public static string GetUserRam()
        {
            long memKb;
            GetPhysicallyInstalledSystemMemory(out memKb);
            return (memKb / 1024 / 1024).ToString();

        }

        /// <summary>
        /// Возвращает количество ядер процессора пользователя.
        /// </summary>
        public static string GetUserCoreCount()
        {
            return Environment.ProcessorCount.ToString();
        }

        /// <summary>
        /// Перечисление языков единицы измерения.
        /// </summary>
        public enum Language { Ru, En };

        /// <summary>
        /// Возвращает список подключенных дисков и их объем в ГБ.
        /// </summary>
        /// <param name="Language">Язык единицы измерения.</param>
        public static string[,] GetUserDrives(Language Language)
        {
            bool LNG = Language == Language.Ru ? true : false;
            string[,] drives = new string[Environment.GetLogicalDrives().Length, 2];
            for (int i = 0; i < Environment.GetLogicalDrives().Length; i++)
            {
                string drive = Environment.GetLogicalDrives()[i];
                try
                {
                    drives[i, 0] =
                    (LNG ? "Диск " : "Drive ") + drive.Replace(":\\", "") + (LNG ? "(ГБ)" : "(GB)");
                    drives[i, 1] =
                    (Math.Round((double)(new DriveInfo(drive).TotalSize - new DriveInfo(drive).AvailableFreeSpace) / 1000000000, 0)).ToString()
                    + "/" +
                    (Math.Round((double)new DriveInfo(drive).TotalSize / 1000000000, 0)).ToString();
                }
                catch { }
            }
            return drives;
        }
    }
}
