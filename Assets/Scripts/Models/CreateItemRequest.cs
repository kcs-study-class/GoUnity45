using System;

[Serializable]
public class CreateItemRequest
{
    public string name = string.Empty;
    public CreateItemRequest(string name)
    {
        this.name = name;
    }
}
