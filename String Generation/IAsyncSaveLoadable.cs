using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public interface IAsyncSaveLoadable<TSelf>
{
    public static abstract Task<TSelf> LoadAsync(string path);
    public Task SaveAsync(string path);
}
