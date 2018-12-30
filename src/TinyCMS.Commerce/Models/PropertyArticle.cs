﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TinyCMS.Commerce.Models
{
    public class PropertyArticle : IShopArticleWithProperties
    {
        public ObservableCollection<IProperty> Properties { get; set; }
        public string Name { get; set; }
        public string ArticleNr { get; set; }
        public float Price { get; set; }
        public float Tax { get; set; }

#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore
    }
}
