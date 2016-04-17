namespace LeagueSharp.Data.Updater
{
    using System;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///     An ASCII progress bar
    /// </summary>
    public class ProgressBar : IDisposable, IProgress<double>
    {
        #region Constants

        private const string Animation = @"|/-\";

        private const int BlockCount = 20;

        #endregion

        #region Fields

        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(0.0125);

        private readonly Timer timer;

        private int animationIndex = 0;

        private double currentProgress = 0;

        private string currentText = string.Empty;

        private bool disposed = false;

        #endregion

        #region Constructors and Destructors

        public ProgressBar()
        {
            this.timer = new Timer(this.TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                this.ResetTimer();
            }
        }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            lock (this.timer)
            {
                this.disposed = true;
                this.UpdateText(string.Empty);
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref this.currentProgress, value);
        }

        #endregion

        #region Methods

        private void ResetTimer()
        {
            this.timer.Change(this.animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        private void TimerHandler(object state)
        {
            lock (this.timer)
            {
                if (this.disposed)
                {
                    return;
                }

                var progressBlockCount = (int)(this.currentProgress * BlockCount);
                var percent = (int)(this.currentProgress * 100);
                var text =
                    $"[{new string('#', progressBlockCount)}{new string('-', BlockCount - progressBlockCount)}] {percent,3}% {Animation[this.animationIndex++ % Animation.Length]}";
                this.UpdateText(text);

                this.ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            var commonPrefixLength = 0;
            var commonLength = Math.Min(this.currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == this.currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            var outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', this.currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            var overlapCount = this.currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            this.currentText = text;
        }

        #endregion
    }
}