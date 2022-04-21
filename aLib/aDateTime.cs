#region • Usings

using System;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с датой и временем.
    /// </summary>
    public class aDateTime
    {
        /// <summary>
        /// Добавление нуля перед одноразрядным числом.
        /// </summary>
        /// <param name="Integer">Входное число.</param>
        private static string Formating(int Integer) =>  $"{(Integer < 10 ? "0" : "")}{Integer}";

        /// <summary>
        /// Работа с временем.
        /// </summary>
        public class Time
        {
            /// <summary>
            /// Разделитель времени.
            /// </summary>
            private char Separator = '-';

            /// <summary>
            /// Конструктор класса.
            /// </summary>
            /// <param name="Separator">Кастомный разделитель времени.</param>
            public Time(char Separator = '-') => this.Separator = Separator;

            /// <summary>
            /// Возвращает текущее врменя формата HH.MM.SS.
            /// </summary>
            public string Current
            {
                get => $"{Hour}{Separator}{Minute}{Separator}{Second}";
            }

            /// <summary>
            /// Возвращает текущий час.
            /// </summary>
            public string Hour
            {
                get => Formating(DateTime.Now.Hour);
            }

            /// <summary>
            /// Возвращает текущую минуту.
            /// </summary>
            public string Minute
            {
                get => Formating(DateTime.Now.Minute);
            }

            /// <summary>
            /// Возвращает текущую секунду.
            /// </summary>
            public string Second
            {
                get => Formating(DateTime.Now.Second);
            }

            /// <summary>
            /// Возвращает текущую миллисекунду.
            /// </summary>
            public string Millisecond
            {
                get => Formating(DateTime.Now.Millisecond);
            }
        }

        /// <summary>
        /// Работа с датой.
        /// </summary>
        public class Data
        {
            /// <summary>
            /// Разделитель даты.
            /// </summary>
            private char Separator = '.';

            /// <summary>
            /// Конструктор класса.
            /// </summary>
            /// <param name="Separator">Кастомный разделитель даты.</param>
            public Data(char Separator = '-') => this.Separator = Separator;

            /// <summary>
            /// Возвращает текущую дату формата DD.MM.YYYY.
            /// </summary>
            public string Current
            {
                get => $"{Day}{Separator}{Month}{Separator}{Year}";
            }

            /// <summary>
            /// Возвращает текущий год.
            /// </summary>
            public static string Year
            {
                get => Formating(DateTime.Now.Year);
            }

            /// <summary>
            /// Возвращает текущий месяц.
            /// </summary>
            public static string Month
            {
                get => Formating(DateTime.Now.Month);
            }

            /// <summary>
            /// Возвращает текущий день.
            /// </summary>
            public static string Day
            {
                get => Formating(DateTime.Now.Day);
            }
        }
    }
}
