using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace Doinject.Context
{
    internal struct Item
    {
        public TargetTypeInfo Type { get; set; }
        public IInternalResolver Resolver { get; set; }
    }

    public class ContextTreeWindow : EditorWindow
    {
        [SerializeField]
        protected VisualTreeAsset uxml;
        [SerializeField]
        protected VisualTreeAsset itemUXML;

        private TreeView treeView;
        private MultiColumnTreeView instancesView;
        private ContextNode selectedNode;
        private List<KeyValuePair<TargetTypeInfo, IInternalResolver>> bindingDataSource;
        private List<TreeViewItemData<Item>> treeViewDataSource;

        [MenuItem("Window/Doinject/DI Context Tree")]
        public static void Summon()
        {
            GetWindow<ContextTreeWindow>("DI Context Tree");
        }

        private IList<TreeViewItemData<ContextNode>> BuildTree(ContextNode targetNode)
        {
            var items = new List<TreeViewItemData<ContextNode>>();
            foreach (var node in targetNode.Children)
            {
                var children = BuildTree(node).ToList();
                items.Add(new TreeViewItemData<ContextNode>(node.Context.Id, node, children));
            }
            return items;
        }

        private void CreateGUI()
        {
            uxml.CloneTree(rootVisualElement);
            treeView = rootVisualElement.Q<TreeView>();
            instancesView = rootVisualElement.Q<MultiColumnTreeView>("Instances");

            treeView.SetRootItems(BuildTree(ContextTracker.Instance.Root));
            treeView.makeItem = () => new Label();
            treeView.bindItem = (VisualElement element, int index) =>
                (element as Label).text = treeView.GetItemDataForIndex<ContextNode>(index).Context.ToString();
            treeView.selectionChanged += OnTreeItemSelected;

            var typeColumn = instancesView.columns["Type"];
            var resolverColumn = instancesView.columns["Resolver"];
            var strategyColumn = instancesView.columns["Strategy"];
            var instanceCountColumn = instancesView.columns["InstanceCount"];
            instancesView.SetRootItems(treeViewDataSource);
            typeColumn.makeCell = itemUXML.CloneTree;
            resolverColumn.makeCell = itemUXML.CloneTree;
            strategyColumn.makeCell = itemUXML.CloneTree;
            instanceCountColumn.makeCell = itemUXML.CloneTree;
            typeColumn.bindCell = (e, i)
                => e.Q<Label>().text = GenericTypeToString(instancesView.GetItemDataForIndex<Item>(i).Type.Type);
            resolverColumn.bindCell = (e, i)
                => e.Q<Label>().text = instancesView.GetItemDataForIndex<Item>(i).Resolver.ShortName;
            strategyColumn.bindCell = (e, i)
                => e.Q<Label>().text = instancesView.GetItemDataForIndex<Item>(i).Resolver.StrategyName;
            instanceCountColumn.bindCell = (e, i)
                => e.Q<Label>().text = instancesView.GetItemDataForIndex<Item>(i).Resolver.InstanceCount.ToString();
        }

        private string GenericTypeToString(Type type)
        {
            var resolverName = type.Name;
            if (resolverName.Contains("`"))
                resolverName = resolverName.Split("`")[0];
            var result = $"{resolverName}";
            var types = type.GenericTypeArguments
                .Select(x => x.Name);
            if (types.Any())
                result += $"<{string.Join(", ", types)}>";
            return result;
        }

        private string ResolverToString(IInternalResolver resolver)
        {
            var resolverType = resolver.GetType();
            var resolverName = resolverType.Name;
            if (resolverName.Contains("`"))
                resolverName = resolverName.Split("`")[0];
            var result = $"{resolverName}";
            var types = resolverType.GenericTypeArguments
                .Select(x => x.Name);
            if (types.Any())
                result += $"<{string.Join(", ", types)}>";
            return result;
        }

        private void OnTreeItemSelected(IEnumerable<object> selection)
        {
            selectedNode = selection.FirstOrDefault() as ContextNode;
            if (selectedNode is not null)
            {
                bindingDataSource = selectedNode.Context.RawContainer.ReadOnlyBindings.ToList();
                var id = 0;
                treeViewDataSource = selectedNode.Context.RawContainer.ReadOnlyBindings
                    .Select(x =>
                        new TreeViewItemData<Item>(id++,
                            new Item {
                                Type = x.Key,
                                Resolver = x.Value,
                            }))
                    .ToList();
            }
            else
            {
                bindingDataSource = new List<KeyValuePair<TargetTypeInfo, IInternalResolver>>();
                treeViewDataSource = new List<TreeViewItemData<Item>>();
            }

            instancesView.SetRootItems(treeViewDataSource);
            instancesView.Rebuild();
        }

        private void Update()
        {
            if (ContextTracker.Instance.Dirty)
            {
                ContextTracker.Instance.ResetDirty();
                treeView.SetRootItems(BuildTree(ContextTracker.Instance.Root));
                treeView.Rebuild();
            }
        }
    }
}