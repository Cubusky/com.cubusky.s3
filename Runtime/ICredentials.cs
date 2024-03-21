using Amazon.Runtime;

namespace Cubusky.S3
{
    public interface ICredentials
    {
        AWSCredentials credentials { get; }
    }
}
