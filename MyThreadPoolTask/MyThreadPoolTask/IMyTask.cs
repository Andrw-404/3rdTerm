// <copyright file="IMyTask.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyThreadPoolTask
{
    /// <summary>
    /// Represents a task that returns a result and supports continuations.
    /// </summary>
    /// <typeparam name="TResult">Type of task completion result.</typeparam>
    public interface IMyTask<TResult>
    {
        /// <summary>
        /// Gets a value indicating whether the task is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets the result of the task completion.
        /// </summary>
        TResult Result { get; }

        /// <summary>
        /// Creates a continuation that runs after the current task is completed.
        /// </summary>
        /// <typeparam name="TNewResult">Continuation result type.</typeparam>
        /// <param name="next">A continuation function that accepts the result of the current task and returns a new value.</param>
        /// <returns>A new task that represents a continuation.</returns>
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> next);
    }
}
