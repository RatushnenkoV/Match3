using UnityEngine;
using System.Collections.Generic;

public enum BonusType { VerticalLine, HorizontalLine, Bomb};

public class BonusSpriteDictionary
{
    public static Dictionary<BonusType, Sprite> spriteDict = new Dictionary<BonusType, Sprite>()
    {
        { BonusType.VerticalLine, Resources.Load<Sprite>("Elements/lineBonus") },
        { BonusType.HorizontalLine, Resources.Load<Sprite>("Elements/lineBonus") },
        { BonusType.Bomb, Resources.Load<Sprite>("Elements/bombBonus") }
    };

    public static Sprite GetSprite(BonusType bonusType)
    {
        return spriteDict[bonusType];
    }
}

public class Bonus : MonoBehaviour
{
    public Element ownerElement;
    
    public BonusAction bonusAction;

    public void SetBonusAction(BonusAction bonusAction)
    {
        this.bonusAction = bonusAction;
        GetComponent<SpriteRenderer>().sprite = bonusAction.sprite;
        if (bonusAction is LineAction && ((LineAction)bonusAction).direction == Direction.Vertical)
        {
            Quaternion target = Quaternion.Euler(0, 0, 90);
            transform.rotation = target;
        }
    }

    public virtual void Action()
    {
        if (bonusAction != null)
        {
            bonusAction.Action(ownerElement);
        }
    }
}

