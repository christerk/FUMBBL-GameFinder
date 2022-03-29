using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class EventQueue
    {
        private BlockingCollection<Action> _eventQueue;
        private ILogger _logger;
        public EventHandler? Tick;
        private bool _started;

        public int TickTimeout { get; set; } = 1000;

        public EventQueue(ILogger logger)
        {
            _eventQueue = new();
            _logger = logger;
        }

        public void Start()
        {
            lock (this)
            {
                if (!_started)
                {
                    _eventQueue = new();
                    Task.Run((Action)(() =>
                    {
                        var lastTick = DateTime.Now;
                        while (!_eventQueue.IsAddingCompleted)
                        {
                            try
                            {
                                if (_eventQueue.TryTake(out Action? action, TickTimeout))
                                {
                                    action.Invoke();
                                }
                                if ((DateTime.Now - lastTick).TotalMilliseconds > 1000)
                                {
                                    Tick?.Invoke(this, EventArgs.Empty);
                                    lastTick = DateTime.Now;
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e.Message, e);
                            }
                        }
                    }));
                    _started = true;
                }
            }
        }

        public void Stop()
        {
            _eventQueue.CompleteAdding();
        }

        private void Dispatch(Action action) => _eventQueue.Add(action);

        public Task DispatchAsync(Action action)
        {
            action.Invoke();
            return Task.CompletedTask;
        }

        public async Task DispatchAsync(Func<Task> asyncAction)
        {
            TaskCompletionSource result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(async () =>
            {
                await asyncAction.Invoke();
                result.SetResult();
            }
            );
            await result.Task;
        }

        #region Serialized() helper methods
        public Task<T> Serialized<T>(Action<TaskCompletionSource<T>> func)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(result));
            return result.Task;
        }

        public Task<T> Serialized<P, T>(Action<P, TaskCompletionSource<T>> func, P param)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(param, result));
            return result.Task;
        }

        public Task<T> Serialized<P1, P2, T>(Action<P1, P2, TaskCompletionSource<T>> func, P1 param1, P2 param2)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(param1, param2, result));
            return result.Task;
        }
        #endregion
    }
}
