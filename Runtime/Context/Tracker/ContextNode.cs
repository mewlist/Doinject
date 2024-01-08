using System;
using System.Collections.Generic;
using System.Linq;

namespace Doinject.Context
{
    public class ContextNode
    {
        public List<ContextNode> Children { get; } = new();
        public Context Context { get; private set; }

        public void Add(Context context)
        {
            var node = Find(context);
            if (node is not null) throw new Exception("Already added");

            if (context.Parent is not null)
            {
                var parentNode = Find(context.Parent);
                if (parentNode is null)
                    Children.Add(new ContextNode {Context = context});
                else
                    parentNode.Children.Add(new ContextNode {Context = context});
            }
            else
            {
                Children.Add(new ContextNode {Context = context});
            }
        }

        public void Remove(Context context)
        {
            var node = FindInChildren(context);
            if (node is null) return; // parent context is already removed
            node.Children.RemoveAll(x => x.Context == context);
        }

        public ContextNode Find(Context context)
        {
            if (Context == context) return this;
            return Children
                .Select(child => child.Find(context))
                .FirstOrDefault(node => node != null);
        }

        public ContextNode Find(int id)
        {
            if (Context?.Id == id) return this;
            return Children
                .Select(child => child.Find(id))
                .FirstOrDefault(node => node != null);
        }

        public ContextNode FindInChildren(Context context)
        {
            if (Children.Any(x => x.Context == context)) return this;
            return Children
                .Select(child => child.FindInChildren(context))
                .FirstOrDefault(node => node != null);
        }
    }
}