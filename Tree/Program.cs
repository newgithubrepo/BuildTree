using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Tree
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "(id,created,employee(id,firstname,employeeType(id), lastname),location)";
            Console.WriteLine(input);

            ItemSerializer ser = new ItemSerializer(ItemDelimiters.Comma, NestingPairs.Braces);
            Item<string> root = ser.Deserialize(input);
            Console.WriteLine(root.ToString());

            root.Sort();
            Console.WriteLine(root.ToString());

            Console.ReadLine();

        }


        #region classes

        public class Item<T> : IComparable<Item<T>>
        {
            public T Me { get; set; }
            public List<Item<T>> Children { get; set; }

            public Item()
            {
                Children = new List<Item<T>>();
            }

            public void Sort()
            {
                Children.Sort();
                foreach (var child in Children)
                    child.Sort();

            }

            public override string ToString()
            {

                if (Children.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    
                    BuildTree(ref sb, 0);
                    return sb.ToString();
                }

                return Me.ToString().Trim();
            }

            public void BuildTree(ref StringBuilder sb, int level)
            {
                
                foreach (Item<T> child in Children)
                {
                    sb.AppendLine(string.Format("{0} {1}", new String('-', level), child.Me).Trim());
                    if (child.Children.Any())
                    {
                        level++;
                        child.BuildTree(ref sb, level);
                        level--;
                    }

                }


            }



            public int CompareTo(Item<T> other)
            {

                if (other == null) return 1;

                return Me.ToString().CompareTo(other.Me.ToString());
            }
        }


        public class ItemSerializer
        {
            private string _delimiter;
            private string _left;
            private string _right;

            public ItemSerializer(ItemDelimiters delimiter, NestingPairs pair)
            {

                if (delimiter != ItemDelimiters.Comma || pair != NestingPairs.Braces)
                    throw new NotSupportedException("Only comma delimiter and nesting braces are supported at this point!");
                _delimiter = ",";
                _left = "(";
                _right = ")";
            }

            public Item<string> Deserialize(string input)
            {

                Item<string> root = new Item<string>();
                List<string> parts = Regex.Split(input, string.Format(@"({0}|\{1}|\{2})", _delimiter, _left, _right))
                    .Where(s => Regex.IsMatch(s, string.Format(@"(\w+|\{0}|\{1})", _left, _right)))
                    .Select(s => s.Trim())
                    .ToList();
                AddChildren(parts, ref root);
                return root;
            }

            private int AddChildren(List<string> parts, ref Item<string> item)
            {
                if (parts == null)
                    throw new ArgumentNullException("parts");

                if (!parts.Any())
                    throw new ApplicationException("No items in parts!");

                int position = 0;
                if (IsLeft(parts.First()))
                    position++;

                
                while (position < parts.Count() && !IsLeftOrRight(parts[position]))
                {
                    Item<string> child = new Item<string>() { Me = parts[position] };
                    item.Children.Add(child);
                    position++;
                    if (IsLeft(parts[position]))
                    {
                        position++;
                        position += AddChildren(parts.GetRange(position, parts.Count() - position - 1), ref child) + 1;
                        continue;
                    }
                    if (IsRight(parts[position]))
                    {
                        break;
                    }
                }

                return position;

            }

            private bool IsLeft(string testString)
            {
                return testString == _left;
            }

            private bool IsRight(string testString)
            {
                return testString == _right;
            }

            private bool IsLeftOrRight(string testString)
            {
                return IsLeft(testString) || IsRight(testString);
            }

        }

        #endregion


        #region enums

        public enum ItemDelimiters
        {
            Comma,
            Pipe
        }

        public enum NestingPairs
        {
            Braces,
            Brackets
        }


        #endregion
    }
}
