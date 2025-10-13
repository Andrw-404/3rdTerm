using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThreadPoolTask
{
    public interface IMyTask<TResult>
    {
        bool IsCompleted { get; }

        TResult Result { get; }

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> next);
    }
}
