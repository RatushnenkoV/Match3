using UnityEngine;
using System.Threading;
using System.Collections.Generic;

class BombAction: BonusAction
{
    public BombAction()
    {
        this.sprite = Resources.Load<Sprite>("Elements/bombBonus");
    }

    public override void Action(Element element)
    {
        int x = element.fieldPosition.x, y = element.fieldPosition.y;
        ElementController elementController = element.elementController;
        Field field = elementController.field;

        new Timer((object state) =>
        {
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i >= 0 && i < field.Scales.x &&
                        j >= 0 && j < field.Scales.y &&
                        field.field[i, j] != null)
                    {
                        elementController.ActivateElement(field.field[i, j]);
                    }
                }
            }
        } , null, 250, Timeout.Infinite);
    }
}