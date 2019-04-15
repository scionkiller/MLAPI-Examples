namespace Alpaca.Serialization
{

public interface IPoolable<T> : ICapacityConstruct<T>, System.IDisposable where T : class
{
	bool InUse();
	void SetInUse();
}

public interface ICapacityConstruct<T> where T : class
{
	// creates a copy of T with the same capacity (freshly constructed)
	T CapacityConstruct();
}

public interface Test
{
	void Write<T>( T value );
}

// this should be used with small capacity numbers
public class FixedPool<T> where T : class, IPoolable<T>
{
	T[] _pool;

	// pool will use the prototype as the first element, DON'T USE IT
	public FixedPool( T prototype, int capacity )
	{
		_pool = new T[capacity];
		_pool[0] = prototype;
		for( int i = 1; i < capacity; ++i )
		{
			_pool[i] = prototype.CapacityConstruct();
		}
	}

	public T Get()
	{
		for( int i = 0; i < _pool.Length; ++i )
		{
			if( !_pool[i].InUse() )
			{
				_pool[i].SetInUse();
				return _pool[i];
			}
		}

		Log.Error( "FixedPool of " + typeof(T).Name + " with capacity " + _pool.Length + " has been exhausted!" );
		return null;
	}
}

} // namespace Alpaca.Serialization