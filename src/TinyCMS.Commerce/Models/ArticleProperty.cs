using System.ComponentModel;

namespace TinyCMS.Commerce.Models
{
    public class ArticleProperty : IProperty
    {
        public string Value { get; set; }
        public string Key { get; set; }
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore
    }
}
