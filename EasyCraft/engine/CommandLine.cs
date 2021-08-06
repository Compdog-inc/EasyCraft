using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public struct Arguments
    {
        Dictionary<string, string> flags;
        List<string> words;

        public Arguments(Dictionary<string, string> _flags, List<string> _words)
        {
            flags = _flags;
            words = _words;
        }

        public string GetFlag(string name)
        {
            if (flags.ContainsKey(name.ToLowerInvariant()))
                return flags[name.ToLowerInvariant()];
            return string.Empty;
        }

        public bool HasFlag(string name)
        {
            return flags.ContainsKey(name.ToLowerInvariant());
        }

        public bool HasWord(string word, bool ignoreCase = true)
        {
            foreach (string s in words)
                if (string.Equals(s, word, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) return true;
            return false;
        }

        public string[] GetWords()
        {
            return words.ToArray();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("Flags:");
            foreach(string key in flags.Keys)
            {
                builder.Append("    ").Append(key).Append(": ").AppendLine(flags[key]);
            }
            builder.AppendLine("Words:");
            foreach(string word in words)
            {
                builder.Append("    ").AppendLine(word);
            }
            return string.Format("{{{0}}}", builder);
        }
    }

    public static class CommandLine
    {
        public static Arguments ParseArguments(string[] args, params char[] flagIndicators)
        {
            Dictionary<string, string> flags = new Dictionary<string, string>();
            List<string> words = new List<string>();

            string _key = string.Empty;

            foreach (string arg in args)
            {
                if (StartsWithAny(arg, out string sarg, flagIndicators))
                {
                    if (!string.IsNullOrEmpty(_key)) flags.Add(_key, string.Empty);
                    _key = sarg.ToLowerInvariant();
                }
                else
                {
                    if (!string.IsNullOrEmpty(_key)) flags.Add(_key, arg);
                    else words.Add(arg);
                    _key = string.Empty;
                }
            }

            return new Arguments(flags, words);
        }

        private static bool StartsWithAny(string str, out string substr, params char[] chars)
        {
            foreach (char ch in chars)
                if (str.StartsWith(ch.ToString()))
                {
                    substr = str.Substring(1);
                    return true;
                }
            substr = str;
            return false;
        }

        private static bool StartsWithAny(string str, out string substr, params string[] prefixes)
        {
            foreach (string prefix in prefixes)
                if (str.StartsWith(prefix))
                {
                    substr = str.Substring(prefix.Length);
                    return true;
                }
            substr = str;
            return false;
        }
    }
}