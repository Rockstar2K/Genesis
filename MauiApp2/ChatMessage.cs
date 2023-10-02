using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2
{
    public class ChatMessage
    {

       public string Text { get; set; }
       public bool IsUser { get; set; }  // true if this message is from the user, false if from the bot

    }
}
