#region • Usings

using System.Windows.Forms;
using System;
using System.Drawing;

using MetroFramework.Animation;
using aLib.Utils;
using MaterialSkin.Controls;

#endregion

namespace aLib.Windows.Interface
{
    /// <summary>
    /// Работа с анимациями Windows Metro Style.
    /// </summary>
    public class aAnimations
    {
        public static void MoveAndSize(Control Control, Size Size, Point Point, TransitionType TransitionType, int Duration)
        {
            Move(Control, Point, TransitionType, Duration);
            aAnimations.Size(Control, Size, TransitionType, Duration);
        }

        /// <summary>
        /// Плавное перемещение объекта.
        /// </summary>
        /// <param name="Control">Объект.</param>
        /// <param name="Point">Новая позиция.</param>
        /// <param name="TransitionType">Тип анимации.</param>
        /// <param name="Duration">Продолжительность анимации.</param>
        public static void Move(Control Control, Point Point, TransitionType TransitionType, int Duration)
        {
            new MoveAnimation().Start(Control, Point, TransitionType, Duration);
        }

        /// <summary>
        /// Плавное изменение размеров объекта.
        /// </summary>
        /// <param name="Control">Объект.</param>
        /// <param name="Size">Новые размеры.</param>
        /// <param name="TransitionType">Тип анимации.</param>
        /// <param name="Duration">Продолжительность анимации.</param>
        public static void Size(Control Control, Size Size, TransitionType TransitionType, int Duration)
        {
            new ExpandAnimation().Start(Control, Size, TransitionType, Duration);
        }

        /// <summary>
        /// Плавное изменение цветовой схемы свойства объекта.
        /// </summary>
        /// <param name="Control">Объект.</param>
        /// <param name="Property">Свойство цвета.</param>
        /// <param name="Color">Новый цвет (схема).</param>
        /// <param name="Duration">Продолжительность анимации.</param>
        public static void Color(Control Control, string Property, Color Color, int Duration)
        {
            new ColorBlendAnimation().Start(Control, Property, Color, Duration);
        }

        /// <summary>
        /// Работа с прозрачностью форм и их переходами.
        /// </summary>
        public class AlphaBlend
        {
            /// <summary>
            /// Изменение прозрачности форм Windows.
            /// </summary>
            public class Opacity
            {
                /// <summary>
                /// Повышение прозрачности до указанного значения.
                /// </summary>
                /// <param name="Form">Используемая форма.</param>
                /// <param name="Value">Значение прозрачности, которого необходимо достичь.</param>
                /// <param name="Duration">Скорость изменения прозрачности.</param>
                public static void Decrement(Form Form, double Value, int Duration)
                {
                    Timer T = new Timer() { Interval = Duration };
                    T.Tick += (O, EA) =>
                    {
                        if (!(Form.Opacity != Value)) T.Stop();
                        else Form.Opacity -= 0.01;
                    };
                    T.Start();
                }

                /// <summary>
                /// Понижение прозрачности до указанного значения.
                /// </summary>
                /// <param name="Form">Используемая форма.</param>
                /// <param name="Value">Значение прозрачности, которого необходимо достичь.</param>
                /// <param name="Duration">Скорость изменения прозрачности.</param>
                public static void Increment(Form Form, double Value, int Duration)
                {
                    System.Windows.Forms.Timer T = new System.Windows.Forms.Timer() { Interval = (int)(Duration * 0.5) };
                    T.Tick += (object O, EventArgs EA) =>
                    {
                        if (!(Form.Opacity != Value)) T.Stop();
                        else Form.Opacity += 0.01;
                    };
                    T.Start();
                }

                /// <summary>
                /// Понижение прозрачности до значения по умолчанию.
                /// </summary>
                /// <param name="Form">Используемая форма.</param>
                /// <param name="Duration">Скорость изменения прозрачности.</param>
                public static void Increment(Form Form, int Duration)
                {
                    System.Windows.Forms.Timer T = new System.Windows.Forms.Timer() { Interval = (int)(Duration * 0.5) };
                    T.Tick += (object O, EventArgs EA) =>
                    {
                        if (!(Form.Opacity != 1)) T.Stop();
                        else Form.Opacity += 0.01;
                    };
                    T.Start();
                }
            }
            /// <summary>
            /// Плавное появление формы.
            /// </summary>
            /// <param name="Form">Загружаемая форма.</param>
            /// <param name="Duration">Скорость появления.</param>
            public static void Show(Form Form, int Duration)
            {
                System.Windows.Forms.Timer Event = new System.Windows.Forms.Timer { Interval = Duration };
                Event.Tick += delegate (object sender, EventArgs e)
                {
                    if (Form.Opacity != 1) { Form.Opacity += 0.1; }
                    else { Event.Stop(); }
                }; Event.Start();
            }

            /// <summary>
            /// Плавное появление формы до нужной прозрачности.
            /// </summary>
            /// <param name="Form">Загружаемая форма.</param>
            /// <param name="Opacity">Конечная прозрачность.</param>
            /// <param name="Duration">Скорость появления.</param>
            public static void Show(Form Form, double Opacity, int Duration)
            {
                System.Windows.Forms.Timer Event = new System.Windows.Forms.Timer { Interval = Duration };
                Event.Tick += delegate (object sender, EventArgs e)
                {
                    if (Form.Opacity != Opacity) { Form.Opacity += 0.1; }
                    else { Event.Stop(); }
                }; Event.Start();
            }

