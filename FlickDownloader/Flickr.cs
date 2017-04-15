using FlickDownloader.Models;
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
        public Flickr()
        {
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

                builder.Append(Global.FlickrApiUrl);
                builder.Append("?method=");
                builder.Append(method);

                foreach (var key in parameters)
                {
                    string attr = "&" + key.Key + "=" + key.Value;
                    builder.Append(attr);
                }

#if DEBUG
                Console.WriteLine(builder.ToString());
#endif
                return builder.ToString();
            }
        }

        public string BuildPhotoUrl(string farm, string server, string id, string secret)
        {
            //https://farm{farm-id}.staticflickr.com/{server-id}/{id}_{o-secret}_o.(jpg|gif|png)
            return String.Format($"https://farm{farm}.staticflickr.com/{server}/{id}_{secret}.jpg");
        }

        public IEnumerable<string> GetAlbumPhotos(string album_id)
        {
            var request = FetchAlbumPhotos(album_id);

            if (!String.IsNullOrEmpty(request))
            {
                var doc = GetXMLDocument(request);

                if (doc != null)
                {
                    var photos = doc.Descendants("photo");

                    if (photos != null)
                    {
                        var photos_list = (from photo in photos
                                           select new PhotosModel
                                           {
                                               Url = photo.Attribute("url_o")?.Value ?? String.Empty,
                                               Id = photo.Attribute("id")?.Value ?? String.Empty,
                                               Secret = photo.Attribute("secret")?.Value ?? String.Empty,
                                               Server = photo.Attribute("server")?.Value ?? String.Empty,
                                               Farm = photo.Attribute("farm")?.Value ?? String.Empty
                                           });

                        var ImageLinks = new List<string>();

                        foreach (var p in photos_list)
                        {
                            var url = "";
                            if (String.IsNullOrEmpty(p.Url))
                            {
                                url = BuildPhotoUrl(p.Farm, p.Server, p.Id, p.Secret);
                            }
                            else
                            {
                                url = p.Url;
                            }

                            ImageLinks.Add(url);
                        }
                        return ImageLinks;
                    }
                }
            }

            return null;
        }

        public IEnumerable<string> GetUserPhotos(string userId)
        {
            var request = FetchUserPhotos(userId);

            if (!String.IsNullOrEmpty(request))
            {
                var doc = GetXMLDocument(request);

                if (doc != null)
                {
                    var photos = doc.Descendants("photo");

                    if (photos != null)
                    {

                        var photos_list = (from photo in photos
                                           select new PhotosModel
                                           {
                                               Id = photo.Attribute("id")?.Value ?? String.Empty,
                                               Owner = photo.Attribute("owner")?.Value ?? String.Empty,
                                               Secret = photo.Attribute("secret")?.Value ?? String.Empty,
                                               Server = photo.Attribute("server")?.Value ?? String.Empty,
                                               Farm = photo.Attribute("farm")?.Value ?? String.Empty,
                                               Title = photo.Attribute("title")?.Value ?? String.Empty,
                                           });

                        var ImageLinks = new List<string>();

                        foreach (var p in photos_list)
                        {
                            var url = BuildPhotoUrl(p.Farm, p.Server, p.Id, p.Secret);
                            ImageLinks.Add(url);
                        }

                        return ImageLinks;
                    }
                }
            }

            return null;
        }

        private string FetchUserPhotos(string userId)
        {
            if (String.IsNullOrEmpty(userId)) return "";

            return BuildRequest("flickr.people.getPhotos", new Dictionary<string, string> { { "api_key", Global.FlickrApiKey }, { "user_id", userId } });
        }

        private string FetchAlbumPhotos(string album_id)
        {
            if (String.IsNullOrEmpty(album_id))
            {
                Console.WriteLine("ERROR: getAlbumPhotos - null 'album_id' parameter");
                return "";
            }
            else
            {
                return BuildRequest("flickr.photosets.getPhotos", new Dictionary<string, string> { { "api_key", Global.FlickrApiKey }, { "photoset_id", album_id }, { "extras", "url_o" } /*, { "user_id", User }*/ });
            }
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
