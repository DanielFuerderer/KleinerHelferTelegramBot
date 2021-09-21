using System;

namespace Data.Entities
{
  public class Person : IdBase<int>
  {
    public Person(int id) : base(id) { }

    protected Person() { }

    public string Name { get; set; }

    public DateTime Birthday { get; set; }
  }
}