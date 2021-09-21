using System.Collections.Generic;

namespace Data.Entities
{
  public class IdBase<T>
  {
    protected IdBase() { }

    public IdBase(T id)
    {
      Id = id;
    }

    public T Id { get; set; }

    protected bool Equals(IdBase<T> other)
    {
      return EqualityComparer<T>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      if (obj.GetType() != GetType())
      {
        return false;
      }

      return Equals((IdBase<T>)obj);
    }

    public override int GetHashCode()
    {
      return EqualityComparer<T>.Default.GetHashCode(Id);
    }
  }
}