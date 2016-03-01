using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickDownloader
{
    class Flickr
    {
        private string Key = "ff7eff8dc769f13b5c59070f2420745c";
        private string Secret = "62941e2546ae47c0";
        private string User = "harupl";

        private string ApiUrl = @"https://api.flickr.com/services/rest/";
        public Flickr()
        {
            var request = getPhotos("72157661540487564");
            Console.WriteLine(request);
        }

        private string BuildRequest(string method, IDictionary<string, string> parameters)
        {
            if (String.IsNullOrEmpty(method))
            {
                Console.WriteLine("ERROR: SendRequest - null 'method' parameter");
                return "";
            }
            else
            {
                var builder = new StringBuilder();

                builder.Append(ApiUrl);
                builder.Append("?method=");
                builder.Append(method);

                foreach (var key in parameters)
                {
                    string attr = "&" + key.Key + "=" + key.Value;
                    builder.Append(attr);
                }

                return builder.ToString();
            }
        }

        private string getPhotos(string gallery_id)
        {
            if (String.IsNullOrEmpty(gallery_id))
            {
                Console.WriteLine("ERROR: getPhotos - null 'gallery_id' parameter");
                return "";
            }
            else
            {
                return BuildRequest("flickr.galleries.getPhotos", new Dictionary<string, string> { { "api_key", Key }, { "gallery_id", gallery_id } });
            }
        }

        private void GetPhotosBuildUrl()
        {
        }
    }
}
