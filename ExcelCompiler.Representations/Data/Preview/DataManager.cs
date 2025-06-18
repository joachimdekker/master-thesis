using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ExcelCompiler.Representations.Data.Preview;

public class DataManager : IDictionary<string, IDataRepository>
{
    private Dictionary<string, IDataRepository> _repositories;
    public DataManager(IEnumerable<IDataRepository> repositories)
    {
        _repositories = repositories.ToDictionary(r => r.Name);
    }

    public IDataRepository this[string name]
    {
        get => _repositories[name];
        set => _repositories[name] = value;
    }
    
    #region ReadOnlyDictionary

    public void Add(string key, IDataRepository value)
    {
        _repositories.Add(key, value);
    }

    public bool ContainsKey(string key) => _repositories.ContainsKey(key);
    public bool Remove(string key) => _repositories.Remove(key);

    public bool TryGetValue(string key, [NotNullWhen(true)] out IDataRepository? value)
        => _repositories.TryGetValue(key, out value);
    
    public ICollection<string> Keys => _repositories.Keys;
    
    public ICollection<IDataRepository> Values => _repositories.Values;


    public IEnumerator<KeyValuePair<string, IDataRepository>> GetEnumerator()
    {
        return _repositories.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<string, IDataRepository> item)
    {
        _repositories.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _repositories.Clear();
    }

    public bool Contains(KeyValuePair<string, IDataRepository> item) => _repositories.Contains(item);

    /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(KeyValuePair<string, IDataRepository>[] array, int arrayIndex)
    {
        foreach (var (index, value) in _repositories.Index())
        {
            array[index + arrayIndex] = value;
        }
    }

    public bool Remove(KeyValuePair<string, IDataRepository> item)
    {
        if (!_repositories.TryGetValue(item.Key, out var val) || val != item.Value) return false;
        return _repositories.Remove(item.Key);
    }

    public int Count => _repositories.Count;
    public bool IsReadOnly => false;

    #endregion
}