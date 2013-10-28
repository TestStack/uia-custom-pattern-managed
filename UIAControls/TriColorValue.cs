using System;

namespace UIAControls
{
    /// <summary>
    /// A choice among three colors
    /// </summary>
    public enum TriColorValue
    {
        Red,
        Yellow,
        Green,
    };

    public static class TriColorValueHelper
    {
        public static bool IsFirst(TriColorValue value)
        {
            return (value == TriColorValue.Red);
        }

        public static bool IsLast(TriColorValue value)
        {
            return (value == TriColorValue.Green);
        }

        public static TriColorValue NextValue(TriColorValue value)
        {
            if (IsLast(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            value++;
            return value;
        }

        public static TriColorValue PreviousValue(TriColorValue value)
        {
            if (IsFirst(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            value--;
            return value;
        }
    }
}