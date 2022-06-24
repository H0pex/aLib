#region • Usings

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Net.Cache;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp; // рекомендуемая версия: 106.13.0
using aLib.Utils;
using System.Threading.Tasks;

#endregion

namespace aLib.WebKit
{
    /// <summary>
    /// Работа с интернетом.
    /// </summary>
    public class aWeb
    {
        /// <summary>
        /// Реализация RestAPI обращений.
        /// </summary>
        public class RestAPI
        {
            /// <summary>
            /// Структура краткой инфомрации ответа сервера.
            /// </summary>
            public struct UniversalRestResponse
            {
                /// <summary>
                /// Текст ответа.
                /// </summary>
                public string PlainText { get; set; }

                /// <summary>
                /// Статус ответа сервера.
                /// </summary>
                public int StatusCode { get; set; }

                /// <summary>
                /// JSON структура ответа севрера для пользователской десериализации.
                /// </summary>
                public object CustomResponse { get; set; }
            }

            /// <summary>
            /// Основной домен запроса.
            /// </summary>
            public string Domain = default;

            /// <summary>
            /// Родительский контейнер ответа API по умолчанию.
            /// </summary>
            public string ParentResponseSection = default;

            /// <summary>
            /// Конструктор класса.
            /// </summary>
            /// <param name="Domain">Основной домен запроса с вложенными (если есть) роутами без параметров.</param>
            /// <param name="ParentResponseSection">Родительский контейнер ответа API по умолчанию.</param>
            public RestAPI(string Domain = null, string ParentResponseSection = "data") => (this.Domain, this.ParentResponseSection) = (Domain, ParentResponseSection);

            /// <summary>
            /// Запрос к серверу с возможными параметрами <paramref name="RequestParameters"/> и телом запроса <paramref name="RequestBody"/>, ожидается объект для сериализации, в случае POST запроса.
            /// </summary>
            /// <param name="RequestMethod">Тип запроса.</param>
            /// <param name="RequestParameters">Дополнительные параметры (без '?').</param>
            /// <param name="RequestBody">Тело для POST запроса; ожидается структура – проводится автоматическая JSON сериализация.</param>
            /// <param name="Messages">Требование выводить сообщения от сервера, если таковые ожидаются.</param>
            /// <param name="StatusOnly">Требование возвращать только статус ответа сервера в числовом формате.</param>
            /// <param name="SimpleText">Требование возвращать не сериализованный контенст ответа, требует наличие выходного типа string.</param>
            public T Exec<T>(
                Method RequestMethod,
                string RequestParameters = default,
                object RequestBody = default,
                bool Messages = false,
                bool StatusOnly = false,
                bool SimpleText = false)
            {
                /// Указывает протокол безопасности TLS 1.2.
                /// Протокол TLS 1.2 определяется в документе IETF RFC 5246.
                /// В операционных системах Windows это значение поддерживается, начиная с Windows 7.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var _client = new RestClient($"{Domain + (string.IsNullOrEmpty(RequestParameters) ? string.Empty : $"?{RequestParameters}")}");
                var _request = new RestRequest(RequestMethod).AddHeader("Content-Type", "application/json");

                /// Для POST/PUT запроса добавляем тело запроса.
                if ((RequestMethod == Method.POST || RequestMethod == Method.PUT) && RequestBody != null)
                    _request.AddJsonBody(JsonConvert.SerializeObject(RequestBody));

                /// Выполняем запрос на сервер.
                var response = _client.Execute(_request);

                /// Если нужно вернуть только статус ответа.
                if (StatusOnly && typeof(T) == typeof(int))
                    return (T)Convert.ChangeType((int)response.StatusCode, typeof(T));

                /// Если нужно вернуть содержимое ответа в виде plantext.
                if (SimpleText && typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(response.Content, typeof(T));

                /// Приводим ответ к виду JObject.
                var jresponse = JObject.Parse(response.Content);

                /// Если есть сообщения от сервера – выводим.                                
                if (Messages && !string.IsNullOrEmpty(jresponse["data"].Value<string>("message")))
                    Console.WriteLine($"{jresponse["data"]["message"]}");

                /// Приводим JSON ответ сервера к нужной структуре ответа.
                return JsonConvert.DeserializeObject<T>(jresponse["data"].ToString());
            }


            /// <summary>
            /// Запрос к серверу с возможными параметрами <paramref name="RequestParameters"/> и телом запроса <paramref name="RequestBody"/>, ожидается объект для сериализации, в случае POST запроса и получением универсального сжатого ответа.
            /// </summary>
            /// <param name="RequestMethod">Тип запроса.</param>
            /// <param name="RequestParameters">Дополнительные GET параметры (без '?').</param>
            /// <param name="RequestBody">Тело для POST запроса; ожидается структура – проводится автоматическая JSON сериализация.</param>
            public async Task<UniversalRestResponse> UExec<OutStruct>(
                Method RequestMethod = Method.GET,
                string RequestParameters = default,
                object RequestBody = default)
            {
                // Указывает протокол безопасности TLS 1.2.
                // Протокол TLS 1.2 определяется в документе IETF RFC 5246.
                // В операционных системах Windows это значение поддерживается, начиная с Windows 7.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var _urr = new UniversalRestResponse();
                var _client = new RestClient($"{Domain + (string.IsNullOrEmpty(RequestParameters) ? string.Empty : $"?{RequestParameters}")}");
                var _request = new RestRequest(RequestMethod).AddHeader("Content-Type", "application/json");

                // Для POST/PUT запроса добавляем тело запроса.
                if ((RequestMethod == Method.POST || RequestMethod == Method.PUT) && RequestBody != null)
                    _request.AddJsonBody(JsonConvert.SerializeObject(RequestBody));

                // Выполняем запрос на сервер.
                var response = _client.Execute(_request);

                _urr.StatusCode = (int)response.StatusCode;
                _urr.PlainText = response.Content.ToString();

                try
                {    
                    // Приводим ответ к виду JObject.
                    var jresponse = JObject.Parse(response.Content);
                    var _customContent = jresponse[ParentResponseSection].ToString();

                    _urr.CustomResponse = JsonConvert.DeserializeObject<OutStruct>(_customContent);
                    //Console.WriteLine($"Custom deserialize: OK");
                }
                catch
                {
                    _urr.CustomResponse = null;
                    //Console.WriteLine($"CustomResponse: deserialize is failed");
                }

                //Console.WriteLine($"URR: {JsonConvert.SerializeObject(_urr)}");
                return _urr;
            }
        }


        /// <summary>
        /// Работа с сайтом 2ip.ru
        /// НЕ ПРОВЕРЯЛОСЬ (последняя проверка 2016г.)
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
            request.Method = WebRequestMethods.Http.Get;
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
        /// Возвращает статус указанной страницы.
        /// </summary>
        /// <param name="Url">Адрес страницы.</param>
        /// <returns>Статус ответа сервера.</returns>
        public static int GetStatusCode(string Url)
        {
            try
            {
                var _client = new RestClient(Url);
                var _request = new RestRequest(Method.GET).AddHeader("Content-Type", "application/json");
                var response = _client.Execute(_request);
                return (int)response.StatusCode;
            }
            catch
            {
                return 400;
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
                MemoryStream _ms = new MemoryStream();
                objWebStream.CopyTo(_ms, 8192);
                return _ms;
            }
        }
    }
}