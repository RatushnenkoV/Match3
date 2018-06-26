using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ActionDelegate();
public enum ElementType { Apple, Milk, Orange, Bread, Broccoli }

public class ElementSpriteDictionary
{
    public static Dictionary<ElementType, Sprite> spriteDict = new Dictionary<ElementType, Sprite>()
    {
        { ElementType.Apple,  Resources.Load<Sprite>("Elements/apple") },
        { ElementType.Milk, Resources.Load<Sprite>("Elements/milk") },
        { ElementType.Orange,  Resources.Load<Sprite>("Elements/orange") },
        { ElementType.Bread, Resources.Load<Sprite>("Elements/bread") },
        { ElementType.Broccoli, Resources.Load<Sprite>("Elements/broccoli") }
    };

    public static Sprite GetSprite(ElementType elType)
    {
        return spriteDict[elType];
    }
}

public class Element : MonoBehaviour {

    public ElementController elementController = ElementController.GetInstance();

    public static Vector3 scales;
    public static float space = 0.5f;

    #region Gameplay fields
    public ElementType elementType;
    public Vector2Int fieldPosition;
    public static Element selectedElement;
    public GameObject bonus;
    #endregion Gameplay fields

    #region Update fields
    public bool hasGoalPosition;
    public Vector3 goalPosition;
    public ActionDelegate DoAfterMove;

    public bool hasGoalColor;
    public Color goalColor;
    public ActionDelegate DoAfterSetColor;

    public bool hasGoalScales;
    public Vector3 goalScales;

    public bool highlighted;
    public float highlightValue = 0;
    #endregion Update filds

    #region Initialization
    public void Init(Vector2Int fieldPosition, ElementType elementType)
    {
        this.elementType = elementType;
        this.fieldPosition = fieldPosition;

        GetComponent<SpriteRenderer>().sprite = ElementSpriteDictionary.GetSprite(elementType);

        scales = GetComponent<SpriteRenderer>().bounds.size;

        transform.position = FieldToScenePos(new Vector2Int(fieldPosition.x, -1));
        SetGoalPosition(FieldToScenePos(fieldPosition));
    }
    
	void Start () {
		if (!elementController.canPlay)
        {
            GetComponent<SpriteRenderer>().color = Color.black;
        }
        bonus = transform.GetChild(0).gameObject;
        bonus.GetComponent<Bonus>().ownerElement = this;
    }
    #endregion Initialization

    #region Update
    void Update () {
        UpdatePosition();
        UpdateColor();
        UpdateScales();

        if (highlighted)
        {
            highlightValue += 0.1f;
            float scale = 1 + (Selected() ? 1 : 0);
            transform.localScale = new Vector3(scale + Mathf.Sin(highlightValue) / 4, scale + Mathf.Sin(highlightValue) / 4, 1);
        }
	}

    public void UpdatePosition()
    {
        if (hasGoalPosition)
        {
            Vector3 speed = (goalPosition - transform.position) / 4;
            float minSpeed = 0.1f;
            if (speed.magnitude <= minSpeed)
            {
                transform.position = goalPosition;
                hasGoalPosition = false;

                if (DoAfterMove != null)
                {
                    DoAfterMove.Invoke();
                    DoAfterMove = null;
                }

                elementController.Match(this);
            }
            else
            {
                transform.position += speed;
            }
        }
    }

