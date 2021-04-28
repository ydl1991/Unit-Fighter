using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum CardType
{
    kUnit = 0,
    kEffect
}

public struct Card
{
    // Number of Card Colors
    private static ReadOnlyCollection<Color32> s_kCardColors = new ReadOnlyCollection<Color32> (new Color32[] 
    {
        new Color32(144, 144, 144, 114),    // Grey
        new Color32(  0, 176,  24, 114),    // Green
        new Color32(  0, 169, 255, 114),    // Blue
        new Color32(255,   0, 171, 114),    // Purple
        new Color32(255, 205,   0, 114),    // Gold
    });

    public string elemAttr { get; set; }        // Elemental Attribute
    public string profAttr { get; set; }        // Professional Attribute
    public Color32 color { get; set; }          // Level Color
    public int deckId { get; set; }             // Card Belong to Deck
    public CardType type { get; set; }

    public Card(string elemAttr, string profAttr, Color32 color, int deckId, CardType type)
    {
        this.elemAttr = elemAttr;
        this.profAttr = profAttr;
        this.color = color;
        this.deckId = deckId;
        this.type = type;
    }

    public Card(string elemAttr, string profAttr, int deckId, CardType type)
    {
        this.elemAttr = elemAttr;
        this.profAttr = profAttr;
        this.color = s_kCardColors[deckId];
        this.deckId = deckId;
        this.type = type;
    }
}

public class CardDeck
{
    // Number of Professional Attributes
    private static ReadOnlyCollection<string> s_kProfAttr = new ReadOnlyCollection<string> (new string[] 
    {
        "Guardian", 
        "Fighter", 
        "Assassin", 
        "Gunner"
    });

    // Number of Elemental Attributes by Level
    private static ReadOnlyCollection<List<string>> s_kElemAttrByLevel= new ReadOnlyCollection<List<string>> (new List<string>[] 
    {
        new List<string> { "Metal", "River", "Earth" },
        new List<string> { "Fire", "Ice" },
        new List<string> { "Wind", "Forest" },
        new List<string> { "Thunder" },
        new List<string> { },
    });

    // Special Attributes by Level
    private static ReadOnlyCollection<List<Tuple<string, string>>> s_kSpecialCardByLevel= new ReadOnlyCollection<List<Tuple<string, string>>> (new List<Tuple<string, string>>[] 
    {
        new List<Tuple<string, string>> { },
        new List<Tuple<string, string>> { },
        new List<Tuple<string, string>> { },
        new List<Tuple<string, string>> 
        { 
            Tuple.Create<string, string>("Heal", "Crystal"), 
            Tuple.Create<string, string>("Shield", "Crystal"), 
            Tuple.Create<string, string>("Silence", "Crystal") 
        },
        new List<Tuple<string, string>> 
        { 
            Tuple.Create<string, string>("Sacred", "Crystal"),
            Tuple.Create<string, string>("Sacred", "Pastor"),
            Tuple.Create<string, string>("Sacred", "Rock")
        },
    });

    // Number of Cards in Deck by Level
    private static ReadOnlyCollection<int> s_kNumCardsInDeckByLevel = new ReadOnlyCollection<int> (new int[] 
    {
        39,
        26,
        21,
        13,
        10
    });

    // Drop Rate by Player Level
    private static ReadOnlyCollection<List<float>> s_kDropRateByPlayerLevel = new ReadOnlyCollection<List<float>> (new List<float>[] 
    {
        new List<float> {    1f,     0f,    0f,    0f,     0f },
        new List<float> {    1f,     0f,    0f,    0f,     0f },
        new List<float> { 0.70f, 0.300f,    0f,    0f,     0f },
        new List<float> { 0.55f, 0.300f, 0.15f,    0f,     0f },
        new List<float> { 0.40f, 0.300f, 0.25f, 0.05f,     0f },
        new List<float> { 0.29f, 0.295f, 0.31f, 0.10f, 0.005f },
        new List<float> { 0.24f, 0.280f, 0.31f, 0.15f, 0.020f },
        new List<float> { 0.20f, 0.240f, 0.31f, 0.20f, 0.050f },
        new List<float> { 0.10f, 0.190f, 0.31f, 0.30f, 0.100f },
    });

