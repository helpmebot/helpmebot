﻿namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;
    
    public class CategoryWatcherItem : EntityBase
    {
        public virtual string Title { get; set; }
        public virtual DateTime InsertTime { get; set; }
        public virtual string Watcher { get; set; }
    }
}