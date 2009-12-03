﻿/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase
{
  /// <summary>
  /// This optional attribute can be used to display author information in the 
  /// settings pane. 
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class AuthorAttribute : Attribute
  {
    public readonly string Author;
    public readonly string Email;
    public readonly string Institute;
    public readonly string URL;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorAttribute"/> class.
    /// </summary>
    /// <param name="author">The author.</param>
    /// <param name="email">The email - optional, validated with regex.</param>
    /// <param name="institute">The institute - optional.</param>
    /// <param name="url">The URL - optional, validated with regex.</param>
    public AuthorAttribute(string author, string email, string institute, string url)
    {
      this.Author = author;
      this.Email = email;
      this.Institute = institute;
      this.URL = url;
    }
  }
}
