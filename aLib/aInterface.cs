using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using MaterialSkin;
using MaterialSkin.Controls;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;

namespace aLib
{
    /// <summary>
    /// Работа с интерфейсом пользователя
    /// </summary>
    public class aInterface
    {
        /// <summary>
        /// Основной цвет
        /// </summary>
        private static Color primaryColor = Color.FromArgb(25, 170, 101);
        public static Color PrimaryColor { get => primaryColor; set => primaryColor = value; }

        /// <summary>
        /// Светлый оттенок основного цвета
        /// </summary>
        private static Color lightPrimaryColor = Color.FromArgb(16, 113, 67);
        public static Color LightPrimaryColor { get => lightPrimaryColor; set => lightPrimaryColor = value; }

        /// <summary>
        /// Темный оттенок основного цвета
        /// </summary>
        private static Color darkPrimaryColor = Color.FromArgb(16, 113, 67);
        public static Color DarkPrimaryColor { get => darkPrimaryColor; set => darkPrimaryColor = value; }

        /// <summary>
        /// Акцентированный цвет
        /// </summary>
        private static Color accentColor = Color.FromArgb(101, 198, 152);
        public static Color AccentColor { get => accentColor; set => accentColor = value; }

        /// <summary>
        /// Цвет текста
        /// </summary>
        private static TextShade textShade = TextShade.WHITE;
        public static TextShade TextShade { get => textShade; set => textShade = value; }

        /// <summary>
        /// Структура индивидуального интерфейса
        /// </summary>
        /// <param name="Priamry">Основной цвет</param>
        /// <param name="LightPrimary">Светлый оттенок основного цвета</param>
        /// <param name="DarkPrimary">Темный оттенок основного цвета</param>
        /// <param name="Accent"> Акцентированный цвет</param>
        /// <param name="Shade">Цвет текста</param>
        public struct CustomInterface
        {
            public static Color PrimaryColor { get; set; }
            public static Color LightPrimary { get; set; }
            public static Color DarkPrimary { get; set; }
            public static Color Accent { get; set; }
            public static TextShade Shade { get; set; }
        }

        /// <summary>
        /// Установка общего стиля
        /// </summary>
        /// <param name="Form">Родительская форма</param>
        public static void ApplyMaterialStyle(MaterialForm Form/*, CustomInterface CustomInterface*/)
        {
            MaterialSkinManager MSM = MaterialSkinManager.Instance;
            MSM.AddFormToManage(Form);
            MSM.Theme = MaterialSkinManager.Themes.DARK;
            MSM.ColorScheme = new ColorScheme(
                PrimaryColor,
                LightPrimaryColor,
                PrimaryColor,
                AccentColor,
                TextShade);
        }
               
        /// <summary>
        /// Принудительное включение двойной буферизации
        /// </summary>
        /// <param name="ctrl">Объект для включения</param>
        /// <param name="isEnable">Статус буфера</param>
        public static void DoubleBuffer(object ctrl, bool isEnable)
        {
            Type dgvType = ctrl.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(ctrl, isEnable, null);
        }

        /// <summary>
        /// Действие с задержкой
        /// </summary>
        /// <param name="action">Событие</param>
        /// <param name="senders">Объект заморозки</param>
        public static void DelayEvent(Action action, params object[] senders)
        {
            var isSender = senders.Any();
            if(isSender)
                foreach (var once in senders)
                    ((Control)once).Enabled = false;

            Timer _t = new Timer() { Interval = 400 };
            _t.Tick += (object o, EventArgs ea) =>
            {
                _t.Stop();
                action.Invoke();
                if (isSender)
                    foreach (var once in senders)
                        ((Control)once).Enabled = true;

            };
            _t.Start();
        }
    }
}