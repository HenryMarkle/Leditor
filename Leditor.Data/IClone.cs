namespace Leditor.Data;

public interface IClone<T> where T : class { T Clone(); }