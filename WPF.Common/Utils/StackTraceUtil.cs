using WPF.Common.Mvvm;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AIXAC_RX.Common.Utils
{
    public static class StackTraceUtil
    {
        /// <summary>
        /// 현재 스택의 함수 이름을 반환
        /// </summary>
        /// <returns>현재 스택의 함수 이름</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string CurrentFunction(this ViewModelBase viewModel)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}
