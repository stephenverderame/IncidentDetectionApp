using System;
using System.Collections.Generic;
using System.Text;

namespace IncidentDetection
{
    public class HuffmanCollection
    {
        private HuffmanTree[] trees;
        public HuffmanCollection()
        {
            trees = new HuffmanTree[26];
            for(int i = 0; i < trees.Length; ++i)
            {
                trees[i] = new HuffmanTree(Convert.ToChar(Convert.ToInt32('a') + i));
            }
        }
        public void add(string word)
        {
            trees[Convert.ToInt32(word.ToLower()[0]) - Convert.ToInt32('a')].add(word.ToCharArray());
        }
        public List<string> getWords(string key = null)
        {
            if(key != null)
                return trees[Convert.ToInt32(key.ToLower()[0]) - Convert.ToInt32('a')].getWords(key?.ToCharArray());
            else
            {
                List<string> list = new List<string>();
                foreach (var tree in trees)
                    list.AddRange(tree.getWords());
                return list;
            }
        }
    }
    class HuffmanTree
    {
        private Node root;
        public HuffmanTree(char rootLetter)
        {
            root = new Node(rootLetter);
        }
        public void add(char[] word)
        {
            Node currentNode = root;
            for(int i = 1; i < word.Length; ++i)
            {
                int index = Convert.ToInt32(Char.ToLower(word[i])) - Convert.ToInt32('a');
                if(currentNode.children[index] == null)
                {
                    currentNode.children[index] = new Node(Char.ToLower(word[i]));
                }
                if (i == word.Length - 1)
                    currentNode.children[index].stop = true;
                currentNode = currentNode.children[index];
            }
        }
        public List<string> getWords(char[] key = null)
        {
            List<string> list = new List<string>();
            if(key == null)
                root.getWords("", ref list);
            else
            {
                Node n = root;
                int i = 0;
                while (n != null && n.letter == key[i++] && i < key.Length)
                {
                    n = n.children[Convert.ToInt32(char.ToLower(key[i])) - Convert.ToInt32('a')];
                }
                if (i == key.Length)
                    n.getWords(new string(key), ref list);
            }
            return list;
        }

    }
    class Node
    {
        public char letter { get; set; }
        public bool stop { get; set; }
        public Node[] children;
        public Node(char letter)
        {
            this.letter = letter;
            stop = false;
            children = new Node[26];
        }
        public void getWords(string track, ref List<string> words)
        {
            track += letter;
            if (stop) words.Add(track);
            foreach(var child in children)
            {
                if(child != null)
                {
                    child.getWords(track, ref words);
                }
            }
        }
    }
}
