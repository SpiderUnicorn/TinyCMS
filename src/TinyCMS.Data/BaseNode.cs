﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using PropertyChanged;
using TinyCMS.Interfaces;

namespace TinyCMS.Data
{

    [Serializable]
    public abstract class BaseNode : INode, INotifyPropertyChanged
    {
        public string Id { get; set; }

        [JsonIgnore]
        public string ParentId { get; set; }

        [JsonIgnore]
        public bool IsParsed { get; set; }
        public abstract string Type { get; }

        [SchemaType("tags", false)]
        public IList<string> Tags { get; set; }

        [SchemaType("children")]
        public ObservableCollection<INode> Children { get; set; } = new ObservableCollection<INode>();

        [field : NonSerialized]
#pragma warning disable 0067

        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore

    }
}
