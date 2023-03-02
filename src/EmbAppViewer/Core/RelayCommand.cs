using System;

namespace EmbAppViewer.Core
{
    public class RelayCommand : TypedRelayCommand<object>
    {
        public RelayCommand(Action<object?> methodToExecute, Func<object?, bool>? canExecuteEvaluator = null)
            : base(methodToExecute, canExecuteEvaluator)
        {
        }
    }
}
