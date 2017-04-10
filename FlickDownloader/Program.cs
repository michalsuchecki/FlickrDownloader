using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlickDownloader
{
    class Program
    {
        static void DownloadPhoto(string url)
        {
            if (String.IsNullOrEmpty(url)) return;

            var filename = Path.GetFileName(url);

            WebRequest request = WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (BinaryReader stream = new BinaryReader(response.GetResponseStream()))
                {
                    using (var file = File.Create(filename))
                    {
                        var buffer = stream.ReadBytes((int)response.ContentLength);
                        file.Write(buffer, 0, buffer.Length);
                    }
                }
                Console.WriteLine($"Downloading file {filename}");
            }
            else
            {
                Console.WriteLine($"Error can't download {filename}");
            }
        }


        // TODO: add checking if album exist...

        static void Main(string[] args)
        {
            var albumId = "72157649192671893"; // nature

            Flickr f = new Flickr();

            var images = f.GetAlbumPhotos(albumId);


            foreach (var img in images)
            {
                DownloadPhoto(img);
                Console.WriteLine(img);
            }
            
            Console.ReadKey();
        }
    }
}
