﻿using System;
using System.ComponentModel;
namespace TinyCMS.Data.Nodes
{
    [Serializable]
    public class Row : BaseNode
    {
        public override string Type => "row";
    }
}
