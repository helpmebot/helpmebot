namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;
    
    public class CategoryItem : EntityBase
    {
        public virtual string Title { get; set; }
        public virtual DateTime InsertTime { get; set; }
        public virtual WatchedCategory Watcher { get; set; }
    }
}