    public void UpdateColor()
    {
        if (hasGoalColor)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Color dColor = (goalColor - spriteRenderer.color) / 8;
            float minDColor = 0.01f;
            if (Mathf.Abs(dColor.r + dColor.g + dColor.b) <= minDColor)
            {
                spriteRenderer.color = goalColor;
                hasGoalColor = false;

                if (DoAfterSetColor != null)
                {
                    DoAfterSetColor.Invoke();
                    DoAfterSetColor = null;
                }
            }
            else
            {
                spriteRenderer.color += dColor;
            }
        }
    }

    public void UpdateScales()
    {
        if (hasGoalScales)
        {
            Vector3 dScales = (goalScales - transform.localScale) / 4;
            float minDSpeed = 0.1f;
            if (dScales.magnitude <= minDSpeed)
            {
                transform.localScale = goalScales;
                hasGoalScales = false;
            }
            else
            {
                transform.localScale += dScales;
            }
        }
    }
    #endregion Update

    #region Setters
    public void SetFieldPosition(Vector2Int fieldPosition, ActionDelegate DoAfterMove = null)
    {
        this.fieldPosition = fieldPosition;
        SetGoalPosition(FieldToScenePos(fieldPosition), DoAfterMove);
    }

    public void SetGoalPosition(Vector3 goalPosition, ActionDelegate DoAfterMove = null)
    {
        this.DoAfterMove = DoAfterMove;
        this.goalPosition = goalPosition;
        this.hasGoalPosition = true;
    }

    public void SetGoalColor(Color goalColor, ActionDelegate DoAfterSetColor = null)
    {
        this.DoAfterSetColor = DoAfterSetColor;
        this.goalColor = goalColor;
        this.hasGoalColor = true;
    }

    public void SetGoalScales(Vector3 goalScales)
    {
        this.goalScales = goalScales;
        this.hasGoalScales = true;
    }
    #endregion Setters

    #region Static methods
    public static Vector2 FieldToScenePos(Vector2Int fieldPos)
    {
        Field field = Field.GetInstance();
        Vector3 fieldScales = field.GetSceneScales();
        return field.position + new Vector3(space + fieldPos.x * (scales.x + space), - space - fieldPos.y * (scales.y + space), 0) - new Vector3(fieldScales.x/2, -fieldScales.y/2, 0);
    }

    public static ElementType GetRandomElementType()
    {
        return (ElementType)Random.Range(0, 5);
    }

    public static ElementType GetNextElementType(ElementType elType)
    {
        elType++;
        if ((int)elType >= 5)
        {
            elType = 0;
        }
        return elType;
    }

    public static void SelectElement(Element element)
    {
        if (selectedElement != null)
        {
            selectedElement.Unselect();
        }
        selectedElement = element;
        if (selectedElement != null)
        {
            selectedElement.Select();
        }
    }
    #endregion Static methods

    #region Selection
    public void OnMouseDown()
    {
        if (elementController.canPlay && !hasGoalPosition)
        {
            if (selectedElement == null)
            {
                SelectElement(this);
            }
            else
            {
                if (selectedElement == this)
                {
                    SelectElement(null);
                }
                else
                {
                    if (Mathf.Abs(selectedElement.fieldPosition.x - fieldPosition.x) + Mathf.Abs(selectedElement.fieldPosition.y - fieldPosition.y) == 1)
                    {
                        ElementController.GetInstance().Swap(this, selectedElement, true);
                        SelectElement(null);
                    }
                    else
                    {
                        SelectElement(this);
                    }
                }
            }
        }
    }

    public void Select()
    {
        highlighted = false;
        SetGoalScales(new Vector3(1.5f, 1.5f, 1));
    }

    public void Unselect()
    {
        SetGoalScales(new Vector3(1, 1, 1));
    }

    public bool Selected()
    {
        return selectedElement == this;
    }

    public void Highlight()
    {
        highlighted = true;
        highlightValue = 0;
    }

    public void Unhighlight()
    {
        highlighted = false;
        transform.localScale = new Vector3(1, 1, 1) * (Selected()?1.5f:1);
    }
    #endregion Selection

    #region Bonuses
    public void SetBonus(BonusAction bonusAction)
    {
        bonus.GetComponent<Bonus>().SetBonusAction(bonusAction);
    }

    public void ActivateBonus()
    {
        bonus.GetComponent<Bonus>().Action();
    }
    #endregion bonuses
}
