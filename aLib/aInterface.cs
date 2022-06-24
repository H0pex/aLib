#region • Usings
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
#endregion

namespace aLib.Windows.Interface
{
    /// <summary>
    /// Работа с интерфейсом пользователя.
    /// </summary>
    public class aInterface
    {
        /// <summary>
        /// Перечисление свойств визуализации для объекта формы.
        /// </summary>
        public enum VisualTypes
        {
            Enabled,
            Visible
        }

        /// <summary>
        /// Стиль Material для формы.
        /// </summary>
        public class MaterialStyle
        {
            /// <summary>
            /// Основная тема приложения.
            /// </summary>
            private static MaterialSkinManager.Themes themeColor;
            private static MaterialSkinManager.Themes themeColorDefault = MaterialSkinManager.Themes.DARK;

            /// <summary>
            /// Основной цвет.
            /// </summary>
            private static Color primaryColor;
            private static Color primaryColorDefault = Color.FromArgb(25, 170, 101);

            /// <summary>
            /// Светлый оттенок основного цвета.
            /// </summary>
            private static Color lightPrimaryColor;
            private static Color lightPrimaryColorDefault = Color.FromArgb(16, 113, 67);

            /// <summary>
            /// Темный оттенок основного цвета.
            /// </summary>
            private static Color darkPrimaryColor;
            private static Color darkPrimaryColorDefault = Color.FromArgb(16, 113, 67);

            /// <summary>
            /// Акцентированный цвет.
            /// </summary>
            private static Color accentColor;
            private static Color accentColorDefault = Color.FromArgb(101, 198, 152);

            /// <summary>
            /// Цвет текста.
            /// </summary>
            private static TextShade textShadeColor;
            private static TextShade textShadeColorDefault = TextShade.WHITE;

            /// <summary>
            /// Структура цветов интерфейса.
            /// </summary>
            /// <param name="ThemeColor">Основная тема приложения.</param>
            /// <param name="PrimaryColor">Основной цвет.</param>
            /// <param name="LightPrimaryColor">Светлый оттенок основного цвета.</param>
            /// <param name="DarkPrimaryColor">Темный оттенок основного цвета.</param>
            /// <param name="AccentColor"> Акцентированный цвет.</param>
            /// <param name="TextShadeColor">Цвет текста.</param>
            public struct ColorSettings
            {
                public MaterialSkinManager.Themes ThemeColor { get => themeColor; set => themeColor = Equals(value, null) ? themeColorDefault : value; }
                public Color PrimaryColor { get => primaryColor; set => primaryColor = Equals(value, null) ? primaryColorDefault : value; }
                public Color LightPrimaryColor { get => lightPrimaryColor; set => lightPrimaryColor = Equals(value, null) ? lightPrimaryColorDefault : value; }
                public Color DarkPrimaryColor { get => darkPrimaryColor; set => darkPrimaryColor = Equals(value, null) ? darkPrimaryColorDefault : value; }
                public Color AccentColor { get => accentColor; set => accentColor = Equals(value, null) ? accentColorDefault : value; }
                public TextShade TextShadeColor { get => textShadeColor; set => textShadeColor = Equals(value, null) ? textShadeColorDefault : value; }
            }

            /// <summary>
            /// Установка общего стиля Matrial (только для MaterialForm).
            /// </summary>
            /// <param name="Form">Родительская форма.</param>
            public static void ApplyStyle(MaterialForm Form, ColorSettings ColorSettings)
            {
                MaterialSkinManager MSM = MaterialSkinManager.Instance;
                MSM.AddFormToManage(Form);

                MSM.Theme = ColorSettings.ThemeColor;
                MSM.ColorScheme = new ColorScheme(
                    ColorSettings.PrimaryColor,
                    ColorSettings.LightPrimaryColor,
                    ColorSettings.PrimaryColor,
                    ColorSettings.AccentColor,
                    ColorSettings.TextShadeColor);
            }
        }
               
        /// <summary>
        /// Принудительное включение двойной буферизации для объекта <paramref name="ctrl"/>.
        /// </summary>
        /// <param name="ctrl">Объект для включения.</param>
        /// <param name="isEnable">Статус включения буфера.</param>
        public static void DoubleBuffer(object ctrl, bool isEnable)
        {
            Type dgvType = ctrl.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(ctrl, isEnable, null);
        }

        /// <summary>
        /// Действие с задержкой.
        /// </summary>
        /// <param name="action">Событие.</param>
        /// <param name="duration">Задержка события.</param>
        /// <param name="senders">Объект заморозки.</param>
        public static void DelayEvent(Action action, int duration = 400, params object[] senders)
        {
            var isSender = senders.Any();
            if(isSender)
                foreach (var once in senders)
                    ((Control)once).Enabled = false;

            Timer _t = new Timer() { Interval = duration };
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

        /// <summary>
        /// Задает значение <paramref name="state"/> у свойства <paramref name="visualType"/> для всех объектов <paramref name="controls"/>.
        /// </summary>
        /// <param name="visualType">Перечисление свойств визуализации для объекта формы.</param>
        /// <param name="state">Значение свойства для свойства <paramref name="visualType"/>.</param>
        /// <param name="controls">Объекты для применеия значения свойства.</param>
        public static void ApplyVERange(VisualTypes visualType, bool state, params Control[] controls)
        {
            foreach (var control in controls)
                switch (visualType)
                {
                    case VisualTypes.Enabled:
                        control.Enabled = state;

                        break;
                    case VisualTypes.Visible:
                        control.Visible = state;
                        break;
                }
        }
    }
}