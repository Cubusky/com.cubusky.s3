#nullable enable
using Amazon.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cubusky.S3
{
    [CreateAssetMenu(fileName = nameof(BasicCredentials), menuName = nameof(Cubusky) + "/" + nameof(S3) + "/" + nameof(BasicCredentials))]
    public class BasicCredentials : ScriptableObject, ICredentials, ISerializationCallbackReceiver
    {
        private const string usageWarning = "THESE KEYS ARE NOT ENCRPYTED! \nDo not use this object in builds or post it on GitHub unless you protect it first. \nIn the worst case scenario, hackers can use your AWS services to mine bitcoins, leaving you with massive debt.";

        [HelpBox(usageWarning, HelpBoxMessageType.Warning)]
        [SerializeField] private string? _accessKey;
        [SerializeField, Password, Tooltip("This key is not actually protected from hackers. Opening this object in a text editor reveals your secret key. The masking is just a formality so you can screen share savely.")] private string? _secretKey;

        public string? accessKey
        {
            get => _accessKey;
            set => SetCredentials(_accessKey = value);
        }

        public string? secretKey 
        {
            get => _secretKey;
            set => SetCredentials(_secretKey = value);
        }

        public AWSCredentials? credentials { get; private set; }
        private void SetCredentials(params object?[] obj)
        {
            try
            {
                credentials = new BasicAWSCredentials(accessKey, secretKey);
            }
            catch
            {
                credentials = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var credentials = this.credentials?.GetCredentials();
            if (credentials != null)
            {
                accessKey = credentials.AccessKey;
                secretKey = credentials.SecretKey;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => SetCredentials();
    }
}
