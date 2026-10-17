using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ApiClient : MonoBehaviour
{
    private const string EndpointItems = "items";

    [Header("サーバー設定")]
    [Tooltip("GoサーバーのエンドポイントURL")]
    [SerializeField]
    private string _url = "http://localhost:8080/";

    [Header("UI")]
    [Tooltip("POSTする名前を入力する欄")]
    [SerializeField]
    private TMP_InputField _nameInput;

    [Tooltip("GET を実行するボタン")]
    [SerializeField]
    private Button _getButton;

    [Tooltip("POST を実行するボタン")]
    [SerializeField]
    private Button _postButton;

    [Tooltip("通信結果を表示するテキスト")]
    [SerializeField]
    private TMP_Text _resultText;


    private StringBuilder _builder = new StringBuilder();


    private void OnClickGet()
    {
        StartCoroutine(GetItems());
    }

    private void OnClickPost()
    {
        StartCoroutine(PostItem());
    }

    public IEnumerator GetItems()
    {
        // 本来は + ではなく、string.Format などで連結すべき、あと引数化。
        using (UnityWebRequest request = UnityWebRequest.Get(_url + EndpointItems))
        {
            // リクエストを送信して応答が返ってくるまで待つ.
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string msg = $"GET Failed:  {request.error}";
                Debug.LogError(msg);
                SetResultText(msg);
                yield break;
            }

            string json = request.downloadHandler.text;

            string wrapped = string.Format("{{\"items\":{0}}}", json);

            ItemList list = JsonUtility.FromJson<ItemList>(wrapped);

            if (TryMakeSBFromItemList(list, "GET", ref _builder))
            {
                SetResultText(_builder.ToString());
            }

        }
    }

    public IEnumerator PostItem()
    {
        // _nameInput(InputFieldのnullチェック).
        if (_nameInput == null)
        {
            SetResultText("[Post] _nameInput is Null.");
            yield break;
        }
        string name = _nameInput.text;

        // name null チェック.
        if (string.IsNullOrEmpty(name))
        {
            SetResultText("[Post] _nameInput.text is Null or Empty.");
            yield break;
        }

        // name 空白チェック.
        if (name == " " // 半角スペース
            || name == "　")　// 全角スペース
        {
            SetResultText("[Post] _nameInput.text is Space.");
            yield break;
        }

        // 上記のチェック処理は別メソッド化しても良いかもしれない。

        string bodyJson = JsonUtility.ToJson(new CreateItemRequest(name));

        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

        using (UnityWebRequest req
            = new UnityWebRequest(_url + EndpointItems, UnityWebRequest.kHttpVerbPOST))
        {
            // POST の場合は UploadHandlerRaw を使う必要がある。
            // ここら辺はできればObjectpool化して使い回すと良いかもしれない。
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // DownloadHandlerBuffer は必須ではないが、レスポンスを受け取る場合は必要。
            // ここら辺はできればObjectpool化して使い回すと良いかもしれない。
            req.downloadHandler = new DownloadHandlerBuffer();

            // Content-Type を application/json にする必要がある。
            // 拡張メソッドを作って、JsonUtility.ToJson() で生成した文字列をそのまま渡せるようにすると便利かもしれない。
            req.SetRequestHeader("Content-Type", "application/json");

            // 追加予定：Postボタンの無効化.

            // リクエストを送信して応答が返ってくるまで待つ.
            yield return req.SendWebRequest();

            // 追加予定：Postボタンの有効化.

            // 通信結果のチェック.
            if (req.result != UnityWebRequest.Result.Success)
            {
                // 失敗時の処理.
                string msg = $"POST Failed:  {req.error}";
                Debug.LogError(msg);
                SetResultText(msg);
                yield break;
            }

            // 成功時の処理.
            string json = req.downloadHandler.text;

            Item item = JsonUtility.FromJson<Item>(json);

            _builder.Clear();

            _builder.AppendLine("[POST] Lists");
            _builder.AppendLine();

            if (TryGetItemData(item, _builder))
            {
                SetResultText(_builder.ToString());
            }

        }

        if (_nameInput != null)
        {
            _nameInput.text = string.Empty;
        }
    }


    private static bool TryMakeSBFromItemList(ItemList list, string method, ref StringBuilder builder)
    {
        if (builder == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(method))
        {
            // method が null または空文字の場合は Unknown とする。
            // const string Unknown = "Unknown"; にすべき。
            method = "Unknown";
        }

        builder.Clear();
        builder.AppendFormat("[{0}] Lists", method);
        builder.AppendLine();
        builder.AppendLine();

        if (list == null
            || list.items == null
            || list.items.Length <= 0)
        {
            builder.AppendLine("(Data is Empty)");
        }
        else
        {
            foreach (Item item in list.items)
            {
                TryGetItemData(item, builder);
            }
        }
        return true;
    }


    private static bool TryGetItemData(Item item, StringBuilder builder)
    {
        if (item == null
            || builder == null)
        {
            return false;
        }
        builder.AppendFormat("ID:{0}, Name:{1}", item.id, item.name);
        builder.AppendLine();
        return true;
    }




    private void SetResultText(string text)
    {
        if (_resultText != null)
        {
            _resultText.SetText(text);
        }
    }


    #region UnityEvent

    private void Awake()
    {
        if (_getButton != null)
        {
            _getButton.onClick.AddListener(OnClickGet);
        }

        if (_postButton != null)
        {
            _postButton.onClick.AddListener(OnClickPost);
        }
    }

    private void OnDestroy()
    {
        if (_getButton != null)
        {
            _getButton.onClick.RemoveAllListeners();
        }

        if (_postButton != null)
        {
            _postButton.onClick.RemoveListener(OnClickPost);
        }
    }

    #endregion
}