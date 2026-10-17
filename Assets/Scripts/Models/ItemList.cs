using System;

/// <summary>
/// JsonUtility は配列を取り扱えないため、配列をラップしたクラスを作成.
/// </summary>
[Serializable]
public class ItemList
{
    public Item[] items;
}

/// <summary>
/// サーバーとやりとりする１件分のデータ.
/// JsonUtility で使用するため、フィールド名はJSONのキーと同じにする.
/// </summary>
[Serializable]
public class Item
{
    public int id;
    public string name;
}