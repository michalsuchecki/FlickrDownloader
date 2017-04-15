using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickDownloader
{
    class Global
    {
        public static string FlickrApiKey { get; } = "YOUR_API_KEY";
        public static string FlickrApiUrl { get; } = @"https://api.flickr.com/services/rest/";
    }
}
