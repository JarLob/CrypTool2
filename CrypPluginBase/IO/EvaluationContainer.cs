/*
   Copyright 2009-2010 Matthäus Wander, University of Duisburg-Essen

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
using System.Numerics;
namespace Cryptool.PluginBase.IO
{
    /// <summary>
    /// Create a container to pass evaluation data with different members to another CT2 plugin.
    /// 
    /// <para></para>
    /// </summary>
    public class EvaluationContainer
    {
        #region Fields and constructors

        private TimeSpan _runtime;
        private BigInteger _decryptions;
        private BigInteger _restarts;
        // remove if always equal to _decryptions
        private BigInteger _keys;
        private BigInteger _populationSize;
        private BigInteger _tabuSetSize;

        /// <summary>
        /// Init empty EvaluationContainer
        /// </summary>
        public EvaluationContainer()
        {
        }

        /// <summary>
        /// Init EvaluationContainer
        /// </summary>
        public EvaluationContainer(TimeSpan t, BigInteger decryptions, BigInteger restarts)
        {
            this._runtime = t;
            this._decryptions = decryptions;
            this._restarts = restarts;
        }

        /// <summary>
        /// Init EvaluationContainer
        /// </summary>
        public EvaluationContainer(TimeSpan t, BigInteger decryptions, BigInteger restarts, BigInteger keys)
        {
            this._runtime = t;
            this._decryptions = decryptions;
            this._restarts = restarts;
            this._keys = keys;
        }

        /// <summary>
        /// Init EvaluationContainer
        /// </summary>
        public EvaluationContainer(BigInteger decryptions, BigInteger restarts)
        {
            this._decryptions = decryptions;
            this._restarts = restarts;
        }

        /// <summary>
        /// Init EvaluationContainer
        /// </summary>
        public EvaluationContainer(BigInteger decryptions, BigInteger restarts, BigInteger populationSize)
        {
            this._decryptions = decryptions;
            this._restarts = restarts;
            this._populationSize = populationSize;
        }

        #endregion

        #region Public properties

        public bool hasValueSet
        {
            get
            {
                if (hasRuntime ||
                    hasDecryptions ||
                    hasRestarts ||
                    hasKeys ||
                    hasPopulationSize ||
                    hasTabuSetSize)
                    return true;
                return false;
            }
        }

        public bool hasRuntime
        {
            get { return _runtime != null && _runtime.CompareTo(new TimeSpan()) != 0; }
        }

        public bool hasDecryptions
        {
            get { return _decryptions != null && _decryptions != 0; }
        }

        public bool hasRestarts
        {
            get { return _restarts != null && _restarts != 0; }
        }

        public bool hasKeys
        {
            get { return _keys != null && _keys != 0; }
        }

        public bool hasPopulationSize
        {
            get { return _populationSize != null && _populationSize != 0; }
        }

        public bool hasTabuSetSize
        {
            get { return _tabuSetSize != null && _tabuSetSize != 0; }
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Set runtime if not already set
        /// </summary>
        public void SetRuntime(TimeSpan t)
        {
            if (!hasRuntime)
                _runtime = t;
        }
        
        /// <summary>
        /// Set number of decryptions if not already set
        /// </summary>
        public void SetDecryptions(BigInteger i)
        {
            if (!hasDecryptions)
                _decryptions = i;
        }
        
        /// <summary>
        /// Set number of restarts if not already set
        /// </summary>
        public void SetRestarts(BigInteger i)
        {
            if (!hasRestarts)
                _restarts = i;
        }
        
        /// <summary>
        /// Set number of keys if not already set
        /// </summary>
        public void SetKeys(BigInteger i)
        {
            if (!hasKeys)
                _keys = i;
        }
        
        /// <summary>
        /// Set population size if not already set
        /// </summary>
        public void SetPopulationSize(BigInteger i)
        {
            if (!hasPopulationSize)
                _populationSize = i;
        }
        
        /// <summary>
        /// Set tabu set size if not already set
        /// </summary>
        public void SetTabuSetSize(BigInteger i)
        {
            if (!hasTabuSetSize)
                _tabuSetSize = i;
        }

        /// <summary>
        /// Get the runtime as TimeSpan
        /// </summary>
        public bool GetRuntime(out TimeSpan t)
        {
            if (hasRuntime)
            {
                t = _runtime;
                return true;
            }

            t = new TimeSpan();
            return false;
        }

        /// <summary>
        /// Get the number of decryptions as BigInteger
        /// </summary>
        public BigInteger GetDecryptions()
        {
            if (hasRuntime)
                return _decryptions;

            return 0;
        }

        /// <summary>
        /// Get the number of decryptions as BigInteger
        /// </summary>
        public BigInteger GetRestarts()
        {
            if (hasRestarts)
                return _restarts;

            return 0;
        }

        /// <summary>
        /// Get the number of decryptions as BigInteger
        /// </summary>
        public BigInteger GetKeys()
        {
            if (hasKeys)
                return _keys;

            return 0;
        }

        /// <summary>
        /// Get the number of decryptions as BigInteger
        /// </summary>
        public BigInteger GetPopulationSize()
        {
            if (hasPopulationSize)
                return _populationSize;

            return 0;
        }

        /// <summary>
        /// Get the number of decryptions as BigInteger
        /// </summary>
        public BigInteger GetTabuSetSize()
        {
            if (hasTabuSetSize)
                return _tabuSetSize;

            return 0;
        }

        /// <summary>
        /// Returns a string representation of all set private members
        /// </summary>
        public override string ToString()
        {
            string outputString = "";
            bool isEmpty = true;

            if (hasRuntime) {
                outputString += "Runtime: " + _runtime;
                isEmpty = false;
            }

            if (hasDecryptions) {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Decryptions: " + _decryptions;
                isEmpty = false;
            }

            if (hasRestarts) {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Restarts: " + _restarts;
                isEmpty = false;
            }

            if (hasKeys) {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Keys: " + _keys;
                isEmpty = false;
            }

            if (hasPopulationSize) {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Population size: " + _populationSize;
                isEmpty = false;
            }

            if (hasTabuSetSize) {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Tabu set size: " + _tabuSetSize;
                isEmpty = false;
            }

            return outputString;
        }

        #endregion

        #region Private/protected methods

        

        #endregion

    }
}
