using SRTM.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SRTM.Sources
{
    public class SourceHelpers
    {
        /// <summary>
        /// Downloads a remote file and stores the data in the local one.
        /// </summary>
        public static bool Download(string local, string remote, bool logErrors = false)
        {
            var client = new HttpClient();
            return PerformDownload(client, local, remote, logErrors);
        }

        /// <summary>
        /// Downloads a remote file and stores the data in the local one. The given credentials are used for authorization.
        /// </summary>
        public static bool DownloadWithCredentials(NetworkCredential credentials, string local, string remote,
            bool logErrors = false)
        {
            try
            {
                string resource = remote;
                string urs = "https://urs.earthdata.nasa.gov";
                string username = credentials.UserName;
                string password = credentials.Password;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", @"EDL-YOUR_BEARER_TOKEN from nasa");
                CredentialCache cache = new CredentialCache();
                cache.Add(new Uri(urs), "Basic", new NetworkCredential(username, password));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource);
                request.Method = "GET";
                request.Credentials = cache;
                request.CookieContainer = new CookieContainer();
                request.PreAuthenticate = false;
                request.AllowAutoRedirect = true;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                long length = response.ContentLength;
                string type = response.ContentType;
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);


                // Process the stream data (e.g. save to file)

                using (var outputStream = File.OpenWrite(local))
                {
                    stream.CopyTo(outputStream);
                }


                // Tidy up

                stream.Close();
                reader.Close();
            }
            catch (Exception ex)
            { return false; }

            return true;
        }

        private static bool PerformDownload(HttpClient client, string local, string remote, bool logErrors = false)
        {
            var Logger = LogProvider.For<SourceHelpers>();

            try
            {
                if (File.Exists(local))
                {
                    File.Delete(local);
                }

                using (var stream = client.GetStreamAsync(remote).Result)
                using (var outputStream = File.OpenWrite(local))
                {
                    stream.CopyTo(outputStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                if (logErrors)
                {
                    Logger.ErrorException("Download failed.", ex);
                }
            }
            return false;
        }
    }
}
