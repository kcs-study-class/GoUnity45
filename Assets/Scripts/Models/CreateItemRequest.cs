// CreateItemRequest.cs
using System;

[Serializable]
public class CreateItemRequest
{
    public string name;

    public CreateItemRequest(string name)
    {
        this.name = name;
    }
}
