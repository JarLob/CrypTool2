﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
    public class Node : IComparable<Node>
    {

        public double Dist { get; set; }
        public Node previous { get; set; }

        public Point Point { get; set; }
        public HashSet<Node> Vertices { get; private set; }

        public double traverseCost(Node dest)
        {
            if (!Vertices.Contains(dest))
                return Double.PositiveInfinity;

            if (dest.Point.X == Point.X)
                return Math.Abs(dest.Point.Y - Point.Y);
            return Math.Abs(dest.Point.X - Point.X);
        }

        public IEnumerable<Node> neighbors()
        {
            return Vertices;
        }


        private static int uniqueCounter;
        protected readonly int uniqueIndex;

        public Node()
        {
            this.Dist = Double.PositiveInfinity;
            this.previous = null;
            this.Vertices = new HashSet<Node>();
            uniqueIndex = ++uniqueCounter;
        }

        public int CompareTo(Node other)
        {
            int res = Dist.CompareTo(other.Dist);
            //if res and other is equal then apply different CompareTo() value (OrderedSet deletes any State if 

            if (res == 0)
                return uniqueIndex.CompareTo(other.uniqueIndex);
            return res;
        }

        public LinkedList<Node> makePath()
        {
            LinkedList<Node> result = new LinkedList<Node>();
            Node s = this;
            while (s != null)
            {
                result.AddFirst(s);
                s = s.previous;
            }

            return result;
        }

    }

    public class Dijkstra<T>  where T : Node {

        public LinkedList<Node> findPath(IEnumerable<T> graph, T start, T goal) {
    
            OrderedSet<T> unvisitedNodes = new OrderedSet<T>();

            foreach(T n in graph) {
                if(n.Equals(start))
                {
                    n.Dist = 0;
                }
                unvisitedNodes.Add(n);
            }

            while (unvisitedNodes.Count!=0 ) {
                var visitingNode = unvisitedNodes.RemoveFirst();

               if (visitingNode.Dist == Double.PositiveInfinity) {
                    break;
                }

           
                if (goal.Equals(visitingNode)) {
                    return visitingNode.makePath();
                }

                foreach (T v in visitingNode.neighbors()) {
                    double altPathCost = visitingNode.Dist + visitingNode.traverseCost(v);
                
                    if (altPathCost < v.Dist) {
                        unvisitedNodes.Remove(v);
                        v.Dist = altPathCost;
                        v.previous = visitingNode;
                        unvisitedNodes.Add(v);
                    }
                }
            }

            return null;
        }
    }

}
