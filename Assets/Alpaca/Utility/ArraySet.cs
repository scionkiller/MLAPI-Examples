// This is a simple, linear-searching Set of tuples <Key, Value>
//
// Searches are simple O(n) comparison of equality on the Key.
// Can iterate over container in O(n)
// Attempting to add the same Key will overwrite the previous value (keys are unique)
//
// Null values are not allowed to be stored, in order to enable this paradigm:
// someArraySet[key] = null; // treated like someArraySet.Delete(key);
//
// Warnings:
// Use small capacities only (less than 100, approximately)
public class ArraySet<K, V> where K : struct where V : class
{
	V[] _value;
	K[] _key;
	int _count;

	public ArraySet( int capacity )
	{
		_value = new V[capacity];
		_key   = new K[capacity];
		_count = 0;
	}

	// iteration interface
	public int GetCount() { return _count; }
	public V GetAt( int index ) { return _value[index]; }

	// dictionary-like lookup interface
	public V Get( K key )
	{
		for( int i = 0; i < _count; ++i )
		{
			if( _key[i].Equals(key) ) { return _value[i]; }
		}
			
		return null;
	}

	// returns true if it actually added a value
	public bool Add( K key, V value )
	{
		System.Diagnostics.Debug.Assert( value != null );

		for( int i = 0; i < _count; ++i )
		{
			if( _key[i].Equals(key) )
			{
				_value[i] = value;
				return false;
			}
		}

		System.Diagnostics.Debug.Assert( _count < _value.Length );

		_key[_count] = key;
		_value[_count] = value;
		++_count;

		return true;
	}

	// returns true if it actually removed a value
	public bool Remove( K key )
	{
		bool found = false;
		for( int i = 0; i < _count; ++i )
		{
			if( _key[i].Equals(key) )
			{
				found = true;
				// stomp value with last value in array (might be same value)
				_value[i] = _value[_count-1];
				_key[i] = _key[_count-1];
			}
		}

		if( found )
		{
			// remove last value in array
			--_count;
			_value[_count] = null;
			return true;
		}

		return false;
	}

	public V this[K key]
	{
		get
		{
			return Get( key );
		}
		set
		{
			if( value != null )
			{
				Add( key, value );
			}
			else
			{
				Remove( key );
			}
		}
	}
}