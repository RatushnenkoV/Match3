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
        int X = centredElement.fieldPosition.x, Y = centredElement.fieldPosition.y;
        bool left = true, right = true, up = true, down = true;

        for (int i = 1; i < Mathf.Max(field.GetLength(0), field.GetLength(1)); i++)
        {
            if (left)
            {
                if (X - i < 0 || field[X - i, Y] == null || field[X - i, Y].elementType != elType)
                {
                    left = false;
                }
                else
                {
                    HorizontalMatch.Add(field[X - i, Y]);
                }
            }
            if (right)
            {
                if (X + i >= field.GetLength(0) || field[X + i, Y] == null || field[X + i, Y].elementType != elType)
                {
                    right = false;
                }
                else
                {
                    HorizontalMatch.Add(field[X + i, Y]);
                }
            }
            if (up)
            {
                if (Y - i < 0 || field[X, Y - i] == null || field[X, Y - i].elementType != elType)
                {
                    up = false;
                }
                else
                {
                    VerticalMatch.Add(field[X, Y - i]);
                }
            }
            if (down)
            {
                if (Y + i >= field.GetLength(1) || field[X, Y + i] == null || field[X, Y + i].elementType != elType)
                {
                    down = false;
                }
                else
                {
                    VerticalMatch.Add(field[X, Y + i]);
                }
            }
            if (!left && !right && !down && !up)
            {
                break;
            }
        }

        if (HorizontalMatch.Count >= minMatch - 1 && VerticalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Both;
            HorizontalMatch.Add(field[X, Y]);
            HorizontalMatch.AddRange(VerticalMatch);
            return HorizontalMatch;
        }

        if (HorizontalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Horizontal;
            HorizontalMatch.Add(field[X, Y]);
            return HorizontalMatch;
        }

        if (VerticalMatch.Count >= minMatch - 1)
        {
            direction = Direction.Vertical;
            VerticalMatch.Add(field[X, Y]);
            return VerticalMatch;
        }

        direction = Direction.None;
        return null;
    }

    bool SkippedElementInMatch(List<Element> match, ref ElementType elementType, ref Vector2Int position)
    {
        Dictionary<ElementType, int> dict = new Dictionary<ElementType, int>();
        foreach (var el in match)
        {
            if (dict.ContainsKey(el.elementType))
            {
                dict[el.elementType]++;
            } else
            {
                dict.Add(el.elementType, 1);
            }
        }

        if (dict.Count != 2) return false;
        
        foreach (var pair in dict)
        {
            if (pair.Value == 2)
            {
                elementType = pair.Key;
            }
        }

        foreach (var pair in dict)
        {
            if (pair.Value == 1)
            {
                foreach (var el in match)
                {
                    if (el.elementType != elementType)
                    {
                        position = el.fieldPosition;
                        break;
                    }
                }
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
                ElementType elType = (ElementType)0;
                Vector2Int position = new Vector2Int(0, 0);

                if (i+3 < Scales.x)
                {
                    if (field[i, j] != null && field[i + 1, j] != null && field[i + 2, j] != null)
                    {
                        List<Element> match = new List<Element>() { field[i, j], field[i + 1, j], field[i + 2, j] };
                        if (SkippedElementInMatch(match, ref elType, ref position))
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
                        if (SkippedElementInMatch(match, ref elType, ref position))
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
