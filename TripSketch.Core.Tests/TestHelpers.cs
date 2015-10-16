using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TripSketch.Core.Tests
{
    public static class TestHelpers
    {
        /// <summary>
        /// Executes an action and waits for a corresponding property change event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifier"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="updateAction">The logic that will cause the property to change</param>
        /// <param name="timeoutMS">An optional timeout, if the property change is not immediate</param>
        /// <returns></returns>
        public static async Task WaitForPropertyChangeAsync<T>(this INotifyPropertyChanged notifier, Expression<Func<T>> propertyExpression, Action updateAction, int timeoutMS = 0)
        {
            await WaitForPropertyChangeAsync(notifier, GetPropertyName(propertyExpression), timeoutMS, updateAction);
        }

        public static async Task WaitForPropertyChangeAsync(INotifyPropertyChanged notifier, string property, int timeoutMS, Action updateAction)
        {
            var tcs = new TaskCompletionSource<object>();

            PropertyChangedEventHandler handler = null;
            handler = (s, e) =>
            {
                if (e.PropertyName == property)
                {
                    notifier.PropertyChanged -= handler;
                    tcs.SetResult(null);
                }
            };

            notifier.PropertyChanged += handler;

            if (updateAction != null)
            {
                updateAction();
            }

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMS));

            if (completedTask != tcs.Task)
            {
                throw new TimeoutException("Timeout waiting for " + property + " property to change on " + notifier.GetType().ToString());
            }
        }

        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("Unsupported property expression. Must be '() => Property'");
            }

            var member = memberExpression.Member;

            if (memberExpression == null)
            {
                throw new ArgumentException("Unsupported property expression. Must be '() => Property'");
            }

            return member.Name;
        }
    }
}
