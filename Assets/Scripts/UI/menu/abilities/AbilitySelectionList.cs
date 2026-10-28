using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySelectionList : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isActiveList;

    private AbilitySelectionMenu menu;
    private AbilitySelectionItem itemPrefab;
    private Transform dragLayer;

    private readonly List<AbilitySelectionItem> items = new();

    public bool IsActiveList => isActiveList;

    public IReadOnlyList<Ability> Abilities
    {
        get
        {
            List<Ability> result = new();

            foreach (AbilitySelectionItem item in items)
            {
                if (item != null && item.Ability != null)
                    result.Add(item.Ability);
            }

            return result;
        }
    }

    public void Initialize(
        AbilitySelectionMenu selectionMenu,
        AbilitySelectionItem prefab,
        Transform newDragLayer)
    {
        menu = selectionMenu;
        itemPrefab = prefab;
        dragLayer = newDragLayer;
    }

    public void SetAbilities(IEnumerable<Ability> abilities)
    {
        foreach (AbilitySelectionItem item in items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        items.Clear();

        foreach (Ability ability in abilities)
        {
            if (ability == null)
                continue;

            AbilitySelectionItem item = Instantiate(itemPrefab, transform);
            item.Initialize(ability, this, dragLayer);
            items.Add(item);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        AbilitySelectionItem item =
            eventData.pointerDrag.GetComponent<AbilitySelectionItem>();

        if (item == null || item.OwnerList == this)
            return;

        menu.TryMoveAbility(item, this);
    }

    public bool Contains(Ability ability)
    {
        foreach (AbilitySelectionItem item in items)
        {
            if (item != null && item.Ability == ability)
                return true;
        }

        return false;
    }

    public void TransferItemTo(
        AbilitySelectionItem item,
        AbilitySelectionList target)
    {
        if (item == null || target == null || !items.Remove(item))
            return;

        target.items.Add(item);
        item.transform.SetParent(target.transform, false);
        item.SetOwnerList(target);
        item.transform.SetAsLastSibling();
    }

    public void ReturnItem(
        AbilitySelectionItem item,
        Transform originalParent,
        int originalIndex)
    {
        if (item == null || originalParent == null)
            return;

        item.transform.SetParent(originalParent, false);
        item.transform.SetSiblingIndex(originalIndex);
    }
}