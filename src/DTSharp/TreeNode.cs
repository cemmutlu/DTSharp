using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DTSharp
{
    /// <summary>
    /// Data structure for a decision tree
    /// </summary>
    public class TreeNode
    {
        [XmlIgnore]
        public TreeNode Parent { get; set; }

       

        public OutputProperties OutputProperties { get; set; }
        public Feature Feature { get; set; }
        public Dictionary<FeatureValue, TreeNode> ChildNodes { get; set; } = new Dictionary<FeatureValue, TreeNode>();



        [XmlIgnore]
        public IEnumerable<TreeNode> Ancestors
        {
            get
            {
                var cursor = Parent;
                while (cursor != null)
                {
                    yield return cursor;
                    cursor = cursor.Parent;
                }
            }
        }
        public bool IsRoot => Parent == null;
        public bool IsLeaf => ChildNodes.Count == 0;

        public IEnumerable<TreeNode> DescendantsAndSelf
        {
            get
            {
                return new[] { this }.Concat(ChildNodes.Select(x => x.Value).SelectMany(x => x.DescendantsAndSelf));
            }
        }


        public TreeNode FindNode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
