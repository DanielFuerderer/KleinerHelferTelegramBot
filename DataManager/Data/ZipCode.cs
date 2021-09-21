namespace Data.Data
{
  public class ZipCode
  {
    public ZipCode(int zipCode)
    {
      Value = zipCode;
    }

    public int Value { get; }

    public static implicit operator ZipCode(int zipCode)
    {
      return new ZipCode(zipCode);
    }

    public static implicit operator int(ZipCode zipCode)
    {
      return zipCode?.Value ?? 0;
    }

    public override string ToString()
    {
      return Value.ToString();
    }

    protected bool Equals(ZipCode other)
    {
      return Value == other.Value;
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

      return obj.GetType() == GetType() && Equals((ZipCode)obj);
    }

    public override int GetHashCode()
    {
      return Value;
    }
  }
}