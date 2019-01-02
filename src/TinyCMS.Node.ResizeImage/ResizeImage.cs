using System;
using System.ComponentModel;
using TinyCMS.Data;

namespace TinyCMS.Node.ResizeImage
{
    [Serializable]
    public class ResizeImage : BaseNode, INotifyPropertyChanged
    {

        public override string Type => "resizeimage";

        private string _sourceUrl;
        public string SourceUrl
        {
            get => _sourceUrl;
            set
            {
                if (value != _sourceUrl)
                {
                    _sourceUrl = value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        FetchDownloadUrl();
                    }
                }
            }
        }

        private void FetchDownloadUrl()
        {
            ImageResizeHelper.Instance.GetResizedImageUrl(_sourceUrl, (data) =>
            {
                DownloadUrl = data.Url;
                //SourceUrl = string.Empty;
            });
        }

        [EditorType("none")]
        public string DownloadUrl { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Alt { get; set; }
    }
}
