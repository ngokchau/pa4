using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace WebRole
{
    public class Trie
    {
        private Node root;
        private List<string> suggestions = new List<string>();
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudTable statsTable = storageAccount.CreateCloudTableClient().GetTableReference("statstable");
        private int counter = 0;

        public Trie()
        {
            root = new Node('$');
        }

        public Node Root
        {
            get { return this.root; }
        }

        public List<string> Suggestions
        {
            get { return this.suggestions; }
        }

        public void Insert(string word)
        {
            char[] letters = word.ToLower().ToCharArray();

            Insert(root, letters, 0);
        }

        private void Insert(Node current, char[] letters, int index)
        {
            if (index < letters.Length)
            {
                if ((current.children.Count == 0 || !current.children.ContainsKey(letters[index])))
                {
                    current.children[letters[index]] = new Node(letters[index]);
                }

                current = current.children[letters[index]];
                Insert(current, letters, index + 1);
            }
            else
            {
                current.isWord = true;
                //TableOperation word = TableOperation.InsertOrReplace(new Stats("trie", counter++.ToString()));
                //statsTable.Execute(word);
            }
        }

        public void Search(string prefix)
        {
            this.suggestions.Clear();
            char[] letters = prefix.ToLower().ToCharArray();
            StringBuilder sb = new StringBuilder();

            Search(root, letters, 0, sb);
        }

        private void Search(Node current, char[] prefix, int index, StringBuilder sb)
        {
            if (index < prefix.Length && current.children.ContainsKey(prefix[index]))
            {
                sb.Append(prefix[index]);

                current = current.ChildNode(prefix[index]);
                Search(current, prefix, index + 1, sb);
            }
            else
            {
                if (current.isWord)
                {
                    this.suggestions.Add(sb.ToString());
                }
                DFS(current, sb);
            }
        }

        private void DFS(Node current, StringBuilder sb)
        {
            foreach (char c in current.children.Keys)
            {
                if (this.suggestions.Count < 10)
                {
                    sb.Append(c);
                    if (current.ChildNode(c).isWord)
                    {
                        this.suggestions.Add(sb.ToString());
                    }
                    DFS(current.ChildNode(c), sb);
                    sb.Remove(sb.Length - 1, 1);
                }
            }
        }
    }
}