using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace schema
{
    internal class datas
    {
        private IEnumerable<INode> nodes;
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
        public datas(IEnumerable<INode> nodes)
        {
            this.nodes = nodes;
        }
        public INode getRoot()
        {
            return nodes.Single(x => x.Type == NodeType.Root);
        }
        public IEnumerable<INode> getnodes()
        {
            return nodes;
        }

        public INode getParentParent(INode node)//back one folder
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
    }
}
