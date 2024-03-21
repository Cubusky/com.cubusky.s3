#nullable enable
using Amazon;
using UnityEngine;

namespace Cubusky.S3
{
    [CreateAssetMenu(fileName = nameof(Region), menuName = nameof(Cubusky) + "/" + nameof(S3) + "/" + nameof(Region))]
    public class Region : ScriptableObject, ISerializationCallbackReceiver, IRegionEndpoint
    {
        [SerializeField, Delayed] private string? _region;

        public RegionEndpoint? region { get; set; }

        public static implicit operator RegionEndpoint?(Region region) => region.region;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _region = region?.SystemName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            try
            {
                region = RegionEndpoint.GetBySystemName(_region);
            }
            catch
            {
                region = null;
            }
        }
    }
}
