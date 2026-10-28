using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AbilitySelectionItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text abilityName;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalIndex;
    private Transform dragLayer;

    public Ability Ability { get; private set; }
    public AbilitySelectionList OwnerList { get; private set; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(
        Ability ability,
        AbilitySelectionList owner,
        Transform newDragLayer)
    {
        Ability = ability;
        OwnerList = owner;
        dragLayer = newDragLayer;

        if (icon != null)
            icon.sprite = ability.icon;

        if (abilityName != null)
            abilityName.text = ability.abilityName;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Ability == null || OwnerList == null)
            return;

        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex();

        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Если ни один список не принял элемент — возвращаем его.
        if (transform.parent == dragLayer && OwnerList != null)
            OwnerList.ReturnItem(this, originalParent, originalIndex);
    }

    public void SetOwnerList(AbilitySelectionList newOwner)
    {
        OwnerList = newOwner;
    }
}