using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace Doinject.Context
{
    public class ContextTreeWindow : EditorWindow
    {
        [SerializeField]
        protected VisualTreeAsset uxml;

        private TreeView treeView;
        private ListView bindingsView;
        private ListView instancesView;
        private TabView tabView;
        private ContextNode selectedNode;
        private List<KeyValuePair<TargetTypeInfo, ConcurrentObjectBag>> instanceDataSource;
        private List<KeyValuePair<TargetTypeInfo, IInternalResolver>> bindingDataSource;

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
            bindingsView = rootVisualElement.Q<ListView>("Bindings");
            instancesView = rootVisualElement.Q<ListView>("Instances");
            tabView = rootVisualElement.Q<TabView>();

            treeView.SetRootItems(BuildTree(ContextTracker.Instance.Root));
            treeView.makeItem = () => new Label();
            treeView.bindItem = (VisualElement element, int index) =>
                (element as Label).text = treeView.GetItemDataForIndex<ContextNode>(index).Context.ToString();
            treeView.selectionChanged += OnTreeItemSelected;

            bindingsView.makeItem = () => new Label();
            bindingsView.bindItem = (VisualElement element, int index) =>
                (element as Label).text = $"{bindingDataSource[index].Key.Type.Name} => {ResolverToString(bindingDataSource[index].Value)}";
            instancesView.makeItem = () => new Label();
            instancesView.bindItem = (VisualElement element, int index) =>
                (element as Label).text = $"{instanceDataSource[index].Key.Type.Name} x {instanceDataSource[index].Value.Count}";

            bindingsView.visible = true;
            instancesView.visible = false;

            tabView.activeTabChanged += (from, to) =>
            {
                if (to.tabIndex == 0)
                {
                    bindingsView.visible = true;
                    instancesView.visible = false;
                }
                else
                {
                    bindingsView.visible = false;
                    instancesView.visible = true;
                }
            };
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
            selectedNode = selection.First() as ContextNode;
            bindingDataSource = selectedNode.Context.Container.ReadOnlyBindings.ToList();
            instanceDataSource = selectedNode.Context.Container.ReadOnlyInstanceMap.ToList();
            bindingsView.itemsSource = bindingDataSource;
            instancesView.itemsSource = instanceDataSource;
            bindingsView.Rebuild();
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