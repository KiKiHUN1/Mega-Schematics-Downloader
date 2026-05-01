using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace schema
{
    internal class InodeDataClass
    {
        private readonly IEnumerable<INode> nodes;
        private IEnumerable<INode>? Searchednodes;
        public bool IsNulll()
        {
            if (nodes == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public InodeDataClass(IEnumerable<INode> nodes)
        {
            this.nodes = nodes;
            Searchednodes = null;
        }
        public INode GetRoot()
        {
            return nodes.Single(x => x.Type == NodeType.Root);
        }
        public IEnumerable<INode> Getnodes(bool filtered = false)
        {
            if (!filtered)
            {
                return nodes;
            }
            else
            {
                if (Searchednodes != null)
                {
                    return Searchednodes;
                }
                else
                {
                    return new List<INode>();
                }
            }
            
        }
       

        public INode GetParentParent(INode node)//back one folder
        {
            INode parent = nodes.Single(x => x.Id == node.ParentId);
            return parent;
        }
        public string GetParents(INode node)
        {
            List<string> parents = new List<string>();
            while (node.ParentId != null)
            {
                INode parentNode = nodes.Single(x => x.Id == node.ParentId);
                parents.Insert(0, parentNode.Name);
                node = parentNode;
            }

            return string.Join("\\", parents);
        }
        public int SearchFor(string keyword)
        {
            Searchednodes = nodes.Where(x => x.Type == NodeType.File).Where(x => x.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
            return Searchednodes.Count();
        }
    }
}
