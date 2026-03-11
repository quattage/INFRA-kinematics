
// SIGNATURE :)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.quatworks.INFRASEC.Extensions {
    public static class IEnumerableExtensions {
        public static bool IsNullOrEmpty(this IEnumerable source) {
            if(source != null) {
                foreach(object obj in source)
                    return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) {
            if(source == null) return false;
            foreach(T obj in source)
                return false;
            return true;
        }

        public static bool IsNullorEmpty(this Array source) {
            if(source == null) return true;
            foreach(object obj in source)
                return false;
            return true;
        }

        public static int IndexOf<T>(this T[] source, object find) {
            for(int x = 0; x < source.Length; x++)
                if(find.Equals(source[x])) return x;
            return -1;
        }


        public static string ContentsToString<T>(this IEnumerable<T> source) {
            string output = "";
            foreach(T obj in source) {
                output += obj.ToString();
                output += ", ";
            }
            return "[" + output + "]";
        }
    }
}