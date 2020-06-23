using System;
using System.Collections.Generic;
using System.Text;

namespace IncidentDetection
{
    public class TrieCollection<T>
    {
        private Trie<T>[] trees;
        public TrieCollection()
        {
            trees = new Trie<T>[26];
            for (int i = 0; i < trees.Length; ++i)
            {
                trees[i] = new Trie<T>(Convert.ToChar(Convert.ToInt32('a') + i));
            }
        }
        public void add(string word, T data)
        {
            string key = fixWord(word);
            trees[Convert.ToInt32(key.ToLower()[0]) - Convert.ToInt32('a')].add(key.ToCharArray(), data);
        }
        public List<KeyValuePair<string, T>> getWords(string key = null)
        {
            if(key != null) key = fixWord(key);
            if (key != null && key.Length >= 1)
                return trees[Convert.ToInt32(key.ToLower()[0]) - Convert.ToInt32('a')].getWords(key?.ToCharArray());
            else
            {
                var list = new List<KeyValuePair<string, T>>();
                foreach (var tree in trees)
                    list.AddRange(tree.getWords());
                return list;
            }
        }
        public void set(string key, T data)
        {
            string newKey = "";
            for (int i = 1; i < key.Length; ++i)
                if(char.IsLetter(key[i])) newKey += key[i];
            trees[Convert.ToInt32(char.ToLower(key[0])) - Convert.ToInt32('a')].set(newKey, data);
        }
        public T get(string key)
        {
            string newKey = "";
            for (int i = 1; i < key.Length; ++i)
                if(char.IsLetter(key[i])) newKey += key[i];
            return trees[Convert.ToInt32(char.ToLower(key[0])) - Convert.ToInt32('a')].get(newKey);
        }
        private string fixWord(string word)
        {
            string key = "";
            foreach (var c in word)
                if (char.IsLetter(c)) key += c;
            return key;
        }
    }
    class Trie<T>
    {
        private Node<T> root;
        public Trie(char rootLetter)
        {
            root = new Node<T>(rootLetter);
        }
        public void add(char[] word, T data)
        {
            Node<T> currentNode = root;
            for (int i = 1; i < word.Length; ++i)
            {
                int index = Convert.ToInt32(Char.ToLower(word[i])) - Convert.ToInt32('a');
                if (currentNode.children[index] == null)
                {
                    currentNode.children[index] = new Node<T>(Char.ToLower(word[i]));
                }
                if (i == word.Length - 1)
                {
                    currentNode.children[index].stop = true;
                    currentNode.children[index].data = data;
                }
                currentNode = currentNode.children[index];
            }
        }
        public List<KeyValuePair<string, T>> getWords(char[] key = null)
        {
            List<KeyValuePair<string, T>> list = new List<KeyValuePair<string, T>>();
            if (key == null)
                root.getWords("", ref list);
            else
            {
                Node<T> n = root;
                int i = 0;
                while (n != null && n.letter == key[i++] && i < key.Length)
                {
                    n = n.children[Convert.ToInt32(char.ToLower(key[i])) - Convert.ToInt32('a')];
                }
                if (i == key.Length && n != null)
                {
                    string s = "";
                    for (int j = 0; j < key.Length - 1; ++j)
                        s += key[j];
                    n.getWords(s, ref list);
                }
            }
            return list;
        }
        public void set(string key, T data)
        {
            int track = 0;
            root.children[Convert.ToInt32(char.ToLower(key[0])) - Convert.ToInt32('a')].set(key, data, ref track);
        }
        public T get(string key)
        {
            int track = 0;
            return root.children[Convert.ToInt32(char.ToLower(key[0])) - Convert.ToInt32('a')].get(key, ref track);
        }

    }
    class Node<T>
    {
        public char letter { get; set; }
        public bool stop { get; set; }
        public T data { get; set; }
        public Node<T>[] children;
        public Node(char letter)
        {
            this.letter = letter;
            stop = false;
            children = new Node<T>[26];
            data = default(T);
        }
        public void getWords(string track, ref List<KeyValuePair<string, T>> words)
        {
            track += letter;
            if (stop) words.Add(new KeyValuePair<string, T>(track, data));
            foreach (var child in children)
            {
                if (child != null)
                {
                    child.getWords(track, ref words);
                }
            }
        }
        public void set(string key, T data, ref int track)
        {
            if (track + 1 == key.Length && key[track] == letter && stop)
                this.data = data;
            else if (track + 1 < key.Length)
            {
                int index = Convert.ToInt32(char.ToLower(key[++track])) - Convert.ToInt32('a');
                if (children[index] != null) children[index].set(key, data, ref track);
            }

        }
        public T get(string key, ref int track)
        {
            if (track + 1 == key.Length && key[track] == letter && stop)
                return data;
            else if (track + 1 < key.Length)
            {
                int index = Convert.ToInt32(char.ToLower(key[++track])) - Convert.ToInt32('a');
                if (children[index] != null) return children[index].get(key, ref track);
                return default(T);
            }
            else
                return default(T);
        }
    }
}
