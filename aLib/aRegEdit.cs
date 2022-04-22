#region • Usings

using Microsoft.Win32;

using System;

#endregion

namespace aLib.Utils
{
    /// <summary>
    /// Работа с реестром.
    /// </summary>
    public class aRegEdit
    {
        /// <summary>
        /// Защищенная запись данных в реестр.
        /// </summary>
        public class Security
        {
            /// <summary>
            /// Защита входной строки.
            /// </summary>
            /// <param name="String">Входная строка без защиты.</param>
            private static string ToSec(string String) => aEncryptions.CryptUN.ToAUN(String);

            /// <summary>
            /// Снятие защиты со входной строки.
            /// </summary>
            /// <param name="String">Входная строка с защитой.</param>
            private static string ToString(string String) => aEncryptions.CryptUN.ToString(String);

            /// <summary>
            /// Чтение защищенного реестра.
            /// </summary>
            /// <param name="Folder">Искомая папка реестра.</param>
            /// <param name="Param">Искомый строковой параметр.</param>
            public static string Read(string Folder, string Param) => ToString(aRegEdit.Read(Folder, ToSec(Param)));

            /// <summary>
            /// Запись защищенных данных реестр.
            /// </summary>
            /// <param name="Folder">Искомая папка реестра.</param>
            /// <param name="Param">Искомый строковой параметр.</param>
            /// <param name="Value">Записываемые данные.</param>
            public static void Write(string Folder, string Param, string Value) => aRegEdit.Write(Folder, ToSec(Param), ToSec(Value));
        }

        /// <summary>
        /// Запись в реестр.
        /// </summary>
        /// <param name="Folder">Создаваемая папка реестра.</param>
        /// <param name="Param">Строковой параметр.</param>
        /// <param name="Value">Значение параметра.</param>
        public static void Write(string Folder, string Param, string Value)
        {
            RegistryKey saveKey = Registry.CurrentUser.CreateSubKey($@"Software\{System.Windows.Forms.Application.CompanyName}\" + Folder);
            saveKey.SetValue(Param, Value);
            saveKey.Dispose();
        }

        /// <summary>
        /// Чтение реестра.
        /// </summary>
        /// <param name="Folder">Искомая папка реестра.</param>
        /// <param name="Param">Искомый строковой параметр.</param>
        public static string Read(string Folder, string Param)
        {
            RegistryKey readKey = Registry.CurrentUser.OpenSubKey($@"Software\{System.Windows.Forms.Application.CompanyName}\" + Folder) != null
                            ? Registry.CurrentUser.OpenSubKey($@"Software\{System.Windows.Forms.Application.CompanyName}\" + Folder)
                            : Registry.CurrentUser;
            var Response = (string)readKey.GetValue(Param);
            return !string.IsNullOrEmpty(Response) ? Response : null;
        }

        /// <summary>
        /// Удаление значения.
        /// </summary>
        /// <param name="Folder">Искомая папка реестра.</param>
        /// <param name="Param">Искомый строковой параметр.</param>
        public static void DeleteValue(string Folder, string Param)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\{System.Windows.Forms.Application.CompanyName}\" + Folder, true);
            key.DeleteValue(Param, false);
            key.Dispose();
        }

        /// <summary>
        /// Удаление параметра.
        /// </summary>
        /// <param name="Folder">Искомая папка реестра.</param>
        public static void DeleteKey(string Folder) => Registry.CurrentUser.DeleteSubKey(Folder, true);
    }
}
