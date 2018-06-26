using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Vertical, Horizontal, None, Both };

public class Field {
    public Element[,] field = new Element[8,8];

    public Vector3 position = new Vector3(1.5f, -4, 0);
    public Vector2Int Scales { get { return new Vector2Int(field.GetLength(0), field.GetLength(1)); } }

    #region Singleton
    static readonly Field instance = new Field();

    Field()
    {
    }

    public static Field GetInstance()
    {
        return instance;
    }
    #endregion Singleton

    #region Matching
    public List<Element> GetMatches(Element centredElement)
    {
        Direction direction;
        return GetMatches(centredElement, out direction);
    }

    public List<Element> GetMatches(Element centredElement, out Direction direction)
    {
        int minMatch = 3;

        List<Element> VerticalMatch = new List<Element>();
        List<Element> HorizontalMatch = new List<Element>();

        ElementType elType = centredElement.elementType;
        int x = centredElement.fieldPosition.x, y = centredElement.fieldPosition.y;
        
        int koef = 1;
        for (int k = 0; k < 2; k++)
        {
            for (int i = 1; (x + i * koef >= 0 && x + i * koef < Scales.x && field[x + i * koef, y].elementType == elType); i++)
            {
                HorizontalMatch.Add(field[x + i * koef, y]);
            }
            koef *= -1;
        }

        for (int k = 0; k < 2; k++)
        {
            for (int i = 1; (y + i * koef >= 0 && y + i * koef < Scales.y && field[x, y + i*koef].elementType == elType); i++)
            {
                VerticalMatch.Add(field[x, y + i*koef]);
            }
            koef *= -1;
        }

        if (HorizontalMatch.Count >= minMatch - 1 && VerticalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Both;
            HorizontalMatch.Add(field[x, y]);
            HorizontalMatch.AddRange(VerticalMatch);
            return HorizontalMatch;
        }

        if (HorizontalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Horizontal;
            HorizontalMatch.Add(field[x, y]);
            return HorizontalMatch;
        }

        if (VerticalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Vertical;
            VerticalMatch.Add(field[x, y]);
            return VerticalMatch;
        }

        direction = Direction.None;
        return null;
    }

    bool SkippedElementInMatch(List<Element> match, out ElementType elementType, out Vector2Int position)
    {
        Dictionary<ElementType, int> appearancesCount = new Dictionary<ElementType, int>();
        foreach (var el in match)
        {
            if (appearancesCount.ContainsKey(el.elementType))
            {
                appearancesCount[el.elementType]++;
            } else
            {
                appearancesCount.Add(el.elementType, 1);
            }
        }

        // Если в словаре только 1 тип - матч собран. Если больше 2х - его нельзя собрать
        // Если в словаре нет типа, который встречается только 1 раз - матч нельзя собрать
        elementType = (ElementType)0;
        position = new Vector2Int();
        if (appearancesCount.Count != 2 || !appearancesCount.ContainsValue(1)) return false;

        foreach (var pair in appearancesCount)
        {
            if (pair.Value == match.Count-1)
            {
                elementType = pair.Key;
            }
        }

        foreach (var el in match)
        {
            if (el.elementType != elementType)
            {
                position = el.fieldPosition;
                break;
            }
        }
        return true;
    }

    public Element GetProbableMatchingElement()
    {
        for (int i = 0; i < Scales.x; i++)
        {
            for (int j = 0; j < Scales.y; j++)
            {
                ElementType elType;
                Vector2Int position; 

                if (i+3 < Scales.x)
                {
                    if (field[i, j] != null && field[i + 1, j] != null && field[i + 2, j] != null)
                    {
                        List<Element> match = new List<Element>() { field[i, j], field[i + 1, j], field[i + 2, j] };
                        if (SkippedElementInMatch(match, out elType, out position))
                        {
                            if (position.y - 1 >= 0 && field[position.x, position.y - 1] != null && field[position.x, position.y - 1].elementType == elType) return field[position.x, position.y - 1];
                            if (position.y + 1 < Scales.y && field[position.x, position.y + 1] != null && field[position.x, position.y + 1].elementType == elType) return field[position.x, position.y + 1];
                        }
                    }
                }

                if (j+3 < Scales.y)
                {
                    if (field[i, j] != null && field[i, j + 1] != null && field[i, j + 2] != null)
                    {
                        List<Element> match = new List<Element>() { field[i, j], field[i, j + 1], field[i, j + 2] };
                        if (SkippedElementInMatch(match, out elType, out position))
                        {
                            if (position.x - 1 >= 0 && field[position.x - 1, position.y] != null && field[position.x - 1, position.y].elementType == elType) return field[position.x - 1, position.y];
                            if (position.x + 1 < Scales.x && field[position.x + 1, position.y] != null && field[position.x + 1, position.y].elementType == elType) return field[position.x + 1, position.y];
                        }
                    }
                }
            }
        }
        return null;
    }
    #endregion Matching

    #region Element colntrol
    public void Clear()
    {
        for (int i = 0; i < Scales.x; i++)
        {
            for (int j = 0; j < Scales.y; j++)
            {
                if (field[i,j] != null)
                {
                    MonoBehaviour.Destroy(field[i, j].gameObject);
                    field[i, j] = null;
                }
            }
        }
    }

    public void SetElementsBlure(bool blure)
    {
        for (int i = 0; i < Scales.x; i++)
        {
            for (int j = 0; j < Scales.y; j++)
            {
                if (field[i,j] != null)
                {
                    Element el = field[i, j];
                    el.SetGoalColor(blure? Color.black: Color.white);
                }
            }
        }
    }
    #endregion Element control

    public Vector3 GetSceneScales()
    {
        return new Vector3(Scales.x * (Element.scales.x + Element.space) + Element.space, Scales.y * (Element.scales.y + Element.space) + Element.space, 0);
    }
}
