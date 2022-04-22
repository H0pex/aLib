#region • Usings

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;
using Ionic.Zlib;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с файловой системой.
    /// </summary>
    public class aNTFS
    {
        /// <summary>
        /// Работа с путями.
        /// </summary>
        public class Paths
        { 
            /// <summary>
            /// Возвращает строку, длина которой равна MaxLength.
            /// </summary>
            /// <param name="Resource">Входная строка.</param>
            /// <param name="MaxLength">Масимальная длина.</param>
            public static string StringLimit(string Resource, int MaxLength)
            {
                return Resource.Length > MaxLength ? Resource.Remove(MaxLength) + ".." : Resource;
            }

            /// <summary>
            /// Возвращает директорию выбранного файла.
            /// </summary>
            /// <param name="FilePath">Путь до файла.</param>
            /// <param name="Limit">Допустимая длина строки, излишки заменяются на многоточие. Длина остается прежней при значении 0.</param>
            public static string GetDirectory(string FilePath, int Limit)
            {
                var Return = FilePath.Remove(FilePath.LastIndexOf("\\") + 1);
                return Limit < 1 ? Return : StringLimit(Return, Limit);
            }

            /// <summary>
            /// Возвращает имя выбранного файла.
            /// </summary>
            /// <param name="FilePath">Путь до файла.</param>
            /// <param name="Extension">При значении false расширение файла не возвращается.</param>
            public static string GetFileName(string FilePath, bool Extension)
            {
                var Return = FilePath.Remove(0, FilePath.LastIndexOf("\\") + 1);
                return Extension ? Return : Return.Remove(Return.LastIndexOf('.'));
            }

            /// <summary>
            /// Возвращает расширение выбранного файла.
            /// </summary>
            /// <param name="FilePath">Путь до файла.</param>
            /// <param name="Extension">При значении false точка перед расширением не возвращается.</param>
            public static string GetExtension(string FilePath, bool Dot)
            {
                var Return = FilePath.Remove(0, FilePath.LastIndexOf('.') + 1);
                return Dot ? '.' + Return : Return;
            }
        }

        /// <summary>
        /// Работа с папками.
        /// </summary>
        public class Folders
        {
            /// <summary>
            /// Создание и возвращение директории.
            /// </summary>
            /// <param name="Path">Путь создаваемой директории.</param>
            /// <param name="Attribute">Визуальные аттрибуты директории.</param>
            public static string Make(string Path, FileAttributes Attribute)
            {
                DirectoryInfo drInfo = new DirectoryInfo(Path);
                if (!drInfo.Exists) { drInfo.Create(); }
                drInfo.Attributes = Attribute;
                return Path;
            }

            /// <summary>
            /// Установка аттрибутов для директории.
            /// </summary>
            /// <param name="Path">Путь до директории.</param>
            public static void SetAttribute(string Path, FileAttributes Attribute)
            {
                new DirectoryInfo(Path).Attributes = Attribute;
            }

            /// <summary>
            /// Возвращает количество файлов в директории.
            /// </summary>
            /// <param name="Path">Путь к директории.</param>
            /// <param name="Filter">Фильтр поиска файлов.</param>
            /// <param name="SearchOption">Опция поиска в подпапках.</param>
            public static int GetFilesCount(string Path, string Filter, SearchOption SearchOption)
            {
                return new DirectoryInfo(Path).GetFiles(Filter, SearchOption).Length;
            }

            /// <summary>
            /// Возвращает размер директории с возможным округлением.
            /// </summary>
            /// <param name="Path">Путь до директории.</param>
            /// <param name="Round">Значение округления размера.</param>
            public static int GetFullSize(string Path, int Round)
            {
                if (!Directory.Exists(Path)) return 0;
                double _size = 0;
                long size = 0;
                foreach (string file in Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories))
                    size += new FileInfo(file).Length;
                _size = Math.Round((double)size / 1000000, Round);
                return (int)_size;
            }
        }

        /// <summary>
        /// Работа с файлами.
        /// </summary>
        public class Files
        {
            /// <summary>
            /// Работа с INI-файлами.
            /// </summary>
            public class Ini
            {
                #region IDisposable

                //bool disposed;
                //protected virtual void Dispose(bool disposing)
                //{
                //    if (!disposed) if (disposing) Dispose();
                //    disposed = true;
                //}
                //public void Dispose()
                //{
                //    Dispose(true);
                //    GC.SuppressFinalize(this);
                //}

                #endregion

                /// <summary>
                /// Возвращает или устанавливает путь к INI файлу.
                /// </summary>
                public string Path { get { return SPath; } set { SPath = value; } }

                /// <summary>
                /// Максимальный размер для чтения значения из файла.
                /// </summary>
                private const int MSize = 1024;

                /// <summary>
                /// Переменная хранения пути к INI-файлу.
                /// </summary>
                private string SPath = null;

                /// <summary>
                /// Импорт приватной функции для чтения файла.
                /// </summary>
                [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
                private static extern int Get(string Section, string Key, string Def, StringBuilder Buffer, int Size, string Path);

                /// <summary>
                /// Импорт приватной функции для записи в файл.
                /// </summary>
                [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
                private static extern int Put(string Section, string Key, string String, string Path);

                /// <summary>
                /// Конструктор, принимающий путь к INI-файлу.
                /// </summary>
                /// <param name="Path">Путь к INI-файлу.</param>
                public Ini(string Path)
                {
                    SPath = Path;
                }

                /// <summary>
                /// Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно).
                /// </summary>
                public Ini() : this("") { }

                /// <summary>
                /// Возвращает значение из INI-файла по указанным секции и ключу.
                /// </summary>
                /// <param name="Section">Секция файла.</param>
                /// <param name="Key">Ключ секции.</param>
                public string Get(string Section, string Key)
                {
                    StringBuilder buffer = new StringBuilder(MSize);
                    Get(Section, Key, null, buffer, MSize, SPath);
                    return buffer.ToString();
                }

                /// <summary>
                /// Записывает значение в INI-файл по указанным секции и ключу.
                /// </summary>
                /// <param name="Section">Секция файла.</param>
                /// <param name="Key">Ключ секции.</param>
                /// <param name="Value">Значение ключа секции.</param>
                public void Put(string Section, string Key, string Value)
                {
                    Put(Section, Key, Value, SPath);
                }
            }

            /// <summary>
            /// Работа с ZIP-файлами.
            /// </summary>
            public class Zip
            {
                /// <summary>
                /// Получает сервисный ключ.
                /// </summary>
                /// <param name="Token">Приватный токен пользователя.</param>
                private static string AddKey(string Token)
                {
                    return aWeb.GetContents("http://assclub.ru/AMG/SkinChanger/App/Scripts/AMG.php?" + "cmd=Zpp" + "&token=" + Token);
                }

                /// <summary>
                /// Добавляет директорию в архив.
                /// </summary>
                /// <param name="AddPath">Путь к директории.</param>
                /// <param name="ZipPath">Путь к архиву.</param>
                public static void AddDirectory(string AddPath, string ZipPath)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddDirectory(AddPath);
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Добавляет директорию в архив с защитой.
                /// </summary>
                /// <param name="AddPath">Путь к директории.</param>
                /// <param name="ZipPath">Путь к архиву.</param>
                /// <param name="AddKey">Сервисный ключ.</param>
                public static void AddDirectory(string AddPath, string ZipPath, string AddKey)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.Password = Zip.AddKey(AddKey);
                    zip.AddDirectory(AddPath);
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Добавляет файл к архиву.
                /// </summary>
                /// <param name="AddPath">Путь до файла.</param>
                /// <param name="ZipPath">Путь до архива.</param>
                public static void AddFile(string AddPath, string ZipPath)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddFile(AddPath, "");
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Добавляет файл к архиву с защитой.
                /// </summary>
                /// <param name="AddPath">Путь до файла.</param>
                /// <param name="ZipPath">Путь до архива.</param>
                /// <param name="AddKey">Сервисный ключ.</param>
                public static void AddFile(string AddPath, string ZipPath, string AddKey)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.Password = Zip.AddKey(AddKey);
                    zip.AddFile(AddPath, "");
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Добавляет файлы к архиву.
                /// </summary>
                /// <param name="AddPath">Путь до файла.</param>
                /// <param name="ZipPath">Путь до архива.</param>
                public static void AddFile(string[] AddPaths, string ZipPath)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddFiles(AddPaths, "");
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Добавляет файлы к архиву с защитой.
                /// </summary>
                /// <param name="AddPath">Путь до файла.</param>
                /// <param name="ZipPath">Путь до архива.</param>
                /// <param name="AddKey">Сервисный ключ.</param>
                public static void AddFile(string[] AddPaths, string ZipPath, string AddKey)
                {
                    ZipFile zip = new ZipFile();
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.Password = Zip.AddKey(AddKey);
                    zip.AddFiles(AddPaths, "");
                    zip.Save(ZipPath);
                    zip.Dispose();
                }

                /// <summary>
                /// Распаковывает архив.
                /// </summary>
                /// <param name="ZipPath">Путь к архиву.</param>
                /// <param name="ExtractPath">Путь извлечения.</param>
                public static async Task Extract(string ZipPath, string ExtractPath)
                {
                    await Task.Run(() => { 
                        foreach (ZipEntry zip_ex in ZipFile.Read(ZipPath))
                            zip_ex.Extract(ExtractPath, ExtractExistingFileAction.DoNotOverwrite);
                    });
                }

                /// <summary>
                /// Распаковывает архив с защитой.
                /// </summary>
                /// <param name="ZipPath">Путь к архиву.</param>
                /// <param name="ExtractPath">Путь извлечения.</param>
                /// <param name="AddKey">Сервисный ключ.</param>
                public static void Extract(string ZipPath, string ExtractPath, string AddKey)
                {
                    foreach (ZipEntry zip_ex in ZipFile.Read(ZipPath))
                    { zip_ex.Password = Zip.AddKey(AddKey); zip_ex.Extract(ExtractPath, ExtractExistingFileAction.DoNotOverwrite); }
                }
            }
        }

        /// <summary>
        /// Приватный импорт классов для ярлыка.
        /// </summary>
        private static class ShellLink
        {
            [ComImport,
            Guid("000214F9-0000-0000-C000-000000000046"),
            InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            internal interface IShellLinkW
            {
                [PreserveSig]
                int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, ref IntPtr pfd, uint fFlags);
                [PreserveSig]
                int GetIDList(out IntPtr ppidl);

                [PreserveSig]
                int SetIDList(IntPtr pidl);

                [PreserveSig]
                int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);

                [PreserveSig]
                int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

                [PreserveSig]
                int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);

                [PreserveSig]
                int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

                [PreserveSig]
                int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);

                [PreserveSig]
                int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

                [PreserveSig]
                int GetHotkey(out ushort pwHotkey);

                [PreserveSig]
                int SetHotkey(ushort wHotkey);

                [PreserveSig]
                int GetShowCmd(out int piShowCmd);

                [PreserveSig]
                int SetShowCmd(int iShowCmd);

                [PreserveSig]
                int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);

                [PreserveSig]
                int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

                [PreserveSig]
                int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

                [PreserveSig]
                int Resolve(IntPtr hwnd, uint fFlags);

                [PreserveSig]
                int SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
            }

            [ComImport,
            Guid("00021401-0000-0000-C000-000000000046"),
            ClassInterface(ClassInterfaceType.None)]
            private class shl_link { }

            internal static IShellLinkW CreateShellLink()
            {
                return (IShellLinkW)(new shl_link());
            }
        }

        /// <summary>
        /// Создание ярлыка.
        /// </summary>
        public class ShortCuts
        {
            /// <summary>
            /// Создание ярлыка.
            /// </summary>
            /// <param name="FilePath">Путь до файла.</param>
            /// <param name="LinkPath">Путь до создаваемого ярлыка.</param>
            /// <param name="WorkDir">Рабочая папка программы ярлыка.</param>
            /// <param name="Description">Описание ярлыка программы.</param>
            public static void Create(string FilePath, string LinkPath, string WorkDir, /*string Arguments,*/ string Description)
            {
                ShellLink.IShellLinkW shlLink = ShellLink.CreateShellLink();
                Marshal.ThrowExceptionForHR(shlLink.SetDescription(Description));
                Marshal.ThrowExceptionForHR(shlLink.SetPath(FilePath));
                // Marshal.ThrowExceptionForHR(shlLink.SetArguments(Arguments)); // добавление аргументов запуска
                Marshal.ThrowExceptionForHR(shlLink.SetWorkingDirectory(WorkDir));
                ((System.Runtime.InteropServices.ComTypes.IPersistFile)shlLink).Save(LinkPath, false);
            }
        }
    }
}
