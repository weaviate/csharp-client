using Weaviate.Client.Internal;

namespace Weaviate.Client.Tests.Unit;

public class TestMultiKeySortedList
{
    private record TestItem(int Key, string Value);

    [Fact]
    public void Add_WithItem_AddsToCorrectKey()
    {
        var list = new MultiKeySortedList<int, TestItem>(item => item.Key);

        var item1 = new TestItem(1, "first");
        var item2 = new TestItem(1, "second");
        var item3 = new TestItem(2, "third");

        list.Add(item1);
        list.Add(item2);
        list.Add(item3);

        Assert.Equal(2, list[1].Length);
        Assert.Contains(item1, list[1]);
        Assert.Contains(item2, list[1]);
        Assert.Single(list[2]);
        Assert.Contains(item3, list[2]);
    }

    [Fact]
    public void Add_WithKeyAndValue_AddsToCorrectKey()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 1, "second" },
            { 2, "third" },
        };

        Assert.Equal(new[] { "first", "second" }, list[1]);
        Assert.Equal(new[] { "third" }, list[2]);
    }

    [Fact]
    public void Add_WithKeyAndArray_AddsAllValues()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
            { 2, new[] { "third" } },
        };

        Assert.Equal(new[] { "first", "second" }, list[1]);
        Assert.Equal(new[] { "third" }, list[2]);
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ReturnsTrue()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s)) { { 1, "value" } };

        Assert.True(list.ContainsKey(1));
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        Assert.False(list.ContainsKey(1));
    }

    [Fact]
    public void Remove_WithExistingKey_RemovesKeyAndReturnsTrue()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s)) { { 1, "value" } };

        var result = list.Remove(1);

        Assert.True(result);
        Assert.False(list.ContainsKey(1));
    }

    [Fact]
    public void Remove_WithNonExistingKey_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        var result = list.Remove(1);

        Assert.False(result);
    }

    [Fact]
    public void TryGetValue_WithExistingKey_ReturnsTrueAndValues()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 1, "second" },
        };

        var result = list.TryGetValue(1, out var values);

        Assert.True(result);
        Assert.Equal(new[] { "first", "second" }, values);
    }

    [Fact]
    public void TryGetValue_WithNonExistingKey_ReturnsFalseAndNull()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        var result = list.TryGetValue(1, out var values);

        Assert.False(result);
        Assert.Null(values);
    }

    [Fact]
    public void Add_KeyValuePair_AddsValues()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));
        var kvp = new KeyValuePair<int, string[]>(1, ["first", "second"]);

        list.Add(kvp);

        Assert.Equal(new[] { "first", "second" }, list[1]);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 2, "second" },
        };

        list.Clear();

        Assert.Empty(list);
        Assert.False(list.ContainsKey(1));
        Assert.False(list.ContainsKey(2));
    }

    [Fact]
    public void Contains_WithMatchingKeyValuePair_ReturnsTrue()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };
        var kvp = new KeyValuePair<int, string[]>(1, ["first", "second"]);

        var result = list.Contains(kvp);

        Assert.True(result);
    }

    [Fact]
    public void Contains_WithNonMatchingValues_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };
        var kvp = new KeyValuePair<int, string[]>(1, ["different"]);

        var result = list.Contains(kvp);

        Assert.False(result);
    }

    [Fact]
    public void Contains_WithNonExistingKey_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));
        var kvp = new KeyValuePair<int, string[]>(1, ["value"]);

        var result = list.Contains(kvp);

        Assert.False(result);
    }

    [Fact]
    public void CopyTo_CopiesAllKeyValuePairs()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
            { 2, new[] { "third" } },
        };

        var array = new KeyValuePair<int, string[]>[2];
        list.CopyTo(array, 0);

        Assert.Equal(1, array[0].Key);
        Assert.Equal(new[] { "first", "second" }, array[0].Value);
        Assert.Equal(2, array[1].Key);
        Assert.Equal(new[] { "third" }, array[1].Value);
    }

    [Fact]
    public void Remove_KeyValuePair_WithMatchingPair_RemovesAndReturnsTrue()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };
        var kvp = new KeyValuePair<int, string[]>(1, ["first", "second"]);

        var result = list.Remove(kvp);

        Assert.True(result);
        Assert.False(list.ContainsKey(1));
    }

    [Fact]
    public void Remove_KeyValuePair_WithNonMatchingPair_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };
        var kvp = new KeyValuePair<int, string[]>(1, ["different"]);

        var result = list.Remove(kvp);

        Assert.False(result);
        Assert.True(list.ContainsKey(1));
    }

    [Fact]
    public void GetEnumerator_ReturnsAllKeyValuePairs()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
            { 2, new[] { "third" } },
        };

        var items = list.ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal(1, items[0].Key);
        Assert.Equal(new[] { "first", "second" }, items[0].Value);
        Assert.Equal(2, items[1].Key);
        Assert.Equal(new[] { "third" }, items[1].Value);
    }

    [Fact]
    public void Indexer_Get_ReturnsValues()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };

        var result = list[1];

        Assert.Equal(new[] { "first", "second" }, result);
    }

    [Fact]
    public void Indexer_Get_WithNonExistingKey_ReturnsEmpty()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        var result = list[1];

        Assert.Empty(result);
    }

    [Fact]
    public void Keys_ReturnsAllKeys()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 2, "second" },
            { 3, "third" },
        };

        Assert.Equal([1, 2, 3], list.Keys);
    }

    [Fact]
    public void Values_ReturnsAllValues()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 1, "second" },
            { 2, "third" },
        };

        Assert.Equal(["first", "second", "third"], list.Values);
    }

    [Fact]
    public void IDictionary_Keys_ReturnsAllKeys()
    {
        IDictionary<int, string[]> list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first" } },
            { 2, new[] { "second" } },
        };

        Assert.Equal([1, 2], list.Keys);
    }

    [Fact]
    public void IDictionary_Values_ReturnsAllValueArrays()
    {
        IDictionary<int, string[]> list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
            { 2, new[] { "third" } },
        };

        var values = list.Values.ToList();
        Assert.Equal(2, values.Count);
        Assert.Equal(new[] { "first", "second" }, values[0]);
        Assert.Equal(new[] { "third" }, values[1]);
    }

    [Fact]
    public void Count_ReturnsNumberOfKeys()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 1, "second" },
            { 2, "third" },
        };

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void IsReadOnly_ReturnsFalse()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void IDictionary_Indexer_Get_ReturnsArray()
    {
        IDictionary<int, string[]> list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first", "second" } },
        };

        var result = list[1];

        Assert.Equal(new[] { "first", "second" }, result);
    }

    [Fact]
    public void IDictionary_Indexer_Get_WithNonExistingKey_ReturnsEmptyArray()
    {
        IDictionary<int, string[]> list = new MultiKeySortedList<int, string>(s => int.Parse(s));

        var result = list[999];

        Assert.Empty(result);
    }

    [Fact]
    public void IDictionary_Indexer_Set_ReplacesValues()
    {
        IDictionary<int, string[]> list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, new[] { "first" } },
        };

        list[1] = ["replaced1", "replaced2"];

        var result = list[1];
        Assert.Equal(new[] { "replaced1", "replaced2" }, result);
    }

    [Fact]
    public void SortedOrder_MaintainsSortedKeys()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 5, "five" },
            { 1, "one" },
            { 3, "three" },
        };

        Assert.Equal([1, 3, 5], list.Keys);
    }

    [Fact]
    public void MultipleValuesPerKey_MaintainsInsertionOrder()
    {
        var list = new MultiKeySortedList<int, string>(s => int.Parse(s))
        {
            { 1, "first" },
            { 1, "second" },
            { 1, "third" },
        };

        Assert.Equal(new[] { "first", "second", "third" }, list[1]);
    }
}
