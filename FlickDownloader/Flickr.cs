using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;

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
            //Console.WriteLine(request);
            var data = GetWebContent(request);

            ParseXMLDocument(data);
            //Console.WriteLine(data);
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

        private string GetWebContent(string uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                return "";
            }
            else
            {
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;

                var data = client.DownloadString(uri);

                if (String.IsNullOrEmpty(data))
                {
                    return "";
                }
                else
                {
                    return data;
                }
            }
        }

        private void ParseXMLDocument(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            var rsp_node = doc.SelectSingleNode("/rsp");

            if (rsp_node != null && rsp_node.Attributes.Count > 0)
            {
                var attr = rsp_node.Attributes.Item(0);

                if (attr.Name == "stat")
                {
                    if (attr.Value == "ok")
                    {
                        // TODO: Parse document
                    }
                    else
                    {
                        var error = rsp_node.SelectSingleNode("err");

                        var error_msg = "Response Error: ";
                        foreach (XmlAttribute err_attr in error.Attributes)
                        {
                            error_msg += err_attr.Value + " ";
                        }
                        Console.WriteLine(error_msg);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR - ParseXMLDocument - missing 'stat' attribute");
                    return;
                }

            }

            //Console.WriteLine(rsp_node.InnerText);
        }
    }
}
