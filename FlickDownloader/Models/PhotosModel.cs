using System;

namespace FlickDownloader.Models
{
    class PhotosModel
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string Farm { get; set; }

        public string Server { get; set; }

        public string Secret { get; set; }

        public string Owner { get; set; }

        public string Title { get; set; }
    }
}
