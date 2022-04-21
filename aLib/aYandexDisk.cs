#region • Usings

using System.Threading;

using YandexDisk.Client.Http;
using YandexDisk.Client.Clients;

#endregion

namespace aLib
{
    /// <summary>
    /// Взаимодействие с API Yandex Disk
    /// </summary>
    public class aYandexDisk
    {
        /// <summary>
        /// Приватный токен диска.
        /// </summary>
        protected static string Token { get; private set; }

        /// <summary>
        /// Конструктор класса YD.
        /// </summary>
        /// <param name="Token">Приватный токен диска.</param>
        public aYandexDisk(string Token) => aYandexDisk.Token = Token;

        /// <summary>
        /// Ассинхронная выгрузка на диск
        /// </summary>
        /// <param name="DiskPath">Путь на диске с расширением.</param>
        /// <param name="LocalPath">Локальный путь с расширением.</param>
        internal static async void UpLoad(string DiskPath, string LocalPath) =>
                await new DiskHttpApi(Token).Files.UploadFileAsync(DiskPath, false, LocalPath, CancellationToken.None);

        /// <summary>
        /// Ассинхронная загрузка с диска
        /// </summary>
        /// <param name="DiskPath">Путь на диске с расширением.</param>
        /// <param name="LocalPath">Локальный путь с расширением.</param>
        internal static async void DownLoad(string DiskPath, string LocalPath) =>
                await new DiskHttpApi(Token).Files.DownloadFileAsync(DiskPath, LocalPath);
    }
}
