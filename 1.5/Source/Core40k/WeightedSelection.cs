using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class WeightedSelection<T>
    {
        private struct Entry
        {
            public double accumulatedWeight;
            public T item;
        }

        private List<Entry> entries = new List<Entry>();
        private double accumulatedWeight;
        private Random rand = new Random();

        public void AddEntry(T item, double weight)
        {
            accumulatedWeight += weight;
            entries.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
        }

        public T GetRandom()
        {
            var r = rand.NextDouble() * accumulatedWeight;

            foreach (var entry in entries.Where(entry => entry.accumulatedWeight >= r))
            {
                return entry.item;
            }
            return default(T); //should only happen when there are no entries
        }

        public T GetRandomUnique()
        {
            var r = rand.NextDouble() * accumulatedWeight;

            foreach (var entry in entries)
            {
                if (!(entry.accumulatedWeight >= r)) continue;
                
                var unique = new Entry{ item = entry.item, accumulatedWeight = entry.accumulatedWeight };
                entries.Remove(entry);
                return unique.item;
            }
            return default(T); //should only happen when there are no entries
        }

        public bool NoEntriesOrNull()
        {
            return entries.NullOrEmpty();
        }
    }
}   