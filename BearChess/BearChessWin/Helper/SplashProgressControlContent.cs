using System.Text;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class SplashProgressControlContent
    {

        public string Identifier { get; set; }

        /// <summary>
        /// Current information
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Sub information
        /// </summary>
        public string SubLabel { get; set; }

        /// <summary>
        /// Progressbar is usong  <see cref="MaxValue"/> und <see cref="CurrentValue"/>.
        /// </summary>
        public bool ShowValues { get; set; }

        /// <summary>
        /// Max value for the progressbar where <see cref="MaxValue"/> &gt;=0.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Current value for the progressbar where  0 &lt;= <see cref="CurrentValue"/> &lt;= <see cref="MaxValue"/>
        /// </summary>
        public double CurrentValue { get; set; }

        /// <summary>
        /// Cancel indicated
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Cancel allowed
        /// </summary>
        public bool ShowCancel { get; set; }

        /// <summary>
        ///  Work is finished (e.g. <see cref="CurrentValue"/> is equal to <see cref="MaxValue"/>).
        /// </summary>
        public bool IsFinished { get; set; }

        public SplashProgressControlContent()
        {
            CurrentValue = 0;
        }


    }
}
