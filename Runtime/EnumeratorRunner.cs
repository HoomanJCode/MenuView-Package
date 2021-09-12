using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class EnumeratorRunner : MonoBehaviour
    {
        public delegate void TaskCompletedDelegate();

        public delegate void TaskCompletedDelegateWithResult<in TResult>(TResult result);

        public delegate Task TaskToRunDelegate();

        public delegate Task<TResult> TaskToRunDelegateWithResult<TResult>();

        public static EnumeratorRunner Singletone;

        private static readonly Queue<Action> SyncRunList = new Queue<Action>();

        private void Update()
        {
            lock (SyncRunList)
            {
                while (SyncRunList.Any())
                    SyncRunList.Dequeue().Invoke();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            Singletone = new GameObject(nameof(EnumeratorRunner)).AddComponent<EnumeratorRunner>();
        }

        public static void Run(Action targetTask)
        {
            lock (SyncRunList)
            {
                SyncRunList.Enqueue(targetTask);
            }
        }

        public static void Run<TResult>(TaskToRunDelegateWithResult<TResult> taskToRun,
            TaskCompletedDelegateWithResult<TResult> taskCompleted, float timeOut = 5)
        {
            Run(() => { Singletone.StartCoroutine(TaskRunnerEnumerator(taskToRun, taskCompleted, timeOut)); });
        }

        private static IEnumerator TaskRunnerEnumerator<TResult>(TaskToRunDelegateWithResult<TResult> taskToRun,
            TaskCompletedDelegateWithResult<TResult> taskCompleted, float timeOut = 5)
        {
            var task = taskToRun.Invoke();
            var timer = 0f;
            while (!task.IsCompleted)
            {
                timer += Time.deltaTime;
                if (timer > timeOut)
                    yield break;
                yield return null;
            }

            taskCompleted.Invoke(task.Result);
        }

        public static void Run(TaskToRunDelegate taskToRun, TaskCompletedDelegate taskCompleted, float timeOut = 5)
        {
            Run(() => { Singletone.StartCoroutine(TaskRunnerEnumerator(taskToRun, taskCompleted, timeOut)); });
        }

        private static IEnumerator TaskRunnerEnumerator(TaskToRunDelegate taskToRun,
            TaskCompletedDelegate taskCompleted, float timeOut = 5)
        {
            var task = taskToRun.Invoke();
            var timer = 0f;
            while (!task.IsCompleted)
            {
                timer += Time.deltaTime;
                if (timer > timeOut)
                    yield break;
                yield return null;
            }

            taskCompleted.Invoke();
        }

        public static void Run(Action taskToRun, float after)
        {
            Run(() => { Singletone.StartCoroutine(TaskRunnerEnumerator(taskToRun, after)); });
        }

        private static IEnumerator TaskRunnerEnumerator(Action taskToRun, float after)
        {
            yield return new WaitForSeconds(after);
            taskToRun.Invoke();
        }
    }
}