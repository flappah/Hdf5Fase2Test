using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hdf5Fase2Test.Base
{
    public static class Extensions
    {
        #region Generic extension methods

        /// <summary>
        ///     Checks if one or more of the given items is present in item.
        /// </summary>
        /// <typeparam name="T">which type</typeparam>
        /// <param name="item">T type of item to execute method on</param>
        /// <param name="items">items to look for</param>
        /// <returns></returns>
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            return items.Contains(item);
        }

        #endregion

        #region int extension methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string ShortMonthName(this int i)
        {
            var dateTime = new DateTime(2022, i, 1);
            return dateTime.ToString("MMM", new CultureInfo("en-US")).Replace(".", "").ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string FullMonthName(this int i)
        {
            var dateTime = new DateTime(2022, i, 1);
            return dateTime.ToString("MMMM");
        }

        #endregion

        #region String extension methods 

        /// <summary>
        ///     Returns the index of the given day name
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns>index of the given day-name</returns>
        public static int DayNumber(this string item)
        {
            if (String.IsNullOrEmpty(item))
            {
                return -1;
            }

            string[] dayNames = { "", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };

            var index = dayNames.ToList().FindIndex(i => i.Trim().ToLower().Equals(item.Trim().ToLower()));
            return index;
        }

        /// <summary>
        ///     Returns EN or NL language code based on the input string
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns>EN or NL</returns>
        public static string IsWhichLanguage(this string item)
        {
            return item.ContainsWords(true, false, "the", "be", "are", "and", "or", "to", "north", "south", "netherlands", "germany", "german", "belgium", "chart", "maritime", "routeing", "streams", "sea", "lake", "river") ? "EN" : "NL";
        }

        /// <summary>
        ///     Capitalizes the string
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string Capitalize(this string item)
        {
            if (String.IsNullOrEmpty(item))
            {
                return item;
            }

            return char.ToUpper(item.First()) + item.Substring(1).ToLower();
        }

        /// <summary>
        ///     Converts input string to camelcase
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns>camelCase version of the string</returns>
        public static string ToCamelCase(this string item)
        {
            if (!string.IsNullOrEmpty(item) && item.Length > 1)
            {
                return Char.ToLowerInvariant(item[0]) + item.Substring(1);
            }
            return item;
        }

        /// <summary>
        ///     Returns the string before a given character (divider)
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="divider">character to use as end of string part</param>
        /// <param name="includeDivider">include divider in the returned string</param>
        /// <returns></returns>
        public static string StringBefore(this string item, string divider, bool includeDivider = true)
        {
            if (String.IsNullOrEmpty(item))
            {
                return "";
            }

            if (!item.Contains(divider))
            {
                return item;
            }

            return item.Substring(0, item.IndexOf(divider) + (includeDivider ? 1 : 0));
        }

        /// <summary>
        ///     Returns the string after a given character (divider)
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="divider">character to use as end of string part</param>
        /// <returns></returns>
        public static string StringAfter(this string item, string divider)
        {
            if (String.IsNullOrEmpty(item))
            {
                return "";
            }

            if (!item.Contains(divider))
            {
                return "";
            }

            return item.LastPart(divider); ;
        }

        /// <summary>
        ///      Checks if one or more of the given itemstrings are contained in the target item
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="ignoreCase">true if the case can be ignored</param>
        /// <param name="items">array of items used in the match</param>
        /// <returns>True if one of the list of string items is contained inside the specified string</returns>
        public static bool Contains(this string item, bool ignoreCase = false, params string[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            foreach (string value in items)
            {
                if (ignoreCase)
                {
                    if (item.ToUpper().Contains(value.ToUpper()))
                    {
                        return true;
                    }
                }
                else
                {
                    if (item.Contains(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///      Checks if one or more of the given itemstrings are contained as a word in the targetstring
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="ignoreCase">true if the case can be ignored</param>
        /// <param name="matchAllWords">true if all words be matched</param>
        /// <param name="items">array of items used in the match</param>
        /// <returns>True if one of the list of string items is contained inside the specified string</returns>
        public static bool ContainsWords(this string item, bool ignoreCase = false, bool matchAllWords = false, params string[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            string[] tokenizedItemString = item.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in tokenizedItemString)
            {
                if (ignoreCase)
                {
                    if (matchAllWords == false)
                    {
                        if (items.ToList().Exists(p => p.ToUpper().Equals(value.ToUpper())))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (items.ToList().Exists(p => p.ToUpper().Equals(value.ToUpper())) == false)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (matchAllWords == false)
                    {
                        if (items.ToList().Exists(p => p.Equals(value)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (items.ToList().Exists(p => p.Equals(value)) == false)
                        {
                            return false;
                        }
                    }
                }
            }

            return matchAllWords ? true : false;
        }

        /// <summary>
        ///     Returns the day name of a given index
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns></returns>
        public static string DayName(this int index)
        {
            string[] dayNames = { "", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };

            if (index < 0 || index > dayNames.Length)
            {
                return "";
            }

            return dayNames[index];
        }

        /// <summary>
        ///     Assumes given string is a (partial) timestring and converts it to a valid xs:time string
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns>Valid xs:time string</returns>
        public static string ToXsTime(this string item)
        {
            if (!String.IsNullOrEmpty(item) && item.Length > 1)
            {
                var splittedItems = new List<string>();
                if (item.IsNumeric())
                {
                    if (item.Length == 2)
                    {
                        item = $"0{item.First()}0{item.Last()}";
                    }
                    else if (item.Length == 3 || item.Length == 5)
                    {
                        item = $"0{item}";
                    }

                    var regex = new Regex("([0-9]{2})([0-9]{2})([0-9]{0,2})");

                    if (regex.IsMatch(item))
                    {
                        Match match = regex.Match(item);
                        for (int i = 1; i < match.Groups.Count; i++)
                        {
                            if (short.TryParse(match.Groups[i].Value, out short numValue))
                            {
                                splittedItems.Add(numValue.ToString("00"));
                            }
                        }
                    }
                }
                else
                {
                    splittedItems = item.RegexSplit();
                }

                if (splittedItems.Count == 2)
                {
                    splittedItems.Add("00");
                }

                foreach (var splittedItem in splittedItems)
                {
                    if (!splittedItem.IsNumeric())
                    {
                        return "";
                    }
                }

                return splittedItems.Join(":");
            }

            return "";
        }

        /// <summary>
        ///     Assumes given string is a (partial) datetimestring and converts it to a valid xs:dateTime string
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns>Valid xs:datetime string</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string ToXsDateTime(this string item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns true if the string contains a numerical value
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <returns></returns>
        public static bool IsNumeric(this string item)
        {
            return (int.TryParse(item, out _) || double.TryParse(item, out _));
        }

        /// <summary>
        ///     Returns part after specified start character
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="startFrom">start from specified character</param>
        /// <returns>New string</returns>
        public static string LastPart(this string item, char startFrom)
        {
            var start = item.LastIndexOf(startFrom);
            if (start <= 0)
            {
                return item;
            }

            return item.Substring(start + 1, item.Length - start - 1);
        }

        /// <summary>
        ///     Returns part after specified start character
        /// </summary>
        /// <param name="item">string to execute method on</param>
        /// <param name="startFrom">start from specified character</param>
        /// <returns>New string</returns>
        public static string LastPart(this string item, string startFrom)
        {
            var start = item.LastIndexOf(char.Parse(startFrom));
            if (start <= 0)
            {
                return item;
            }

            return item.Substring(start + 1, item.Length - start - 1);
        }

        /// <summary>
        ///     Cut length characters off of the left part of the string
        /// </summary>
        /// <param name="item">extension string</param>
        /// <param name="length">number of characters to cut off</param>
        /// <returns>New string</returns>
        public static string StripLeft(this string item, int length)
        {
            if (item.Length - length < 0)
            {
                return item;
            }

            return item.Substring(length, item.Length - length);
        }

        /// <summary>
        ///     Cut length characters off of the right part of the string
        /// </summary>
        /// <param name="item">extension string</param>
        /// <param name="length">number of characters to cut off</param>
        /// <returns>New string</returns>
        public static string StripRight(this string item, int length)
        {
            if (item.Length - length < 0)
            {
                return item;
            }

            return item.Substring(0, item.Length - length);
        }

        /// <summary>
        /// Converts string to float
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static float ToFloat(this string item)
        {
            if (!float.TryParse(item, out float convertedValue))
            {
                float.TryParse(item.Replace(".", ","), out convertedValue);
            }

            return convertedValue;
        }

        /// <summary>
        /// Converts string to double
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static double ToDouble(this string item)
        {
            if (!double.TryParse(item, out double convertedValue))
            {
                double.TryParse(item.Replace(".", ","), out convertedValue);
            }

            return convertedValue;
        }

        /// <summary>
        /// Converts string to number
        /// </summary>
        /// <param name="item">string</param>
        /// <returns>int value</returns>
        public static int ToNumber(this string item)
        {
            if (int.TryParse(item, out int value))
            {
                return value;
            }

            return 0;
        }

        /// <summary>
        ///     Split the given string based on a variable split token. Initially it tries to split on 
        ///     the comma separator. If that fails it tries to see what the split token can be by using 
        ///     a regex. It deletes all alphanumerical characters and sees what is left behind. That is
        ///     used as the split token (separation character).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Splitted string</returns>
        public static List<string> RegexSplit(this string item)
        {
            var splittedList = new List<string>();
            if (!String.IsNullOrEmpty(item))
            {
                if (item.Contains(","))
                {
                    splittedList.AddRange(item.Split(new[] { ',' }));
                }
                else
                {
                    var regex = new Regex("[a-zA-Z0-9$]*");
                    var replacedNotAllowedAcronyms = regex.Replace(item, "");
                    if (replacedNotAllowedAcronyms.Length > 0)
                    {
                        var separationCharacter = replacedNotAllowedAcronyms[0];
                        splittedList.AddRange(item.Split(separationCharacter));
                    }
                }
            }

            return splittedList;
        }

        /// <summary>
        ///     Recombines the given string based on a given separation character. Default it tries 
        ///     to join on the comma as separation character.
        /// </summary>
        /// <param name="item">String containing multiple items separated by a specific character</param>
        /// <param name="separationString">New separation STRING used to recombine the stringitems</param>
        /// <returns></returns>
        public static string Recombine(this string item, string separationString = ",")
        {
            var splittedList = item.RegexSplit();

            var recombinedString = "";
            if (splittedList.Count > 0)
            {
                foreach (var acronym in splittedList)
                {
                    recombinedString += $"{(separationString.Length > 1 ? separationString[0].ToString() : "")}{acronym}{separationString}";
                }
                recombinedString = recombinedString.Substring(0, recombinedString.Length - 1);
            }

            return recombinedString;
        }

        #endregion

        #region List extension methods

        /// <summary>
        ///    Clone implementation for lists 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> items) where T : ICloneable
        {
            return items.Select(item => (T)item.Clone()).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int Count<T>(this List<T> items)
        {
            var count = 0;
            foreach (T item in items)
            {
                count++;
            }
            return count;
        }

        /// <summary>
        ///     Limits the amount of listitems to the specified count
        /// </summary>
        /// <param name="item">List</param>
        /// <param name="itemAmount">Amount of items to limit to</param>
        /// <returns>Resulting list</returns>
        public static List<T> LimitTo<T>(this List<T> item, int itemAmount)
        {
            if (item == null)
            {
                return new List<T>();
            }

            if (item.Count > itemAmount)
            {
                item.RemoveRange(itemAmount, item.Count - itemAmount);
            }

            return item;
        }

        /// <summary>
        ///     Adds the unique values of given enumerable to a given list
        /// </summary>
        /// <param name="items"></param>
        public static void AddRangeUnique(this List<string> items, IEnumerable<string> itemsToAdd)
        {
            items.AddRange(itemsToAdd);
            _ = items.Distinct().ToList();
        }

        /// <summary>
        ///     Joins a list of strings to a string separated by the specified delimeter
        /// </summary>
        /// <param name="items">list of strings to join</param>
        /// <param name="delimiter">delimiter. No delimiter means comma</param>
        /// <returns>joined string</returns>
        public static string Join(this List<string> items, string delimiter = ",")
        {
            if (items == null)
                return "";

            return String.Join<string>(delimiter, items);
        }

        #endregion

        #region Dictionary extension methods

        /// <summary>
        ///     Adds a dictionary to a dictionary and overwrites the data present in the first dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="dicToAdd"></param>
        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => dic[x.Key] = x.Value);
        }

        /// <summary>
        ///     Adds a dictionary to a dictionary but only adds new non existent values
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="dicToAdd"></param>
        public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); });
        }

        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="dicToAdd"></param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => dic.Add(x.Key, x.Value));
        }

        /// <summary>
        ///     Checks if a key is present in the dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool ContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }

        #endregion

        #region IEnumerable extension methods

        /// <summary>
        ///     Executes an action over each enumerated source item. Source item type can be given.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        ///     Executes an action over each enumerated source item but bails out if the 
        ///     function returns an error (returns false) upon execution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="func"></param>
        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            foreach (var item in source)
            {
                bool result = func(item);
                if (result) break;
            }
        }

        /// <summary>
        ///     Dispose all option for each enumerable
        /// </summary>
        /// <param name="set"></param>
        public static void DisposeAll(this IEnumerable set)
        {
            foreach (Object obj in set)
            {
                var disp = obj as IDisposable;
                if (disp != null)
                {
                    disp.Dispose();
                }
            }
        }

        #endregion
    }
}