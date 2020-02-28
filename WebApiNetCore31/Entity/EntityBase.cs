using System;

namespace Entity
{
    public class EntityBase<TKey>
    {
        public TKey Id { get; set; }
    }
}
