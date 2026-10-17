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
                string msg = $"GET 失敗:  {request.error}";
                Debug.LogError(msg);
                SetResultText(msg);
                yield break;
            }

            string json = request.downloadHandler.text;

            string wrapped = string.Format("{{\"items\":{0}}}", json);

            ItemList list = JsonUtility.FromJson<ItemList>(wrapped);

            _builder.Clear();
            _builder.AppendLine("[GET] 一覧");
            _builder.AppendLine();

            if (list == null
                || list.items == null
                || list.items.Length <= 0)
            {
                _builder.AppendLine("(データなし)");
            }
            else
            {
                foreach (Item item in list.items)
                {
                    _builder.AppendFormat("ID:{0}, Name:{1}", item.id, item.name);
                    _builder.AppendLine();
                }
            }
            SetResultText(_builder.ToString());
        }
    }


    public IEnumerator PostItem()
    {
        // 入力チェック（サーバー側のバリデーションと同じ条件を先に弾く）
        string name = _nameInput != null ? _nameInput.text : null;

        if (string.IsNullOrEmpty(name)
            || name == " "
            || name == "　")
        {
            SetResultText("[POST] name を入力してください。");
            yield break;
        }

        // {"name":"..."} の形に整形
        string bodyJson = JsonUtility.ToJson(new CreateItemRequest(name));
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

        // POST 送信。生 JSON を送るため UploadHandlerRaw を使う
        using (UnityWebRequest request = new UnityWebRequest(_url + EndpointItems, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            SetInteractable(false);

            // リクエストを送信して応答が返ってくるまで待つ
            yield return request.SendWebRequest();

            SetInteractable(true);

            if (request.result != UnityWebRequest.Result.Success)
            {
                string msg = $"POST 失敗: {request.error}";
                Debug.LogError(msg);
                SetResultText(msg);
                yield break;
            }

            // サーバーは追加された 1 件（{"id":..,"name":..}）を返す
            string json = request.downloadHandler.text;
            Item created = JsonUtility.FromJson<Item>(json);

            _builder.Clear();
            _builder.AppendLine("[POST] 登録完了");
            _builder.AppendLine();

            if (created == null)
            {
                _builder.AppendLine("(レスポンスの解析に失敗しました)");
            }
            else
            {
                _builder.AppendFormat("ID:{0}, Name:{1}", created.id, created.name);
                _builder.AppendLine();
            }

            SetResultText(_builder.ToString());

            // 送信後に入力欄をクリア（任意）
            if (_nameInput != null)
            {
                _nameInput.text = string.Empty;
            }
        }
    }

    // 通信中にボタンを無効化して多重送信を防ぐ
    private void SetInteractable(bool interactable)
    {
        if (_getButton != null)
        {
            _getButton.interactable = interactable;
        }

        if (_postButton != null)
        {
            _postButton.interactable = interactable;
        }
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
            _getButton.onClick.RemoveListener(OnClickGet);
        }

        if (_postButton != null)
        {
            _postButton.onClick.RemoveListener(OnClickPost);
        }
    }

    #endregion
}