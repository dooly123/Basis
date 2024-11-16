using System.Collections.Generic;
using System.Linq;
using System;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public abstract partial class BasisNetworkSendBase
    {
        public class FloatQueue
        {
            private List<float> values;
            private int maxSize;

            public FloatQueue(int maxSize = 5)
            {
                this.maxSize = maxSize;
                values = new List<float>();
            }

            // Method to add a new float to the list
            public void Add(float value)
            {
                values.Add(value);
                if (values.Count > maxSize)
                {
                    values.RemoveAt(0);  // Removes the first element (oldest)
                }
            }

            // Method to calculate the median of the current values
            public float Median()
            {
                // Sort the list to find the median
                var sortedValues = values.OrderBy(v => v).ToList();
                // Return the middle element (since we know there will be 5 elements, the median is at index 2)
                int Index = sortedValues.Count / 2;
                return sortedValues[Index];
            }
        }

    }
}