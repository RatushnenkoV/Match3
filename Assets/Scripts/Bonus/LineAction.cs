using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class LineAction: BonusAction
{
    public Direction direction;

    public LineAction(Direction direction)
    {
        this.direction = direction;
        this.sprite = Resources.Load<Sprite>("Elements/lineBonus");
    }

    public override void Action(Element element)
    {
        int x = element.fieldPosition.x, y = element.fieldPosition.y;
        ElementController elementController = element.elementController;
        Field field = elementController.field;

        new Timer((object state) => {
            if (direction == Direction.Vertical)
            {
                for (int i = 0; i < field.Scales.y; i++)
                {
                    if (field.field[x, i] != null)
                    {
                        elementController.ActivateElement(field.field[x, i]);
                    }
                }
            }
            else if (direction == Direction.Horizontal)
            {
                for (int i = 0; i < field.Scales.x; i++)
                {
                    if (field.field[i, y] != null)
                    {
                        elementController.ActivateElement(field.field[i, y]);
                    }
                }
            }
        }, null, 250, Timeout.Infinite);
    }
}