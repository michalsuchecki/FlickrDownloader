using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FlickDownloader
{
    class Flickr
    {
        private string Key = "ff7eff8dc769f13b5c59070f2420745c"; // ff7eff8dc769f13b5c59070f2420745c
        //private string Secret = "62941e2546ae47c0";
        //private string User = "harupl";

        private string ApiUrl = @"https://api.flickr.com/services/rest/";
        public Flickr()
        {
            GetPhotos("72157661540487564");
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

        private string BuildPhotoUrl(string farm, string server, string id, string secret)
        {
            //https://farm{farm-id}.staticflickr.com/{server-id}/{id}_{o-secret}_o.(jpg|gif|png)
            return String.Format("https://farm{0}.staticflickr.com/{1}/{2}_{3}.jpg",farm, server, id, secret);
        }

        public void GetPhotos(string gallery_id)
        {
            var request = getPhotos(gallery_id);

            if (!String.IsNullOrEmpty(request))
            {
                var doc = GetXMLDocument(request);

                if (doc != null)
                {
                    var photos = doc.Descendants("photo");

                    var photos_list = (from photo in doc.Descendants("photo")
                             select new
                             {
                                 id = photo.Attribute("id").Value,
                                 secret = photo.Attribute("secret").Value,
                                 server = photo.Attribute("server").Value,
                                 farm = photo.Attribute("farm").Value
                             }).ToList();

                    foreach (var p in photos_list)
                    {
                        var image_url = BuildPhotoUrl(p.farm, p.server, p.id, p.secret);
                        Console.WriteLine(image_url);
                    }

                }

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

        private XDocument GetXMLDocument(string uri)
        {
            XDocument doc = XDocument.Load(uri);

            var status = doc.Element("rsp");
            var status_attr = status.Attribute("stat");

            if (status_attr != null && status_attr.Value.ToLower() == "ok")
            {
                //return status; 
                return doc;
            }
            else
            {
                var error = status.Element("err");

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

            return null;
        }
    }
}
