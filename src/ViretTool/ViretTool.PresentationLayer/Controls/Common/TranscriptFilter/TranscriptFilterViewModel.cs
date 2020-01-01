using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.Common.TranscriptFilter
{
    public class TranscriptFilterViewModel : PropertyChangedBase
    {
        private string _inputText;
        public string InputText 
        {
            get => _inputText;
            set
            {
                if (_inputText == value)
                {
                    return;
                }

                _inputText = value;
                NotifyOfPropertyChange();
            }
        }


        public void Execute(KeyEventArgs keyArgs)
        {
            if (keyArgs.Key == Key.Enter)
            {
                // Do Stuff
            }
        }
    }
}
