using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.Model;

namespace FChatDicebot
{
    public class BotMessageQueue
    {
        Queue<BotMessage> Messages;

        public BotMessageQueue()
        {
            Messages = new Queue<BotMessage>();
        }

        public BotMessage GetNextMessage()
        {
            if (Messages.Count > 0)
            {
                return Messages.Dequeue();
            }
            else
                return null;
        }

        public void AddMessage(BotMessage message)
        {
            Messages.Enqueue(message);
        }

        public BotMessage Peek()
        {
            return Messages.Peek();
        }

        public bool HasMessages()
        {
            return Messages.Count > 0;
        }

        //TODO: AddUrgentMessage to move a message to the front of the queue on add
        //public void AddUrgentMessage(BotMessage message)
        //{
        //    Messages.Enqueue(message);
        //}
    }
}
