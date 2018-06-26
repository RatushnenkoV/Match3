using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ElementController {
    public Game game;
    public Field field = Field.GetInstance();

    public bool canPlay = true;
    public float timeFromLastSwap = 0;

    public GameObject prefab;
    public AudioClip matchClip, swapClip;
    public AudioSource audioSource;

    #region Singleton
    static readonly ElementController instance = new ElementController();

    ElementController()
    {
        
    }

    public static ElementController GetInstance()
    {
        return instance;
    }

    public void Init()
    {
        audioSource = Camera.main.GetComponent<AudioSource>();
        matchClip = Resources.Load<AudioClip>("Audio/Match");
        swapClip = Resources.Load<AudioClip>("Audio/Motion");
    }
    #endregion

    #region Field control
    public void FillField()
    {
        timeFromLastSwap = 0;
        field.Clear();
        for (int i = 0; i < field.Scales.x; i++)
        {
            for (int j = 0; j < field.Scales.y; j++)
            {
                GameObject elementPrefab = (GameObject)Resources.Load("Prefabs/Element");
                GameObject elementGO = MonoBehaviour.Instantiate(elementPrefab);
                Element element = elementGO.GetComponent<Element>();
                ElementType elementType = Element.GetRandomElementType();
                while ((i - 2 >= 0 &&
                    field.field[i - 1, j].elementType == elementType &&
                    field.field[i - 2, j].elementType == elementType) ||
                    (j - 2 >= 0 &&
                    field.field[i, j - 1].elementType == elementType &&
                    field.field[i, j - 2].elementType == elementType))
                {
                    elementType = Element.GetNextElementType(elementType);
                }
                field.field[i, j] = element;
                element.Init(new Vector2Int(i, j), elementType);
            }
        }
        CheckMatches();
    }

    public void UnhighlightAll()
    {
        for (int i = 0; i < field.Scales.x; i++)
        {
            for (int j = 0; j < field.Scales.y; j++)
            {
                if (field.field[i, j] != null)
                {
                    field.field[i, j].Unhighlight();
                }
            }
        }
        timeFromLastSwap = 0;
    }

    public void Match(Element centerElement)
    {
        Direction direction;
        List<Element> match = field.GetMatches(centerElement, out direction);
        if (match != null)
        {
            foreach (Element el in match)
            {
                if (el == centerElement && match.Count > 3)
                {
                    el.ActivateBonus();
                    if (match.Count == 4)
                    {
                        el.SetBonus(new LineAction(direction));
                    } else
                    {
                        el.SetBonus(new BombAction());
                    }
                    game.AddScore();
                }
                else
                {
                    ActivateElement(el);
                }
            }
            audioSource.PlayOneShot(matchClip);
        }
    }

    public void CheckMatches()
    {
        for (int i = 0; i < field.Scales.x; i++)
        {
            for (int j = 0; j < field.Scales.y; j++)
            {
                List<Element> match = field.GetMatches(field.field[i, j]);
                if (match != null)
                {
                    return;
                }
            }
        }
        if (field.GetProbableMatchingElement() != null)
        {
            return;
        }

        FillField();
    }

    public void DropElements()
    {
        for (int i = 0; i < field.Scales.x; i++)
        {
            for (int j = field.Scales.y - 1; j >= 0; j--)
            {
                if (field.field[i, j] == null)
                {
                    bool falled = false;
                    for (int k = j - 1; k >= 0; k--)
                    {
                        if (field.field[i, k] != null)
                        {
                            field.field[i, k].SetFieldPosition(new Vector2Int(i, j));
                            field.field[i, j] = field.field[i, k];
                            field.field[i, k] = null;
                            falled = true;
                            break;
                        }
                    }
                    if (!falled)
                    {
                        GameObject elementPrefab = (GameObject)Resources.Load("Prefabs/Element");
                        GameObject elementGO = MonoBehaviour.Instantiate(elementPrefab);
                        Element element = elementGO.GetComponent<Element>();
                        field.field[i, j] = element;
                        element.Init(new Vector2Int(i, j), Element.GetRandomElementType());
                    }
                }
            }
        }
        CheckMatches();
    }

    public void MatchAll()
    {
        for (int i = 0; i < field.Scales.x; i++)
        {
            for (int j = 0; j < field.Scales.y; j++)
            {
                Match(field.field[i, j]);
            }
        }
    }
    #endregion Field control

    #region Element control
    public void Swap(Element element1, Element element2, bool withCheck = false)
    {
        ActionDelegate AfterSwap = null;
        if (withCheck)
        {
            AfterSwap = (() =>
            {
                var match1 = field.GetMatches(element1);
                var match2 = field.GetMatches(element2);
                if ((match1 == null) && (match2 == null))
                {
                    Swap(element1, element2);
                }
                else
                {
                    UnhighlightAll();
                    timeFromLastSwap = 0;
                }
            });
        }

        Vector2Int fieldPos1 = element1.fieldPosition;
        element1.SetFieldPosition(element2.fieldPosition);
        element2.SetFieldPosition(fieldPos1, AfterSwap);
        field.field[element1.fieldPosition.x, element1.fieldPosition.y] = element1;
        field.field[element2.fieldPosition.x, element2.fieldPosition.y] = element2;

        audioSource.PlayOneShot(swapClip);
    }

    public void ActivateElement(Element element, ActionDelegate doAfterActivate = null)
    {
        element.SetGoalColor(Color.red, () =>
        {
            if (doAfterActivate != null)
            {
                doAfterActivate.Invoke();
            }
            if (element.bonus != null)
            {
                element.bonus.GetComponent<Bonus>().Action();
            }
            DeleteElement(element);
            DropElements();
        });
    }

    public void DeleteElement(Element element)
    {
        if (game != null)
        {
            game.AddScore();
        }
        if (field.field[element.fieldPosition.x, element.fieldPosition.y] == element)
        {
            field.field[element.fieldPosition.x, element.fieldPosition.y] = null;
        }
        MonoBehaviour.Destroy(element.gameObject);
    }
    #endregion Element control

    public void Update()
    {
        if (timeFromLastSwap >= 0) 
        {
            timeFromLastSwap += Time.deltaTime;
            if (timeFromLastSwap >= 5)
            {
                Element probableMatchingElement = field.GetProbableMatchingElement();
                if (probableMatchingElement != null)
                {
                    probableMatchingElement.Highlight();
                }
                timeFromLastSwap = -1;
            }
        }
    }
}
