using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCAPathFinder.UI.Models;
using DCAPathFinder.UI.Tutorial2;

namespace DCAPathFinder.Logic
{
    public class SearchResult
    {
        public bool[] activeSBoxes;
        public int round;
        public DateTime startTime;
        public DateTime endTime;
        public Algorithms currentAlgorithm;
        public List<CharacteristicUI> result;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult()
        {
            result = new List<CharacteristicUI>();
        }
    }
}