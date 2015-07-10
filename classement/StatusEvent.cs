using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagMyFiles
{
    static class StatusEvent
    {
        public static void FireStatus(object sender, StatusEventHandler eventHandler, string str, System.Drawing.Color c)
        {
            //StatusEventHandler handler = eventHandler;
            if (eventHandler != null)
            {
                StatusEventArgs args = new StatusEventArgs();
                args.Text = str;
                args.TextColor = c;

                eventHandler(sender, args);
            }
        }
        public static void FireStatusError(object sender, StatusEventHandler eventHandler, string str)
        {
            FireStatus(sender, eventHandler, str, System.Drawing.Color.Red);
        }

        public static void FireStatusInfo(object sender, StatusEventHandler eventHandler, string str)
        {
            FireStatus(sender, eventHandler, str, System.Drawing.Color.Blue);
        }
        
    }

    public class StatusEventArgs : EventArgs
    {
        public string Text { get; set; }
        public System.Drawing.Color TextColor { get; set; }
    }

    public delegate void StatusEventHandler(object sender,StatusEventArgs args);

}
