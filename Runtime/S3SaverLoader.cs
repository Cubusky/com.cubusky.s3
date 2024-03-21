#nullable enable
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

using Object = UnityEngine.Object;
using System.Linq;

namespace Cubusky.S3
{
    internal static class AmazonS3Extensions
    {
        public static Task<GetObjectResponse> GetObjectOneLinerAsync(this IAmazonS3 client, string bucketName, string key, string? versionId = null, CancellationToken cancellationToken = default) => string.IsNullOrEmpty(versionId)
            ? client.GetObjectAsync(bucketName, key, cancellationToken)
            : client.GetObjectAsync(bucketName, key, versionId, cancellationToken);

        public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var items = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                items.Add(item);
            }
            return items;
        }
    }

    public interface IS3Saver : ISaver, ISaver<MemoryStream>
    {
        IAmazonS3 client { get; }
        string bucketName { get; }
        string key { get; }
        List<global::Amazon.S3.Model.Tag>? tags { get; }

        void ISaver<string>.Save(string data) => Save(data);
        void ISaver<byte[]>.Save(byte[] data) => Save(data);
        void ISaver<MemoryStream>.Save(MemoryStream data) => Save(data);

        new PutObjectResponse Save(string data) => SaveAsync(data).GetAwaiter().GetResult();
        new PutObjectResponse Save(byte[] data) => SaveAsync(data).GetAwaiter().GetResult();
        new PutObjectResponse Save(MemoryStream data) => SaveAsync(data).GetAwaiter().GetResult();

        Task ISaver<string>.SaveAsync(string data, CancellationToken cancellationToken) => SaveAsync(data, cancellationToken);
        Task ISaver<byte[]>.SaveAsync(byte[] data, CancellationToken cancellationToken) => SaveAsync(data, cancellationToken);
        Task ISaver<MemoryStream>.SaveAsync(MemoryStream data, CancellationToken cancellationToken) => SaveAsync(data, cancellationToken);

        new Task<PutObjectResponse> SaveAsync(string data, CancellationToken cancellationToken = default) => SaveAsync(Encoding.UTF8.GetBytes(data), cancellationToken);
        new Task<PutObjectResponse> SaveAsync(byte[] data, CancellationToken cancellationToken = default) => SaveAsync(new MemoryStream(data), cancellationToken);
        new Task<PutObjectResponse> SaveAsync(MemoryStream data, CancellationToken cancellationToken = default)
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = data,
                TagSet = tags
            };

            return client.PutObjectAsync(putObjectRequest, cancellationToken);
        }
    }

    public interface IS3Loader : ILoader, ILoader<MemoryStream>, ILoader<GetObjectResponse>
    {
        IAmazonS3 client { get; }
        string bucketName { get; }
        string key { get; }
        string? versionId { get; }

        string ILoader<string>.Load<TData>() => LoadAsync<string>().GetAwaiter().GetResult();
        byte[] ILoader<byte[]>.Load<TData>() => LoadAsync<byte[]>().GetAwaiter().GetResult();
        MemoryStream ILoader<MemoryStream>.Load<TData>() => LoadAsync<MemoryStream>().GetAwaiter().GetResult();
        GetObjectResponse ILoader<GetObjectResponse>.Load<TData>() => LoadAsync<GetObjectResponse>().GetAwaiter().GetResult();

        async Task<string> ILoader<string>.LoadAsync<TData>(CancellationToken cancellationToken) => Encoding.UTF8.GetString(await LoadAsync<byte[]>(cancellationToken));
        async Task<byte[]> ILoader<byte[]>.LoadAsync<TData>(CancellationToken cancellationToken) => (await LoadAsync<MemoryStream>(cancellationToken)).GetBuffer();
        async Task<MemoryStream> ILoader<MemoryStream>.LoadAsync<TData>(CancellationToken cancellationToken)
        {
            var response = await LoadAsync<GetObjectResponse>(cancellationToken);
            MemoryStream memoryStream = new();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream;
        }

        Task<GetObjectResponse> ILoader<GetObjectResponse>.LoadAsync<TData>(CancellationToken cancellationToken) => client.GetObjectOneLinerAsync(bucketName, key, versionId, cancellationToken);
    }

    public interface IS3EnumerableLoader : IEnumerableLoader, IEnumerableLoader<MemoryStream>, IEnumerableLoader<GetObjectResponse>, IEnumerableLoader<ListObjectsV2Response>
    {
        IAmazonS3 client { get; }
        string bucketName { get; }
        string prefix { get; }
        public int maxKeys => 1000;
        public int maxCalls => -1;
        string? versionId { get; }

        IEnumerable<string> ILoader<IEnumerable<string>>.Load<TData>() => LoadAsync<IEnumerable<string>>().GetAwaiter().GetResult();
        IEnumerable<byte[]> ILoader<IEnumerable<byte[]>>.Load<TData>() => LoadAsync<IEnumerable<byte[]>>().GetAwaiter().GetResult();
        IEnumerable<MemoryStream> ILoader<IEnumerable<MemoryStream>>.Load<TData>() => LoadAsync<IEnumerable<MemoryStream>>().GetAwaiter().GetResult();
        IEnumerable<GetObjectResponse> ILoader<IEnumerable<GetObjectResponse>>.Load<TData>() => LoadAsync<IEnumerable<GetObjectResponse>>().GetAwaiter().GetResult();
        IEnumerable<ListObjectsV2Response> ILoader<IEnumerable<ListObjectsV2Response>>.Load<TData>() => LoadAsync<IEnumerable<ListObjectsV2Response>>().GetAwaiter().GetResult();

        Task<IEnumerable<string>> ILoader<IEnumerable<string>>.LoadAsync<TData>(CancellationToken cancellationToken) => LoadAsyncEnumerable<string>(cancellationToken).ToEnumerableAsync(cancellationToken);
        Task<IEnumerable<byte[]>> ILoader<IEnumerable<byte[]>>.LoadAsync<TData>(CancellationToken cancellationToken) => LoadAsyncEnumerable<byte[]>(cancellationToken).ToEnumerableAsync(cancellationToken);
        Task<IEnumerable<MemoryStream>> ILoader<IEnumerable<MemoryStream>>.LoadAsync<TData>(CancellationToken cancellationToken) => LoadAsyncEnumerable<MemoryStream>(cancellationToken).ToEnumerableAsync(cancellationToken);
        Task<IEnumerable<GetObjectResponse>> ILoader<IEnumerable<GetObjectResponse>>.LoadAsync<TData>(CancellationToken cancellationToken) => LoadAsyncEnumerable<GetObjectResponse>(cancellationToken).ToEnumerableAsync(cancellationToken);
        Task<IEnumerable<ListObjectsV2Response>> ILoader<IEnumerable<ListObjectsV2Response>>.LoadAsync<TData>(CancellationToken cancellationToken) => LoadAsyncEnumerable<ListObjectsV2Response>(cancellationToken).ToEnumerableAsync(cancellationToken);

        async IAsyncEnumerable<string> IEnumerableLoader<string>.LoadAsyncEnumerable<TData>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var buffer in LoadAsyncEnumerable<byte[]>(cancellationToken))
            {
                yield return Encoding.UTF8.GetString(buffer);
            }
        }

        async IAsyncEnumerable<byte[]> IEnumerableLoader<byte[]>.LoadAsyncEnumerable<TData>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var memoryStream in LoadAsyncEnumerable<MemoryStream>(cancellationToken))
            {
                yield return memoryStream.GetBuffer();
            }
        }

        async IAsyncEnumerable<MemoryStream> IEnumerableLoader<MemoryStream>.LoadAsyncEnumerable<TData>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var response in LoadAsyncEnumerable<GetObjectResponse>(cancellationToken))
            {
                MemoryStream memoryStream = new();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                yield return memoryStream;
            }
        }

        async IAsyncEnumerable<GetObjectResponse> IEnumerableLoader<GetObjectResponse>.LoadAsyncEnumerable<TData>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var response in LoadAsyncEnumerable<ListObjectsV2Response>().WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                foreach (var s3Object in response.S3Objects)
                {
                    yield return await client.GetObjectOneLinerAsync(bucketName, s3Object.Key, versionId, cancellationToken);
                }
            }
        }

        async IAsyncEnumerable<ListObjectsV2Response> IEnumerableLoader<ListObjectsV2Response>.LoadAsyncEnumerable<TData>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (maxCalls == 0)
            {
                yield break;
            }

            var listObjects = client.Paginators.ListObjectsV2(new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix,
                MaxKeys = maxKeys,
            });

            int currentCalls = maxCalls;
            await foreach (var response in listObjects.Responses.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return response;

                if (--currentCalls == 0)
                {
                    yield break;
                }
            }
        }
    }

    [Serializable]
    public class S3SaverLoader : IS3Saver, IS3Loader, IS3EnumerableLoader, ISerializationCallbackReceiver, ISaverLoader, ISaverLoader<MemoryStream>
    {
        [SerializeField, OfType(typeof(IRegionEndpoint)), Tooltip("The region that the bucket resides in.")] private Object? _region;
        [field: SerializeField, Tooltip("The bucket that will be saved to and loaded from.")] public string bucketName { get; set; } = string.Empty;
        [field: SerializeField, Path(true), Tooltip("The directory that will be saved to and loaded from.")] public string prefix { get; set; } = string.Empty;
        [field: SerializeField, Tooltip("The file that will be saved to and loaded from.")] public string key { get; set; } = string.Empty;

        [Header("Save")]
        [SerializeField, OfType(typeof(ICredentials))] private Object? _putCredentials;
        [field: SerializeField] public List<Tag>? tags { get; set; }

        [Header("Load")]
        [SerializeField, OfType(typeof(ICredentials))] private Object? _getCredentials;
        [field: SerializeField, Range(0, 1000), Tooltip("The maximum results that will be loaded in a single batch. Amazon allows collecting a maximum of a 1,000 results at once. It is recommended to always keep this at a 1,000.")] public int maxKeys { get; set; } = 1000;
        [field: SerializeField, Tooltip("The maximum calls that will be made to an S3 server for a single loading operation. -1 means calls will be made until all objects have been loaded. Note that every call made to an S3 server incurs Amazon service costs.")] public int maxCalls { get; set; } = -1;
        [field: SerializeField] public string versionId { get; set; } = string.Empty;

        public IRegionEndpoint? region { get; set; }
        public ICredentials? putCredentials { get; set; }
        public ICredentials? getCredentials { get; set; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _region = region as Object;
            _putCredentials = putCredentials as Object;
            _getCredentials = getCredentials as Object;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            region = _region as IRegionEndpoint;
            putCredentials = _putCredentials as ICredentials;
            getCredentials = _getCredentials as ICredentials;
        }

        List<global::Amazon.S3.Model.Tag>? IS3Saver.tags => tags?.ConvertAll(tag => (global::Amazon.S3.Model.Tag)tag);

        IAmazonS3 IS3Saver.client => region?.region == null
            ? throw new NullReferenceException($"{nameof(region)}.{nameof(region.region)} must not be null.")
            : putCredentials?.credentials == null
                ? new AmazonS3Client(region.region)
                : new AmazonS3Client(putCredentials.credentials, region.region);

        IAmazonS3 IS3Loader.client => region?.region == null
            ? throw new NullReferenceException($"{nameof(region)}.{nameof(region.region)} must not be null.")
            : getCredentials?.credentials == null
                ? new AmazonS3Client(region.region)
                : new AmazonS3Client(getCredentials.credentials, region.region);

        IAmazonS3 IS3EnumerableLoader.client => ((IS3Loader)this).client;

        string IS3Saver.key => $"{prefix.Replace('\\', '/')}/{key}";
        string IS3Loader.key => (this as IS3Saver).key;
    }
}
