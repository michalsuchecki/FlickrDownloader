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
    enum FlickrImageSizes
    {
        Flickr_BiggestSize, // Default: download biggest image
        Flickr_OriginalSize,
        Flickr_LargeSize


    };

    class Flickr
    {
        Dictionary<FlickrImageSizes, string> _ImageSizeSufix = new Dictionary<FlickrImageSizes, string>()
        {
            { FlickrImageSizes.Flickr_LargeSize, "_b" },
            { FlickrImageSizes.Flickr_OriginalSize, "_o" },
        };

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

        public IEnumerable<PhotoSizesModel> GetPhotoSizes(string photoId)
        {
            if (String.IsNullOrEmpty(photoId)) return null;

            var request = FetchPhotoSizes(photoId);

            var sizes = GetXMLData(request, "size");

            if (sizes != null)
            {
                var items = (from size in sizes
                             select new PhotoSizesModel
                             {
                                 Label = size.Attribute("label")?.Value ?? String.Empty,
                                 Width = size.Attribute("width")?.Value ?? String.Empty,
                                 Height = size.Attribute("height")?.Value ?? String.Empty,
                                 Source = size.Attribute("source")?.Value ?? String.Empty,
                                 Url = size.Attribute("url")?.Value ?? String.Empty,
                                 Media = size.Attribute("media")?.Value ?? String.Empty,
                             });

                return items;
            }

            return null;
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

        public IEnumerable<string> GetUserPhotos(string userId /*, FlickrImageSizes size = FlickrImageSizes.Flickr_BiggestSize*/)
        {
            if (String.IsNullOrEmpty(userId)) return null;

            var request = FetchUserPhotos(userId);

            var data = GetXMLData(request, "photo");

            if (data != null)
            {
                var photos = (from d in data
                              select new PhotosModel
                              {
                                  Id = d.Attribute("id")?.Value ?? String.Empty,
                                  Owner = d.Attribute("owner")?.Value ?? String.Empty,
                                  Secret = d.Attribute("secret")?.Value ?? String.Empty,
                                  Server = d.Attribute("server")?.Value ?? String.Empty,
                                  Farm = d.Attribute("farm")?.Value ?? String.Empty,
                                  Title = d.Attribute("title")?.Value ?? String.Empty,
                              });

                List<string> urls = new List<string>();

                if (photos != null)
                {
                    foreach (var photo in photos)
                    {
                        var sizes = GetPhotoSizes(photo.Id);

                        var url = (from s in sizes
                                   select s.Source)
                                  .LastOrDefault();

                        urls.Add(url);
                    }
                }
                return urls;
            }
            return null;
        }

        private string FetchPhotoSizes(string photoId)
        {
            if (String.IsNullOrEmpty(photoId)) return "";

            return BuildRequest("flickr.photos.getSizes", new Dictionary<string, string> { { "api_key", Global.FlickrApiKey }, { "photo_id", photoId } }); ;
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

        private IEnumerable<XElement> GetXMLData(string source, string elementName)
        {
            var doc = GetXMLDocument(source);
            if (doc != null)
            {
                var result = doc.Descendants(elementName);
                return result;
            }
            return null;
        }
    }
}
