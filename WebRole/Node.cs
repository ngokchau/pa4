using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole
{
    public class Node
    {
        public char letter;
        public Dictionary<char, Node> children;
        public bool isWord;

        public Node() { }

        public Node(char c)
        {
            this.letter = c;
            this.children = new Dictionary<char, Node>();
            this.isWord = false;
        }

        public Node ChildNode(char c)
        {
            if (this.children != null && this.children.ContainsKey(c))
            {
                return this.children[c];
            }
            return null;
        }
    }
}