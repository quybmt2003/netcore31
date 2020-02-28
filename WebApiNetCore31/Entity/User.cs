using System;

namespace Entity
{
    public class User : EntityBase<string>
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
