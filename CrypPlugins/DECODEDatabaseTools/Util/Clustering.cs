/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools.Util
{
    /// <summary>
    /// A ClusterSet is a self-generating set of different Clusters using a dedicated matchBorder (threshold)
    /// </summary>
    public class ClusterSet : INotifyPropertyChanged
    {
        private double _matchBorder;
        private List<Cluster> _clusters = new List<Cluster>();
        private List<TextDocument> _documents = new List<TextDocument>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ClusterSet(double matchBorder = 15)
        {
            _matchBorder = matchBorder;
        }

        /// <summary>
        /// Adds this document to our ClusterSet
        /// </summary>
        /// <param name="document"></param>
        public void AddDocument(TextDocument document)
        {

            //Step 0: Check, if document is already in the internal document list
            if (_documents.Contains(document))
            {
                return;
            }

            //Step 1: create frequencies of document
            TextDocumentWithFrequencies textDocumentWithFrequencies = new TextDocumentWithFrequencies();
            textDocumentWithFrequencies.TextDocument = document; //this creates the frequencies

            //Step 2: check, if this belongs to any of our clusters
            Cluster bestMatchingCluster = null;
            double currentBestMatchingValue = double.MaxValue;
            foreach (var cluster in _clusters)
            {
                double matchValue = cluster.GetMatchValue(textDocumentWithFrequencies);
                if (matchValue < _matchBorder && matchValue < currentBestMatchingValue)
                {
                    currentBestMatchingValue = matchValue;
                    bestMatchingCluster = cluster;
                }
            }
            if (bestMatchingCluster != null)
            {
                //Step 2.1: we found a best-matching cluster; thus, we add the document
                bestMatchingCluster.AddTextDocumentWithFrequencies(textDocumentWithFrequencies);
            }
            else
            {
                //Step 2.1: we did not find a best-matching cluster; thus, we create a new one
                Cluster cluster = new Cluster();
                cluster.AddTextDocumentWithFrequencies(textDocumentWithFrequencies);
                _clusters.Add(cluster);
            }

            //Store document in the overall list of all documents
            _documents.Add(document);

            //Notify everyone that our clusters and documents have been changed
            OnPropertyChanged("Clusters");
            OnPropertyChanged("Documents");

        }

        /// <summary>
        /// Returns all clusters of this cluster set
        /// </summary>
        public List<Cluster> Clusters
        {
            get
            {
                return _clusters;
            }
        }

        /// <summary>
        /// Returns all documents of this cluster set
        /// </summary>
        public List<TextDocument> Documents
        {
            get
            {
                return _documents;
            }
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

    }

    /// <summary>
    /// A cluster is a set of text document with similiar symbol frequencies
    /// </summary>
    public class Cluster : INotifyPropertyChanged
    {        
        private List<TextDocumentWithFrequencies> _documents = new List<TextDocumentWithFrequencies>();
        private Dictionary<Symbol, double> _frequencies = new Dictionary<Symbol, double>();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the match value of the given document and this cluster
        /// A "perfect match" is equal to 0.0
        /// </summary>
        /// <param name="textDocumentWithFrequencies"></param>
        /// <returns></returns>
        public double GetMatchValue(TextDocumentWithFrequencies textDocumentWithFrequencies)
        {
            double matchValue = 0;
            foreach(var key in textDocumentWithFrequencies.Frequencies.Keys)
            {
                double frequencyDocument = textDocumentWithFrequencies.Frequencies[key];
                double frequencyCluster = 0;
                if (_frequencies.ContainsKey(key))
                {
                    frequencyCluster = _frequencies[key];
                }
                matchValue += Math.Abs(frequencyCluster - frequencyDocument);
            }
            return matchValue;
        }

        /// <summary>
        /// Adds a text document to this Cluster,
        /// also updates frequencies of the cluster
        /// </summary>
        /// <param name="textDocumentWithFrequencies"></param>
        public void AddTextDocumentWithFrequencies(TextDocumentWithFrequencies textDocumentWithFrequencies)
        {                        
            _documents.Add(textDocumentWithFrequencies);
            UpdateFrequencies();
            OnPropertyChanged("Frequencies");
            OnPropertyChanged("FrequenciesSortedBySymbol");
        }

        /// <summary>
        /// Updates the symbol frequencies of this cluster
        /// </summary>
        private void UpdateFrequencies()
        {
            _frequencies.Clear();

            Dictionary<Symbol, int> _absoluteValues = new Dictionary<Symbol, int>();
            int totalSymbols = 0;
            foreach (var document in _documents)
            {
                foreach (var page in document.TextDocument.Pages)
                {
                    foreach (var line in page.Lines)
                    {
                        // we don't count frequencies of the comments
                        if (line.LineType == LineType.Comment)
                        {
                            continue;
                        }
                        foreach (var token in line.Tokens)
                        {
                            // we don't count frequencies of tags inside the text
                            if (token.TokenType == TokenType.Tag)
                            {
                                continue;
                            }
                            foreach (var symbol in token.Symbols)
                            {
                                if (!_absoluteValues.ContainsKey(symbol))
                                {
                                    _absoluteValues[symbol] = 0;
                                }
                                _absoluteValues[symbol] = _absoluteValues[symbol] + 1;
                                totalSymbols++;
                            }
                        }
                    }
                }
            }
            foreach (var key in _absoluteValues.Keys)
            {
                double frequency = ((double)_absoluteValues[key]) / ((double)totalSymbols) * 100;
                _frequencies.Add(key, frequency);
            }
        }

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public Dictionary<Symbol, double> Frequencies
        {
            get
            {
                return _frequencies;
            }
        }

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public List<KeyValuePair<Symbol, double>> FrequenciesSortedBySymbol
        {
            get
            {
                List<KeyValuePair<Symbol, double>> frequencies = _frequencies.ToList();
                frequencies.Sort(delegate (KeyValuePair<Symbol, double> a, KeyValuePair<Symbol, double> b)
                {
                    return a.Key.CompareTo(b.Key);
                });
                return frequencies;
            }
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }    

    /// <summary>
    /// A TextDocumentWithFrequencies is a wrapper for a TextDocument
    /// It also contains the relative frequencies of the symbols of the TextDocument
    /// </summary>
    public class TextDocumentWithFrequencies
    {
        private readonly Dictionary<Symbol, double> _frequencies = new Dictionary<Symbol, double>();
        private TextDocument _textDocument;

        /// <summary>
        /// The text document of this TextDocumentWithFrequencies
        /// </summary>
        public TextDocument TextDocument
        {
            get
            {
                return _textDocument;
            }
            set
            {
                _textDocument = value;
                UpdateFrequencies();
            }
        }
       
        /// <summary>
        /// Computes the symbol frequencies of all symbols in this document
        /// </summary>
        private void UpdateFrequencies()
        {
            _frequencies.Clear();

            Dictionary<Symbol, int> _absoluteValues = new Dictionary<Symbol, int>();
            int totalSymbols = 0;

            foreach (var page in _textDocument.Pages)
            {
                foreach (var line in page.Lines)
                {
                    // we don't count frequencies of the comments
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }
                    foreach (var token in line.Tokens)
                    {
                        // we don't count frequencies of tags inside the text
                        if (token.TokenType == TokenType.Tag)
                        {
                            continue;
                        }
                        foreach(var symbol in token.Symbols)
                        {
                            if (!_absoluteValues.ContainsKey(symbol))
                            {
                                _absoluteValues[symbol] = 0;
                            }
                            _absoluteValues[symbol] = _absoluteValues[symbol] + 1;
                            totalSymbols++;
                        }
                    }
                }
            }
            foreach(var key in _absoluteValues.Keys)
            {
                double frequency = ((double)_absoluteValues[key]) / ((double)totalSymbols) * 100;
                _frequencies.Add(key, frequency);
            }
        }

        /// <summary>
        /// Returns all frequencies of the TextDocument of this TextDocumentWithFrequencies
        /// </summary>
        public Dictionary<Symbol, double> Frequencies
        {
            get
            {
                return _frequencies;
            }
        }

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public List<KeyValuePair<Symbol, double>> FrequenciesSortedBySymbol
        {
            get
            {
                List<KeyValuePair<Symbol, double>> frequencies = _frequencies.ToList();
                frequencies.Sort(delegate (KeyValuePair<Symbol, double> a, KeyValuePair<Symbol, double> b)
                {
                    return a.Key.CompareTo(b.Key);
                });
                return frequencies;
            }
        }

        /// <summary>
        /// Returns the ToString of the internal text document if it is not equal to null
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(_textDocument != null)
            {
                return _textDocument.ToString();
            }
            else
            {
                return base.ToString();
            }
        }
    }

}
