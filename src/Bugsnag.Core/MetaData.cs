﻿using System.Collections.Generic;
using System.Linq;
using InternalMetadata = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;

namespace Bugsnag
{
    public class Metadata
    {
        public const string DefaultTabName = "Custom Data";

        public InternalMetadata MetadataStore { get; private set; }

        public Metadata()
        {
            MetadataStore = new InternalMetadata();
        }

        public void AddToTab(string tabEntryKey, object tabEntryValue)
        {
            AddToTab(DefaultTabName, tabEntryKey, tabEntryValue);
        }

        public void AddToTab(string tabName, string tabEntryKey, object tabEntryValue)
        {
            // If the tab doesn't exist create a new tab with a single entry
            if (!MetadataStore.ContainsKey(tabName))
            {
                var newTabData = new Dictionary<string, object> { { tabEntryKey, tabEntryValue } };
                MetadataStore.Add(tabName, newTabData);
            }
            else
            {
                // If the tab entry exists, overwrite the entry otherwise add it as a new entry
                if (MetadataStore[tabName].ContainsKey(tabEntryKey))
                    MetadataStore[tabName][tabEntryKey] = tabEntryValue;
                else
                    MetadataStore[tabName].Add(tabEntryKey, tabEntryValue);
            }
        }

        public void RemoveTab(string tabName)
        {
            // If the tab doesn't exist simply do nothing
            if (!MetadataStore.ContainsKey(tabName))
                return;

            MetadataStore.Remove(tabName);
        }

        public void RemoveTabEntry(string tabName, string tabEntryKey)
        {
            // If the tab doesn't exist or the tab entry doesn't exist simply do nothing
            if (!MetadataStore.ContainsKey(tabName) ||
                !MetadataStore[tabName].ContainsKey(tabEntryKey))
                return;

            MetadataStore[tabName].Remove(tabEntryKey);
        }

        public static Metadata MergeMetaData(params Metadata[] data)
        {
            var aggData = data.ToList();
            aggData.Insert(0, new Metadata());
            return aggData.Aggregate(Merge);
        }

        private static Metadata Merge(Metadata currentData, Metadata dataToAdd)
        {
            var currStore = currentData.MetadataStore;
            var storeToAdd = dataToAdd.MetadataStore;

            // Loop through all the tabs that are in the data to add...
            foreach (var newTab in storeToAdd)
            {
                // If the tab doesn't exist in current data, add a blank tab
                if (!currStore.ContainsKey(newTab.Key))
                    currStore.Add(newTab.Key, new Dictionary<string, object>());

                var currTab = currStore[newTab.Key];

                // Loop through all the entries in the tab to add...
                foreach (var newTabEntry in newTab.Value)
                {
                    // Only add the entry if its a new tab entry, otherwise use the existing
                    // entry and ignore the entry to be merged
                    if (!currTab.ContainsKey(newTabEntry.Key))
                        currTab.Add(newTabEntry.Key, newTabEntry.Value);
                }
            }
            return currentData;
        }
    }
}
