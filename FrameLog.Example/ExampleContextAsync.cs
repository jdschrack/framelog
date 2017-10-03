using FrameLog.Example.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FrameLog.Example
{
    public partial class ExampleContext
    {
        public async Task<ISaveResult<ChangeSet>> SaveAsync(User author, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Logger.SaveChangesAsync(author, cancellationToken);
        }
    }
}
