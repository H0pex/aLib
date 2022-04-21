#region • Usings

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Management;
using System.Threading.Tasks;
using aLib.Interfaces;
using System.Linq;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с шифрованием данных.
    /// </summary>
    public class aEncryptions
    {
        public class Aes
        {
            private static byte[] key = Enumerable.Range(0, 32).Select(x => (byte)x).ToArray();

            public static string Encrypt(string data, bool islong = false)
            {
                var outD  = string.Empty;
                var chars = "\\/:*?\"<>|".ToCharArray();
                do
                {
                    using (AesManaged aes = new AesManaged() { Key = key })
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write, true))
                        {
                            var gb = Encoding.UTF8.GetBytes(data);
                            cs.Write(gb, 0, gb.Length);
                        }
                        outD = Convert.ToBase64String(ms.ToArray());
                    }
                } while (outD.IndexOfAny(chars) >= 0 || (islong ? !islong : outD.Length >= 255));
                return outD;
            }

            public static string Decrypt(string base64)
            {
                if (string.IsNullOrEmpty(base64))
                    return null;

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    byte[] iv = new byte[16];
                    ms.Read(iv, 0, iv.Length);
                    using (AesManaged aes = new AesManaged() { Key = key, IV = iv })
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read, true))
                    using (MemoryStream output = new MemoryStream())
                    {
                        cs.CopyTo(output);
                        return Encoding.UTF8.GetString(output.ToArray());
                    }
                }
            }
        }

        public class UniByte : ICrypto
        {
            private string key = string.Empty;

            public UniByte(string private_key) => key = private_key;

            public string Encrypt(string data)
            {
                byte[] input = Encoding.UTF8.GetBytes(data);
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] keys = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                    using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                    {
                        ICryptoTransform transform = tripDes.CreateEncryptor();
                        byte[] results = transform.TransformFinalBlock(input, 0 , input.Length);
                        return Convert.ToBase64String(results, 0, results.Length);
                    }
                }
            }

            public string Decrypt(string data)
            {
                byte[] input = Convert.FromBase64String(data);
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] keys = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                    using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                    {
                        ICryptoTransform transform = tripDes.CreateDecryptor();
                        byte[] results = transform.TransformFinalBlock(input, 0, input.Length);
                        return Encoding.UTF8.GetString(results);
                    }
                }
            }
        }

        /// <summary>
        /// Работа с лицензированием ПО.
        /// </summary>
        public class License
        {
            /// <summary>
            /// Получает ID машины.
            /// </summary>
            public static string GetUHId()
            {
                StringBuilder sb = new StringBuilder();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sb.Append(queryObj["NumberOfCores"]);
                    sb.Append(queryObj["ProcessorId"]);
                    sb.Append(queryObj["Name"]);
                    sb.Append(queryObj["SocketDesignation"]);

                    Console.WriteLine(queryObj["ProcessorId"]);
                    Console.WriteLine(queryObj["Name"]);
                    Console.WriteLine(queryObj["SocketDesignation"]);
                }

                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sb.Append(queryObj["Manufacturer"]);
                    sb.Append(queryObj["Name"]);
                    sb.Append(queryObj["Version"]);

                    Console.WriteLine(queryObj["Manufacturer"]);
                    Console.WriteLine(queryObj["Name"]);
                    Console.WriteLine(queryObj["Version"]);
                }

                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sb.Append(queryObj["Product"]);
                    Console.WriteLine(queryObj["Product"]);
                }

                var bytes = Encoding.ASCII.GetBytes(sb.ToString());
                SHA256Managed sha = new SHA256Managed();

                byte[] hash = sha.ComputeHash(bytes);

                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }

            /// <summary>
            /// Асинхронно получает ID машины.
            /// </summary>
            public static async Task<string> GetUHIdAsync() => await Task.Run(() => GetUHId());

            /// <summary>
            /// Проверяет наличие лицензии для AppName.
            /// </summary>
            /// <param name="AppName">Название лицензируемого ПО.</param>
            private static bool CheckLic(string AppName)
            {
                string ID = GetUHId();
                string LID = CryptUN.ToString(aRegEdit.Security.Read(AppName, "License"));
                Console.WriteLine("ID: " + ID);
                Console.WriteLine("License ID: " + LID);
                if (ID == LID) return true; else return false;
            }

            /// <summary>
            /// Запись лицензии ПО.
            /// </summary>
            /// <param name="AppName">Название лицензируемого ПО.</param>
            /// <param name="Key">Ключ лицензируемого ПО.</param>
            private static void ActivatedLicense(string AppName, string Key)
            {
                if (!string.IsNullOrWhiteSpace(Key))
                {
                    try
                    {
                        if (GetUHId() == CryptUN.ToString(Key)) aRegEdit.Security.Write(AppName, "License", Key);
                    }
                    catch
                    {
                        aSystem.NotifyBox("Недействительный ключ!", aSystem.NotifyType.Error, 3000);
                    }
                }
                else
                    aSystem.NotifyBox("Ключ не введен!", aSystem.NotifyType.Error, 3000);
            }
        }

        /// <summary>
        /// Шестнадцатеричное шифрование.
        /// </summary>
        public class Hex
        {
            /// <summary>
            /// Шифрует принимаемую строку в Hex.
            /// </summary>
            /// <param name="text">Принимаемая строка.</param>
            public static string ToHex(string text)
            {
                var sb = new StringBuilder();
                var bytes = Encoding.Unicode.GetBytes(text);
                foreach (var t in bytes)                
                    sb.Append(t.ToString("X2"));
                return sb.ToString();
            }

            /// <summary>
            /// Дешифрует принимаемый Hex в читаемый String.
            /// </summary>
            /// <param name="text">Шифрованная строка Hex.</param>
            public static string ToString(string text)
            {
                try
                {
                    var bytes = new byte[text.Length / 2];
                    for (var i = 0; i < bytes.Length; i++)                    
                        bytes[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);                    
                    return Encoding.Unicode.GetString(bytes);
                }
                catch { return text; }
            }

            /// <summary>
            /// Возвращает Hex строку указанной длины.
            /// </summary>
            /// <param name="Length">Максимальная длина строки Hex.</param>
            public static string RandomName(int Length)
            {
                string[] A = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
                string Hex = null;
                for (int i = 0; i < Length && i < 26; i++)
                {
                    System.Threading.Thread.Sleep(15);
                    foreach (var OneChar in Encoding.Unicode.GetBytes(A[new Random().Next(A.Length)]))
                        Hex += new StringBuilder().AppendFormat("{0:X2} ", OneChar);
                }
                return Hex.ToUpper().Remove(Length);
            }
        }

        /// <summary>
        /// ASCII (64) шифрование.
        /// </summary>
        public class Base64
        {
            /// <summary>
            /// Шифрует принимаемую строку в Base64.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string ToBase64(string String)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(String));
            }

            /// <summary>
            /// Дешифрует принимаемый Base64 в читаемый String.
            /// </summary>
            /// <param name="Base64">Шифрованная строка Base64.</param>
            public static string ToString(string Base64)
            {
                try { return Encoding.UTF8.GetString(Convert.FromBase64String(Base64)); } catch { return Base64; }
            }
        }

        /// <summary>
        /// Юникод шифрование. 
        /// </summary>
        public class UTF8
        {
            /// <summary>
            /// Шифрует принимаемую строку в UTF8.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string ToUTF8(string String)
            {
                byte[] utf8Bytes = Encoding.GetEncoding("UTF-8").GetBytes(String);
                byte[] win1251Bytes = Encoding.Convert(Encoding.GetEncoding("Windows-1251"), Encoding.GetEncoding("UTF-8"), utf8Bytes);
                return Encoding.GetEncoding("UTF-8").GetString(win1251Bytes);
            }

            /// <summary>
            /// Дешифрует принимаемый UTF8 в читаемый String.
            /// </summary>
            /// <param name="UTF8">Шифрованная строка UTF8.</param>
            public static string ToString(string UTF8)
            {
                byte[] utf8Bytes = Encoding.GetEncoding("Windows-1251").GetBytes(UTF8);
                byte[] win1251Bytes = Encoding.Convert(Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("Windows-1251"), utf8Bytes);
                return Encoding.GetEncoding("Windows-1251").GetString(win1251Bytes);
            }
        }

        /// <summary>
        /// Шифрование байтами.
        /// </summary>
        public class Byte
        {
            /// <summary>
            /// Шифрует принимаемую строку в Byte.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string Encrypt(string String)
            {
                if (string.IsNullOrEmpty(String)) return "";

                byte[] initVecB = Encoding.ASCII.GetBytes("a8doSuDitOz1hZe#");
                byte[] solB = Encoding.ASCII.GetBytes("double");
                byte[] ishTextB = Encoding.UTF8.GetBytes(String);

                PasswordDeriveBytes derivPass = new PasswordDeriveBytes("string", solB, "SHA1", 2);
                byte[] keyBytes = derivPass.GetBytes(256 / 8);
                RijndaelManaged symmK = new RijndaelManaged();
                symmK.Mode = CipherMode.CBC;

                byte[] cipherTextBytes = null;

                using (ICryptoTransform ToAUNor = symmK.CreateEncryptor(keyBytes, initVecB))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, ToAUNor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(ishTextB, 0, ishTextB.Length);
                            cryptoStream.FlushFinalBlock();
                            cipherTextBytes = memStream.ToArray();
                            memStream.Close();
                            cryptoStream.Close();
                        }
                    }
                }

                symmK.Clear();
                return Convert.ToBase64String(cipherTextBytes);
            }

            /// <summary>
            /// Дешифрует принимаемый Byte в читаемый String.
            /// </summary>
            /// <param name="Byte">Шифрованная строка Byte.</param>
            public static string Decrypt(string Byte)
            {
                try
                {
                    if (string.IsNullOrEmpty(Byte)) return "";

                    byte[] initVecB = Encoding.ASCII.GetBytes("a8doSuDitOz1hZe#");
                    byte[] solB = Encoding.ASCII.GetBytes("double");
                    byte[] cipherTextBytes = Convert.FromBase64String(Byte);

                    PasswordDeriveBytes derivPass = new PasswordDeriveBytes("string", solB, "SHA1", 2);
                    byte[] keyBytes = derivPass.GetBytes(256 / 8);

                    RijndaelManaged symmK = new RijndaelManaged();
                    symmK.Mode = CipherMode.CBC;

                    byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                    int byteCount = 0;

                    using (ICryptoTransform decryptor = symmK.CreateDecryptor(keyBytes, initVecB))
                    {
                        using (MemoryStream mSt = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(mSt, decryptor, CryptoStreamMode.Read))
                            {
                                byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                mSt.Close();
                                cryptoStream.Close();
                            }
                        }
                    }

                    symmK.Clear();
                    return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
                }
                catch
                {
                    return "Not";
                }
            }
        }

        /// <summary>
        /// 128-битное шифрование.
        /// </summary>
        public class MD5
        {
            /// <summary>
            /// Перечисление типов ресурса.
            /// </summary>
            public enum SourceType { String, File };

            /// <summary>
            /// Шифрует принимаемый ресурс в MD5.
            /// </summary>
            /// <param name="Type">Тип принимаемого ресурса - String (строка) или File.</param>
            /// <param name="Source">Принимаемый ресурс.</param>
            public static string GetMD5(SourceType Type, string Source)
            {
                switch (Type)
                {
                    case SourceType.String:
                        byte[] data = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Source));
                        StringBuilder sBuilder = new StringBuilder();
                        for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
                        return sBuilder.ToString();
                    case SourceType.File:
                        using (FileStream fs = File.OpenRead(Source))
                        {
                            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
                            byte[] fileData = new byte[fs.Length];
                            fs.Read(fileData, 0, (int)fs.Length);
                            byte[] checkSum = md5.ComputeHash(fileData);
                            return BitConverter.ToString(checkSum).Replace("-", string.Empty);
                        }
                    default:
                        return "First parameter is invalid";
                }
            }
        }

        /// <summary>
        /// Кодирование байтами данных ASCII.
        /// </summary>
        public class CryptBYB6
        {
            /// <summary>
            /// Шифрует принимаемую строку в BYB6.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string ToBYB6(string String)
            {
                try
                {
                    for (int i = 0; i < 5; i++) String = Base64.ToBase64(String);
                    return String;
                }
                catch { return "Not"; }
            }

            /// <summary>
            /// Дешифрует принимаемый BYB6 в читаемый String.
            /// </summary>
            /// <param name="String">Шифрованная строка BYB6.</param>
            public static string ToString(string String)
            {
                try
                {
                    for (int i = 0; i < 5; i++) String = Base64.ToString(String);
                    return String;
                }
                catch { return "Not"; }
            }
        }

        /// <summary>
        /// Универсальная кодировка .
        /// </summary>
        public class CryptUN
        {
            /// <summary>
            /// Шифрует принимаемую строку в AUN.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string ToAUN(string String)
            {
                string iS = string.Empty;
                foreach (char c in CryptBYB6.ToBYB6(String))
                {
                    try { iS += '$' + (Convert.ToInt32(c.ToString()) * 5 - 3).ToString(); }
                    catch { iS += '$' + c.ToString(); }
                }
                return iS.Remove(0, 1).Replace('/', '^').Replace('+', ',').Replace('=', '*');
            }

            /// <summary>
            /// Дешифрует принимаемый AUN в читаемый String.
            /// </summary>
            /// <param name="String">Шифрованная строка AUN.</param>
            public static string ToString(string String)
            {
                try
                {
                    string iS = string.Empty;
                    string[] S = String.Split('$');
                    foreach (var c in S)
                        try { iS += ((Convert.ToInt32(c.ToString()) + 3) / 5).ToString(); }
                        catch { iS += c; }
                    return CryptBYB6.ToString(iS.Replace('*', '=').Replace(',', '+').Replace('^', '/'));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Кодировка лицензионного MKey ключа.
        /// </summary>
        public class CryptMKey
        {
            /// <summary>
            /// Шифрует принимаемый массив в лицензионный ключ MKey.
            /// </summary>
            /// <param name="ArrayKeys">Массив данных для ключа.</param>
            public static string ToAMK(string[] ArrayKeys)
            {
                char[] E =
                        CryptBYB6.ToBYB6(
                            CryptUN.ToAUN(ArrayKeys[0]) + "!" +
                            CryptUN.ToAUN(ArrayKeys[2]) + "!" +
                            CryptUN.ToAUN(ArrayKeys[1])
                        ).ToCharArray();

                Array.Reverse(E);
                return new string(E);
            }

            /// <summary>
            /// Дешифрует лицензионный ключ MKey в массив данных лицензии.
            /// </summary>
            /// <param name="Key">Принимаемый ключ.</param>
            public static string[] ToArrayStringKeys(string Key)
            {
                char[] D = Key.ToCharArray();
                Array.Reverse(D);
                Key = new string(D);

                string[] DKey =
                    {
                    CryptUN.ToString(CryptBYB6.ToBYB6(Key).Split('!')[0]),
                    CryptUN.ToString(CryptBYB6.ToBYB6(Key).Split('!')[2]),
                    CryptUN.ToString(CryptBYB6.ToBYB6(Key).Split('!')[1])
                };
                return DKey;
            }
        }

        /// <summary>
        /// Шифрование строки со сжатием.
        /// </summary>
        public class Package
        {
            /// <summary>
            /// Объявление кодироваок.
            /// </summary>
            internal static Encoding UNI = Encoding.Unicode;
            internal static Encoding UTF = Encoding.UTF8;

            /// <summary>
            /// Шифрование и сжатие строки.
            /// </summary>
            /// <param name="String">Принимаемая строка.</param>
            public static string ToPackage(string String)
            {
                return UNI.GetString(UTF.GetBytes(String));

            }

            /// <summary>
            /// Дешифрует сжатую строку в читаемый String.
            /// </summary>
            /// <param name="String">Сжатая строка.</param>
            public static string ToString(string String)
            {
                return UTF.GetString(Encoding.Convert(UNI, UTF, UNI.GetBytes(String)));
            }
        }

        /// <summary>
        /// Шифрование строки с коротким сжатием.
        /// </summary>
        public class PackageLow
        {
            /// <summary>
            /// Подготовка строки к трансформированию.
            /// </summary>
            internal class MakeString
            {
                string NString = "";
                /// <summary>
                /// Установка шага строки.
                /// </summary>
                /// <param name="Step">Шаг строки.</param>
                //public MakeString(string Step) => NString = Step;

                /// <summary>
                /// Упаковка строки с учетом шага.
                /// </summary>
                /// <param name="Step">Шаг строки.</param>
                /// <param name="Count">Значение шага строки.</param>
                public string Package(string Step, int Count)
                {
                    int Position = NString.IndexOf(Step);
                    if (Position == -1) return "";
                    Position = (Position + Count) % NString.Length;
                    if (Position < 0) Position += NString.Length;
                    return NString.Substring(Position, 1);
                }
            }

            /// <summary>
            /// Класс упаковки строки.
            /// </summary>
            internal class Low : System.Collections.Generic.List<MakeString>
            {
                /// <summary>
                /// Добавление алфавитов сжатия.
                /// </summary>
                public Low()
                {
                    //Add(new MakeString("abcdefghijklmnopqrstuvwxyz"));
                    //Add(new MakeString("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
                    //Add(new MakeString("абвгдеёжзийклмнопрстуфхцчшщъыьэюя"));
                    //Add(new MakeString("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ"));
                    //Add(new MakeString("0123456789"));
                    ////Add(new MakeString("!\"#$%^&*()+=-_'?.,|/`~№:;@[]{}"));
                    //Add(new MakeString("#$%&()'; +=-_~№,@[]{}"));
                }

                /// <summary>
                /// Расчет сжатия строки и вывод.
                /// </summary>
                /// <param name="Step">Шаг сжатия строки.</param>
                /// <param name="Count">Величина шага сжатия.</param>
                public string LowCodeс(string Step, int Count)
                {
                    string
                        Response    = string.Empty,
                            TMP         = string.Empty;
                    for (int i = 0; i < Step.Length; i++)
                    {
                        foreach (MakeString v in this)
                        {
                            TMP = v.Package(Step.Substring(i, 1), Count);
                            if (TMP != "")
                            {
                                Response += TMP;
                                break;
                            }
                        }
                        if (TMP == "") Response += Step.Substring(i, 1);
                    }
                    return Response;
                }
            }

            /// <summary>
            /// Сжатие строки с учетом шага.
            /// </summary>
            /// <param name="String">Входная строка.</param>
            /// <param name="Count">Шаг строки.</param>
            public static string ToPackageLow(string String, int Count)
            {
                string Response = String;
                for (int i = 0; i < Count; i++) Response = new Low().LowCodeс(Response, Count);
                return Response;
            }

            /// <summary>
            /// Распаковка строки.
            /// </summary>
            /// <param name="String">Входная строка.</param>
            /// <param name="Count">Шаг строки.</param>
            public static string StringEquals(string String, int Count)
            {
                string Response = String;
                for (int i = 0; i < Count; i++) Response = new Low().LowCodeс(Response, -Count);
                return Response;
            }
        }

        public class PackageUTF
        {
            /// <summary>
            /// Сжатие строки байтами.
            /// </summary>
            /// <param name="String">Входная строка.</param>
            public static string ToPackageUTF(string String)
            {
                return string.Join("", Encoding.UTF7.GetBytes(String));
            }

            /// <summary>
            /// Сравнивание строк.
            /// </summary>
            /// <param name="EncString">Шифрованная строка.</param>
            /// <param name="DecString">Не шифрованная строка.</param>
            public static bool UTFEquals(string EncString, string DecString)
            {
                return Equals(EncString, Encoding.UTF7.GetBytes(DecString));
            }
        }
    }
}
