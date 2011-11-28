﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.CrypWin.Helper
{
  /// <summary>
  /// DicHelper: creates new entry if not exists. 
  /// </summary>
  public static class DictionaryHelper
  {
    /// <summary>
    /// Gets or create an Dic entry. 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value for the given key.</returns>
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
    {
      TValue ret;
      if (!dictionary.TryGetValue(key, out ret))
      {
        ret = new TValue();
        dictionary[key] = ret;
      }
      return ret;
    }

  }
}
