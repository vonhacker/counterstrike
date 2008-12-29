using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSL.Common
{
    public class ErrorMessageBox
    {
        private ErrorMessageBox() { }

        private static WindowException _windowException;
        public static void Show(Exception ex)
        {
            if (_windowException == null)
                _windowException = new WindowException();
            _windowException.SetText(ex);
            _windowException.Show();
        }
    }
}
