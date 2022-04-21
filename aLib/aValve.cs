#region • Usings

using System;
using System.IO;
using System.Text.RegularExpressions;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с разделами Valve.
    /// </summary>
    public class aValve
    {
        /// <summary>
        /// Работа с конфигом игры Counter-Strike Source.
        /// </summary>
        public class aCounterStrikeSourceCFG
        {
            /// <summary>
            /// Получает значение запрашиваемого параметра.
            /// </summary>
            /// <param name="Parameter">Запрашиваемый параметр.</param>
            static public string Get(object Parameter)
            {
                using (StreamReader SR = new StreamReader(Environment.CurrentDirectory + @"\cstrike\cfg\config.cfg"))
                {
                    string cfg = SR.ReadToEnd();
                    Regex rgx = new Regex("\n" + Parameter + " \"(.*)\"");
                    string value = rgx.Match(cfg).Groups[1].ToString();
                    return value;
                }
            }

            /// <summary>
            /// Устанавливает значение парметра.
            /// </summary>
            /// <param name="Parameter">Искомый параметр.</param>
            /// <param name="Value">Устанавливаемое значение параметра.</param>
            static public void Put(object Parameter, string Value)
            {
                using (StreamReader SR = new StreamReader(Environment.CurrentDirectory + @"\cstrike\cfg\config.cfg"))
                {
                    using (StreamWriter SW = new StreamWriter(Environment.CurrentDirectory + @"\cstrike\cfg\config.cfg"))
                    {
                        SW.Write(new Regex("\n" + Parameter + " \".*\"").Replace(SR.ReadToEnd(), "\n" + Parameter + " \"" + Value + "\""));
                    }
                }
            }
        }
    }
}
