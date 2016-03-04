using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FlickDownloader
{
    class Flickr
    {
        private string Key = "ff7eff8dc769f13b5c59070f2420745"; // ff7eff8dc769f13b5c59070f2420745c
        //private string Secret = "62941e2546ae47c0";
        //private string User = "harupl";

        private string ApiUrl = @"https://api.flickr.com/services/rest/";
        public Flickr()
        {
            var request = getPhotos("72157661540487564");

            ParseXMLDocument(request);
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

        private void ParseXMLDocument(string uri)
        {
            XDocument doc = XDocument.Load(uri);

            var status = doc.Element("rsp");
            var status_attr = status.Attribute("stat");

            if (status_attr != null && status_attr.Value.ToLower() == "ok")
            {
            }
            else
            {
                var error =status.Element("err");

                if (error != null)
                {
                    var error_msg = "Response Error:";
                    foreach (var attr in error.Attributes())
                    {
                        error_msg += " " + attr.Value;
                    }

                    Console.WriteLine(error_msg);
                }
            }

            Console.WriteLine("break");
        }
    }
}
