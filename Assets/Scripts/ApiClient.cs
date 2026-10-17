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

    [Header("サーバー設定")] [Tooltip("GoサーバーのエンドポイントURL")] [SerializeField]
    private string _url = "http://localhost:8080/";

    [Header("UI")] [Tooltip("POSTする名前を入力する欄")] [SerializeField]
    private TMP_InputField _nameInput;

    [Tooltip("GET を実行するボタン")] [SerializeField]
    private Button _getButton;

    [Tooltip("POST を実行するボタン")] [SerializeField]
    private Button _postButton;

    [Tooltip("通信結果を表示するテキスト")] [SerializeField]
    private TMP_Text _resultText;


    private StringBuilder _builder = new StringBuilder();


    private void OnClickGet()
    {
        StartCoroutine(GetItems());
    }

    private void OnClickPost()
    {
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