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

namespace Cryptool.Core
{
    [Serializable]
    public class UnknownFileFormatException : Exception
    {
        public UnknownFileFormatException() { }
        public UnknownFileFormatException(string message) : base(message) { }
        public UnknownFileFormatException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class TypeLoadException : Exception
    {
        public TypeLoadException() { }
        public TypeLoadException(string message) : base(message) { }
        public TypeLoadException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class StoreAddingException : Exception
    {
        public StoreAddingException() { }
        public StoreAddingException(string message) : base(message) { }
        public StoreAddingException(string message, Exception inner) : base(message, inner) { }
    }


    [global::System.Serializable]
    public class AssemblyNotSignedException : Exception
    {
        public AssemblyNotSignedException() { }
        public AssemblyNotSignedException(string message) : base(message) { }
        public AssemblyNotSignedException(string message, Exception inner) : base(message, inner) { }
    }
}
