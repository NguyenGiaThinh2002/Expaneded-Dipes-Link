using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DynamicDataRowViewModel : ObservableObject
{
    private readonly Dictionary<string, object> _data;

    public DynamicDataRowViewModel(IEnumerable<string> keys)
    {
        _data = new Dictionary<string, object>();
        foreach (var key in keys)
        {
            _data[key] = null;
        }
    }

    public object this[string key]
    {
        get => _data.ContainsKey(key) ? _data[key] : null;
        set
        {
            if (_data.ContainsKey(key))
            {
                _data[key] = value;
                OnPropertyChanged(key);
            }
            else
            {
                _data.Add(key, value);
                OnPropertyChanged(key);
            }
        }
    }

    public IEnumerable<string> Keys => _data.Keys;


    public bool ContainsKey(string key)
    {
        return _data.ContainsKey(key);
    }

    public object GetValue(string key)
    {
        return _data.ContainsKey(key) ? _data[key] : null;
    }
}
