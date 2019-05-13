/*
   Copyright 2017 Bastian Heuser, University of Kassel

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

        private int _ID;
        private TimeSpan _runtime;
        private int _decryptions;
        private int _restarts;
        private int _populationSize;
        private int _tabuSetSize;

        /// <summary>
        /// Init empty EvaluationContainer.
        /// </summary>
        public EvaluationContainer()
        {
        }

        /// <summary>
        /// Init EvaluationContainer.
        /// </summary>
        public EvaluationContainer(int id, TimeSpan t, int decryptions, int restarts)
        {
            this._ID = id;
            this._runtime = t;
            this._decryptions = decryptions;
            this._restarts = restarts;
        }

        /// <summary>
        /// Init EvaluationContainer.
        /// </summary>
        public EvaluationContainer(int id, int decryptions, int restarts)
        {
            this._ID = id;
            this._decryptions = decryptions;
            this._restarts = restarts;
        }

        /// <summary>
        /// Init EvaluationContainer.
        /// </summary>
        public EvaluationContainer(int id, int decryptions, int restarts, int populationSize)
        {
            this._ID = id;
            this._decryptions = decryptions;
            this._restarts = restarts;
            this._populationSize = populationSize;
        }

        #endregion

        #region Public properties
        
        /// <summary>
        /// Check if this EvaluationContainer has any value set.
        /// </summary>
        public bool hasValueSet
        {
            get
            {
                if (hasID ||
                    hasRuntime ||
                    hasDecryptions ||
                    hasRestarts ||
                    hasPopulationSize ||
                    hasTabuSetSize)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Check if this EvaluationContainer equals the given container.
        /// </summary>
        /// <param name="e">The EvaluationContainer to compare with</param>
        /// <returns>True for equality, false for inequality</returns>
        public bool Equals(EvaluationContainer e)
        {
            if ((!hasID || this._ID == e._ID) &&
                (!hasRuntime || this._runtime == e._runtime) &&
                (!hasDecryptions || this._decryptions == e._decryptions) &&
                (!hasRestarts || this._restarts == e._restarts) &&
                (!hasPopulationSize || this._populationSize == e._populationSize) &&
                (!hasTabuSetSize || this._tabuSetSize == e._tabuSetSize))
                return true;
            return false;
        }

        public bool hasID
        {
            get { return _ID != null && _ID != 0; }
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
        /// Set complete EvaluationContainer.
        /// </summary>
        public void SetEvaluationContainer(EvaluationContainer e)
        {
            SetID(e._ID);
            SetRuntime(e._runtime);
            SetDecryptions(e._decryptions);
            SetRestarts(e._restarts);
            SetPopulationSize(e._populationSize);
            SetTabuSetSize(e._tabuSetSize);
        }

        /// <summary>
        /// Set ID if not already set.
        /// </summary>
        public void SetID(int id)
        {
            if (!hasID)
                _ID = id;
        }

        /// <summary>
        /// Set runtime if not already set.
        /// </summary>
        public void SetRuntime(TimeSpan t)
        {
            if (!hasRuntime)
                _runtime = t;
        }

        /// <summary>
        /// Set number of decryptions if not already set.
        /// </summary>
        public void SetDecryptions(int i)
        {
            if (!hasDecryptions)
                _decryptions = i;
        }
        
        /// <summary>
        /// Set number of restarts if not already set.
        /// </summary>
        public void SetRestarts(int i)
        {
            if (!hasRestarts)
                _restarts = i;
        }
        
        /// <summary>
        /// Set population size if not already set.
        /// </summary>
        public void SetPopulationSize(int i)
        {
            if (!hasPopulationSize)
                _populationSize = i;
        }
        
        /// <summary>
        /// Set tabu set size if not already set.
        /// </summary>
        public void SetTabuSetSize(int i)
        {
            if (!hasTabuSetSize)
                _tabuSetSize = i;
        }

        /// <summary>
        /// Get the ID as int.
        /// </summary>
        public int GetID()
        {
            if (hasID)
                return _ID;

            return 0;
        }

        /// <summary>
        /// Get the runtime as TimeSpan.
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
        /// Get the number of decryptions as int.
        /// </summary>
        public int GetDecryptions()
        {
            if (hasRuntime)
                return _decryptions;

            return 0;
        }

        /// <summary>
        /// Get the number of restarts as int.
        /// </summary>
        public int GetRestarts()
        {
            if (hasRestarts)
                return _restarts;

            return 0;
        }

        /// <summary>
        /// Get the number of population size as int.
        /// </summary>
        public int GetPopulationSize()
        {
            if (hasPopulationSize)
                return _populationSize;

            return 0;
        }

        /// <summary>
        /// Get the number of tabu set size as int.
        /// </summary>
        public int GetTabuSetSize()
        {
            if (hasTabuSetSize)
                return _tabuSetSize;

            return 0;
        }

        /// <summary>
        /// Returns a string representation of all set private members.
        /// </summary>
        public override string ToString()
        {
            string outputString = "";
            bool isEmpty = true;

            if (hasID)
            {
                outputString += "ID: " + _ID;
                isEmpty = false;
            }

            if (hasRuntime)
            {
                if (!isEmpty)
                    outputString += "\n";
                outputString += "Runtime: " + _runtime;
                isEmpty = false;
            }

            if (hasDecryptions)
            {
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
    }
}