    private List<Card>[] m_deck;            // Card Deck from Level 1 - 5, Cost from $1 - $5

    private XOrShiftRNG m_rng;              // Random Number Generator

    public CardDeck()
    {
        m_rng = new XOrShiftRNG();
        
        m_deck = new List<Card>[5];
        for (int i = 0; i < m_deck.Length; ++i)
        {
            m_deck[i] = new List<Card>(1000);
        }

        FillDeck();
    }

    public void FillDeck()
    {
        FillRegularCards();
        FillSpecialCards();
        Shuffle();
    }
    
    private void FillRegularCards()
    {
        for (int index = 0; index < m_deck.Length; ++index)
        {
            foreach (string elemAttr in s_kElemAttrByLevel[index])
            {
                foreach (string profAttr in s_kProfAttr)
                {
                    m_deck[index].AddRange(
                        Enumerable.Repeat<Card>(
                            new Card(elemAttr, profAttr, index, CardType.kUnit), 
                            s_kNumCardsInDeckByLevel[index]
                        )
                    );
                }
            }
        }
    }

    private void FillSpecialCards()
    {
        for (int index = 0; index < s_kSpecialCardByLevel.Count; ++index)
        {
            if (s_kSpecialCardByLevel[index].Count == 0)
                continue;
            
            foreach (var specialAttrCombo in s_kSpecialCardByLevel[index])
            {
                CardType type = specialAttrCombo.Item2 == "Crystal" ? CardType.kEffect : CardType.kUnit;

                m_deck[index].AddRange(
                    Enumerable.Repeat<Card>(
                        new Card(specialAttrCombo.Item1, specialAttrCombo.Item2, index, type), 
                        s_kNumCardsInDeckByLevel[index]
                    )
                );
            }
        }
    }

    public void Shuffle()
    {
        foreach (var list in m_deck)
        {
            GameUtil.Shuffle(list, m_rng);
        }
    }

    public void DrawCards(int playerLevel, List<Card?> cards)
    {
        RecycleOldCards(cards);
        FillNewCards(playerLevel, cards);
    }

    public void RecycleOldCards(List<Card?> cards)
    {
        foreach (var card in cards)
        {
            if (!card.HasValue)
                continue;

            m_deck[card.Value.deckId].Add(card.Value);
        }
    }

    private void FillNewCards(int playerLevel, List<Card?> cards)
    {
        playerLevel -= 1;

        for (int i = 0; i < cards.Count; ++i)
        {
            float val = m_rng.RandomFloat();
            int deckIndex = 0;

            while (val > 0f)
            {
                val -= s_kDropRateByPlayerLevel[playerLevel][deckIndex];

                if (val > 0f)
                {
                    ++deckIndex;
                }
                else if (m_deck[deckIndex].Count <= 0)
                {
                    val = m_rng.RandomFloat();
                    deckIndex = 0;
                }
            }

            int cardIndex = m_rng.RandomIntRange(0, m_deck[deckIndex].Count);
            cards[i] = m_deck[deckIndex][cardIndex];
            m_deck[deckIndex].RemoveAt(cardIndex);
        }
    }

    public int DeckSize()
    {
        int size = 0;
        Array.ForEach(m_deck, deck => size += deck.Count);

        return size;
    }

    public List<float> DropRates(int playerLevel)
    {
        return s_kDropRateByPlayerLevel[playerLevel - 1];
    }

    public void RecycleUnit(Unit unit)
    {
        Debug.Log($"deck id: {unit.BelongToDeck()}, deck size before: {m_deck[unit.BelongToDeck()].Count}");
        m_deck[unit.BelongToDeck()].Add(unit.ToCard());
        Debug.Log($"deck size after: {m_deck[unit.BelongToDeck()].Count}");
    }

    public void RecycleCrystals(string attrName, int deckId, int numberToRecycle)
    {
        for (int i = 0; i < numberToRecycle; ++i)
        {
            m_deck[deckId].Add(new Card(attrName, "Crystal", deckId, CardType.kEffect));
        }
    }
}
