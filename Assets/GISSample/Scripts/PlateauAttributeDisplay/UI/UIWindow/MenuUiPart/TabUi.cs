using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TabUi
{
    private readonly VisualElement tabRoot;
    private const string ClassNameTab = "tab";
    private const string ClassNameTabSelected = "tab-selected";
    private const string ClassNameContent = "tab-content";
    private const string ClassNameContentRoot = "tab-contents";
    private const string ClassNameContentUnselected = "tab-content-unselected";
    private VisualElement selectedTab;
    private Dictionary<VisualElement, VisualElement> tabToContent;
    public TabUi(VisualElement tabRoot)
    {
        this.tabRoot = tabRoot;
        var tabs = GetAllTabs().ToList();
        
        // タブとコンテンツを紐付けます。
        // タブの順番と、".tab-contents"の子の".tab-content"の順番が一致するものと過程して紐付けます。
        var contentsRoot = tabRoot.Q(className: ClassNameContentRoot);
        var contents = contentsRoot.Children().Where(e => e.ClassListContains(ClassNameContent)).ToList();
        tabToContent = new();
        for (int i = 0; i < tabs.Count; i++)
        {
            tabToContent.Add(tabs[i], contents[i]);
        }
        
        // タブクリック時のコールバックを追加
        tabs.ForEach(tab => tab.RegisterCallback<ClickEvent>(OnTabClicked));
        
        ChangeSelectedTab(tabs.First());
    }

    private void OnTabClicked(ClickEvent e)
    {
        var clickedTab = e.currentTarget as VisualElement;
        if (clickedTab == selectedTab) return;
        ChangeSelectedTab(clickedTab);
    }

    private UQueryBuilder<VisualElement> GetAllTabs()
    {
        return tabRoot.Query(className: ClassNameTab);
    }

    private void ChangeSelectedTab(VisualElement nextSelectedTab)
    {
        selectedTab = nextSelectedTab;
        foreach (var (tab, content) in tabToContent)
        {
            if (tab == nextSelectedTab)
            {
                tab.AddToClassList(ClassNameTabSelected);
                content.RemoveFromClassList(ClassNameContentUnselected);
            }
            else
            {
                tab.RemoveFromClassList(ClassNameTabSelected);
                content.AddToClassList(ClassNameContentUnselected);
            }
        }
    }
    
}
