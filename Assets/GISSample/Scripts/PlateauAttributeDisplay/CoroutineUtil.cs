using System.Collections;

namespace GISSample.PlateauAttributeDisplay
{
    public static class CoroutineUtil
    {
        public static readonly int YIELD_STEP = 40; // Coroutine実行時に一度に処理するGameObject数
        public static readonly int YIELD_STEP_FAST = 200;　// Coroutine実行時に一度に処理するGameObject数（高速）

        /// <summary>
        /// コルーチンを無視して実行
        /// </summary>
        /// <param name="routine"></param>
        public static void RunToEnd(IEnumerator routine)
        {
            while (routine.MoveNext())
            {
                if (routine.Current is IEnumerator nested)
                {
                    // 子コルーチンも最後まで実行
                    RunToEnd(nested);
                }
                // IEnumerator 以外（WaitForSeconds など）は無視
            }

        }

    }
}