            /// <summary>
            /// Появление формы с раздвижением из центра по вертикали.
            /// </summary>
            /// <param name="Form">Загружаемая форма.</param>
            public static void ShowReSize(Form Form)
            {
                int[] BS = new int[]
                    {
                    Form.Width,
                    Form.Height,
                    Form.Location.X,
                    Form.Location.Y
                    };
                Form.Height = 0;
                Form.DesktopLocation = new Point(BS[2], BS[3]);
                Size(Form, new Size(BS[0], BS[1]), TransitionType.EaseInQuad, 10);
                Move(Form, new Point(BS[2], BS[3]), TransitionType.EaseOutCubic, 10);
            }

            /// <summary>
            /// Плавное скрытие формы.
            /// </summary>
            /// <param name="Form">Скрываемая форма.</param>
            /// <param name="Duration">Скорость скрытия.</param>
            public static void Hide(Form Form, int Duration)
            {
                System.Windows.Forms.Timer Event = new System.Windows.Forms.Timer { Interval = Duration };
                Event.Tick += delegate (object sender, EventArgs e)
                {
                    if (Form.Opacity != 0) { Form.Opacity -= 0.1; }
                    else { Event.Stop(); Form.Hide(); }
                }; Event.Start();
            }

            /// <summary>
            /// Плавное закрытие формы.
            /// </summary>
            /// <param name="senderClass">Закрываемая форма. Поддержваются классы форм Form и MaterialForm.</param>
            /// <param name="Duration">Скорость закрытия.</param>
            public static void Close(object senderClass, int Duration)
            {
                var isMaterialForm = senderClass is MaterialForm;
                
                Timer Event = new Timer { Interval = Duration };
                Event.Tick += (o, e) =>
                {
                    if ((isMaterialForm ? (MaterialForm)senderClass : (Form)senderClass).Opacity == 0)
                    {
                        Event.Stop();
                        (isMaterialForm ? (MaterialForm)senderClass : (Form)senderClass).Close();
                    }
                    (isMaterialForm ? (MaterialForm)senderClass : (Form)senderClass).Opacity -= 0.1;
                };
                Event.Start();
            }

            /// <summary>
            /// Плавное сворачивание формы в пенель задач.
            /// </summary>
            /// <param name="Form">Сворачиваемая форма.</param>
            /// <param name="Duration">Скорость сворачивания.</param>
            public static void Minimized(Form Form, int Duration)
            {
                System.Windows.Forms.Timer Event = new System.Windows.Forms.Timer { Interval = Duration };
                Event.Tick += delegate (object sender, EventArgs e)
                {
                    if (Form.Opacity != 1) { Form.Opacity -= 0.1; }
                    else { Event.Stop(); Form.WindowState = FormWindowState.Minimized; }
                }; Event.Start();
            }

            /// <summary>
            /// Плавное закрытие формы с выходом из приложения.
            /// </summary>
            /// <param name="Form">Закрываемая форма.</param>
            /// <param name="Duration">Скорость закрытия.</param>
            public static void Exit(Form Form, int Duration)
            {
                System.Windows.Forms.Timer Event = new System.Windows.Forms.Timer { Interval = Duration };
                Event.Tick += delegate (object sender, EventArgs e)
                {
                    if (Form.Opacity != 1) { Form.Opacity -= 0.1; }
                    else { Environment.Exit(0); }
                }; Event.Start();
            }

            /// <summary>
            /// Закрытие формы со сжатием в центр по вертикали и выходом из приложения.
            /// </summary>
            /// <param name="Form">Закрываемая форма.</param>
            public static void ExitReSize(Form Form)
            {
                int[] BS = new int[]
                    {
                    Form.Width,
                    Form.Height,
                    Form.Location.X,
                    Form.Location.Y
                    };
                System.Threading.Thread TH = new System.Threading.Thread(delegate ()
                { for (; ; ) if (Form.Height < 5) Environment.Exit(0); }); TH.Start();
                Size(Form, new Size(BS[0], 0), TransitionType.EaseInQuad, 10);
                Move(Form, new Point(BS[2], BS[3] + BS[1] / 2), TransitionType.EaseOutCubic, 10);
            }

            /// <summary>
            /// Плавное появление и исчезание сообщения.
            /// </summary>
            /// <param name="Form">Форма.</param>
            /// <param name="Duration">Скорость.</param>
            /// <param name="TimeWait">Продолжительность видимости.</param>
            public static void AlphaBlandMessage(Form Form, int Duration, int TimeWait)
            {
                Form.Show();
                Size(Form, new Size(320, 100), TransitionType.EaseInOutQuad, 15);

                System.Windows.Forms.Timer Hide = new System.Windows.Forms.Timer() { Interval = Duration };
                Hide.Tick += delegate (object O, EventArgs E)
                {
                    Form.Opacity -= Form.Opacity > 0 ? 0.1 : 0;
                    if (Form.Opacity < 0.1) { Hide.Stop(); Form.Dispose(); aSystem.NotifyCount--; }
                };

                System.Windows.Forms.Timer Wait = new System.Windows.Forms.Timer() { Interval = (TimeWait != 0) ? TimeWait : 360000 };
                Wait.Tick += delegate (object O, EventArgs E) { Wait.Stop(); Hide.Start(); };

                System.Windows.Forms.Timer Show = new System.Windows.Forms.Timer() { Interval = Duration };
                Show.Tick += delegate (object O, EventArgs E)
                {
                    Form.Opacity += Form.Opacity < 0.8 ? 0.1 : 0;
                    if (Form.Opacity > 0.9)
                    {
                        Show.Stop();
                        Wait.Start();

                    }
                };
                Show.Start();
            }
        }
    }
}
