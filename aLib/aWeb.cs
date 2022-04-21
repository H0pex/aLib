#region • Usings

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Net.Cache;

using Newtonsoft.Json.Linq;

#endregion

namespace aLib
{
    /// <summary>
    /// Работа с интернетом.
    /// </summary>
    public class aWeb
    {
        /// <summary>
        /// Работа с сайтом 2ip.ru
        /// НЕ ПРОВЕРЯЛОСЬ
        /// </summary>
        private static class Parsing2ip
        {
            static string
                    Cut,
                    Site = GetContents("http://2ip.ru");

            /// <summary>
            /// Возвращает ip пользователя.
            /// </summary>
            public static string Ip()
            {
                Cut = Site.Remove(0, Site.IndexOf("d_clip_button") + 15);
                Cut = Cut.Remove(Cut.IndexOf("</"));
                return aEncryptions.UTF8.ToString(Cut);
            }

            /// <summary>
            /// Возвращает страну и город пребывания пользователя.
            /// </summary>
            public static string Location()
            {
                Cut = Site.Remove(0, Site.IndexOf(aEncryptions.UTF8.ToString("<th>Откуда вы:")));
                Cut = Cut.Remove(0, Cut.IndexOf("<a"));
                Cut = Cut.Remove(0, Cut.IndexOf(">") + 1);
                Cut = Cut.Remove(Cut.IndexOf("</a>"));
                return aEncryptions.UTF8.ToString(Cut);
            }

            /// <summary>
            /// Возвращает имя провайдера пользователя.
            /// </summary>
            public static string Provider()
            {
                Cut = Site.Remove(0, Site.IndexOf(aEncryptions.UTF8.ToString("<th>Ваш провайдер:")));
                Cut = Cut.Remove(0, Cut.IndexOf("<a"));
                Cut = Cut.Remove(0, Cut.IndexOf(">") + 1);
                Cut = Cut.Remove(Cut.IndexOf("</a>"));
                return aEncryptions.UTF8.ToString(Cut.Trim());
            }
        }

        /// <summary>
        /// Соединение через сокет.
        /// </summary>
        /// <param name="Server">Сервер соединения.</param>
        /// <param name="Port">Порт соединения.</param>
        private static Socket ConnectSocket(string Server, int Port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;
            hostEntry = Dns.GetHostEntry(Server);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, Port);
                Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }

        /// <summary>
        /// Запрос на ответ через соккетное соединение.
        /// </summary>
        /// <param name="Server">Сервер соединения.</param>
        /// <param name="Port">Порт соединения.</param>
        /// <param name="Url">Адрес соединения.</param>
        private static string SocketSendReceive(string Server, int Port, string Url)
        {
            string request =
                    "GET /" +
                    Url + " HTTP/1.1\r\nHost: " +
                    Server + "\r\nConnection: Close\r\n\r\n";
            byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            byte[] bytesReceived = new byte[256];
            Socket s = ConnectSocket(Server, Port);
            if (s == null) return ("Connection failed");
            s.Send(bytesSent, bytesSent.Length, 0);
            int bytes = 0;
            string page = "Default HTML page on " + Server + ":\r\n";
            do
            {
                bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                page = page + Encoding.UTF8.GetString(bytesReceived, 0, bytes);
            } while (bytes > 0);

            return page.Remove(0, page.LastIndexOf('\n') + 1).Replace("\n", "");
        }

        /// <summary>
        /// Метод обращения к REST API.
        /// </summary>
        public enum RequestMethod
        {
            GET,
            POST,
            DELETE
        }

        /// <summary>
        /// Получение ответа по запросу из Url.
        /// </summary>
        /// <param name="Url">Адрес соединения.</param>
        public static string GetContents(string Url, RequestMethod Method = RequestMethod.GET)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = /*Method.ToString(); */ WebRequestMethods.Http.Get;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
            request.ProtocolVersion = HttpVersion.Version11;
            request.AllowAutoRedirect = true;
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            request.Headers = new WebHeaderCollection();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream(), Encoding.Default).ReadToEnd();
            }
            catch (Exception ex)
            {
                return $"No connection to the server.\nURL: {Url}\n{ex.Message}";
            }
        }

        /// <summary>
        /// Получение потока по запросу.
        /// </summary>
        /// <param name="Url">Адрес запроса</param>
        public static Stream GetStream(string Url)
        {
            using (WebResponse wrFileResponse = WebRequest.Create(Url).GetResponse())
            using (Stream objWebStream = wrFileResponse.GetResponseStream())
            {
                MemoryStream _MS = new MemoryStream();
                objWebStream.CopyTo(_MS, 8192);
                return _MS;
            }
        }
    }
}
