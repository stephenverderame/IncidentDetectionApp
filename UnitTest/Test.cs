using IncidentDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class Test
    {
        public static void Main()
        {
            var trie = new TrieCollection<int>();
            trie.add("mom", 5);
            trie.add("mother", 3);
            trie.add("go", 1);
            trie.add("goodbye", 1);
            trie.add("apple", 6);
            trie.add("app", 7);
            var all = trie.getWords();
            foreach(var w in all)
            {
                Console.WriteLine(w);
            }
            Console.WriteLine();
            var some = trie.getWords("go");
            foreach (var w in some)
            {
                Console.WriteLine(w);
            }
            Console.WriteLine();
            some = trie.getWords("ap");
            foreach (var w in some)
            {
                Console.WriteLine(w);
            }
            Console.WriteLine();
            trie.set("goodbye", 50);
            some = trie.getWords("good");
            foreach (var w in some)
            {
                Console.WriteLine(w);
            }
            Console.ReadKey();
        }
    }
}
