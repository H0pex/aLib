#region • Usings
using System.Threading;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System;
using System.Speech.Synthesis;

using NAudio.Wave;
using YandexDisk.Client.Http;
using YandexDisk.Client.Clients;
#endregion

namespace aLib.WebKit
{
    /// <summary>
    /// Взаимодействие с API Yandex Disk
    /// </summary>
    public class aDisk
    {
        /// <summary>
        /// Приватный токен диска.
        /// </summary>
        protected static string Token { get; private set; }

        /// <summary>
        /// Конструктор класса YD.
        /// </summary>
        /// <param name="Token">Приватный токен диска.</param>
        public aDisk(string Token) => aDisk.Token = Token;

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

    /// <summary>
    /// Класс Yandex речи
    /// </summary>
    public class aSpeech
    {
        /// <summary>
        /// Стандартный голос
        /// </summary>
        static SpeechSynthesizer speechSynthesizerObj;
        internal static void NewSpeech(string Text)
        {
            if (speechSynthesizerObj != null) speechSynthesizerObj.Dispose();
            if (!string.IsNullOrWhiteSpace(Text))
            {
                speechSynthesizerObj = new SpeechSynthesizer();
                speechSynthesizerObj.Rate = 10;
                speechSynthesizerObj.SelectVoice(speechSynthesizerObj.GetInstalledVoices()[1].VoiceInfo.Name);
                speechSynthesizerObj.SpeakAsync(Text);
            }
        }

        /// <summary>
        /// Список голосов
        /// </summary>
        internal enum Speaker
        {
            alyss,
            jane,
            oksana,
            omazh,
            zahar,
            ermil
        }

        /// <summary>
        /// Список интонаций
        /// </summary>
        internal enum Emotion
        {
            good,
            evil,
            neutral
        }

        /// <summary>
        /// Функция вызова голосовой озвучки указанного текста
        /// </summary>
        /// <param name="Text">Воспроизводимый текст</param>
        /// <param name="Speaker">Используемый голос</param>
        /// <param name="Emotion">Интонация голоса</param>
        internal static void NewSpeech(string Text, Speaker Speaker, Emotion Emotion)
        {
            string Request = string.Format(
                    "https://tts.voicetech.yandex.net/generate?" +
                    "text={0}" +
                    "&speed=1.0&format=mp3&lang=ru-RU&speaker={1}" +
                    "&emotion={2}" +
                    "&key=f4c44bad-54dc-44cc-8617-14eefdd9dbd6",
                    Text,
                    Speaker.ToString(),
                    Emotion.ToString());

            Thread Thread = new Thread(delegate ()
            {
                try
                {
                    using (Stream ms = new MemoryStream())
                    {
                        using (Stream Stream = WebRequest.Create(Request).GetResponse().GetResponseStream())
                        {
                            byte[] Buffer = new byte[32768];
                            int Read;
                            while ((Read = Stream.Read(Buffer, 0, Buffer.Length)) > 0) ms.Write(Buffer, 0, Read);
                        }

                        ms.Position = 0;
                        using (WaveStream blockAlignedStream =
                            new BlockAlignReductionStream(
                                WaveFormatConversionStream.CreatePcmStream(
                                    new Mp3FileReader(ms))))
                        {
                            using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                            {
                                waveOut.Init(blockAlignedStream);
                                waveOut.Play();
                                while (waveOut.PlaybackState == PlaybackState.Playing) Thread.Sleep(100);
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
            Thread.Start();
        }
    }
}
