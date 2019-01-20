using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colony_Ship_Horizon
{
    class ItemStack
    {
        public string _itemType;
        public int _stackLimit;
        public int itemsInStack = 1;
        public string _compatibleWith;

        public ItemStack(string itemType, int stackLimit, string compatibleWith)
        {
            _itemType = itemType;
            _stackLimit = stackLimit;
            _compatibleWith = compatibleWith;
        }

        public bool IsStackFull()
        {
            bool isFull = false;
            if (itemsInStack == _stackLimit)
                isFull = true;
            return isFull;
        }
    }
}
