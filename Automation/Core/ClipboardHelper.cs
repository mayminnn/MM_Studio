using FlaUI.Core.Tools;
using System;
using System.Threading;
using System.Windows;

namespace Automation.Core
{
    public static class ClipboardHelper
    {
        public static readonly Mutex ClipboardMutex =
            new Mutex(false, "Global\\MM_Studio_ClipboardMutex");

        public static string CopyAndGetText(Action performCopyGesture)
        {
            ClipboardMutex.WaitOne();
            try
            {
                performCopyGesture();
                Thread.Sleep(500);
                return GetTextSTA();
            }
            finally
            {
                ClipboardMutex.ReleaseMutex();
            }
        }

        public static string GetTextSTA()
        {
            string result = null;
            Exception error = null;

            var thread = new Thread(() =>
            {
                try
                {
                    Retry.WhileTrue(() =>
                    {
                        result = Clipboard.GetText();
                        return string.IsNullOrEmpty(result);
                    }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
                throw error;

            return result;
        }
    }
}