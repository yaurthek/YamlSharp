﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yaml.Model;

namespace Yaml.Parsing
{
    internal class AnchorDictionary
    {
        Dictionary<string, YamlNode> Items = new Dictionary<string, YamlNode>();

        struct RewindInfo
        {
            public RewindInfo(string anchor_name, YamlNode old_value)
            {
                this.anchor_name = anchor_name;
                this.old_value = old_value;
            }
            public string anchor_name;
            public YamlNode old_value;
        }
        Stack<RewindInfo> ItemsToRewind = new Stack<RewindInfo>();

        Func<string, bool> error;

        public AnchorDictionary(Func<string, bool> error)
        {
            this.error = error;
        }
        bool Error(string message)
        {
            return error(message);
        }
        public YamlNode this[string anchor_name]
        {
            get
            {
                if (!Items.ContainsKey(anchor_name))
                    Error($"Anchor {anchor_name} has not been registered.");
                return Items[anchor_name];
            }
        }
        public void Add(string anchor_name, YamlNode node)
        {
            if (Items.ContainsKey(anchor_name))
            {
                // override an existing anchor
                ItemsToRewind.Push(new RewindInfo(anchor_name, this[anchor_name]));
                Items[anchor_name] = node;
            }
            else
            {
                ItemsToRewind.Push(new RewindInfo(anchor_name, null));
                Items.Add(anchor_name, node);
            }
        }
        public int RewindDepth
        {
            get { return ItemsToRewind.Count; }
            set
            {
                if (RewindDepth < value)
                    throw new ArgumentOutOfRangeException();
                while (value < RewindDepth)
                {
                    var rewind_item = ItemsToRewind.Pop();
                    if (rewind_item.old_value == null)
                    {
                        Items.Remove(rewind_item.anchor_name);
                    }
                    else
                    {
                        Items[rewind_item.anchor_name] = rewind_item.old_value;
                    }
                }
            }
        }
    }
}
