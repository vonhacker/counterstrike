using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CSL.Common
{
    public static class SharedDictionaryManager
    {
        private static ResourceDictionary _sharedDictionary;

        public static ResourceDictionary SharedDictionary
        {
            get
            {
                if (_sharedDictionary == null)
                {
                    System.Uri resourceLocater =
                        new System.Uri("/CSL.Common;component/DictionaryIcon.xaml",
                            System.UriKind.Relative);
                    _sharedDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
                }
                return _sharedDictionary;
            }
        }
    }
}
